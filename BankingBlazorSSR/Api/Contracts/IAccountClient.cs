using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface IAccountClient {
   // GET /accounts
   Task<Result<IEnumerable<AccountDto>>> GetAllAsync(CancellationToken ct = default);

   // GET /owners/{ownerId}/accounts
   Task<Result<IEnumerable<AccountDto>>> GetAllByOwnerAsync(
      Guid ownerId,
      CancellationToken ct = default
   );

   // GET /accounts/{accountId}
   Task<Result<AccountDto>> GetByIdAsync(
      Guid accountId,
      CancellationToken ct = default
   );

   // GET /accounts/iban/{iban}
   Task<Result<AccountDto>> GetByIbanAsync(
      string iban,
      CancellationToken ct = default
   );

   // POST /owners/{ownerId}/accounts
   Task<Result<AccountDto>> PostAsync(
      Guid ownerId,
      AccountDto dto,
      CancellationToken ct = default
   );
   
   // -------------------------------------------------------------------------------------
   // Beneficiaries endpoints
   // -------------------------------------------------------------------------------------
   // GET /accounts/{accountId}/beneficiaries
   Task<Result<IEnumerable<BeneficiaryDto>>> GetAllAsync(
      Guid accountId,
      CancellationToken ct
   );
   
   // GET /accounts/{accountId}/beneficiaries/{beneficiaryId}
   Task<Result<BeneficiaryDto>> GetByIdAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );
   
   // POST /accounts/{accountId}/beneficiaries
   Task<Result<BeneficiaryDto>> PostAsync(
      Guid accountId,
      BeneficiaryDto dto,
      CancellationToken ct = default
   );

   // DELETE /accounts/{accountId}/beneficiaries/{beneficiaryId}
   // API returns 204 NoContent -> Result<bool>
   Task<Result<bool>> DeleteAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct
   );

}