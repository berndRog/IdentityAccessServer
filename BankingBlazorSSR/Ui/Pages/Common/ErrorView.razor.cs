using Microsoft.AspNetCore.Components;
namespace BankingBlazorSsr.Ui.Pages.Common;

public partial class ErrorView : ComponentBase {
   [Parameter] public bool Loading { get; set; }
   [Parameter] public string? ErrorMessage { get; set; }
}