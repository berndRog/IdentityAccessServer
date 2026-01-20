using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace OidcOauthServer.Auth.Controllers;

/// <summary>
/// Handles OIDC logout requests (end-session endpoint).
///
/// Responsibilities:
/// - Terminates the local Identity session (cookie).
/// - Validates the post_logout_redirect_uri.
/// - Prevents open redirect vulnerabilities.
/// </summary>
[ApiController]
public sealed class EndSessionController(
   IOpenIddictApplicationManager applicationManager,
   ILogger<EndSessionController> logger
) : Controller {
   [HttpGet("/connect/logout")]
   public async Task<IActionResult> Logout(
      [FromQuery(Name = "post_logout_redirect_uri")]
      string? postLogoutRedirectUri,
      [FromQuery(Name = "client_id")] string? clientId,
      CancellationToken ct = default
   ) {
      // Safe fallback if the redirect URI is missing or invalid
      const string safeFallback = "/";

      // If no redirect was requested, simply sign out locally.
      if (string.IsNullOrWhiteSpace(postLogoutRedirectUri)) {
         return SignOut(
            new AuthenticationProperties { RedirectUri = safeFallback },
            IdentityConstants.ApplicationScheme
         );
      }

      // Only absolute URIs are allowed (OIDC requirement).
      if (!Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out var requestedUri)) {
         logger.LogWarning(
            "Logout rejected: invalid post_logout_redirect_uri '{Uri}'",
            postLogoutRedirectUri
         );

         return SignOut(
            new AuthenticationProperties { RedirectUri = safeFallback },
            IdentityConstants.ApplicationScheme
         );
      }

      // Validate the redirect URI against the registered OpenIddict clients.
      var isAllowed = await IsPostLogoutRedirectAllowedAsync(
         requestedUri,
         clientId,
         ct
      );

      if (!isAllowed) {
         logger.LogWarning(
            "Logout rejected: unregistered post_logout_redirect_uri '{Uri}'",
            requestedUri
         );
      }

      return SignOut(
         new AuthenticationProperties {
            RedirectUri = isAllowed ? requestedUri.ToString() : safeFallback
         },
         IdentityConstants.ApplicationScheme
      );
   }

   /// <summary>
   /// Checks whether the given post-logout redirect URI is registered
   /// for the specified client.
   ///
   /// If no client_id is provided, all registered applications are checked
   /// (acceptable for demo/teaching environments).
   /// </summary>
   private async Task<bool> IsPostLogoutRedirectAllowedAsync(
      Uri requested,
      string? clientId,
      CancellationToken ct
   ) {
      // Preferred path: validate only the specified client.
      if (!string.IsNullOrWhiteSpace(clientId)) {
         var app = await applicationManager.FindByClientIdAsync(clientId, ct);
         if (app is null)
            return false;

         var uris = await applicationManager.GetPostLogoutRedirectUrisAsync(app, ct);
         return uris
            .Select(TryParseAbsoluteUri)
            .Any(u => u is not null && UriEquals(u, requested));
      }

      // Fallback: no client_id provided (acceptable for dev / teaching).
      await foreach (var app in applicationManager.ListAsync(100, 0, ct)) {
         var uris = await applicationManager.GetPostLogoutRedirectUrisAsync(app, ct);
         if (uris
             .Select(TryParseAbsoluteUri)
             .Any(u => u is not null && UriEquals(u, requested))) {
            return true;
         }
      }

      return false;
   }

   private static Uri? TryParseAbsoluteUri(string value) {
      return Uri.TryCreate(value, UriKind.Absolute, out var uri)
         ? uri
         : null;
   }

   /// <summary>
   /// Compares URIs in a tolerant but secure way:
   /// - ignores trailing slashes
   /// - enforces scheme/host/path equality
   /// </summary>
   private static bool UriEquals(Uri a, Uri b) {
      var leftA = a.GetLeftPart(UriPartial.Path).TrimEnd('/');
      var leftB = b.GetLeftPart(UriPartial.Path).TrimEnd('/');

      return string.Equals(leftA, leftB, StringComparison.OrdinalIgnoreCase)
         && string.Equals(a.Query, b.Query, StringComparison.Ordinal)
         && string.Equals(a.Fragment, b.Fragment, StringComparison.Ordinal);
   }
}

/*

DIDAKTISCHE EINORDNUNG – OIDC LOGOUT (END SESSION)

   1) Fachlicher Kontext
   ---------------------
   Dieser Controller implementiert den OIDC-EndSession-Endpoint
   (/connect/logout) für einen Authorization Server auf Basis von
   OpenIddict + ASP.NET Identity.

   Er ist verantwortlich für:
   - das Beenden der lokalen Session (Identity-Cookie)
   - das sichere Zurückleiten des Benutzers nach dem Logout

   2) Zentrales Sicherheitsproblem
   -------------------------------
   Ein Logout-Endpunkt ist besonders sensibel, da er leicht zu
   Open-Redirect-Angriffen führen kann.

   FALSCH:
     Redirect(post_logout_redirect_uri) ungeprüft ausführen

   RICHTIG:
     post_logout_redirect_uri MUSS:
     - absolut sein
     - exakt einer beim Client registrierten URI entsprechen

   3) Warum OpenIddict hier wichtig ist
   -----------------------------------
   OpenIddict speichert pro Client:
   - RedirectUris
   - PostLogoutRedirectUris

   Der Authorization Server ist die einzige Instanz, die verbindlich
   weiß, welche Redirects erlaubt sind.

   Deshalb: Validierung gegen OpenIddict-Application-Store

   4) Warum client_id optional ist
   -------------------------------
   Einige OIDC-Clients (z.B. ASP.NET MVC Middleware) senden beim Logout
   kein client_id.

   Für Lehr- und Dev-Setups ist daher ein Fallback sinnvoll:
   - Prüfung gegen ALLE registrierten Clients

   In produktiven Systemen:
   - client_id erzwingen oder
   - im state-Parameter transportieren

   5) Abgrenzung zu Clients
   ------------------------
   - MVC / Blazor / Android konfigurieren nur Redirects
   - DIE REGELN liegen IM AUTH SERVER

   Clients dürfen niemals entscheiden:
   "Diese Redirect-URL ist ok."

   6) Merksätze für Studierende
   ----------------------------
   ✔ Logout ist sicherheitskritischer als Login
   ✔ post_logout_redirect_uri ≠ SignedOutCallbackPath
   ✔ Open Redirects sind OWASP Top 10
   ✔ OIDC-Server validiert – Clients fragen nur an

   7) Typische Fehler (die ihr jetzt nicht mehr macht)
   ---------------------------------------------------
   - Relative Redirect-URIs
   - Ungeprüftes Redirect()
   - Fehlende PostLogoutRedirectUris in OpenIddict
   - Verwechslung von Callback vs. PostLogoutRedirect

   Ergebnis:
   ---------
   Ein robuster, nachvollziehbarer und lehrbarer OIDC-Logout,
   der exakt zeigt, WO die Verantwortung im System liegt.
*/