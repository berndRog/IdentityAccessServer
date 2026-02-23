using System.Text.Json;
using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;

namespace BankingBlazorSsr.Api.Clients;

public sealed class TransferClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<TransferClient> logger
) : BaseApiClient<TransferClient>(factory, json, logger), ITransferClient {
   private const string Base = "bankingapi/v1";

   // -------------------------------------------------------------------------------------
   // Transfers
   // -------------------------------------------------------------------------------------
   // GET /accounts/{accountId}/transfers
   public Task<Result<IEnumerable<TransferDto>>> GetTransfersByAccountId(
      Guid accountId,
      CancellationToken ct = default
   ) => SendAsync<IEnumerable<TransferDto>>(
      () => _http.GetAsync($"{Base}/accounts/{accountId}/transfers", ct), ct);

   // POST /accounts/{accountId}/transfers
   public Task<Result<TransferDto?>> SendTransfer(
      TransferDto transferDto,
      Guid accountId,
      CancellationToken ct = default
   ) => SendAsync<TransferDto?>(
      () => _http.PostAsJsonAsync($"{Base}/accounts/{accountId}/transfers", transferDto, _json, ct), ct);

   // -------------------------------------------------------------------------------------
   // Transactions (filter)
   // -------------------------------------------------------------------------------------
   // GET /accounts/{accountId}/transactions?start={start}&end={end}
   public Task<Result<IEnumerable<TransactionDto>>> FilterByAccountId(
      Guid accountId,
      string start,
      string end,
      CancellationToken ct = default
   ) => SendAsync<IEnumerable<TransactionDto>>(
      () => _http.GetAsync(
         $"{Base}/accounts/{accountId}/transactions?start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}",
         ct
      ), ct);

   // GET /accounts/{accountId}/transactions/list-items?start={start}&end={end}
   public Task<Result<IEnumerable<TransactionListItemDto>?>> FilterListItemsByAccountId(
      Guid accountId,
      string start,
      string end,
      CancellationToken ct = default
   ) => SendAsync<IEnumerable<TransactionListItemDto>?>(
      () => _http.GetAsync(
         $"{Base}/accounts/{accountId}/transactions/list-items?start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}",
         ct
      ), ct);
}
