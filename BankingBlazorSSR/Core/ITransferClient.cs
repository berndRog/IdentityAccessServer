using BankingBlazorSsr.Core.Dto;
namespace BankingBlazorSsr.Core;

public interface ITransferClient {
   Task<Result<IEnumerable<TransferDto>?>> GetByAccountId(Guid accountId);
   Task<Result<TransferDto?>> SendTransfer(TransferDto transferDto, Guid accountId);
}