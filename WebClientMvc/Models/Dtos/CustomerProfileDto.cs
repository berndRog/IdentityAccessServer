namespace WebClientMvc.Models.Dtos;

public sealed record CustomerProfileDto(
   string Firstname,
   string Lastname,
   string Email,
   string? Street,
   string? PostalCode,
   string? City,
   string? Country
);
