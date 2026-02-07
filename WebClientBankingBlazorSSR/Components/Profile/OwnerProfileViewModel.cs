using System.ComponentModel.DataAnnotations;

namespace WebClientBankingBlazorSSR.Pages.Profile;

public sealed class OwnerProfileFormModel 
{
   [Required(ErrorMessage = "Vorname ist erforderlich.")]
   public string Firstname { get; set; } = string.Empty;

   [Required(ErrorMessage = "Nachname ist erforderlich.")]
   public string Lastname { get; set; } = string.Empty;

   [Required(ErrorMessage = "E-Mail ist erforderlich.")]
   [EmailAddress(ErrorMessage = "E-Mail ist ungültig.")]
   public string Email { get; set; } = string.Empty;

   public string? Street { get; set; }
   public string? PostalCode { get; set; }
   public string? City { get; set; }
   public string? Country { get; set; }
}
