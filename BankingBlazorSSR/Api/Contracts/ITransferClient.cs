using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface ITransferClient {
   Task<Result<IEnumerable<TransferDto>?>> GetByAccountId(Guid accountId);
   Task<Result<TransferDto?>> SendTransfer(TransferDto transferDto, Guid accountId);
}