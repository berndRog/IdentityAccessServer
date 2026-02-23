using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface IAccountClient {
   // GET /accounts
   Task<Result<IEnumerable<AccountDto>>> GetAllAccountsAsync(CancellationToken ct = default);

   // GET /customers/{customerId}/accounts
   Task<Result<IEnumerable<AccountDto>>> GetAccountsByOwnerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   );

   // GET /accounts/{accountId}
   Task<Result<AccountDto>> GetAccountByIdAsync(
      Guid accountId,
      CancellationToken ct = default
   );

   // GET /accounts/iban/{iban}
   Task<Result<AccountDto>> GetAccountByIbanAsync(
      string iban,
      CancellationToken ct = default
   );

   // POST /customers/{customerId}/accounts
   Task<Result<AccountDto>> PostAccountAsync(
      Guid customerId,
      AccountDto dto,
      CancellationToken ct = default
   );
   
   // -------------------------------------------------------------------------------------
   // Beneficiaries endpoints
   // -------------------------------------------------------------------------------------
   // GET /accounts/{accountId}/beneficiaries
   Task<Result<IEnumerable<BeneficiaryDto>>> GetBeneficiariesByAcountIdAsync(
      Guid accountId,
      CancellationToken ct
   );
   
   // GET /accounts/{accountId}/beneficiaries/{beneficiaryId}
   Task<Result<BeneficiaryDto>> GetBeneficiaryByIdAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );
   
   // POST /accounts/{accountId}/beneficiaries
   Task<Result<BeneficiaryDto>> PostBeneficiaryAsync(
      Guid accountId,
      BeneficiaryDto dto,
      CancellationToken ct = default
   );

   // DELETE /accounts/{accountId}/beneficiaries/{beneficiaryId}
   // API returns 204 NoContent -> Result<bool>
   Task<Result<bool>> DeletebenericiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct
   );

}