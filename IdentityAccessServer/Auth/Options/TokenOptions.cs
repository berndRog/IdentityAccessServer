namespace IdentityAccessServer.Auth.Options;

public sealed class TokenOptions
{
   /// <summary>
   /// If true, OpenIddict will encrypt access tokens (JWE).
   /// If false, access tokens are not encrypted (JWS) and can be inspected with jwt.io.
   /// 
   /// Recommended:
   /// - false in Development / teaching (easy debugging)
   /// - true in Production-like setups (token confidentiality)
   /// </summary>
   public bool EncryptAccessTokens { get; init; } = true;
}