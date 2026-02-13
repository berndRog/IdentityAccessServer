using System.Text.Json;
using BankingBlazorSsr.Core;
using BankingBlazorSsr.Core.Dto;
namespace BankingBlazorSsr.Api.Clients;

public sealed class AccountClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<AccountClient> logger
) : BaseApiClient<AccountClient>(factory, json, logger) {
   // GET /accounts
   public Task<Result<IEnumerable<AccountDto>>> GetAllAsync(CancellationToken ct = default) =>
      SendAsync<IEnumerable<AccountDto>>(
         () => _http.GetAsync("accounts", ct), ct);

   // GET /owners/{ownerId}/accounts
   public Task<Result<IEnumerable<AccountDto>>> GetAllByOwnerAsync(
      Guid ownerId,
      CancellationToken ct = default
   ) => SendAsync<IEnumerable<AccountDto>>(
      () => _http.GetAsync($"owners/{ownerId}/accounts", ct), ct);

   // GET /accounts/{accountId}
   public Task<Result<AccountDto>> GetByIdAsync(
      Guid accountId,
      CancellationToken ct = default
   ) => SendAsync<AccountDto>(
      () => _http.GetAsync($"accounts/{accountId}", ct), ct);

   // GET /accounts/iban/{iban}
   public Task<Result<AccountDto>> GetByIbanAsync(
      string iban,
      CancellationToken ct = default
   ) => SendAsync<AccountDto>(
      () => _http.GetAsync($"accounts/iban/{Uri.EscapeDataString(iban)}", ct), ct);

   // POST /owners/{ownerId}/accounts
   public Task<Result<AccountDto>> PostAsync(
      Guid ownerId,
      AccountDto dto,
      CancellationToken ct = default
   ) => SendAsync<AccountDto>(
      () => _http.PostAsJsonAsync($"owners/{ownerId}/accounts", dto, _json, ct), ct);

   // Example command without body (204) -> Result<bool>
   public Task<Result<bool>> DeactivateAsync(Guid accountId, CancellationToken ct = default) =>
      SendAsync<bool>(
         () => _http.PostAsync($"accounts/{accountId}/deactivate", content: null, ct),
         ct
      );
}