using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BankingBlazorSsr.Ui.Pages.Employee;

public partial class EmployeesList : IDisposable {

   [Inject] private IEmployeeClient EmployeeClient { get; set; } = default!;
   [Inject] private NavigationManager NavigationManager { get; set; } = default!;
   [Inject] private ILogger<EmployeesList> Logger { get; set; } = default!;

   private readonly CancellationTokenSource _cts = new();

   private List<EmployeeDto> _employeeDtos = [];

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   protected override async Task OnInitializedAsync() {
      Loading = true;
      ErrorMessage = null;

      try {
         var ct = _cts.Token;

         var result = await EmployeeClient.GetAllAsync(ct);
         if (result.IsFailure) {
            HandleError(result.Error!);
            return;
         }

         _employeeDtos = (result.Value ?? [])
            .OrderBy(e => e.Lastname)
            .ThenBy(e => e.Firstname)
            .ToList();
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("EmployeesList: request was cancelled");
      }
      finally {
         Loading = false;
      }
   }

   private void OpenEmployee(Guid employeeId) {
      if (employeeId == Guid.Empty) {
         ErrorMessage = "Employee Id is missing. Add `Id` to EmployeeDto and pass it from the API.";
         Logger.LogWarning("EmployeesList: OpenEmployee called with empty id. EmployeeDto currently has no Id.");
         return;
      }

      Logger.LogInformation("EmployeesList: nav: /employees/{EmployeeId}", employeeId);
      NavigationManager.NavigateTo($"/employees/{employeeId}");
   }
}

