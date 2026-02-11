using IdentityAccessServer.Infrastructure.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace IdentityAccessServer.Auth.Controllers;

/// <summary>
/// OIDC End-Session endpoint (RP-initiated logout).
///
/// Responsibilities:
/// - Terminates the local Identity session (cookie).
/// - Validates post_logout_redirect_uri against OpenIddict's client registrations.
/// - Redirects the user to a safe target (prevents open redirects).
/// </summary>
[ApiController]
[Route("connect")]
public sealed class EndSessionController(
   IOpenIddictApplicationManager applicationManager,
   SignInManager<ApplicationUser> signInManager,
   ILogger<EndSessionController> logger
) : Controller {
   // GET /connect/endsession
   [HttpGet("endsession")]
   public async Task<IActionResult> EndSession(CancellationToken ct) {
      // OpenIddict 7.x: read the OIDC request via HttpContext.GetOpenIddictServerRequest().
      // This contains the raw parameters (client_id, post_logout_redirect_uri, state, id_token_hint, ...).
      var request = HttpContext.GetOpenIddictServerRequest();
      if (request is null) {
         logger.LogError("OpenIddict server request is missing (passthrough pipeline not active?).");
         return BadRequest("Invalid OIDC logout request.");
      }

      var clientId = request.ClientId; // may be null for some clients
      var postLogoutRedirectUri = request.PostLogoutRedirectUri;

      logger.LogInformation(
         "End-session request received. client_id='{ClientId}', post_logout_redirect_uri='{Uri}'",
         clientId ?? "(none)",
         postLogoutRedirectUri ?? "(none)"
      );

      // 1) Determine a safe redirect target (defense against open redirects)
      const string safeFallback = "/";

      var redirect = safeFallback;

      // 2) Validate post_logout_redirect_uri against OpenIddict registrations
      if (Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out var requestedUri)) {
         var isAllowed = await IsPostLogoutRedirectAllowedAsync(requestedUri, clientId, ct);
         if (isAllowed) {
            redirect = requestedUri.ToString();
            logger.LogInformation("Logout redirect approved: {Redirect}", redirect);
         }
         else {
            logger.LogWarning(
               "Logout redirect rejected: '{Uri}' (client_id='{ClientId}')",
               requestedUri, clientId ?? "(none)"
            );
         }
      }
      else if (!string.IsNullOrWhiteSpace(postLogoutRedirectUri)) {
         logger.LogWarning("Invalid post_logout_redirect_uri: '{Uri}'", postLogoutRedirectUri);
      }

      // 3) Terminate the local Identity session on the authorization server
      await signInManager.SignOutAsync();

      // 4) Redirect back to the client (or fallback)
      return Redirect(redirect);
   }

   /// <summary>
   /// Checks whether the given post-logout redirect URI is registered for the client.
   /// If client_id is missing, this demo implementation falls back to scanning all clients.
   /// </summary>
   private async Task<bool> IsPostLogoutRedirectAllowedAsync(Uri requested, string? clientId, CancellationToken ct) {
      if (!string.IsNullOrWhiteSpace(clientId)) {
         var app = await applicationManager.FindByClientIdAsync(clientId, ct);
         if (app is null) return false;

         var uris = await applicationManager.GetPostLogoutRedirectUrisAsync(app, ct);
         return uris.Select(TryParseAbsoluteUri).Any(u => u is not null && UriEquals(u, requested));
      }

      // Demo/teaching fallback: if client_id isn't provided, scan all apps.
      await foreach (var app in applicationManager.ListAsync(count: 100, offset: 0, ct)) {
         var uris = await applicationManager.GetPostLogoutRedirectUrisAsync(app, ct);
         if (uris.Select(TryParseAbsoluteUri).Any(u => u is not null && UriEquals(u, requested)))
            return true;
      }

      return false;
   }

   private static Uri? TryParseAbsoluteUri(string value)
      => Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;

   /// <summary>
   /// Compares URIs in a tolerant but secure way:
   /// - ignores trailing slashes
   /// - enforces scheme/host/path equality
   /// - ignores query and fragment
   /// </summary>
   private static bool UriEquals(Uri a, Uri b) {
      var leftA = a.GetLeftPart(UriPartial.Path).TrimEnd('/');
      var leftB = b.GetLeftPart(UriPartial.Path).TrimEnd('/');
      return string.Equals(leftA, leftB, StringComparison.OrdinalIgnoreCase);
   }
}

/*
===============================================================================
DIDAKTIK & LERNZIELE (DE)
===============================================================================

1) Warum funktioniert "OpenIddictFeature.PostLogoutRedirectUri" nicht?
---------------------------------------------------------------------
In OpenIddict 7.2.0 liefert OpenIddictServerAspNetCoreFeature nicht die
Properties ClientId/PostLogoutRedirectUri wie in manchen neueren Beispielen.
Stattdessen lesen wir die OIDC-Parameter über:
   HttpContext.GetOpenIddictServerRequest()

2) Warum validieren wir post_logout_redirect_uri selbst?
--------------------------------------------------------
Ein Logout-Endpunkt ist sicherheitskritisch (Open Redirect).
Deshalb gilt:
- post_logout_redirect_uri muss absolut sein
- und muss exakt in OpenIddict als PostLogoutRedirectUri für den Client registriert sein

Merksatz:
   "Der Auth-Server validiert Redirects – nicht der Client."

3) Warum ist client_id optional?
--------------------------------
Einige Clients (oder Middleware-Konfigurationen) liefern beim Logout
kein client_id. Für eine Demo/Lehre ist ein Fallback (Scan über alle Clients)
ok, weil er das Konzept "Whitelist über Server-Registrierung" sichtbar macht.

Für Produktion:
- client_id erzwingen oder
- client_id sicher über state / serverseitige Session zuordnen

4) Passthrough: was bedeutet EnableEndSessionEndpointPassthrough()?
-------------------------------------------------------------------
Passthrough heißt:
OpenIddict verarbeitet das Protokoll, aber die App (Controller) liefert
die HTTP-Antwort. Deshalb braucht es eine passende Route (/connect/endsession).

5) Lernziele
------------
Studierende sollen verstehen:
- OIDC Logout ist ein Protokoll-Flow (nicht nur "Cookie löschen")
- Open Redirect Prevention ist Pflicht
- Redirect-URIs gehören in die Client-Registrierung im AuthServer
- Unterschiede zwischen "Callback-Endpunkt" und "Login-Seite"

===============================================================================
*/