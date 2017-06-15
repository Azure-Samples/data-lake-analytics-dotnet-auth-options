# Authenticating your application against AAD

When building an application that uses the .NET SDK for Data Lake Analytics, an important step is deciding how you want to have your application sign in to (or authenticate against) Azure Active Directory (AAD). Data Lake Analytics requires that all incoming requests are authenticated using AAD, and the request needs to be authenticated as a person or entity with appropriate permissions.

There are a few ways to authenticate against AAD using Azure's .NET SDK for client authentication:

 * Interactive - Device code (NOT SUPPORTED YET)
 * Interactive - User popup
 * Non-interactive - Service principal / application
    * Using a secret key
    * Using a certificate

See [our published sample .NET code](https://azure.microsoft.com/en-us/resources/samples/data-lake-analytics-dotnet-auth-options) for a solution that shows how to use all of these options.

## Prerequisites 
This article, as well as the sample code, expects that you have the following already set up:

 * An Azure subscription, noting the corresponding subscription ID and AAD tenant ID / domain
 * A Data Lake Analytics account within the subscription, noting the account name

## Interactive - User popup
Use this option if you want to have a browser popup appear when the user signs in to your application, showing an AAD login form. From this interactive popup, your application will receive the tokens necessary to use the Data Lake Analytics .NET SDK on behalf of the user.

The user will need to have appropriate permissions in order for your application to perform certain actions. To understand the different permissions involved when using Data Lake Analytics, see [Add a new user](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-manage-use-portal#add-a-new-user).

### Caching the user's login session

## Non-interactive - Service principal / application

Use this option if you want to have your application authenticate against AAD using its own credentials, rather than those of a user. Using this process, your application will receive the tokens necessary to use the Data Lake Analytics .NET SDK as a service principal, which represents your application in AAD.

You will first need to provision a service principal (also known as a registered application) in AAD. To create a service principal with a certificate or a secret key, [follow the steps in this article](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-authenticate-service-principal).

The service principal, just like a user, will need to have appropriate permissions in order for your application to perform certain actions. Regardless of whether a user is the one running your application, the service principal's credentials will be used, and the user's credentials will not be used. To understand the different permissions involved when using Data Lake Analytics, see [Add a new user](https://docs.microsoft.com/azure/data-lake-analytics/data-lake-analytics-manage-use-portal#add-a-new-user).

# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
