using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BankingBlazorSsr.Api.Dtos;

public sealed record CustomerDto {

   // NOTE: Avoid [IgnoreDataMember] with System.Text.Json.
   // It can cause members to be skipped during (de)serialization.
   
   public Guid Id { get; set; } = Guid.Empty;
   
   [Required]
   [MinLength(2, ErrorMessage = "Vorname muss mindestens 2 Zeichen lang sein.")]
   [MaxLength(100, ErrorMessage = "Vorname darf maximal 100 Zeichen lang sein.")]
   public string Firstname { get; set; } = string.Empty;

   [Required]
   [MinLength(2, ErrorMessage = "Nachname muss mindestens 2 Zeichen lang sein.")]
   [MaxLength(100, ErrorMessage = "Nachname darf maximal 100 Zeichen lang sein.")]
   public string Lastname { get; set; } = string.Empty;
   
   // Optional, but if present it must be within bounds.
   // Note: MinLength does not run for null; CustomerProfilePage normalizes whitespace to null.
   [MinLength(2, ErrorMessage = "Firma muss mindestens 2 Zeichen lang sein.")]
   [MaxLength(100, ErrorMessage = "Firma darf maximal 100 Zeichen lang sein.")]
   public string? CompanyName { get; set; }

   [Required]
   [EmailAddress(ErrorMessage = "Bitte geben Sie eine zulässige E-Mail-Adresse ein.")]
   [StringLength(254)] // RFC 5321 practical limit
   public string EmailString { get; set; } = string.Empty;

   public int Status { get; set; } // "Pending = 0 | Active = 1 | Rejected ? 2 | Deactivated = 3"
   
   [StringLength(200)]
   public string? Street { get; set; }

   [StringLength(20)]
   public string? PostalCode { get; set; }

   [StringLength(100)]
   public string? City { get; set; }

   [StringLength(100)]
   public string? Country { get; set; }
}
