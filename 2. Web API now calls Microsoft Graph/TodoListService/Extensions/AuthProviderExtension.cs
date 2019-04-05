using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Auth;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extension class enabling adding the TokenAcquisition service
    /// </summary>
    public static class AuthProviderExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        /// <example>
        /// This method is typically called from the Startup.ConfigureServices(IServiceCollection services)
        /// Note that the implementation of the token cache can be chosen separately.
        /// 
        /// <code>
        /// // Token acquisition service and its cache implementation as a session cache
        /// services.AddTokenAcquisition()
        /// .AddDistributedMemoryCache()
        /// .AddSession()
        /// .AddSessionBasedTokenCache()
        ///  ;
        /// </code>
        /// </example>

        public static IServiceCollection AddGraphAuthProvider(this IServiceCollection services, IConfiguration configuration)
        {
            AzureADOptions _azureAdOptions = new AzureADOptions();
            configuration.Bind("AzureAD", _azureAdOptions);

            string[] scopes = { "user.read" };
            var currentUri = "https://localhost:44351/";

            // Get configured token storage provider.
            ITokenStorageProvider tokenCacheProvider = services.BuildServiceProvider().GetService<ITokenStorageProvider>();

            var credential = new ClientCredential(_azureAdOptions.ClientSecret);
            // Create confidential client application.
            IConfidentialClientApplication confidentialApp = OnBehalfOfProvider.CreateClientApplication(_azureAdOptions.ClientId, currentUri, credential, tokenCacheProvider);

            // Register OnBehalfOfProvider as an authentication provider.
            services.AddSingleton<IAuthenticationProvider>(new OnBehalfOfProvider(confidentialApp, scopes));

            return services;
        }
    }
}
