namespace BankingBlazorSsr.Ui.Models;

public class TransferCreateModel {
   public double Amount { get; set; }
   public string TransferReason { get; set; } = string.Empty;
   public DateTime TransferDate { get; set; } = DateTime.Now;
}
