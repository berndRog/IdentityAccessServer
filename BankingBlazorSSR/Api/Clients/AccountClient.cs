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
   private const string Base = "bankingapi/v1";

   // -------------------------------------------------------------------------------------
   // AccountDto endpoints
   // -------------------------------------------------------------------------------------
   // GET /accounts
   public Task<Result<IEnumerable<AccountDto>>> GetAllAccountsAsync(CancellationToken ct) =>
      SendAsync<IEnumerable<AccountDto>>(
         () => _http.GetAsync($"{Base}/accounts", ct), ct);

   // GET /customers/{customerId}/accounts
   public Task<Result<IEnumerable<AccountDto>>> GetAccountsByOwnerIdAsync(
      Guid customerId,
      CancellationToken ct
   ) => SendAsync<IEnumerable<AccountDto>>(() => _http.GetAsync(
      $"{Base}/customers/{customerId}/accounts", ct), ct);

   // GET /accounts/{accountId}
   public Task<Result<AccountDto>> GetAccountByIdAsync(
      Guid accountId,
      CancellationToken ct
   ) => SendAsync<AccountDto>(() => _http.GetAsync(
      $"{Base}/accounts/{accountId}", ct), ct);

   // GET /accounts/iban/{iban}
   public Task<Result<AccountDto>> GetAccountByIbanAsync(
      string ibanString,
      CancellationToken ct
   ) => SendAsync<AccountDto>(() => _http.GetAsync(
      $"{Base}/accounts/iban/{Uri.EscapeDataString(ibanString)}", ct), ct);

   // POST /customers/{customerId}/accounts
   public Task<Result<AccountDto>> PostAccountAsync(
      Guid customerId,
      AccountDto dto,
      CancellationToken ct
   ) => SendAsync<AccountDto>(() => _http.PostAsJsonAsync(
      $"{Base}/customers/{customerId}/accounts", dto, _json, ct), ct);

   // // Example command without body (204) -> Result<bool>
   // public Task<Result<bool>> DeactivateAsync(Guid accountId, CancellationToken ct) =>
   //    SendAsync<bool>(() => 
   //          _http.PostAsync($"{Base}/accounts/{accountId}/deactivate", content: null, ct), ct);
   //
   // -------------------------------------------------------------------------------------
   // Beneficiaries endpoints
   // -------------------------------------------------------------------------------------
   // GET /accounts/{accountId}/beneficiaries
   public Task<Result<IEnumerable<BeneficiaryDto>>> GetBeneficiariesByAcountIdAsync(
      Guid accountId,
      CancellationToken ct
   ) => SendAsync<IEnumerable<BeneficiaryDto>>(() => _http.GetAsync(
      $"accounts/{accountId}/beneficiaries", ct), ct);

   // GET /accounts/{accountId}/beneficiaries/{beneficiaryId}
   public Task<Result<BeneficiaryDto>> GetBeneficiaryByIdAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct
   ) => SendAsync<BeneficiaryDto>(() => _http.GetAsync(
      $"{Base}/accounts/{accountId}/beneficiaries/{beneficiaryId}", ct), ct);

   // POST /accounts/{accountId}/beneficiaries
   public Task<Result<BeneficiaryDto>> PostBeneficiaryAsync(
      Guid accountId,
      BeneficiaryDto dto,
      CancellationToken ct
   ) => SendAsync<BeneficiaryDto>(() => _http.PostAsJsonAsync(
      $"{Base}/accounts/{accountId}/beneficiaries", dto, _json, ct), ct);

   // DELETE /accounts/{accountId}/beneficiaries/{beneficiaryId}
   // API returns 204 NoContent -> Result<bool>
   public Task<Result<bool>> DeletebenericiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct
   ) => SendAsync<bool>(() => _http.DeleteAsync(
      $"{Base}/accounts/{accountId}/beneficiaries/{beneficiaryId}", ct), ct);
}