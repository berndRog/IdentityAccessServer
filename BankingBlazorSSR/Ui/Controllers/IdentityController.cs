using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
namespace BankingBlazorSsr.Ui.Controllers;

/// <summary>
/// Handles authentication for the Blazor SSR client by delegating to ASP.NET Core authentication middleware.
/// Routes:
/// - GET /identity/login  -> starts OIDC login (Challenge)
/// - GET /identity/logout -> starts OIDC logout (SignOut)
///
/// Note:
/// - The OIDC middleware uses configured callback paths (CallbackPath / SignedOutCallbackPath).
/// - We do not implement a separate "signed-out" action here.
///   The final redirect after logout is controlled by AuthenticationProperties.RedirectUri
///   and/or OpenIdConnectOptions.SignedOutRedirectUri.
/// </summary>
[Route("identity")]
public class IdentityController(
   ILogger<IdentityController> logger
) : Controller {

   /// <summary>
   /// Starts the OIDC login flow.
   ///
   /// Behavior:
   /// - If already authenticated: redirects to a safe local returnUrl (or "/").
   /// - If not authenticated: returns a ChallengeResult that triggers the OIDC middleware.
   ///   After successful login, the middleware redirects the browser to props.RedirectUri.
   /// </summary>
   /// <param name="returnUrl">Optional local URL within this app (must be local).</param>
   [HttpGet("login")]
   public IActionResult Login(string? returnUrl = null) {
      logger.LogInformation("Login requested. ReturnUrl: {ReturnUrl}", returnUrl ?? "(none)");

      if (User.Identity?.IsAuthenticated == true) {
         logger.LogInformation("User already authenticated: {User}", User.Identity.Name);

         var targetUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
         return Url.IsLocalUrl(targetUrl)
            ? LocalRedirect(targetUrl)
            : LocalRedirect("/");
      }

      var target = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
      if (!Url.IsLocalUrl(target))
         target = "/";

      var props = new AuthenticationProperties {
         RedirectUri = target,
         IsPersistent = false
      };

      logger.LogInformation("Challenging OIDC. RedirectUri: {RedirectUri}", props.RedirectUri);

      return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
   }

   /// <summary>
   /// Starts the OIDC logout flow (RP-initiated logout).
   ///
   /// This action does not perform the logout itself. It returns a SignOutResult.
   /// The authentication middleware then:
   /// - clears the local cookie session (Cookie scheme)
   /// - calls the OIDC end-session endpoint (OpenIdConnect scheme)
   /// - receives the technical signed-out callback on SignedOutCallbackPath
   /// - finally redirects the browser to props.RedirectUri (final UX destination)
   /// </summary>
   [HttpGet("logout")]
   public IActionResult Logout() {
      logger.LogInformation("Logout requested for user: {User}", User.Identity?.Name ?? "(anonymous)");

      return SignOut(
         new AuthenticationProperties { RedirectUri = "/" },
         OpenIdConnectDefaults.AuthenticationScheme,
         CookieAuthenticationDefaults.AuthenticationScheme
      );
   }
}

/*
===============================================================================
DIDAKTIK & LERNZIELE (DE)
===============================================================================

0) Was zeigt dieser Controller wirklich?
----------------------------------------
Er implementiert NICHT den OIDC-Flow selbst, sondern nutzt ActionResults,
die die ASP.NET Core Authentication Middleware ausführt:

- Challenge(...) -> Middleware startet Login-Flow
- SignOut(...)   -> Middleware startet Logout-Flow

Merksatz:
   Controller triggert nur, Middleware erledigt das Protokoll.

-------------------------------------------------------------------------------

1) Login: Challenge + sichere returnUrl
---------------------------------------
- Wenn der User nicht eingeloggt ist: Challenge() leitet zum AuthServer um.
- Nach erfolgreichem Login kommt der Browser zur CallbackPath zurück
  (z.B. /signin-oidc) und geht dann zur RedirectUri (returnUrl im Client).

Sicherheit:
- returnUrl muss lokal sein (Url.IsLocalUrl), sonst Open Redirect Risiko.

-------------------------------------------------------------------------------

2) Logout: SignOutResult statt SignOutAsync
-------------------------------------------
Wir verwenden den Standard-Weg:
   return SignOut(...)

Warum?
- kein eigenes await / keine manuelle Reihenfolge im Controller
- Middleware macht Cookie-Clearing und OIDC EndSession korrekt

Wichtig:
- SignedOutCallbackPath (z.B. /signout-callback-oidc) ist ein technischer Endpunkt,
  nicht die "Startseite nach Logout".

Der finale Zielort ist:
- AuthenticationProperties.RedirectUri (hier "/")
  und optional zusätzlich OpenIdConnectOptions.SignedOutRedirectUri.

-------------------------------------------------------------------------------

3) Begriffsklärung (die in der Praxis oft verwechselt wird)
-----------------------------------------------------------
- CallbackPath (Client): technische Rückkehr nach Login (z.B. /signin-oidc)
- SignedOutCallbackPath (Client): technische Rückkehr nach Logout (z.B. /signout-callback-oidc)
- RedirectUri (Login/Logout Properties): finale UX-Weiterleitung im Client nach Abschluss
- PostLogoutRedirectUri (AuthServer Registrierung): Whitelist, wohin der AuthServer zurückleiten darf

Merksatz:
   CallbackPaths sind Technik, RedirectUri ist UX.

-------------------------------------------------------------------------------

4) Lernziele
------------
Studierende sollen verstehen:
- OIDC ist ein Protokollfluss, den Middleware abwickelt
- Controller liefert Trigger (Challenge/SignOut), nicht "Business-Logout"
- Logout ist SSO-relevant: Cookie löschen reicht nicht
- Open Redirect Prevention ist Pflicht (Url.IsLocalUrl)

===============================================================================
*/
