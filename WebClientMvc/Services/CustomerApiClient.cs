using System.Net.Http.Headers;
using WebClientMvc.Models.Dtos;
namespace WebClientMvc.Services;

/// <summary>
/// Default HTTP-based implementation of ICustomersApiClient.
/// </summary>
public sealed class CustomersApiClient(
   HttpClient httpClient,
   IAccessTokenProvider tokenProvider
) : ICustomersApiClient {
   /// <summary>
   /// Creates an authenticated HTTP request with a Bearer access token.
   /// </summary>
   private async Task<HttpRequestMessage> CreateRequestAsync(
      HttpMethod method,
      string uri,
      CancellationToken ct
   ) {
      // Get access token for current user
      var token = await tokenProvider.GetAccessTokenAsync(ct);

      // Create request with Authorization header
      var request = new HttpRequestMessage(method, uri);
      // Add Bearer token to Authorization header
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      return request;
   }

   /// <summary>
   /// Ensures that a Customer aggregate exists for the current identity.
   /// This operation is idempotent.
   /// </summary>
   public async Task<ApiResult<Guid>> EnsureProvisionedAsync(CancellationToken ct) {
      
      // Create authenticated POST customers/provisioned request
      using var request = await CreateRequestAsync(HttpMethod.Post, "customers/provisioned", ct);
      using var response = await httpClient.SendAsync(request, ct);

      if (response.IsSuccessStatusCode) {
         var id = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: ct);
         return ApiResult<Guid>.Ok(id);
      }

      return await ApiResult<Guid>.FromErrorResponseAsync(response, ct);
   }

   /// <summary>
   /// Loads the current customer's profile.
   /// </summary>
   public async Task<ApiResult<CustomerProfileDto>> GetProfileAsync(CancellationToken ct) {
      using var request = await CreateRequestAsync(HttpMethod.Get, "customers/profile", ct);
      using var response = await httpClient.SendAsync(request, ct);

      if (response.IsSuccessStatusCode) {
         var dto = await response.Content.ReadFromJsonAsync<CustomerProfileDto>(cancellationToken: ct);
         return ApiResult<CustomerProfileDto>.Ok(dto!);
      }

      return await ApiResult<CustomerProfileDto>.FromErrorResponseAsync(response, ct);
   }

   /// <summary>
   /// Updates the customer's profile data.
   /// </summary>
   public async Task<ApiResult<CustomerProfileDto>> UpdateProfileAsync(
      CustomerProfileDto dto,
      CancellationToken ct
   ) {
      using var request = await CreateRequestAsync(HttpMethod.Put, "customers/profile", ct);
      request.Content = JsonContent.Create(dto);

      using var response = await httpClient.SendAsync(request, ct);

      if (response.IsSuccessStatusCode) {
         var updated = await response.Content.ReadFromJsonAsync<CustomerProfileDto>(cancellationToken: ct);
         return ApiResult<CustomerProfileDto>.Ok(updated!);
      }

      return await ApiResult<CustomerProfileDto>.FromErrorResponseAsync(response, ct);
   }
}

/* ======================================================================
   DIDAKTIK & LERNZIELE
   ======================================================================

   Zweck dieses API-Clients
   ------------------------
   Der CustomersApiClient ist ein klassischer
   "Infrastructure Adapter" im Sinne von Clean Architecture.

   Er kapselt:
   - HTTP
   - Authentication Header
   - Statuscodes
   - JSON-Serialisierung

   Der MVC-Controller:
   - kennt KEINE URLs
   - kennt KEINE Tokens
   - kennt KEINE HTTP-Details

   Zentrale didaktische Aussagen
   -----------------------------
   1. MVC ist ein Client der API – nicht ihr Besitzer.
      → gleiche API kann von MVC, Blazor, Mobile genutzt werden.

   2. Authentifizierung ≠ Fachlogik
      → Token-Anhängen ist Infrastruktur, kein Use Case.

   3. Idempotenz sichtbar machen
      → EnsureProvisionedAsync zeigt,
        dass "Provisioning" kein einmaliger Sonderfall ist.

   4. Saubere Schichtung
      - Controller: Ablauf / Navigation
      - ApiClient: technische Kommunikation
      - API: fachliche Regeln

   Merksatz
   --------
   „MVC spricht HTTP – die Domäne spricht Bedeutung.“
   ======================================================================
*/

/*
   // POST customers/provisioned  -> Guid (customerId)
   public async Task<ApiResult<Guid>> EnsureProvisionedAsync(
      string accessToken,
      CancellationToken ct
   ) {
      using var request = new HttpRequestMessage(HttpMethod.Post, "customers/provisioned");
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      using var response = await _httpCient.SendAsync(request, ct);
      if (response.IsSuccessStatusCode) {
         var id = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: ct);
         return ApiResult<Guid>.Ok(id);
      }

      return await ApiResult<Guid>.FromErrorResponseAsync(response, ct);
   }

   // GET customers/profile -> CustomerProfileDto (404 if not provisioned)
   public async Task<ApiResult<CustomerProfileDto>> GetMyProfileAsync(
      string accessToken,
      CancellationToken ct
   ) {
      using var request = new HttpRequestMessage(HttpMethod.Get, "customers/profile");
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

      using var response = await _httpCient.SendAsync(request, ct);
      if (response.IsSuccessStatusCode) {
         var dto = await response.Content.ReadFromJsonAsync<CustomerProfileDto>(cancellationToken: ct);
         return ApiResult<CustomerProfileDto>.Ok(dto!);
      }

      return await ApiResult<CustomerProfileDto>.FromErrorResponseAsync(response, ct);
   }

   // PUT /api/customers/profile -> CustomerProfileDto
   public async Task<ApiResult<CustomerProfileDto>> UpdateMyProfileAsync(
      string accessToken,
      CustomerProfileDto dto,
      CancellationToken ct
   ) {
      using var request = new HttpRequestMessage(HttpMethod.Put, "customers/profile");
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      request.Content = JsonContent.Create(dto);

      using var response = await _httpCient.SendAsync(request, ct);
      if (response.IsSuccessStatusCode) {
         var updated = await response.Content.ReadFromJsonAsync<CustomerProfileDto>(cancellationToken: ct);
         return ApiResult<CustomerProfileDto>.Ok(updated!);
      }

      return await ApiResult<CustomerProfileDto>.FromErrorResponseAsync(response, ct);
   }
   */
