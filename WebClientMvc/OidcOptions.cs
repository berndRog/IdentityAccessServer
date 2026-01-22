namespace WebClientMvc;

public sealed class OidcClientOptions {
   public string Authority { get; init; } = default!;
   public string ClientId { get; init; } = default!;
   public string BaseUrl { get; init; } = default!;

   public string RedirectPath { get; init; } = default!;
   public string SignedOutCallbackPath { get; init; } = default!;
   public string PostLogoutRedirectPath { get; init; } = default!;
   public string PostLogoutRedirectFallbackPath { get; init; } = default!;

   public string[] Scopes { get; init; } = Array.Empty<string>();

   // ---- Derived URIs (no string juggling elsewhere) ----
   public Uri RedirectUri =>
      new($"{BaseUrl.TrimEnd('/')}{RedirectPath}");

   public Uri SignedOutCallbackUri =>
      new($"{BaseUrl.TrimEnd('/')}{SignedOutCallbackPath}");

   public Uri PostLogoutRedirectUri =>
      new($"{BaseUrl.TrimEnd('/')}{PostLogoutRedirectPath}");
   
   public Uri PostLogoutRedirectFallbackUri =>
      new($"{BaseUrl.TrimEnd('/')}{PostLogoutRedirectFallbackPath}");
   
}