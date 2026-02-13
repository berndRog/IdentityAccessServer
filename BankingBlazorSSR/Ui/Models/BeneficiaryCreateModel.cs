using System.ComponentModel.DataAnnotations;
namespace BankingBlazorSsr.Ui.Models;

public class BeneficiaryCreateModel {

   [Required]
   [MinLength(2, ErrorMessage = "Vorname must mindestens 2 Zeichen lang sein.")]
   [MaxLength(50, ErrorMessage = "Vorname darf maximal 50 Zeichen lang sein.")]
   public string Firstname { get; set; } = string.Empty;

   [Required]
   [MinLength(2, ErrorMessage = "Nachname must mindestens 2 Zeichen lang sein.")]
   [MaxLength(50, ErrorMessage = "Nachname darf maximal 50 Zeichen lang sein.")]
   public string Lastname { get; set; } = string.Empty;
   
   [Required]
   [IbanValidation]
   public string Iban { get; set; } = string.Empty;
}

public class IbanValidationAttribute : ValidationAttribute {
   protected override ValidationResult IsValid(object? value, ValidationContext validationContext) {
      if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
         return new ValidationResult("IBAN darf nicht leer sein.");

      var iban = (value as string)!.Replace(" ", "").ToUpper();
      if (iban.Length < 15 || iban.Length > 34)
         return new ValidationResult("Ungültige IBAN-Länge.");
      // Implement full IBAN checksum validation here if needed
      // Example: Rearrange, convert to numeric, check modulo 97 == 1
      return ValidationResult.Success!;
   }
}
