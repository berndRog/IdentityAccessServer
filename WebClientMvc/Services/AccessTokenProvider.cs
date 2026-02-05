using Microsoft.AspNetCore.Authentication;
namespace WebClientMvc.Services;

/// <summary>
/// Default implementation that retrieves the access token
/// from the current HttpContext (OIDC cookie session).
/// </summary>
public sealed class AccessTokenProviderFromHttpContext(
   IHttpContextAccessor accessor
) : IAccessTokenProvider {
   public async Task<string> GetAccessTokenAsync(CancellationToken ct) {
      
      // Get current HttpContext
      var httpContext = accessor.HttpContext
         ?? throw new InvalidOperationException("No HttpContext available.");

      // Retrieve access_token from OIDC session
      var token = await httpContext.GetTokenAsync("access_token");
      
      // Validate token presence
      if (string.IsNullOrWhiteSpace(token))
         throw new InvalidOperationException(
            "Missing access_token. Ensure OpenID Connect is configured with SaveTokens = true."
         );

      return token;
   }
}

/* ======================================================================
   DIDAKTIK & LERNZIELE
   ======================================================================

   Warum ein AccessTokenProvider?
   ------------------------------
   - MVC-Controller sollen KEINE Ahnung haben,
     * woher* ein Access Token kommt.
   - Das Token kann aus:
       * HttpContext
       * Cache
       * Future Refresh-Mechanismen
     stammen – der Controller bleibt davon unabhängig.

   Zentrale Lernziele
   ------------------
   1. Trennung von Verantwortung:
      - Controller: orchestrieren Use Cases
      - TokenProvider: technische Authentifizierungsdetails

   2. Vermeidung von "stringly typed" Zugriff:
      - Kein direkter Zugriff auf HttpContext in Fachcode

   3. Vorbereitung auf Erweiterungen:
      - Token Refresh
      - andere Auth-Flows
      - Testbarkeit (Mock des Providers)

   Merksatz
   --------
   „Ein Controller kennt den Benutzer – nicht das Token.“
   ======================================================================
*/