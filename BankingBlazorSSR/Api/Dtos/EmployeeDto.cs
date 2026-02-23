using System.ComponentModel.DataAnnotations;
namespace BankingBlazorSsr.Api.Dtos;

public sealed record EmployeeDto {

   public Guid Id { get; set; } = Guid.Empty;
   
   [Required]
   [MinLength(2, ErrorMessage = "Vorname must mindestens 2 Zeichen lang sein.")]
   [MaxLength(100, ErrorMessage = "Vorname darf maximal 100 Zeichen lang sein.")]
   public string Firstname { get; set; } = string.Empty;
   
   [Required]
   [MinLength(2, ErrorMessage = "Nachname must mindestens 2 Zeichen lang sein.")]
   [MaxLength(100, ErrorMessage = "Nachname darf maximal 100 Zeichen lang sein.")]
   public string Lastname { get; set; } = string.Empty;
   
   [Required]
   [EmailAddress(ErrorMessage = "Bitte geben Sie eine zulässige E-Mail-Adresse ein.")]
   [StringLength(254)] // RFC 5321 practical limit
   public string EmailString { get; set; } = string.Empty;

   [Phone(ErrorMessage = "Please enter a valid phone number.")]
   [StringLength(34)]
   public string? PhoneString { get; set; }
   
   [Required]
   [StringLength(20, MinimumLength = 2,
      ErrorMessage = "Personnel number must be between 2 and 20 characters.")]
   public string PersonnelNumber { get; set; } = string.Empty;
   
   public bool IsActive { get; set; }
   public int AdminRights { get; set; }

}
