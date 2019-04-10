using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Auth;

namespace TodoListService.Extensions
{
    public static class BaseRequestExtensions
    {
        /// <summary>
        /// Adds UserAssertion to request from the validated service access token.
        /// </summary>
        /// <param name="request">The <see cref="IBaseRequest"/>.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> object.</param>
        /// <returns><see cref="IBaseRequest"/></returns>
        public static T WithUser<T>(this T request, HttpContext httpContext) where T : IBaseRequest
        {
            // Get validated service access token.
            string validatedServiceToken = AuthenticationHttpContextExtensions.GetTokenAsync(httpContext, "access_token").GetAwaiter().GetResult();

            UserAssertion userAssertion = new UserAssertion(validatedServiceToken);

            // Call graph passing the validated service access token as the UserAssertion.
            return request.WithUserAssertion(userAssertion);
        }
    }
}
