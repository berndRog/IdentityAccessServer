namespace OidcOauthServer.Auth.Options;

public class ClientOptions {
   public string ClientId { get; init; } = default!;
   public string BaseUrl { get; init; } = default!;
   public string RedirectPath { get; init; } = "/signin-oidc";
   public string PostLogoutRedirectPath { get; init; } = "/signout-callback-oidc";
   public string Type { get; init; } = "Confidential";
}
