namespace BankingBlazorSsr.Api.Dtos;

/// <summary>
/// TransferDto (Überweisung an einen Empfänger)
/// </summary>
public class TransferDto {
   
   public Guid Id { get; init; }
   public DateTime Date { get; set; }
   public string Description { get; set; } = string.Empty;
   public double Amount { get; set; } 
   public Guid AccountId{ get; set; } 
   public Guid BeneficiaryId{ get; set; } 
}