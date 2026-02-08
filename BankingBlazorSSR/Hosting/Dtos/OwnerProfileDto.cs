namespace BankingBlazorSSR.Hosting.Dtos;

public sealed record OwnerProfileDto(
   string Firstname,
   string Lastname,
   string Email,
   string? Street,
   string? PostalCode,
   string? City,
   string? Country
);