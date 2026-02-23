using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace BankingBlazorSsr.Api.Dtos;

public sealed record CustomerDto {

   // NOTE: Avoid [IgnoreDataMember] with System.Text.Json.
   // It can cause members to be skipped during (de)serialization.
   
   public Guid Id { get; set; } = Guid.Empty;
   
   [Required]
   [StringLength(100, MinimumLength = 2,
      ErrorMessage = "First name must be between 2 and 80 characters.")]
   public string Firstname { get; set; } = string.Empty;

   [Required]
   [StringLength(100, MinimumLength = 2,
      ErrorMessage = "Last name must be between 2 and 80 characters.")]
   public string Lastname { get; set; } = string.Empty;
   
   [StringLength(100, MinimumLength = 2,
      ErrorMessage = "Company name must be less then 80 characters.")]
   public string? CompanyName { get; set; }
   
   [Required]
   [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
   [StringLength(254, MinimumLength = 5)] // RFC 5321 practical limit
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
