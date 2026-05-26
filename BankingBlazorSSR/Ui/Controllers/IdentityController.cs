using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace BankingBlazorSsr.Ui.Controllers;

/// <summary>
/// Handles authentication entry points for the Blazor SSR client by delegating
/// to the ASP.NET Core authentication middleware.
///
/// Routes:
/// - GET /identity/login  -> starts the OIDC login flow by returning Challenge(...)
/// - GET /identity/logout -> starts the OIDC logout flow by returning SignOut(...)
///
/// Important:
/// This controller does not implement the OpenID Connect protocol itself.
/// It only returns ActionResults that are then processed by the ASP.NET Core
/// authentication middleware.
///
/// Notes:
/// - The OIDC middleware uses the configured callback paths
///   such as CallbackPath and SignedOutCallbackPath.
/// - The login callback path, for example /signin-oidc, is a technical endpoint.
/// - The logout callback path, for example /signout-callback-oidc, is also a
///   technical endpoint.
/// - The final user-facing redirect after login or logout is controlled by
///   AuthenticationProperties.RedirectUri.
/// </summary>
[Route("identity")]
public class IdentityController(
   ILogger<IdentityController> logger
) : Controller {

   /// <summary>
   /// Starts the OpenID Connect login flow.
   ///
   /// Behavior:
   /// - Determines a safe local target URL.
   /// - If no returnUrl is supplied, the user is sent to "/entry" after login.
   /// - If returnUrl is supplied but is not local, it falls back to "/entry".
   /// - Returns a ChallengeResult that triggers the OIDC middleware.
   /// - After successful login, the middleware processes the OIDC callback,
   ///   creates the local authentication cookie, and redirects the browser to
   ///   props.RedirectUri.
   ///
   /// Note:
   /// This action intentionally does not check whether the user is already
   /// authenticated. Because "prompt=login" is supplied, the Identity Provider
   /// is asked to show the login prompt again.
   /// </summary>
   /// <param name="returnUrl">
   /// Optional local URL within this app. Non-local URLs are rejected to prevent
   /// open redirect attacks.
   /// </param>
   [HttpGet("login")]
   public IActionResult Login(string? returnUrl = null) {
      logger.LogInformation("Login requested. ReturnUrl: {ReturnUrl}", returnUrl ?? "(none)");

      var target = string.IsNullOrWhiteSpace(returnUrl) ? "/entry" : returnUrl;
      if (!Url.IsLocalUrl(target))
         target = "/entry";

      var props = new AuthenticationProperties {
         RedirectUri = target,
         IsPersistent = false
      };

      props.Parameters["prompt"] = "login";

      logger.LogInformation("Challenging OIDC. RedirectUri: {RedirectUri}; CurrentUser: {User}",
         props.RedirectUri,
         User.Identity?.Name ?? "(anonymous)");

      return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
   }

   /// <summary>
   /// Starts the OpenID Connect logout flow.
   ///
   /// This action does not perform the logout manually. It returns a SignOutResult.
   /// The ASP.NET Core authentication middleware then performs the actual work:
   ///
   /// - clears the local cookie session using the Cookie scheme
   /// - starts the OIDC end-session flow using the OpenID Connect scheme
   /// - receives the technical signed-out callback on SignedOutCallbackPath
   /// - finally redirects the browser to AuthenticationProperties.RedirectUri
   ///
   /// In this application, the final user-facing redirect after logout is "/".
   /// </summary>
   [HttpGet("logout")]
   public IActionResult Logout() {
      logger.LogInformation("Logout requested for user: {User}",
         User.Identity?.Name ?? "(anonymous)");

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
   Dieser Controller implementiert NICHT den OpenID-Connect-Flow selbst.
   
   Er nutzt lediglich spezielle ActionResults, die von der ASP.NET-Core-
   Authentication-Middleware verarbeitet werden:
   
   - Challenge(...) -> Middleware startet den Login-Flow
   - SignOut(...)   -> Middleware startet den Logout-Flow
   
   Der Controller ist damit ein Einstiegspunkt in die Authentifizierungsmechanik,
   aber nicht die Implementierung des Protokolls.
   
   Merksatz:
      Controller triggert nur, Middleware erledigt das Protokoll.
   
   -------------------------------------------------------------------------------
   
   1) Login: Challenge + sichere returnUrl
   ---------------------------------------
   Die Methode /identity/login startet den Login-Flow über:
   
      return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
   
   Dadurch wird nicht lokal ein Passwort geprüft. Stattdessen startet ASP.NET Core
   eine OpenID-Connect-Challenge und leitet den Browser zum IdentityAccessServer um.
   
   Nach erfolgreichem Login passiert Folgendes:
   
   1. Der IdentityAccessServer leitet den Browser zur technischen CallbackPath
      der Blazor-SSR-App zurück, z.B. /signin-oidc.
   
   2. Die OpenID-Connect-Middleware verarbeitet die Antwort des Identity Providers.
   
   3. Die Middleware validiert die erhaltenen Informationen und erstellt die lokale
      Cookie-Session der Blazor-SSR-App.
   
   4. Danach wird der Benutzer zu AuthenticationProperties.RedirectUri geleitet.
   
   In diesem Controller ist die RedirectUri standardmäßig:
   
      /entry
   
   Damit landet der Benutzer nach erfolgreichem Login beim zentralen Einstiegspunkt
   der Anwendung. Die fachliche Entscheidung, ob es danach zum Customer- oder
   Employee-Bereich geht, erfolgt nicht hier, sondern im EntryController.
   
   Sicherheit:
   Die optionale returnUrl wird mit Url.IsLocalUrl geprüft.
   
   Warum?
   Ohne diese Prüfung könnte ein Angreifer versuchen, eine externe returnUrl
   einzuschleusen. Nach dem Login würde der Benutzer dann auf eine fremde Seite
   weitergeleitet werden. Das nennt man Open Redirect.
   
   Merksatz:
      returnUrl immer prüfen, bevor sie als Redirect-Ziel verwendet wird.
   
   -------------------------------------------------------------------------------
   
   2) Warum standardmäßig /entry?
   ------------------------------
   Der Login-Endpunkt verwendet:
   
      var target = string.IsNullOrWhiteSpace(returnUrl) ? "/entry" : returnUrl;
   
   Das bedeutet:
   Wenn kein konkretes Ziel angegeben wurde, geht der Benutzer nach erfolgreichem
   Login zuerst zu /entry.
   
   Das ist bewusst so gewählt.
   
   Der IdentityController kümmert sich nur um den technischen Login.
   Der EntryController kümmert sich danach um den fachlichen Einstieg.
   
   Dadurch entsteht eine saubere Trennung:
   
      IdentityController:
         Wie kommt der Benutzer authentifiziert in die Anwendung?
   
      EntryController:
         Wohin gehört der authentifizierte Benutzer fachlich?
   
   Merksatz:
      Login ist technisch, Entry ist fachlich.
   
   -------------------------------------------------------------------------------
   
   3) Warum prompt=login?
   ----------------------
   Im Code steht:
   
      props.Parameters["prompt"] = "login";
   
   Damit wird dem Identity Provider signalisiert, dass erneut eine Login-Aufforderung
   angezeigt werden soll.
   
   Das ist besonders in Lehr- und Demo-Szenarien nützlich, weil man den Login-Flow
   sichtbar machen kann, auch wenn im Browser oder beim Identity Provider eventuell
   noch eine bestehende Sitzung existiert.
   
   Wichtig:
   Der Controller prüft aktuell nicht, ob der Benutzer bereits authentifiziert ist.
   
   Das heißt:
   Auch ein bereits angemeldeter Benutzer wird beim Aufruf von /identity/login
   wieder in den OIDC-Challenge-Flow geschickt.
   
   Das passt zum Code, weil dieser Controller den Login bewusst aktiv starten soll.
   
   Merksatz:
      prompt=login macht den Login-Flow in Demo und Vorlesung sichtbar.
   
   -------------------------------------------------------------------------------
   
   4) Logout: SignOutResult statt manuellem Logout
   -----------------------------------------------
   Die Methode /identity/logout verwendet:
   
      return SignOut(
         new AuthenticationProperties { RedirectUri = "/" },
         OpenIdConnectDefaults.AuthenticationScheme,
         CookieAuthenticationDefaults.AuthenticationScheme
      );
   
   Auch hier führt der Controller den Logout nicht manuell aus.
   
   Er gibt ein SignOutResult zurück. Die ASP.NET-Core-Authentication-Middleware
   übernimmt anschließend die eigentliche Arbeit.
   
   Dabei sind zwei Schemes beteiligt:
   
   1. CookieAuthenticationDefaults.AuthenticationScheme
      -> beendet die lokale Cookie-Session der Blazor-SSR-App
   
   2. OpenIdConnectDefaults.AuthenticationScheme
      -> startet den OIDC-Logout beim IdentityAccessServer
   
   Warum reicht Cookie-Löschen allein nicht?
   
   In einer OpenID-Connect-Anwendung existieren typischerweise zwei Sitzungen:
   
   - lokale Sitzung in der Blazor-SSR-App
   - Sitzung beim Identity Provider / IdentityAccessServer
   
   Wenn nur das lokale Cookie gelöscht wird, kann der Benutzer beim nächsten Login
   eventuell sofort wieder angemeldet werden, weil beim Identity Provider noch eine
   aktive Sitzung besteht.
   
   Deshalb ist der OIDC-Logout SSO-relevant.
   
   Merksatz:
      Cookie löschen beendet die lokale Sitzung.
      OIDC SignOut beendet zusätzlich die Sitzung beim Identity Provider.
   
   -------------------------------------------------------------------------------
   
   5) CallbackPath, SignedOutCallbackPath und RedirectUri
   ------------------------------------------------------
   Diese Begriffe werden in der Praxis oft verwechselt.
   
   CallbackPath:
      Technischer Rückkehrpunkt nach erfolgreichem Login.
      Beispiel: /signin-oidc
   
   SignedOutCallbackPath:
      Technischer Rückkehrpunkt nach erfolgreichem Logout beim Identity Provider.
      Beispiel: /signout-callback-oidc
   
   AuthenticationProperties.RedirectUri:
      Finale benutzerorientierte Weiterleitung innerhalb der Blazor-SSR-App.
   
   Beim Login ist die RedirectUri in diesem Controller typischerweise:
   
      /entry
   
   Beim Logout ist die RedirectUri:
   
      /
   
   PostLogoutRedirectUri:
      Eine beim IdentityAccessServer registrierte beziehungsweise erlaubte
      Rücksprungadresse nach dem Logout.
   
   Merksatz:
      CallbackPaths sind Technik.
      RedirectUri ist UX.
   
   -------------------------------------------------------------------------------
   
   6) Zusammenspiel mit dem EntryController
   ----------------------------------------
   Der IdentityController entscheidet nicht, ob ein Benutzer Customer oder Employee
   ist.
   
   Er startet nur Login und Logout.
   
   Nach erfolgreichem Login leitet er standardmäßig nach /entry weiter. Dort
   übernimmt der EntryController.
   
   Die genaue fachliche Entscheidung gehört in den EntryController, zum Beispiel:
   
   - Welche Rolle hat der Benutzer?
   - Existiert ein fachliches Customer- oder Employee-Profil?
   - Ist Onboarding nötig?
   - Muss ein Employee sein initiales Passwort ändern?
   - Welche Zielseite ist korrekt?
   
   Dieser Controller erwähnt /entry nur als Ziel nach dem Login.
   Die eigentliche Entry-Logik wird im EntryController erklärt.
   
   Merksatz:
      IdentityController startet den Eintritt.
      EntryController entscheidet den Weg.
   
   -------------------------------------------------------------------------------
   
   7) Lernziele
   ------------
   Studierende sollen an diesem Controller verstehen:
   
   - OpenID Connect ist ein Protokollfluss, den die Middleware abwickelt.
   - Der Controller startet den Protokollfluss nur mit Challenge(...).
   - Der Controller beendet die Sitzung nicht manuell, sondern nutzt SignOut(...).
   - Cookie-Logout und OIDC-Logout sind zwei unterschiedliche Aspekte.
   - CallbackPath und SignedOutCallbackPath sind technische Endpunkte.
   - AuthenticationProperties.RedirectUri ist die finale UX-Weiterleitung.
   - Eine returnUrl muss immer mit Url.IsLocalUrl geprüft werden.
   - /entry ist nur der zentrale Einstiegspunkt nach dem Login.
   - Die fachliche Weiterleitung nach Rollen und Profilstatus gehört in den
     EntryController.
   
   ===============================================================================
*/