namespace BankingBlazorSSR.Api.Dtos;

public sealed record OwnerMeDto(
   Guid Id,
   string Email,
   string Subject,
   string Status
);
