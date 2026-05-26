using BankingBlazorSsr.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingBlazorSsr.Ui.Controllers;

/// <summary>
/// Handles the authenticated application entry point for the Blazor SSR client.
///
/// Route:
/// - GET /entry -> determines the correct business landing page for the
///   authenticated user.
///
/// Important:
/// This controller is not responsible for login or logout.
/// Login and logout are handled by IdentityController through the ASP.NET Core
/// authentication middleware.
///
/// This controller assumes that the user is already authenticated. It then uses:
/// - role claims from the authenticated ClaimsPrincipal
/// - profile information from the Banking API
/// - selected domain-specific claims, such as must_change_password
///
/// Based on these inputs, it redirects the user to the correct business area:
/// - customer dashboard
/// - customer onboarding/provisioning
/// - employee dashboard
/// - employee onboarding/provisioning
/// - initial password change
/// - no-access page
/// </summary>
[Authorize]
public sealed class EntryController(
   ICustomerClient customerClient,
   IEmployeeClient employeeClient,
   IConfiguration configuration,
   ILogger<EntryController> logger
) : Controller {

   /// <summary>
   /// Claim type used by the IdentityAccessServer to indicate that an employee
   /// must change the initial password.
   ///
   /// This claim is evaluated only for employees and only after the employee
   /// Banking profile has been loaded.
   /// </summary>
   private const string MustChangePasswordClaimType = "must_change_password";

   /// <summary>
   /// Authenticated application entry point.
   ///
   /// Behavior:
   /// - Requires an authenticated user because the controller is protected by
   ///   [Authorize].
   /// - Checks whether the user has the Customer or Employee role.
   /// - Loads the corresponding Banking profile through the Banking API.
   /// - Redirects the user based on role and profile state.
   ///
   /// Customer flow:
   /// - active profile        -> /customer
   /// - inactive/onboarding   -> /customers/profile?onboarding=true
   /// - missing profile       -> /customers/provision
   ///
   /// Employee flow:
   /// - active profile + must_change_password=true -> IdentityAccessServer
   ///   initial password change page
   /// - active profile                             -> /employee
   /// - inactive/onboarding                        -> /employees/profile?onboarding=true
   /// - missing profile                            -> /employees/provision
   ///
   /// If the user has no supported role, the request is redirected to /no-access.
   /// </summary>
   /// <param name="ct">Cancellation token for the outgoing Banking API calls.</param>
   [HttpGet("/entry")]
   public async Task<IActionResult> Index(CancellationToken ct) {

      // -----------------------------------------------------------------------
      // Customer entry flow
      // -----------------------------------------------------------------------
      // The role comes from the authenticated ClaimsPrincipal.
      // The business profile state comes from the Banking API.
      //
      // This separation is intentional:
      // - the IdentityAccessServer knows who the user is and which role the user has
      // - the Banking API knows whether the domain profile exists and is active
      // -----------------------------------------------------------------------
      if (User.IsInRole("Customer")) {
         var result = await customerClient.GetProfileAsync(ct);

         // Customer profile exists.
         // Status == 1 is treated as active/completed.
         // Any other status leads to the onboarding profile page.
         if (result.IsSuccess && result.Value is not null) {
            return result.Value.Status == 1
               ? Redirect("/customer")
               : Redirect("/customers/profile?onboarding=true");
         }

         // A 404 means:
         // The user is authenticated as Customer, but no Customer profile exists
         // yet in the Banking domain.
         if (result.Error?.Status == 404)
            return Redirect("/customers/provision");

         // For teaching/demo purposes we choose a defensive fallback:
         // if profile lookup fails unexpectedly, send the user to provisioning
         // instead of failing with a technical error page.
         logger.LogWarning(
            "Entry(Customer): profile lookup failed with status {Status}. Falling back to provisioning.",
            result.Error?.Status);

         return Redirect("/customers/provision");
      }

      // -----------------------------------------------------------------------
      // Employee entry flow
      // -----------------------------------------------------------------------
      // Employees have one additional concern:
      // the IdentityAccessServer may issue a must_change_password claim.
      //
      // The claim alone is not enough to redirect immediately. The code first
      // loads the Banking employee profile and checks whether the profile is
      // already active.
      // -----------------------------------------------------------------------
      if (User.IsInRole("Employee")) {
         var mustChangePassword =
            User.FindFirst(MustChangePasswordClaimType)?.Value == "true";

         var result = await employeeClient.GetProfileAsync(ct);

         // Employee profile exists.
         if (result.IsSuccess && result.Value is not null) {

            // Initial password changes are only enforced after employee onboarding
            // has completed and the Banking profile is already active.
            //
            // This avoids sending an employee to the password-change page before
            // the domain-side employee profile is ready.
            if (mustChangePassword && result.Value.IsActive)
               return Redirect(BuildChangeInitialPasswordUrl());

            return result.Value.IsActive
               ? Redirect("/employee")
               : Redirect("/employees/profile?onboarding=true");
         }

         // A 404 means:
         // The user is authenticated as Employee, but no Employee profile exists
         // yet in the Banking domain.
         if (result.Error?.Status == 404)
            return Redirect("/employees/provision");

         // Defensive fallback for demo/teaching:
         // log the unexpected status and continue with provisioning.
         logger.LogWarning(
            "Entry(Employee): profile lookup failed with status {Status}. Falling back to provisioning.",
            result.Error?.Status);

         return Redirect("/employees/provision");
      }

      // -----------------------------------------------------------------------
      // Authenticated, but not authorized for a supported business role
      // -----------------------------------------------------------------------
      // The user has successfully logged in, but does not have one of the roles
      // this application understands for entry routing.
      // -----------------------------------------------------------------------
      logger.LogWarning("Entry: user has no supported role.");
      return Redirect("/no-access");
   }

   /// <summary>
   /// Builds the absolute URL to the IdentityAccessServer page where employees
   /// can change their initial password.
   ///
   /// The generated URL includes a returnUrl back to this EntryController.
   /// After the password has been changed, the browser returns to /entry.
   /// The EntryController then evaluates the current user and profile state again.
   ///
   /// This keeps the flow centralized:
   /// - IdentityAccessServer handles password change
   /// - EntryController handles business routing after the change
   /// </summary>
   /// <returns>
   /// A URL to the IdentityAccessServer initial password change page.
   /// If the IdentityAccessServer authority is missing, the method falls back to
   /// "/employee".
   /// </returns>
   private string BuildChangeInitialPasswordUrl() {
      var authority = configuration["IdentityAccessServer:Authority"]?.TrimEnd('/');

      if (string.IsNullOrWhiteSpace(authority)) {
         logger.LogWarning(
            "Entry(Employee): IdentityAccessServer:Authority missing. Falling back to employee dashboard.");

         return "/employee";
      }

      var returnUrl = Url.ActionLink(
            nameof(Index),
            "Entry",
            values: null,
            protocol: Request.Scheme)
         ?? $"{Request.Scheme}://{Request.Host}{Request.PathBase}/entry";

      return $"{authority}/Identity/Account/ChangeInitialPassword?returnUrl={Uri.EscapeDataString(returnUrl)}";
   }
}

/*
===============================================================================
DIDAKTIK & LERNZIELE (DE)
===============================================================================

0) Was zeigt dieser Controller wirklich?
----------------------------------------
Der EntryController ist der fachliche Einstiegspunkt der Anwendung nach einem
erfolgreichen Login.

Er ist NICHT für den Login selbst verantwortlich.

Der Login wird vorher durch den IdentityController gestartet:

   /identity/login -> Challenge(...) -> OpenID Connect Middleware

Nach erfolgreichem Login wird der Benutzer zur Route /entry weitergeleitet.
Erst dort entscheidet die Anwendung fachlich, wohin der Benutzer gehört.

Merksatz:
   IdentityController = technischer Login/Logout
   EntryController    = fachlicher Einstieg nach dem Login

-------------------------------------------------------------------------------

1) Warum ist der Controller mit [Authorize] geschützt?
------------------------------------------------------
Der EntryController beginnt mit:

   [Authorize]

Das bedeutet:
Nur authentifizierte Benutzer dürfen /entry aufrufen.

Das ist wichtig, weil der Controller davon ausgeht, dass bereits ein Benutzer
vorhanden ist. Er verwendet zum Beispiel:

   User.IsInRole("Customer")
   User.IsInRole("Employee")
   User.FindFirst("must_change_password")

Diese Informationen stehen nur sinnvoll zur Verfügung, wenn vorher die
Authentifizierung durchlaufen wurde und ASP.NET Core aus dem Auth-Cookie einen
ClaimsPrincipal aufgebaut hat.

Merksatz:
   /entry ist kein Login-Endpunkt.
   /entry ist ein Einstiegspunkt für bereits angemeldete Benutzer.

-------------------------------------------------------------------------------

2) Rollenprüfung: Customer oder Employee?
-----------------------------------------
Der Controller unterscheidet zuerst nach Rollen:

   if (User.IsInRole("Customer")) { ... }

   if (User.IsInRole("Employee")) { ... }

Die Rollen stammen aus den Claims des angemeldeten Benutzers. Diese Claims
wurden im Authentifizierungsprozess vom IdentityAccessServer geliefert und in
der Blazor-SSR-App in einen ClaimsPrincipal übernommen.

Damit beantwortet die Rollenprüfung die Frage:

   Welche Art von Benutzer ist angemeldet?

In dieser Anwendung gibt es zwei unterstützte fachliche Rollen:

- Customer
- Employee

Benutzer ohne eine dieser Rollen werden nach /no-access weitergeleitet.

Merksatz:
   Die Rolle entscheidet über den fachlichen Zweig.
   Das Profil entscheidet über den konkreten Zielzustand.

-------------------------------------------------------------------------------

3) Warum reicht die Rolle allein nicht aus?
-------------------------------------------
Ein Benutzer kann erfolgreich authentifiziert sein und die Rolle Customer oder
Employee besitzen. Trotzdem kann in der Banking-Domäne noch etwas fehlen.

Beispiele:

- Ein Customer hat zwar die Rolle Customer, aber noch kein Customer-Profil.
- Ein Customer hat ein Profil, aber das Onboarding ist noch nicht abgeschlossen.
- Ein Employee hat zwar die Rolle Employee, aber noch kein Employee-Profil.
- Ein Employee hat ein Profil, ist aber noch nicht aktiv.
- Ein Employee muss nach der Aktivierung noch das initiale Passwort ändern.

Deshalb fragt der EntryController zusätzlich die Banking API ab:

   customerClient.GetProfileAsync(ct)
   employeeClient.GetProfileAsync(ct)

Damit wird nicht nur die technische Identität betrachtet, sondern auch der
fachliche Zustand des Benutzers.

Merksatz:
   Auth-System kennt Identität und Rolle.
   Banking API kennt das fachliche Profil.

-------------------------------------------------------------------------------

4) Customer Flow
----------------
Wenn der Benutzer die Rolle Customer besitzt, wird das Customer-Profil geladen:

   var result = await customerClient.GetProfileAsync(ct);

Danach gibt es drei wichtige Fälle.

Fall 1: Profil existiert und ist aktiv

   result.IsSuccess && result.Value is not null
   result.Value.Status == 1

Weiterleitung:

   /customer

Fall 2: Profil existiert, ist aber noch nicht aktiv oder Onboarding ist nötig

Weiterleitung:

   /customers/profile?onboarding=true

Fall 3: Profil existiert noch nicht

   result.Error?.Status == 404

Weiterleitung:

   /customers/provision

Damit ergibt sich:

   Customer + aktives Profil
      -> Customer Dashboard

   Customer + Profil vorhanden, aber Onboarding nötig
      -> Customer Profil-Onboarding

   Customer + kein Profil
      -> Customer Provisioning

Merksatz:
   Customer-Rolle bedeutet nicht automatisch fertiges Customer-Profil.

-------------------------------------------------------------------------------

5) Employee Flow
----------------
Wenn der Benutzer die Rolle Employee besitzt, wird zuerst der Claim

   must_change_password

ausgewertet:

   var mustChangePassword =
      User.FindFirst(MustChangePasswordClaimType)?.Value == "true";

Danach wird das Employee-Profil geladen:

   var result = await employeeClient.GetProfileAsync(ct);

Auch hier gibt es mehrere Fälle.

Fall 1: Employee-Profil existiert und ist aktiv

   result.Value.IsActive == true

Weiterleitung:

   /employee

Fall 2: Employee-Profil existiert, ist aber noch nicht aktiv

Weiterleitung:

   /employees/profile?onboarding=true

Fall 3: Employee-Profil existiert noch nicht

   result.Error?.Status == 404

Weiterleitung:

   /employees/provision

Zusätzlich gibt es beim Employee den Sonderfall:

   must_change_password == true
   und
   result.Value.IsActive == true

Dann wird der Benutzer nicht direkt nach /employee geleitet, sondern zuerst zur
Passwortänderung beim IdentityAccessServer.

Merksatz:
   Employee-Rolle bedeutet nicht automatisch aktiver Employee-Zugang.

-------------------------------------------------------------------------------

6) Initialer Passwortwechsel
----------------------------
Für Employees kann der IdentityAccessServer einen Claim setzen:

   must_change_password = true

Dieser Claim bedeutet:
Der Benutzer muss sein initiales Passwort ändern.

Der EntryController erzwingt diese Änderung aber erst, wenn das Employee-Profil
in der Banking API bereits aktiv ist:

   if (mustChangePassword && result.Value.IsActive)
      return Redirect(BuildChangeInitialPasswordUrl());

Warum?

Weil der Passwortwechsel ein Schritt für einen bereits fachlich aktivierten
Employee ist. Wenn das Employee-Profil noch gar nicht existiert oder noch nicht
aktiv ist, soll der Benutzer zuerst durch Provisioning oder Onboarding laufen.

Der Ablauf lautet:

1. Employee meldet sich an
2. /entry lädt das Employee-Profil
3. Profil ist aktiv
4. Claim must_change_password ist true
5. Benutzer wird zum IdentityAccessServer zur Passwortänderung geleitet
6. Nach der Passwortänderung geht es zurück zu /entry
7. /entry prüft erneut
8. Benutzer landet danach auf /employee

Merksatz:
   Passwortwechsel ist Identity-Thema.
   Entscheidung, wann er erzwungen wird, ist Entry-Logik.

-------------------------------------------------------------------------------

7) Warum führt die Passwortänderung zurück nach /entry?
-------------------------------------------------------
Die Methode BuildChangeInitialPasswordUrl erzeugt eine URL mit returnUrl:

   .../ChangeInitialPassword?returnUrl=...

Diese returnUrl zeigt wieder auf /entry.

Das ist bewusst so gewählt.

Nach der Passwortänderung soll nicht direkt blind auf /employee weitergeleitet
werden. Stattdessen soll der zentrale EntryController erneut prüfen:

- Ist der Benutzer noch angemeldet?
- Welche Rolle hat er?
- Ist das Profil aktiv?
- Ist must_change_password noch gesetzt?
- Welche Zielseite ist jetzt korrekt?

Damit bleibt die gesamte fachliche Routing-Entscheidung an einer einzigen Stelle.

Merksatz:
   Nach externen Zwischenschritten immer zurück zum zentralen Entry Point.

-------------------------------------------------------------------------------

8) Warum wird bei 404 provisioniert?
------------------------------------
Ein 404 beim Profilabruf bedeutet in diesem Kontext:

   Der Benutzer ist im IdentityAccessServer bekannt,
   aber das passende fachliche Profil existiert in der Banking API noch nicht.

Deshalb wird weitergeleitet zu:

   /customers/provision

oder:

   /employees/provision

Provisioning bedeutet hier:
Das fachliche Profil muss in der Banking-Domäne angelegt oder initialisiert
werden.

Merksatz:
   Authentifiziert heißt nicht automatisch fachlich provisioniert.

-------------------------------------------------------------------------------

9) Warum gibt es einen Fallback auf Provisioning?
------------------------------------------------
Wenn der Profilabruf fehlschlägt und kein sauberer Erfolgsfall vorliegt, loggt
der Controller eine Warnung und leitet ebenfalls zum Provisioning weiter.

Beispiel:

   logger.LogWarning("Entry(Customer): profile lookup failed ...");

Das ist für ein Lehrprojekt nachvollziehbar, weil der Ablauf robust bleibt und
Studierende nicht sofort auf einer technischen Fehlerseite landen.

In einer produktiven Anwendung könnte man hier je nach Fehler differenzierter
reagieren, zum Beispiel:

- bei 401/403: erneute Anmeldung oder Access-Denied
- bei 500: Fehlerseite
- bei Timeout: Retry oder technische Störungsseite

Merksatz:
   Der Demo-Code priorisiert einen nachvollziehbaren Ablauf.
   Produktionscode sollte Fehlerzustände feiner unterscheiden.

-------------------------------------------------------------------------------

10) Keine unterstützte Rolle
----------------------------
Wenn der Benutzer weder Customer noch Employee ist, endet der Controller mit:

   return Redirect("/no-access");

Das bedeutet:
Der Benutzer ist zwar authentifiziert, aber für diese Anwendung fachlich nicht
zugelassen.

Das ist ein wichtiger Unterschied:

   Nicht authentifiziert
      -> Login erforderlich

   Authentifiziert, aber falsche Rolle
      -> Kein Zugriff / no-access

Merksatz:
   Login erfolgreich heißt nicht: Zugriff auf die Anwendung erlaubt.

-------------------------------------------------------------------------------

11) Warum ist das besser als Logik in Program.cs?
-------------------------------------------------
Program.cs wird beim Start der Anwendung ausgeführt.

Zu diesem Zeitpunkt gibt es keinen konkreten Benutzer und keinen konkreten
HTTP-Request eines angemeldeten Benutzers.

Deshalb gehört diese Logik nicht nach Program.cs:

   User.IsInRole("Customer")
   User.IsInRole("Employee")

Der EntryController läuft dagegen pro Request. Zu diesem Zeitpunkt hat ASP.NET
Core bereits das Auth-Cookie ausgewertet und den ClaimsPrincipal aufgebaut.

Merksatz:
   Program.cs konfiguriert Sicherheit.
   EntryController verwendet Sicherheit.

-------------------------------------------------------------------------------

12) Warum ist das besser als ein Redirect in einer Blazor-Komponente?
---------------------------------------------------------------------
Ein Redirect aus einer Blazor-Komponente kann funktionieren, ist aber bei
Blazor SSR oft schwieriger zu kontrollieren.

Grund:
Blazor SSR hat mehrere Phasen, zum Beispiel initiales serverseitiges Rendering
und optional spätere Interaktivität.

Der Controller arbeitet dagegen im klassischen HTTP-Modell:

1. Request kommt an
2. HttpContext.User ist verfügbar
3. Controller entscheidet
4. Controller gibt Redirect zurück
5. Browser lädt die Zielseite

Für Login-Landing-Flows ist das sehr robust.

Merksatz:
   Komponenten zeigen UI.
   Controller eignen sich gut für serverseitige Redirect-Entscheidungen.

-------------------------------------------------------------------------------

13) Zielseiten trotzdem zusätzlich schützen
-------------------------------------------
Der EntryController verteilt Benutzer auf die passende Seite.

Er ersetzt aber nicht die Autorisierung der Zielseiten.

Ein Benutzer könnte eine URL direkt eingeben:

   /customer
   /employee

Deshalb müssen diese Zielseiten zusätzlich geschützt werden, zum Beispiel:

   @attribute [Authorize(Policy = "CustomersOnly")]

oder:

   @attribute [Authorize(Policy = "EmployeesOnly")]

Der EntryController ist also Komfort und fachliche Einstiegsmatrix.
Die Policies auf den Zielseiten sind die eigentliche Zugriffssicherung.

Merksatz:
   EntryController entscheidet, wohin der Benutzer soll.
   Policies entscheiden, ob er dort wirklich hinein darf.

-------------------------------------------------------------------------------

14) Lernziele
-------------
Studierende sollen an diesem Controller verstehen:

- Authentifizierung und fachlicher Einstieg sind getrennte Verantwortlichkeiten.
- Rollen kommen aus dem ClaimsPrincipal.
- Fachlicher Profilstatus kommt aus der Banking API.
- Eine Rolle allein reicht für die Zielentscheidung nicht aus.
- Onboarding und Provisioning sind fachliche Zustände nach dem Login.
- Ein initialer Passwortwechsel gehört zum IdentityAccessServer, wird aber
  durch die Entry-Logik ausgelöst.
- 404 beim Profilabruf bedeutet hier: Profil muss provisioniert werden.
- Benutzer ohne unterstützte Rolle werden nach /no-access geleitet.
- Zielseiten müssen zusätzlich mit Policies geschützt werden.
- Controller sind für SSR-Redirects nach Login oft robuster als Komponenten.

===============================================================================
*/