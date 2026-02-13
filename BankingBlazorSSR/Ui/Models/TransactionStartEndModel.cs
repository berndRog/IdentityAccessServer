using System.ComponentModel.DataAnnotations;
namespace BankingBlazorSsr.Ui.Models;

public class TransactionStartEndModel {
   [Required(ErrorMessage = "Anfangsdatum ist erforderlich.")]
   public DateTime Start { get; set; } = DateTime.Now;

   [Required(ErrorMessage = "Enddatum ist erforderlich.")]
   public DateTime End { get; set; } = DateTime.Now;
}