using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.Management.DataLake.Analytics;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.Azure.Graph.RBAC;
using Microsoft.Azure.Management.DataLake.Analytics.Models;


namespace AdlaAuthSamples
{
    // For more info:
    //   https://azure.microsoft.com/en-us/resources/samples/data-lake-analytics-dotnet-auth-options
    class Program
    {
        static void Main(string[] args)
        {
            string adlaAccountName = "adlclientqa";
            string resourceGroupName = "adlclienttest";
            string subscriptionId = "ace74b35-b0de-428b-a1d9-55459d7a6e30";

            string domain = "microsoft.onmicrosoft.com";
            var armTokenAudience = new Uri(@"https://management.core.windows.net/");
            var adlTokenAudience = new Uri(@"https://datalake.azure.net/");
            var aadTokenAudience = new Uri(@"https://graph.windows.net/");

            string clientId = "1950a258-227b-4e31-a9cf-717495945fc2";

            var tokenCache = new TokenCache();
            tokenCache.BeforeAccess = BeforeTokenCacheAccess;
            tokenCache.AfterAccess = AfterTokenCacheAccess;

            var armCreds = GetCredsInteractivePopup(domain, armTokenAudience, tokenCache, PromptBehavior.Auto);
            var adlCreds = GetCredsInteractivePopup(domain, adlTokenAudience, tokenCache, PromptBehavior.Auto);
            var aadCreds = GetCredsInteractivePopup(domain, aadTokenAudience, tokenCache, PromptBehavior.Auto);


            var adlaAccountClient = new DataLakeAnalyticsAccountManagementClient(armCreds);
            adlaAccountClient.SubscriptionId = subscriptionId;
            var adlsAccountClient = new DataLakeStoreAccountManagementClient(armCreds);
            adlsAccountClient.SubscriptionId = subscriptionId;

            var adlaCatalogClient = new DataLakeAnalyticsCatalogManagementClient(adlCreds);
            var adlaJobClient = new DataLakeAnalyticsJobManagementClient(adlCreds);
            var adlsFileSystemClient = new DataLakeStoreFileSystemManagementClient(adlCreds);

            var graphClient = new GraphRbacManagementClient(aadCreds);
            graphClient.TenantID = domain;

            var account = adlaAccountClient.Account.Get(resourceGroupName, adlaAccountName);
            Console.WriteLine($"My account's location is: {account.Location}!");

            // string upn = "tim@contoso.com";
            // string displayName = graphClient.Users.Get(upn).DisplayName;
            // Console.WriteLine($"The display name for {upn} is {displayName}!");

            // OLD SDK
            var jobid = System.Guid.NewGuid();
            var ji = new Microsoft.Azure.Management.DataLake.Analytics.Models.JobInformation();
            ji.DegreeOfParallelism = 1;
            ji.Name = "testJob";
            ji.Properties = new USqlJobProperties();
            ji.Type = JobType.USql;
            ji.Properties.Script = "FOO";

            adlaJobClient.Job.Create(adlaAccountName, jobid, ji);

            Console.ReadLine();
        }

        /*
         *  Interactive: User popup
         *  (no token cache to reuse/save session state)
         */
        private static ServiceClientCredentials GetCredsInteractivePopup(string domain, Uri tokenAudience, PromptBehavior promptBehavior = PromptBehavior.Auto)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var clientSettings = new ActiveDirectoryClientSettings
            {
                ClientId = "1950a258-227b-4e31-a9cf-717495945fc2",
                ClientRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob"),
                PromptBehavior = promptBehavior
            };

            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
            serviceSettings.TokenAudience = tokenAudience;

            var creds = UserTokenProvider.LoginWithPromptAsync(domain, clientSettings, serviceSettings).GetAwaiter().GetResult();

            return creds;
        }

        /*
         *  Interactive: User popup
         *  (using a token cache to reuse/save session state)
         */
        private static ServiceClientCredentials GetCredsInteractivePopup(string domain, Uri tokenAudience, TokenCache tokenCache, PromptBehavior promptBehavior = PromptBehavior.Auto)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var clientSettings = new ActiveDirectoryClientSettings
            {
                ClientId = "1950a258-227b-4e31-a9cf-717495945fc2",
                ClientRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob"),
                PromptBehavior = promptBehavior
            };

            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
            serviceSettings.TokenAudience = tokenAudience;

            var creds = UserTokenProvider.LoginWithPromptAsync(domain, clientSettings, serviceSettings, tokenCache).GetAwaiter().GetResult();

            return creds;
        }

        private static void BeforeTokenCacheAccess(TokenCacheNotificationArgs args)
        {
            // NOTE: We recommend that you do NOT store the token cache in plain text -- don't use the code below as-is.
            //       Here's one example of a way to store the token cache in a slightly more secure way, using Data Protection APIs:
            //         http://www.cloudidentity.com/blog/2014/07/09/the-new-token-cache-in-adal-v2/
            string tokenCachePath = @"d:\tokencache.cache";

            if (File.Exists(tokenCachePath))
            {
                args.TokenCache.Deserialize(File.ReadAllBytes(tokenCachePath));
            }
        }

        private static void AfterTokenCacheAccess(TokenCacheNotificationArgs args)
        {
            // NOTE: We recommend that you do NOT store the token cache in plain text -- don't use the code below as-is.
            //       Here's one example of a way to store the token cache in a slightly more secure way, using Data Protection APIs:
            //         http://www.cloudidentity.com/blog/2014/07/09/the-new-token-cache-in-adal-v2/
            string tokenCachePath = @"d:\tokencache.cache";

            File.WriteAllBytes(tokenCachePath, args.TokenCache.Serialize());
        }

        /*
         *  Interactive: Device code login
         *  NOT YET SUPPORTED by Azure's .NET SDK authentication library
         */
        private static ServiceClientCredentials GetCredsDeviceCode()
        {
            throw new NotImplementedException("Azure SDK's .NET authentication library doesn't support device code login yet.");
        }

        /*
         *  Non-interactive: Service principal / application using a secret key
         *  Setup: https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-authenticate-service-principal#create-service-principal-with-password
         */
        private static ServiceClientCredentials GetCredsServicePrincipalSecretKey(string domain, Uri tokenAudience, string clientId, string secretKey)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
            serviceSettings.TokenAudience = tokenAudience;

            var creds = ApplicationTokenProvider.LoginSilentAsync(domain, clientId, secretKey, serviceSettings).GetAwaiter().GetResult();

            return creds;
        }

        /*
         *  Non-interactive: Service principal / application using a certificate
         *  Setup: https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-authenticate-service-principal#create-service-principal-with-self-signed-certificate
         */
        private static ServiceClientCredentials GetCredsServicePrincipalCertificate(string domain, Uri tokenAudience, string clientId, X509Certificate2 certificate)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var clientAssertionCertificate = new ClientAssertionCertificate(clientId, certificate);
            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
            serviceSettings.TokenAudience = tokenAudience;

            var creds = ApplicationTokenProvider.LoginSilentWithCertificateAsync(domain, clientAssertionCertificate, serviceSettings).GetAwaiter().GetResult();

            return creds;
        }
    }
}
