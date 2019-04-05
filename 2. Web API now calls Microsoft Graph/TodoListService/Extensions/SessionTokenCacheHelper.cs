using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extension class enabling adding the CookieBasedTokenCache implentation service
    /// </summary>
    public static class SessionBasedTokenCacheExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddSessionBasedTokenCache(this IServiceCollection services)
        {
            // Token acquisition service
            services.AddSingleton<ITokenStorageProvider, SessionTokenCacheProvider>();
            return services;
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="ITokenStorageProvider"/> for a cookie based token cache implementation
    /// </summary>
    public class SessionTokenCacheProvider : ITokenStorageProvider
    {
        private static readonly ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private IHttpContextAccessor _httpContextAccessor;

        public SessionTokenCacheProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Task<byte[]> GetTokenCacheAsync(string cacheId)
        {

            _httpContextAccessor.HttpContext.Session.LoadAsync().Wait();
            SessionLock.EnterReadLock();
            try
            {
                _httpContextAccessor.HttpContext.Session.TryGetValue(cacheId, out byte[] blob);
                return Task.FromResult(blob);
            }
            finally
            {
                SessionLock.ExitReadLock();
            }
        }

        public Task SetTokenCacheAsync(string cacheId, byte[] tokenCache)
        {
            SessionLock.EnterWriteLock();

            try
            {
                // Reflect changes in the persistent store
                _httpContextAccessor.HttpContext.Session.Set(cacheId, tokenCache);
                _httpContextAccessor.HttpContext.Session.CommitAsync().Wait();

                return Task.FromResult<object>(null);
            }
            finally
            {
                SessionLock.ExitWriteLock();
            }
        }
    }
}
