namespace OidcOauthServer.Auth.Options;

public class ClientOptions {
   public string ClientId { get; init; } = default!;
   public string BaseUrl { get; init; } = default!;
   public string RedirectPath { get; init; } = default!;
   public string PostLogoutRedirectPath { get; init; } = default!;
   public string Type { get; init; } = default!;
}