using System.Text.Json;
using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Clients;

public sealed class AccountClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<AccountClient> logger
) : BaseApiClient<AccountClient>(factory, json, logger), IAccountClient {
   
   // -------------------------------------------------------------------------------------
   // Account endpoints
   // -------------------------------------------------------------------------------------
   // GET /accounts
   public Task<Result<IEnumerable<AccountDto>>> GetAllAsync(CancellationToken ct) =>
      SendAsync<IEnumerable<AccountDto>>(
         () => _http.GetAsync("accounts", ct), ct);

   // GET /owners/{ownerId}/accounts
   public Task<Result<IEnumerable<AccountDto>>> GetAllByOwnerAsync(
      Guid ownerId, 
      CancellationToken ct
   ) => SendAsync<IEnumerable<AccountDto>>(
      () => _http.GetAsync($"owners/{ownerId}/accounts", ct), ct);

   // GET /accounts/{accountId}
   public Task<Result<AccountDto>> GetByIdAsync(
      Guid accountId,
      CancellationToken ct
   ) => SendAsync<AccountDto>(
      () => _http.GetAsync($"accounts/{accountId}", ct), ct);

   // GET /accounts/iban/{iban}
   public Task<Result<AccountDto>> GetByIbanAsync(
      string iban,
      CancellationToken ct 
   ) => SendAsync<AccountDto>(
      () => _http.GetAsync($"accounts/iban/{Uri.EscapeDataString(iban)}", ct), ct);

   // POST /owners/{ownerId}/accounts
   public Task<Result<AccountDto>> PostAsync(
      Guid ownerId,
      AccountDto dto,
      CancellationToken ct 
   ) => SendAsync<AccountDto>(
      () => _http.PostAsJsonAsync($"owners/{ownerId}/accounts", dto, _json, ct), ct);

   // Example command without body (204) -> Result<bool>
   public Task<Result<bool>> DeactivateAsync(Guid accountId, CancellationToken ct) =>
      SendAsync<bool>(
         () => _http.PostAsync($"accounts/{accountId}/deactivate", content: null, ct),
         ct
      );
   
   // -------------------------------------------------------------------------------------
   // Beneficiaries endpoints
   // -------------------------------------------------------------------------------------
   // GET /accounts/{accountId}/beneficiaries
   public Task<Result<IEnumerable<BeneficiaryDto>>> GetAllAsync(
      Guid accountId,
      CancellationToken ct
   ) => SendAsync<IEnumerable<BeneficiaryDto>>(
         () => _http.GetAsync($"accounts/{accountId}/beneficiaries", ct), ct);

   // GET /accounts/{accountId}/beneficiaries/{beneficiaryId}
   public Task<Result<BeneficiaryDto>> GetByIdAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   ) =>
      SendAsync<BeneficiaryDto>(
         () => _http.GetAsync(
            $"accounts/{accountId}/beneficiaries/{beneficiaryId}", ct),
         ct
      );

   // POST /accounts/{accountId}/beneficiaries
   public Task<Result<BeneficiaryDto>> PostAsync(
      Guid accountId,
      BeneficiaryDto dto,
      CancellationToken ct = default) =>
      SendAsync<BeneficiaryDto>(
         () => _http.PostAsJsonAsync(
            $"accounts/{accountId}/beneficiaries", dto, _json, ct),
         ct
      );

   // DELETE /accounts/{accountId}/beneficiaries/{beneficiaryId}
   // API returns 204 NoContent -> Result<bool>
   public Task<Result<bool>> DeleteAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default) =>
      SendAsync<bool>(
         () => _http.DeleteAsync(
            $"accounts/{accountId}/beneficiaries/{beneficiaryId}", ct),
         ct
      );
   
}