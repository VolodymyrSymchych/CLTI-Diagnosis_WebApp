using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CLTI.Diagnosis.Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace CLTI.Diagnosis.Components.Account;

internal sealed class ServerSessionAuthenticationStateProvider(
    IHttpContextAccessor httpContextAccessor,
    ISessionStorageService sessionStorage,
    ILogger<ServerSessionAuthenticationStateProvider> logger)
    : AuthenticationStateProvider
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            var httpUser = httpContext?.User;

            if (httpUser?.Identity?.IsAuthenticated == true)
            {
                return new AuthenticationState(httpUser);
            }

            var token = await sessionStorage.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token) || !_tokenHandler.CanReadToken(token))
            {
                return Anonymous();
            }

            var jwt = _tokenHandler.ReadJwtToken(token);
            if (jwt.ValidTo <= DateTime.UtcNow)
            {
                logger.LogWarning("Server auth state token is expired at {ValidTo}", jwt.ValidTo);
                return Anonymous();
            }

            var identity = new ClaimsIdentity(jwt.Claims, "server-session-jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build server authentication state");
            return Anonymous();
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static AuthenticationState Anonymous()
    {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}
