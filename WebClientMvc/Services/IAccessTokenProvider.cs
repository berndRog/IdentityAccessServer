namespace WebClientMvc.Services;

/// <summary>
/// Abstraction for retrieving an OAuth/OIDC access token
/// for the currently authenticated MVC user.
///
/// This interface decouples MVC controllers and API clients
/// from ASP.NET Core authentication internals.
/// </summary>
public interface IAccessTokenProvider {
   /// <summary>
   /// Returns the access token of the current authenticated user.
   /// Throws if no token is available.
   /// </summary>
   Task<string> GetAccessTokenAsync(CancellationToken ct);
}