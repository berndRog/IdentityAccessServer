namespace BankingBlazorSsr.Core.Dto;

/// <summary>
/// Account (Bankkonto)
/// </summary>
public record AccountDto(
   Guid Id,
   string Iban,
   double Balance,
   Guid OwnerId
);
