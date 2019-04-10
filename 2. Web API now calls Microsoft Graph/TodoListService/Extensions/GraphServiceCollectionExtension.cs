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
    public static class GraphServiceCollectionExtension
    {
        /// <summary>
        /// Adds an authentication provider service for Microsoft Graph.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>A service collection.</returns>
        public static IServiceCollection AddMicrosoftGraph(this IServiceCollection services, IConfiguration configuration)
        {
            AzureADOptions azureAdOptions = new AzureADOptions();
            configuration.Bind("AzureAD", azureAdOptions);

            string[] scopes = { "user.read" };

            // Get registered ITokenStorageProvider service.
            ITokenStorageProvider tokenCacheProvider = services.BuildServiceProvider().GetService<ITokenStorageProvider>();

            // Create a confidential client application.
            IConfidentialClientApplication confidentialApp = OnBehalfOfProvider.CreateClientApplication(
                azureAdOptions.ClientId,
                azureAdOptions.CallbackPath,
                new ClientCredential(azureAdOptions.ClientSecret),
                tokenCacheProvider);

            // Configure an OnBehalfOfProvider.
            OnBehalfOfProvider authProvider = new OnBehalfOfProvider(confidentialApp, scopes);

            // Register OnBehalfOfProvider as a service.
            services.AddSingleton<IAuthenticationProvider>(authProvider);

            // Register IGraphServiceClient as a service.
            services.AddSingleton<IGraphServiceClient, GraphServiceClient>();

            return services;
        }
    }
}
