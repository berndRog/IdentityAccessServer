using BankingBlazorSsr.Core.Dto;
namespace BankingBlazorSsr.Core;

public interface IBeneficiaryClient {
   Task<Result<IEnumerable<BeneficiaryDto>?>> GetByAccount(Guid accountId);
   Task<Result<BeneficiaryDto?>> Post(Guid accountId, BeneficiaryDto beneficiaryDto);
   Task<Result<BeneficiaryDto?>> GetById(Guid beneficiaryId);
}