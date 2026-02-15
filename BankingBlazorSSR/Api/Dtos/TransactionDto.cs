namespace BankingBlazorSsr.Api.Dtos;

/// <summary>
/// TransactionDto (Buchung)
/// </summary>
public record TransactionDto(
   Guid Id,
   DateTime Date,
   double Amount,
   Guid AccountId,
   Guid TransferId
);