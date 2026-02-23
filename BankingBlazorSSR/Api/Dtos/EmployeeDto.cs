using System.ComponentModel.DataAnnotations;
namespace BankingBlazorSsr.Api.Dtos;

public sealed record EmployeeDto {

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
