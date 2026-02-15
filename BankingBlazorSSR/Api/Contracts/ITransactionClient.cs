using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface ITransactionClient {
   //Task<ResultData<IEnumerable<TransactionDto>?>> FilterByAccountId(Guid accountId, string start, string end);
   Task<Result<IEnumerable<TransactionListItemDto>?>> FilterListItemsByAccountId(Guid accountId, string start, string end);
}