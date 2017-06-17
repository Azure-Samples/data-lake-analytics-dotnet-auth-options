# Authenticating your application against Azure Active Directory

## Overview
When building an application that uses the .NET SDK for Data Lake Analytics (ADLA), you need to pick how your application will sign in to Azure Active Directory (AAD). 

There are two fundamental ways to have your application sign-in:
* **Interactive** - Use this method when your application has a user directly using your application and your app needs to perform operations in the context of that user.
* **Non-interactive** - Thus this method when your application is not meant to interact with ADLA as a specific user. This is useful for long-running services.

## Required NuGet packages

* [Microsoft.Rest.ClientRuntime.Azure.Authentication](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime.Azure.Authentication) - v2.3.1
* [Microsoft.Azure.Management.DataLake.Analytics](https://www.nuget.org/packages/Microsoft.Azure.Management.DataLake.Analytics) - v3.0.0
* [Microsoft.Azure.Management.DataLake.Store](https://www.nuget.org/packages/Microsoft.Azure.Management.DataLake.Store) - v2.2.0
* [Microsoft.Azure.Management.ResourceManager](https://www.nuget.org/packages/Microsoft.Azure.Management.ResourceManager) - 1.6.0-preview

You can install these packages via the NuGet commane line with the following commands:

```
Install-Package -Id Microsoft.Rest.ClientRuntime.Azure.Authentication  -Version 2.3.1
Install-Package -Id Microsoft.Azure.Management.DataLake.Analytics  -Version 3.0.0
Install-Package -Id Microsoft.Azure.Management.DataLake.Store  -Version 2.2.0
Install-Package -Id Microsoft.Azure.Management.ResourceManager  -Version 1.6.0-preview
```

## Namespaces used in samples

To simplify the code samples, ensure you have the following `using` statements at the top of your C# code.

```
using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.DataLake.Analytics;
using Microsoft.Azure.Management.DataLake.Analytics.Models;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.Azure.Management.DataLake.Store.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
```


## Basic authentication workflow

For a given domain (tenant). Your code needs to get credentials (tokens) for each end Azure REST endpoint (token audience) that you intend to use. Once the credentials are retrieved, then REST clients are built using those credentials.

#### Token Audiences
These are the Azure REST endpoints (token audiences) that are used in the samples:
* Azure Resource Manager operations: ``https://management.core.windows.net/``. 
* Data plane operations: ``https://datalake.azure.net/``.

#### Domains and Tenant

If your domain is "contoso.com". Then tenant is "contoso.onmicrosoft.com".

#### Client ID

All clients must have a "Client ID" that is known by the domain you are connecting to.

#### Sample code
```
public static Program
{
   public static string TENANT = "microsoft.onmicrosoft.com";
   public static string CLIENTID = "1950a258-227b-4e31-a9cf-717495945fc2";
   public static System.Uri ARM_TOKEN_AUDIENCE = new System.Uri( @"https://management.core.windows.net/");
   public static System.Uri ADL_TOKEN_AUDIENCE = new System.Uri( @"https://datalake.azure.net/" );

   static void Main(string[] args)
   {
      // preparation steps if needed
      var armCreds = GetCreds_____(TENANT, ARM_TOKEN_AUDIENCE, CLIENTID, ... );
      var adlCreds = GetCreds_____(TENANT, ADL_TOKEN_AUDIENCE, CLIENTID, ... );
      // use the creds to create REST client objects
   }
}
```

The `GetCreds_____` represents one of four different helper methods used in the samples. The helper methods are at the bottom of this document.

# Interactive Login options

There are two ways to use interactive login:
* **Interactive Pop-up** - The device the user is using will see a prompt appear and will use that prompt.
* **Interactive Device code** - The device the user is using will NOT see a prompt. This is useful in those cases when, for example, it is not possible to show a prompt. This document does not cover this case yet.

## Authenticate interactively with a user popup

Use this option if you want to have a browser popup appear when the user signs in to your application, showing an AAD login form. From this interactive popup, your application will receive the tokens necessary to use the Data Lake Analytics .NET SDK on behalf of the user.

The token cache minimizes the number of times the users sees a pop-up.

```

static void Main(string[] args)
{
   string MY_DOCUMENTS= System.Environment.GetFolderPath( System.Environment.SpecialFolder.MyDocuments);
   string TOKEN_CACHE_PATH = System.IO.Path.Combine(MY_DOCUMENTS, "my.tokencache");

   var tokenCache = GetTokenCache(TOKEN_CACHE_PATH);
   var armCreds = GetCreds_User_Popup(TENANT, ARM_TOKEN_AUDIENCE, CLIENTID, tokenCache);
   var adlCreds = GetCreds_User_Popup(TENANT, ADL_TOKEN_AUDIENCE, CLIENTID, tokenCache);
   // use the creds to create REST client obkects
}
```

> NOTE: The client id used above is a well known that already exists for all azure services. While it makes the sample code easy to use, for production code you should use generate your own client ids for your application.

> NOTE: The code above stores the token cache to the local machine in plaintext. We recommend writing and reading to a more secure format or location; you can use Data Protection APIs as a more secure approach. [See this blog post for more information](http://www.cloudidentity.com/blog/2014/07/09/the-new-token-cache-in-adal-v2/).


## Authenticate interactively with a device code

Azure Active Directory also supports a form of authentication called "device code" authentication. Using this, you can direct your end-user

This is not supported yet.

# Non-interactive - Service principal - Authentication

Use this option if you want to have your application authenticate against AAD using its own credentials, rather than those of a user. Using this process, your application will receive the tokens necessary to use the Data Lake Analytics .NET SDK as a service principal, 
which represents your application in AAD.

Non-interactive - Service principal / application
 * Using a secret key
 * Using a certificate

## Service principals
To create service principal [follow the steps in this article](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-authenticate-service-principal).

## Authenticate non-interactively with a secret key

```
static void Main(string[] args)
{
  string secret_key = ".....";

  var armCreds = GetCreds_SPI_SecretKey(TENANT, ARM_TOKEN_AUDIENCE, CLIENTID, secret_key);
  var adlCreds = GetCreds_SPI_SecretKey(TENANT, ADL_TOKEN_AUDIENCE, CLIENTID, secret_key);
}
```

## Authenticate non-interactively with a certificate

```
static void Main(string[] args)
{
  var cert = new X509Certificate2(@"d:\cert.pfx", "<certpassword>");

  var armCreds = GetCreds_SPI_Cert(TENANT, ARM_TOKEN_AUDIENCE, CLIENTID, cert);
  var adlCreds = GetCreds_SPI_Cert(TENANT, ADL_TOKEN_AUDIENCE, CLIENTID, cert);
}
```

## Setting up and using Data Lake SDKs
Once your have followed one of the approaches for authentication, you're ready to set up your ADLA .NET SDK client objects, which you'll use to perform various actions with the service. Remember to use the right tokens/credentials with the right clients: use the ADL credentials for data plane operations, and use the ARM credentials for resource- and account-related operations.

You can then perform actions using the clients, like so:

```
  string adla = "<ADLA account name>";
  string adls = "<ADLA account name>";
  string rg = "<resource group name>";
  string subid = "<subscription ID>";

  var adlaAccountClient = new DataLakeAnalyticsAccountManagementClient(armCreds);
  adlaAccountClient.SubscriptionId = subid;

  var adlsAccountClient = new DataLakeStoreAccountManagementClient(armCreds);
  adlsAccountClient.SubscriptionId = subid;

  var adlaCatalogClient = new DataLakeAnalyticsCatalogManagementClient(adlCreds);
  var adlaJobClient = new DataLakeAnalyticsJobManagementClient(adlCreds);
  
  var adlsFileSystemClient = new DataLakeStoreFileSystemManagementClient(adlCreds);

  var adlaaccount = adlaAccountClient.Account.Get(rg, adla);
  var adlsaccount = adlaAccountClient.Account.Get(rg, adls);
```

## Helper functions

### GetTokenCache
```
private static TokenCache GetTokenCache(string path)
{
   var tokenCache = new TokenCache();

   tokenCache.BeforeAccess += notificationArgs =>
   {
       if (File.Exists(path))
       {
           var bytes = File.ReadAllBytes(path);
           notificationArgs.TokenCache.Deserialize(bytes);
       }
   };

   tokenCache.AfterAccess += notificationArgs =>
   {
       var bytes = notificationArgs.TokenCache.Serialize();
       File.WriteAllBytes(path, bytes);
   };
   return tokenCache;
}
```

### GetCreds_User_Popup
```
private static ServiceClientCredentials GetCreds_User_Popup(
   string tenant, 
   System.Uri tokenAudience, 
   string clientId,
   TokenCache tokenCache, 
   PromptBehavior promptBehavior = PromptBehavior.Auto)
{
   SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

   var clientSettings = new ActiveDirectoryClientSettings
   {
       ClientId = clientId,
       ClientRedirectUri = new System.Uri("urn:ietf:wg:oauth:2.0:oob"),
       PromptBehavior = promptBehavior
   };

   var serviceSettings = ActiveDirectoryServiceSettings.Azure;
   serviceSettings.TokenAudience = tokenAudience;

   var creds = UserTokenProvider.LoginWithPromptAsync(
      tenant, 
      clientSettings, 
      serviceSettings, 
      tokenCache).GetAwaiter().GetResult();
   return creds;
}
```

### GetCreds_SPI_SecretKey

```
private static ServiceClientCredentials GetCreds_SPI_SecretKey(
   string tenant, 
   Uri tokenAudience, 
   string clientId, 
   string secretKey)
{
  SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

  var serviceSettings = ActiveDirectoryServiceSettings.Azure;
  serviceSettings.TokenAudience = tokenAudience;

  var creds = ApplicationTokenProvider.LoginSilentAsync(
   tenant, 
   clientId, 
   secretKey, 
   serviceSettings).GetAwaiter().GetResult();
  return creds;
}
```
### GetCreds_SPI_Cert

```
private static ServiceClientCredentials GetCreds_SPI_Cert(
   string tenant, 
   Uri tokenAudience, 
   string clientId, 
   X509Certificate2 certificate)
{
  SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

  var clientAssertionCertificate = new ClientAssertionCertificate(clientId, certificate);
  var serviceSettings = ActiveDirectoryServiceSettings.Azure;
  serviceSettings.TokenAudience = tokenAudience;

  var creds = ApplicationTokenProvider.LoginSilentWithCertificateAsync(
      tenant, 
      clientAssertionCertificate, 
      serviceSettings).GetAwaiter().GetResult();
  return creds;
}
```

## For more information
See  [Azure's .NET SDK for client authentication](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime.Azure.Authentication)

## Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments. 
