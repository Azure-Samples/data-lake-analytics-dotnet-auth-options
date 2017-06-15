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

## Azure Active Directory tokens
When your application authenticates against AAD using any of these methods, tokens are received from AAD that are used for each request to Azure Data Lake Analytics. The token represents the user or the application, depending on the method, and is used to validate that the request is authorized to perform the desired action.

When authenticating, you'll specify a token audience, which specifies the API endpoint for which the token should be valid. For resource- or account-related operations, you'll use the Azure Resource Manager (ARM) token audience ``https://management.core.windows.net/``. For all Data Lake data plane operations, such as job submission, catalog exploration, or file access, you'll use the ADL token audience ``https://datalake.azure.net/``.

## Interactive - User popup
Use this option if you want to have a browser popup appear when the user signs in to your application, showing an AAD login form. From this interactive popup, your application will receive the tokens necessary to use the Data Lake Analytics .NET SDK on behalf of the user.

The user will need to have appropriate permissions in order for your application to perform certain actions. To understand the different permissions involved when using Data Lake Analytics, see [Add a new user](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-manage-use-portal#add-a-new-user).

Here's a code snippet showing how to sign in your user:

    abc

### Caching the user's login session
The basic case for the user popup approach is that the end-user will log in each time the application is run. Often for convenience, application developers choose to allow their users to sign in once and have the application keep track of the session, even after closing and reopening the application. To do this with [Azure's .NET SDK for client authentication](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime.Azure.Authentication), you'll need to use a token cache.

A token cache is an object that stores tokens for retrieval by your application. This object can be saved to a file, and it can be loaded from a file when your application initializes. If the user's token is available and still valid, the user popup won't need to be shown. Here's a code snippet showing how to load and use a ``TokenCache``:

    abc

## Non-interactive - Service principal / application

Use this option if you want to have your application authenticate against AAD using its own credentials, rather than those of a user. Using this process, your application will receive the tokens necessary to use the Data Lake Analytics .NET SDK as a service principal, which represents your application in AAD.

You will first need to provision a service principal (also known as a registered application) in AAD. To create a service principal with a certificate or a secret key, [follow the steps in this article](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-authenticate-service-principal).

The service principal, just like a user, will need to have appropriate permissions in order for your application to perform certain actions. Regardless of whether a user is the one running your application, the service principal's credentials will be used, and the user's credentials will not be used. To understand the different permissions involved when using Data Lake Analytics, see [Add a new user](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-manage-use-portal#add-a-new-user).

Here's a code snippet showing how your application can authenticate as a service principal:

    abc

# Next step after authentication
Once your have followed one of the approaches for authentication, you're ready to set up your ADLA .NET SDK client objects, which you'll use to perform various actions with the service:


    DataLakeAnalyticsJobManagementClient adlaJobClient = new DataLakeAnalyticsJobManagementClient(creds);

# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
