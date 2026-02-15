namespace BankingBlazorSsr.Api.Dtos;

/// <summary>
/// TransactionListItem (Umsätze)
/// </summary>
public record TransactionListItemDto(
   Guid Id,
   DateTime Date,
   double Amount,
   string Description, 
   string FirstName,  // receiver
   string LastName,   // receiver  
   string Iban,       // receiver
   Guid AccountId,    // sender
   Guid TransferId    // sender/receiver
);
