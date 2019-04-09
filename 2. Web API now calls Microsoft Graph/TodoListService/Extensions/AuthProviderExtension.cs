using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Auth;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extension class enabling adding an authentication provider service.
    /// </summary>
    public static class AuthProviderExtension
    {
        /// <summary>
        /// Adds an authentication provider service for Microsoft Graph.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A service collection.</returns>
        public static IServiceCollection AddGraphAuthProvider(this IServiceCollection services, IConfiguration configuration)
        {
            AzureADOptions azureAdOptions = new AzureADOptions();
            configuration.Bind("AzureAD", azureAdOptions);

            string[] scopes = { "user.read" };

            // Get a registered ITokenStorageProvider service.
            ITokenStorageProvider tokenCacheProvider = services.BuildServiceProvider().GetService<ITokenStorageProvider>();

            // Create confidential client application.
            IConfidentialClientApplication confidentialApp = OnBehalfOfProvider.CreateClientApplication(azureAdOptions.ClientId, azureAdOptions.CallbackPath, new ClientCredential(azureAdOptions.ClientSecret), tokenCacheProvider);

            // Register OnBehalfOfProvider as an IAuthenticationProvider service.
            services.AddSingleton<IAuthenticationProvider>(new OnBehalfOfProvider(confidentialApp, scopes));

            return services;
        }
    }
}
