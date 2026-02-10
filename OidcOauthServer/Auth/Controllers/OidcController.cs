using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OidcOauthServer.Auth.Claims;
using OidcOauthServer.Auth.Options;
using OidcOauthServer.Infrastructure.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OidcOauthServer.Auth.Endpoints;

[ApiController]
public sealed class OidcController(
   UserManager<ApplicationUser> users,
   SignInManager<ApplicationUser> signIn,
   IOptions<AuthServerOptions> authOptions,
   ILogger<OidcController> logger
) : Controller {
   private readonly AuthServerOptions _auth = authOptions.Value;

   // --------------------------------------------------------------------
   // /connect/authorize
   // --------------------------------------------------------------------
   [HttpGet("/" + AuthServerOptions.AuthorizationEndpointPath)]
   public async Task<IActionResult> Authorize(CancellationToken ct) {
      var request = HttpContext.GetOpenIddictServerRequest()
         ?? throw new InvalidOperationException("OpenID Connect request missing.");

      logger.LogInformation(
         "Authorize request: client_id='{ClientId}', redirect_uri='{RedirectUri}', scope='{Scope}', response_type='{ResponseType}'",
         request.ClientId, request.RedirectUri, request.Scope, request.ResponseType
      );

      var returnUrl = Request.PathBase + Request.Path + Request.QueryString;

      var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

      if (!authResult.Succeeded) {
         logger.LogInformation("Authorize: no Identity cookie -> challenge, returnUrl='{ReturnUrl}'", returnUrl);

         return Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl },
            IdentityConstants.ApplicationScheme
         );
      }

      var user = await users.GetUserAsync(authResult.Principal!);
      if (user is null) {
         logger.LogWarning(
            "Authorize: Identity cookie principal has no user -> challenge, returnUrl='{ReturnUrl}'",
            returnUrl
         );

         return Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl },
            IdentityConstants.ApplicationScheme
         );
      }

      var principal = await signIn.CreateUserPrincipalAsync(user);
      var identity = (ClaimsIdentity)principal.Identity!;

      // --- Subject (sub) -----------------------------------------------------
      var subject = user.Id; // GUID string
      SetOrReplaceClaim(identity, AuthClaims.Subject, subject);

      // --- Standard / profile claims ----------------------------------------
      if (!string.IsNullOrWhiteSpace(user.Email))
         SetOrReplaceClaim(identity, AuthClaims.Email, user.Email);

      if (!string.IsNullOrWhiteSpace(user.UserName))
         SetOrReplaceClaim(identity, AuthClaims.PreferredUsername, user.UserName);

      // --- Domain-specific ---------------------------------------------------
      // Role mapping for ASP.NET authorization:
      // (- Customer: self-registered, normal access) wird nciht unterstütz
      // - Owner: self-registered, but needs activation (Status check)
      // - Employee: has AdminRights, can manage customers/owners
      var role = user.AdminRights > 0 ? "Employee" : "Owner";
      SetOrReplaceClaim(identity, AuthClaims.Role, role);

      // Keep the AdminRights bitmask (fine-grained permissions for employees)
      SetOrReplaceClaim(identity, AuthClaims.AdminRights, ((int)user.AdminRights).ToString());

      // --- Lifecycle timestamps ---------------------------------------------
      if (user.CreatedAt != default)
         SetOrReplaceClaim(identity, AuthClaims.CreatedAt, user.CreatedAt.ToUniversalTime().ToString("O"));

      if (user.UpdatedAt != default)
         SetOrReplaceClaim(identity, AuthClaims.UpdatedAt, user.UpdatedAt.ToUniversalTime().ToString("O"));

      // --- Scopes & resources ------------------------------------------------
      var requestedScopes = request.GetScopes().ToArray();
      principal.SetScopes(requestedScopes);

      var resources = ResolveResourcesFromScopes(requestedScopes);
      if (resources.Length > 0)
         principal.SetResources(resources);
      
      logger.LogInformation(
         "Authorize: user='{UserName}', sub='{Sub}', role='{Role}', adminRights='{Rights}', scopes=[{Scopes}], resources=[{Resources}]",
         user.UserName,
         subject,
         role,
         user.AdminRights,
         string.Join(", ", requestedScopes),
         resources.Length == 0 ? "<none>" : string.Join(", ", resources)
      );

      // --- Destinations mapping ----------------------------------------------
      foreach (var claim in principal.Claims)
         claim.SetDestinations(ClaimDestinations.GetDestinations(claim, principal));

      foreach (var c in principal.Claims)
         logger.LogDebug("Authorize: claim '{Type}'='{Value}' -> destinations: {Destinations}",
            c.Type, c.Value, string.Join(", ", c.GetDestinations()));

      return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
   }

   private static void SetOrReplaceClaim(ClaimsIdentity identity, string type, string value) {
      var existing = identity.Claims.Where(c => c.Type == type).ToList();
      foreach (var c in existing)
         identity.RemoveClaim(c);

      identity.AddClaim(new Claim(type, value));
   }

   // --------------------------------------------------------------------
   // /connect/token
   // --------------------------------------------------------------------
   [HttpPost("/" + AuthServerOptions.TokenEndpointPath)]
   public async Task<IActionResult> Token(CancellationToken ct) {
      var request = HttpContext.GetOpenIddictServerRequest()
         ?? throw new InvalidOperationException("OpenID Connect request missing.");

      logger.LogInformation(
         "Token request: grant_type='{GrantType}', client_id='{ClientId}', scope='{Scope}'",
         request.GrantType, request.ClientId, request.Scope
      );

      if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType()) {
         var result = await HttpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
         );

         logger.LogInformation("Token: code/refresh -> issuing tokens for client_id='{ClientId}'", request.ClientId);

         return SignIn(result.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      if (request.IsClientCredentialsGrantType()) {
         logger.LogInformation(
            "Token: client_credentials -> issuing access token for client_id='{ClientId}'",
            request.ClientId
         );

         var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

         identity.AddClaim(new Claim(AuthClaims.Subject, request.ClientId!));
         identity.AddClaim(new Claim(AuthClaims.AccountType, "service"));

         var principal = new ClaimsPrincipal(identity);

         var requestedScopes = request.GetScopes().ToArray();
         principal.SetScopes(requestedScopes);

         var resources = ResolveResourcesFromScopes(requestedScopes);
         if (resources.Length > 0)
            principal.SetResources(resources);

         foreach (var claim in principal.Claims)
            claim.SetDestinations(Destinations.AccessToken);

         logger.LogInformation(
            "Token: client_credentials -> client_id='{ClientId}', scopes=[{Scopes}], resources=[{Resources}]",
            request.ClientId,
            string.Join(", ", requestedScopes),
            resources.Length == 0 ? "<none>" : string.Join(", ", resources)
         );

         return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      logger.LogWarning("Token: unsupported grant_type '{GrantType}'", request.GrantType);
      return BadRequest(new { error = "unsupported_grant_type" });
   }

   // --------------------------------------------------------------------
   // /connect/userinfo
   // --------------------------------------------------------------------
   [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
   [HttpGet("/" + AuthServerOptions.UserInfoEndpointPath)]
   public IActionResult UserInfo() {
      logger.LogInformation(
         "UserInfo request: sub='{Sub}', azp='{Azp}'",
         User.FindFirst(AuthClaims.Subject)?.Value,
         User.FindFirst("azp")?.Value
      );

      return Ok(new {
         sub = User.FindFirst(AuthClaims.Subject)?.Value,
         preferred_username = User.FindFirst(AuthClaims.PreferredUsername)?.Value,
         email = User.FindFirst(AuthClaims.Email)?.Value,
         role = User.FindFirst(AuthClaims.Role)?.Value,
         admin_rights = User.FindFirst(AuthClaims.AdminRights)?.Value,
         created_at = User.FindFirst(AuthClaims.CreatedAt)?.Value,
         updated_at = User.FindFirst(AuthClaims.UpdatedAt)?.Value
      });
   }

   // --------------------------------------------------------------------
   // Helpers
   // --------------------------------------------------------------------
   private string[] ResolveResourcesFromScopes(string[] requestedScopes) {
      static bool IsNonApiScope(string s)
         => s.Equals("openid", StringComparison.Ordinal) ||
            s.Equals("profile", StringComparison.Ordinal);

      var apiScopesRequested = requestedScopes
         .Where(s => !IsNonApiScope(s))
         .Distinct(StringComparer.Ordinal)
         .ToArray();

      if (apiScopesRequested.Length == 0)
         return Array.Empty<string>();

      var known = _auth.Apis.Values.ToDictionary(a => a.Scope, a => a.Resource, StringComparer.Ordinal);

      var resources = new List<string>(capacity: apiScopesRequested.Length);

      foreach (var scope in apiScopesRequested) {
         if (known.TryGetValue(scope, out var resource)) {
            resources.Add(resource);
         }
         else {
            logger.LogWarning(
               "Unknown API scope requested: '{Scope}'. No resource/audience mapping found in AuthServer:Apis.",
               scope
            );
         }
      }

      return resources
         .Distinct(StringComparer.Ordinal)
         .ToArray();
   }
}
