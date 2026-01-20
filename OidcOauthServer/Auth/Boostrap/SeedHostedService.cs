using Microsoft.Extensions.Options;
using OidcOauthServer.Auth.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OidcOauthServer.Auth.Seeding;

public sealed class SeedHostedService(
   IServiceProvider sp,
   IConfiguration config,
   ILogger<SeedHostedService> logger
) : IHostedService {
   public async Task StartAsync(CancellationToken ct) {
      using var scope = sp.CreateScope();

      var options = scope.ServiceProvider
         .GetRequiredService<IOptions<AuthServerOptions>>().Value;

      var apps = scope.ServiceProvider
         .GetRequiredService<IOpenIddictApplicationManager>();

      // ------------------------------------------------------------
      // Local helper: Create OR Update (idempotent)
      // Put it HERE so it can "see" apps/logger/ct.
      // ------------------------------------------------------------
      async Task UpsertAsync(OpenIddictApplicationDescriptor descriptor, bool requiresSecret) {
         var existing = await apps.FindByClientIdAsync(descriptor.ClientId!, ct);

         if (existing is null) {
            if (requiresSecret && string.IsNullOrWhiteSpace(descriptor.ClientSecret))
               throw new InvalidOperationException(
                  $"Client '{descriptor.ClientId}' is confidential but no ClientSecret was provided. " +
                  $"Set it via '{AuthServerSecretKeys.WebMvcClientSecret}' / '{AuthServerSecretKeys.ServiceClientSecret}'.");

            await apps.CreateAsync(descriptor, ct);
            logger.LogInformation("Created OpenIddict client: {ClientId}", descriptor.ClientId);
            return;
         }

         if (requiresSecret && string.IsNullOrWhiteSpace(descriptor.ClientSecret))
            throw new InvalidOperationException(
               $"Client '{descriptor.ClientId}' exists and is confidential but no ClientSecret was provided. " +
               $"Set it via configuration (user-secrets / env vars).");

         await apps.UpdateAsync(existing, descriptor, ct);
         logger.LogInformation("Updated OpenIddict client: {ClientId}", descriptor.ClientId);
      }

      // ------------------------------------------------------------
      // 1) Blazor WASM (Public + Code + PKCE)
      // ------------------------------------------------------------
      await UpsertAsync(new OpenIddictApplicationDescriptor {
         ClientId = options.BlazorWasm.ClientId,
         DisplayName = "Blazor WASM",
         ClientType = ClientTypes.Public,

         RedirectUris = { options.BlazorWasmRedirectUri() },
         PostLogoutRedirectUris = { options.BlazorWasmPostLogoutRedirectUri() },

         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,

            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,

            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile,
            Permissions.Prefixes.Scope + options.ScopeApi
         },

         Requirements = {
            Requirements.Features.ProofKeyForCodeExchange
         }
      }, requiresSecret: false);

      // ------------------------------------------------------------
      // 2) Web MVC (Confidential + Code)
      // ------------------------------------------------------------
      await UpsertAsync(new OpenIddictApplicationDescriptor {
         ClientId = options.WebMvc.ClientId,
         ClientSecret = config[AuthServerSecretKeys.WebMvcClientSecret],
         DisplayName = "WebClient MVC",
         ClientType = ClientTypes.Confidential,

         RedirectUris = { options.WebMvcRedirectUri() },
         PostLogoutRedirectUris = { options.WebMvcPostLogoutRedirectUri() },

         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,

            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,

            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile,
            Permissions.Prefixes.Scope + options.ScopeApi
         }
      }, requiresSecret: true);

      // ------------------------------------------------------------
      // 3) Android (Public + Code + PKCE)
      // ------------------------------------------------------------
      await UpsertAsync(new OpenIddictApplicationDescriptor {
         ClientId = options.Android.ClientId,
         DisplayName = "Android App",
         ClientType = ClientTypes.Public,

         RedirectUris = {
            options.AndroidCustomSchemeRedirectUri(),
            options.AndroidLoopbackRedirectUri()
         },
         PostLogoutRedirectUris = {
            options.AndroidPostLogoutRedirectUri()
         },

         Permissions = {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,

            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,

            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Profile,
            Permissions.Prefixes.Scope + options.ScopeApi
         },

         Requirements = {
            Requirements.Features.ProofKeyForCodeExchange
         }
      }, requiresSecret: false);

      // ------------------------------------------------------------
      // 4) Service Client (Confidential + Client Credentials)
      // ------------------------------------------------------------
      await UpsertAsync(new OpenIddictApplicationDescriptor {
         ClientId = options.ServiceClient.ClientId,
         ClientSecret = config[AuthServerSecretKeys.ServiceClientSecret],
         DisplayName = "Service Client",
         ClientType = ClientTypes.Confidential,

         Permissions = {
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.ClientCredentials,
            Permissions.Prefixes.Scope + options.ScopeApi
         }
      }, requiresSecret: true);
   }

   public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

// using Microsoft.Extensions.Options;
// using OidcOauthServer.Auth.Options;
// using OpenIddict.Abstractions;
// using static OpenIddict.Abstractions.OpenIddictConstants;
// namespace OidcOauthServer.Auth.Seeding;
//
// /// <summary>
// /// Seeds demo OpenIddict clients:
// /// - Blazor WASM (public, Code + PKCE)
// /// - Web MVC (confidential, Code)
// /// - Android (public, Code + PKCE)
// /// - Service client (confidential, Client Credentials)
// ///
// /// Notes:
// /// - In OpenIddict 7.x, "openid" does not require an explicit scope permission.
// /// - Client secrets are read from IConfiguration (UserSecrets/Env/KeyVault).
// /// </summary>
// public sealed class SeedHostedService(
//    IServiceProvider _sp,
//    IConfiguration _config,
//    ILogger<SeedHostedService> _logger
// ) : IHostedService {
//    
//
//    public async Task StartAsync(CancellationToken ct) {
//       
//       using var scope = _sp.CreateScope();
//
//       var options = scope.ServiceProvider
//          .GetRequiredService<IOptions<AuthServerOptions>>().Value;
//       var apps = scope.ServiceProvider
//          .GetRequiredService<IOpenIddictApplicationManager>();
//
//       // ------------------------------------------------------------
//       // 1) Blazor WASM (Public + Code + PKCE)
//       // ------------------------------------------------------------
//       if (await apps.FindByClientIdAsync(options.BlazorWasm.ClientId, ct) is null) {
//          await apps.CreateAsync(new OpenIddictApplicationDescriptor {
//             ClientId = options.BlazorWasm.ClientId,
//             DisplayName = "Blazor WASM",
//             ClientType = ClientTypes.Public,
//
//             RedirectUris = { options.BlazorWasmRedirectUri() },
//             PostLogoutRedirectUris = { options.BlazorWasmPostLogoutRedirectUri() },
//
//             Permissions = {
//                Permissions.Endpoints.Authorization,
//                Permissions.Endpoints.Token,
//                Permissions.Endpoints.EndSession,
//
//                Permissions.GrantTypes.AuthorizationCode,
//                Permissions.ResponseTypes.Code,
//
//                Permissions.Prefixes.Scope + Scopes.OpenId,
//                Permissions.Prefixes.Scope + Scopes.Profile,
//                Permissions.Prefixes.Scope + options.ScopeApi
//             },
//
//             Requirements = {
//                Requirements.Features.ProofKeyForCodeExchange
//             }
//          }, ct);
//       }
//
//       // ------------------------------------------------------------
//       // 2) Web MVC (Confidential + Code)
//       // ------------------------------------------------------------
//       if (await apps.FindByClientIdAsync(options.WebMvc.ClientId, ct) is null) {
//          var webMvcSecret = _config[AuthServerSecretKeys.WebMvcClientSecret];
//          if (string.IsNullOrWhiteSpace(webMvcSecret))
//             throw new InvalidOperationException(
//                $"Missing secret '{AuthServerSecretKeys.WebMvcClientSecret}'. " +
//                "Set it via user-secrets or environment variables.");
//
//          await apps.CreateAsync(new OpenIddictApplicationDescriptor {
//             ClientId = options.WebMvc.ClientId,
//             ClientSecret = webMvcSecret,
//             DisplayName = "WebClient MVC",
//             ClientType = ClientTypes.Confidential,
//
//             RedirectUris = { options.WebMvcRedirectUri() },
//             PostLogoutRedirectUris = { options.WebMvcPostLogoutRedirectUri() },
//
//             Permissions = {
//                Permissions.Endpoints.Authorization,
//                Permissions.Endpoints.Token,
//                Permissions.Endpoints.EndSession,
//
//                Permissions.GrantTypes.AuthorizationCode,
//                Permissions.ResponseTypes.Code,
//
//                Permissions.Prefixes.Scope + Scopes.OpenId,
//                Permissions.Prefixes.Scope + Scopes.Profile,
//                Permissions.Prefixes.Scope + options.ScopeApi
//             }
//          }, ct);
//       }
//
//       // ------------------------------------------------------------
//       // 3) Android (Public + Code + PKCE)
//       // ------------------------------------------------------------
//       if (await apps.FindByClientIdAsync(options.Android.ClientId, ct) is null) {
//          await apps.CreateAsync(new OpenIddictApplicationDescriptor {
//             ClientId = options.Android.ClientId,
//             DisplayName = "Android App",
//             ClientType = ClientTypes.Public,
//
//             RedirectUris = { options.AndroidRedirectUri(), options.AndroidLoopbackRedirectUri() },
//             PostLogoutRedirectUris = { options.AndroidPostLogoutRedirectUri() },
//
//             Permissions = {
//                Permissions.Endpoints.Authorization,
//                Permissions.Endpoints.Token,
//                Permissions.Endpoints.EndSession, // <-- add
//
//                Permissions.GrantTypes.AuthorizationCode,
//                Permissions.ResponseTypes.Code,
//
//                Permissions.Prefixes.Scope + Scopes.OpenId,
//                Permissions.Prefixes.Scope + Scopes.Profile,
//                Permissions.Prefixes.Scope + options.ScopeApi
//             },
//
//             Requirements = {
//                Requirements.Features.ProofKeyForCodeExchange
//             }
//          }, ct);
//       }
//
//       // ------------------------------------------------------------
//       // 4) Service / WebApi (Confidential + Client Credentials)
//       // ------------------------------------------------------------
//       if (await apps.FindByClientIdAsync(options.ServiceClient.ClientId, ct) is null) {
//          var serviceSecret = _config[AuthServerSecretKeys.ServiceClientSecret];
//          if (string.IsNullOrWhiteSpace(serviceSecret))
//             throw new InvalidOperationException(
//                $"Missing secret '{AuthServerSecretKeys.ServiceClientSecret}'. " +
//                "Set it via user-secrets or environment variables.");
//
//          await apps.CreateAsync(new OpenIddictApplicationDescriptor {
//             ClientId = options.ServiceClient.ClientId,
//             ClientSecret = serviceSecret,
//             DisplayName = "Service Client",
//             ClientType = ClientTypes.Confidential,
//
//             Permissions = {
//                Permissions.Endpoints.Token,
//                Permissions.GrantTypes.ClientCredentials,
//                Permissions.Prefixes.Scope + options.ScopeApi
//             }
//          }, ct);
//       }
//    }
//
//    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
// }