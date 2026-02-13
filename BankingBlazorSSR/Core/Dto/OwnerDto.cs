namespace BankingBlazorSsr.Core.Dto;

/// <summary>
/// OwnerDto (Kontoinhaber)
/// </summary>
public record OwnerDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string? Email,
   string UserName,  
   string? UserId
);
