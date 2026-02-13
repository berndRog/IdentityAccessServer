// using BankingBlazorSsr.Core;
// using BankingBlazorSsr.Core.Dto;
// namespace BankingBlazorSsr.Api.Clients;
//
// public class TransactionClient(
//    WebClientOptions<TransactionClient> options
// ): BaseWebClient<TransactionClient>(options), ITransactionClient {
//
//    // // GET a list of transactions of an account by accountId and time interval start to end
//    // public async Task<ResultData<IEnumerable<TransactionDto>?>> FilterByAccountId(
//    //    Guid accountId, 
//    //    string start, 
//    //    string end
//    // ) => await GetAllAsync<TransactionDto>($"accounts/{accountId}/transactions/filter?start={start}&end={end}");
//    //    
//    // GET a list of transaction list items of an account by accountId and time interval start to end
//    public async Task<Result<IEnumerable<TransactionListItemDto>?>> FilterListItemsByAccountId(
//       Guid accountId, 
//       string start, 
//       string end
//    ) => await GetAllAsync<TransactionListItemDto>($"accounts/{accountId}/transactions/listitems?start={start}&end={end}");
//                                     
// }