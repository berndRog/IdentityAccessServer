using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace BankingBlazorSsr.Api.Dtos;

/// <summary>
/// AccountDto (Bankkonto)
/// </summary>
public sealed record AccountDto(
   Guid Id,
   string IbanString,
   decimal BalanceDecimal,
   Guid CustomerId
);
