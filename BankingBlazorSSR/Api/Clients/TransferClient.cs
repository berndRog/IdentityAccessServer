// using BankingBlazorSsr.Core;
// using BankingBlazorSsr.Core.Dto;
// namespace BankingBlazorSsr.Api.Clients;
//
// public class TransferClient(
//    WebClientOptions<TransferClient> options
// ): BaseWebClient<TransferClient>(options), ITransferClient {
//
//    // Get Transfer by accountID
//    public async Task<Result<IEnumerable<TransferDto>?>> GetByAccountId(
//       Guid accountId
//    ) => await GetAllAsync<TransferDto>($"accounts/{accountId}/transfers");
//
//    // Send Transfer 
//    public async Task<Result<TransferDto?>> SendTransfer(TransferDto transferDto, Guid accountId) =>
//       await PostAsync<TransferDto>($"accounts/{accountId}/transfers", transferDto);
// }