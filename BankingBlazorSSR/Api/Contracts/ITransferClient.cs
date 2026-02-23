using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface ITransferClient {
   
   Task<Result<IEnumerable<TransferDto>>> GetTransfersByAccountId(
      Guid accountId,
      CancellationToken ct = default
   );

   Task<Result<TransferDto?>> SendTransfer(
      TransferDto transferDto,
      Guid accountId,
      CancellationToken ct = default
   );
   
   Task<Result<IEnumerable<TransactionDto>>> FilterByAccountId(
      Guid accountId,
      string start,
      string end,
      CancellationToken ct = default
   );

   Task<Result<IEnumerable<TransactionListItemDto>?>> FilterListItemsByAccountId(
      Guid accountId,
      string start,
      string end,
      CancellationToken ct = default
   );
}