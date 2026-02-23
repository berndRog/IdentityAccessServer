using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BankingBlazorSsr.Ui.Pages.Employee;

public partial class EmployeeDetail : IDisposable {

   [Inject] private IEmployeeClient EmployeeClient { get; set; } = default!;
   [Inject] private NavigationManager NavigationManager { get; set; } = default!;
   [Inject] private ILogger<EmployeeDetail> Logger { get; set; } = default!;

   [Parameter] public Guid Id { get; set; }

   private readonly CancellationTokenSource _cts = new();

   private EmployeeDto _employeeDto = new();

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   protected override async Task OnInitializedAsync() {
      Logger.LogInformation("EmployeeDetail: OnInitializedAsync Id: {Id}", Id);

      Loading = true;
      ErrorMessage = null;

      try {
         var ct = _cts.Token;

         var resultEmployee = await EmployeeClient.GetByIdAsync(Id, ct);
         if (resultEmployee.IsFailure) {
            HandleError(resultEmployee.Error!);
            return;
         }

         _employeeDto = resultEmployee.Value ?? new EmployeeDto();
         Logger.LogDebug("Loaded employee: {@Employee}", _employeeDto);
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("EmployeeDetail: request was cancelled");
      }
      catch (Exception ex) {
         ErrorMessage = "Unexpected error while loading employee details.";
         Logger.LogError(ex, "EmployeeDetail: unexpected error for Id {Id}", Id);
      }
      finally {
         Loading = false;
         await InvokeAsync(StateHasChanged);
      }
   }

   private void LeaveForm() {
      NavigationManager.NavigateTo("/home");
   }
}

