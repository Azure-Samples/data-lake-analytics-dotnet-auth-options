# Authenticating your application against Azure Active Directory

When building an application that uses the .NET SDK for Data Lake Analytics (ADLA), an important step is deciding how you want to have your application sign in to (or authenticate against) Azure Active Directory (AAD). There are a few ways to authenticate against AAD using [Azure's .NET SDK for client authentication](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime.Azure.Authentication):

 * Interactive - User login
    * Popup
    * Device code (not yet supported)
 * Non-interactive - Service principal / application
    * Using a secret key
    * Using a certificate

See [our published sample .NET code](https://azure.microsoft.com/en-us/resources/samples/data-lake-analytics-dotnet-auth-options) for a solution that shows how to use all of these options. The sample code, as well as the snippets in this article, use the following package versions:
 
 * [Microsoft.Rest.ClientRuntime.Azure.Authentication](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime.Azure.Authentication) - v2.3.1
 * [Microsoft.Azure.Management.DataLake.Analytics](https://www.nuget.org/packages/Microsoft.Azure.Management.DataLake.Analytics) - v3.0.0
 * [Microsoft.Azure.Management.DataLake.Store](https://www.nuget.org/packages/Microsoft.Azure.Management.DataLake.Store) - v2.2.0
 
You will need some or all of the following namespaces included at the top of your class file:

    using System;
    using System.Threading;
    using System.Security.Cryptography.X509Certificates;

    using Microsoft.Rest;
    using Microsoft.Rest.Azure.Authentication;
    using Microsoft.Azure.Management.DataLake.Analytics;
    using Microsoft.Azure.Management.DataLake.Analytics.Models;
    using Microsoft.Azure.Management.DataLake.Store;
    using Microsoft.Azure.Management.DataLake.Store.Models;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

## Azure Active Directory tokens
When your application authenticates against AAD using any of these methods, tokens are received from AAD that are used for each request to Azure Data Lake Analytics. The token represents the user or the application, depending on the method, and is used to validate that the request is authorized to perform the desired action.

When authenticating, you'll specify a token audience, which specifies the API endpoint for which the token should be valid. For resource- or account-related operations, you'll use the Azure Resource Manager (ARM) token audience ``https://management.core.windows.net/``. For all Data Lake data plane operations, such as job submission, catalog exploration, or file access, you'll use the ADL token audience ``https://datalake.azure.net/``.

## Interactive - User popup
Use this option if you want to have a browser popup appear when the user signs in to your application, showing an AAD login form. From this interactive popup, your application will receive the tokens necessary to use the Data Lake Analytics .NET SDK on behalf of the user.

The user will need to have appropriate permissions in order for your application to perform certain actions. To understand the different permissions involved when using Data Lake Analytics, see [Add a new user](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-manage-use-portal#add-a-new-user).

Here's a code snippet showing how to sign in your user:

    ...
    
    static void Main(string[] args)
    {
        const string DOMAIN = "<AAD tenant ID or domain>";
        const string ARM_TOKEN_AUDIENCE = @"https://management.core.windows.net/";
        const string ADL_TOKEN_AUDIENCE = @"https://datalake.azure.net/";

        ServiceClientCredentials armCreds = GetCredsInteractivePopup(DOMAIN, ARM_TOKEN_AUDIENCE);
        ServiceClientCredentials adlCreds = GetCredsInteractivePopup(DOMAIN, ADL_TOKEN_AUDIENCE);
    }
    
    public static ServiceClientCredentials GetCredsInteractivePopup(string domain, string tokenAudience, PromptBehavior promptBehavior = PromptBehavior.Auto)
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        ActiveDirectoryClientSettings clientSettings = new ActiveDirectoryClientSettings
        {
            ClientId = "1950a258-227b-4e31-a9cf-717495945fc2",
            ClientRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob"),
            PromptBehavior = promptBehavior
        };

        ActiveDirectoryServiceSettings serviceSettings = ActiveDirectoryServiceSettings.Azure;
        serviceSettings.TokenAudience = new Uri(tokenAudience);

        ServiceClientCredentials creds = UserTokenProvider.LoginWithPromptAsync(domain, clientSettings, serviceSettings).GetAwaiter().GetResult();

        return creds;
    }

### Caching the user's login session
The basic case for the user popup approach is that the end-user will log in each time the application is run. Often for convenience, application developers choose to allow their users to sign in once and have the application keep track of the session, even after closing and reopening the application. To do this with [Azure's .NET SDK for client authentication](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime.Azure.Authentication), you'll need to use a token cache.

A token cache is an object that stores tokens for retrieval by your application. This object can be saved to a file, and it can be loaded from a file when your application initializes. If the user's token is available and still valid, the user popup won't need to be shown. Here's a code snippet showing how to load and use a ``TokenCache``:

    ...
    
    static void Main(string[] args)
    {
        string domain = "<AAD tenant ID / domain>";
        Uri armTokenAudience = new Uri(@"https://management.core.windows.net/");
        Uri adlTokenAudience = new Uri(@"https://datalake.azure.net/");
        
        // Show how to load the tokenCache into memory

        ServiceClientCredentials armCreds = GetCredsInteractivePopup(domain, armTokenAudience, tokenCache);
        ServiceClientCredentials adlCreds = GetCredsInteractivePopup(domain, adlTokenAudience, tokenCache);
    }
    
    public static ServiceClientCredentials GetCredsInteractivePopup(string domain, Uri tokenAudience, TokenCache tokenCache, PromptBehavior promptBehavior = PromptBehavior.Auto)
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        ActiveDirectoryClientSettings clientSettings = new ActiveDirectoryClientSettings
        {
            ClientId = "1950a258-227b-4e31-a9cf-717495945fc2",
            ClientRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob"),
            PromptBehavior = promptBehavior
        };

        ActiveDirectoryServiceSettings serviceSettings = ActiveDirectoryServiceSettings.Azure;
        serviceSettings.TokenAudience = tokenAudience;

        ServiceClientCredentials creds = UserTokenProvider.LoginWithPromptAsync(domain, clientSettings, serviceSettings, tokenCache).GetAwaiter().GetResult();

        return creds;
    }

## Non-interactive - Service principal / application

Use this option if you want to have your application authenticate against AAD using its own credentials, rather than those of a user. Using this process, your application will receive the tokens necessary to use the Data Lake Analytics .NET SDK as a service principal, which represents your application in AAD.

You will first need to provision a service principal (also known as a registered application) in AAD. To create a service principal with a certificate or a secret key, [follow the steps in this article](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-authenticate-service-principal).

The service principal, just like a user, will need to have appropriate permissions in order for your application to perform certain actions. Regardless of whether a user is the one running your application, the service principal's credentials will be used, and the user's credentials will not be used. To understand the different permissions involved when using Data Lake Analytics, see [Add a new user](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-manage-use-portal#add-a-new-user).

Here's a code snippet showing how your application can authenticate as a service principal that uses a secret key:

    ...
    
    static void Main(string[] args)
    {
        string domain = "<AAD tenant ID / domain>";
        Uri armTokenAudience = new Uri(@"https://management.core.windows.net/");
        Uri adlTokenAudience = new Uri(@"https://datalake.azure.net/");
        
        string clientId = "<service principal / application client ID>";
        string secretKey = "<service principal / application secret key>";

        ServiceClientCredentials armCreds = GetCredsServicePrincipalSecretKey(domain, armTokenAudience, clientId, secretKey);
        ServiceClientCredentials adlCreds = GetCredsServicePrincipalSecretKey(domain, adlTokenAudience, clientId, secretKey);
    }
    
    public static ServiceClientCredentials GetCredsServicePrincipalSecretKey(string domain, Uri tokenAudience, string clientId, string secretKey)
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        var serviceSettings = ActiveDirectoryServiceSettings.Azure;
        serviceSettings.TokenAudience = tokenAudience;

        var creds = ApplicationTokenProvider.LoginSilentAsync(domain, clientId, secretKey, serviceSettings).GetAwaiter().GetResult();

        return creds;
    }

Here's a code snippet showing how your application can authenticate as a service principal that uses a certificate:

    ...
    
    static void Main(string[] args)
    {
        string domain = "<AAD tenant ID / domain>";
        Uri armTokenAudience = new Uri(@"https://management.core.windows.net/");
        Uri adlTokenAudience = new Uri(@"https://datalake.azure.net/");
        
        string clientId = "<service principal / application client ID>";
        X509Certificate2 certificate = new X509Certificate2(@"<path to (PFX) certificate file>", "<certificate password>");

        ServiceClientCredentials armCreds = GetCredsServicePrincipalSecretKey(domain, armTokenAudience, clientId, secretKey);
        ServiceClientCredentials adlCreds = GetCredsServicePrincipalSecretKey(domain, adlTokenAudience, clientId, secretKey);
    }
    
    public static ServiceClientCredentials GetCredsServicePrincipalCertificate(string domain, Uri tokenAudience, string clientId, X509Certificate2 certificate)
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        var clientAssertionCertificate = new ClientAssertionCertificate(clientId, certificate);
        var serviceSettings = ActiveDirectoryServiceSettings.Azure;
        serviceSettings.TokenAudience = tokenAudience;

        var creds = ApplicationTokenProvider.LoginSilentWithCertificateAsync(domain, clientAssertionCertificate, serviceSettings).GetAwaiter().GetResult();

        return creds;
    }

# Next step after authentication
Once your have followed one of the approaches for authentication, you're ready to set up your ADLA .NET SDK client objects, which you'll use to perform various actions with the service. You can then perform actions using the clients, like so:

    static void Main(string[] args)
    {
        ...
        
        DataLakeAnalyticsJobManagementClient adlaJobClient = new DataLakeAnalyticsJobManagementClient(adlCreds);
        DataLakeAnalyticsCatalogManagementClient adlaCatalogClient = new DataLakeAnalyticsCatalogManagementClient(adlCreds);
        DataLakeAnalyticsAccountManagementClient adlaAccountClient = new DataLakeAnalyticsAccountManagementClient(armCreds);
        adlaAccountClient.SubscriptionId = subscriptionId;
        
        DataLakeAnalyticsAccount account = adlaAccountClient.Account.Get(resourceGroupName, adlaAccountName);
        
        Console.WriteLine($"My account's location is: {account.Location}!");
    }

# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
