using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Common;
using BankingBlazorSsr.Ui.Pages.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BankingBlazorSsr.Ui.Pages.Employee;

public partial class EmployeeDetail : IDisposable {

   [Inject] private IEmployeeClient EmployeeClient { get; set; } = default!;
   [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
   [Inject] private NavigationManager NavigationManager { get; set; } = default!;
   [Inject] private ILogger<EmployeeDetail> Logger { get; set; } = default!;

   [Parameter] public Guid Id { get; set; }

   private readonly CancellationTokenSource _cts = new();

   private bool _activating;
   private bool _canActivateEmployee;
   private string? _activateMessage;
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

         var authState = await AuthStateProvider.GetAuthenticationStateAsync();
         var currentUserRights = AdminRightsHelper.GetAdminRights(authState.User);
         _canActivateEmployee = AdminRightsHelper.HasManageEmployeesRight(currentUserRights);

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

   private async Task ActivateAsync() {
      if (_employeeDto.Id == Guid.Empty) {
         ErrorMessage = "Aktivierung nicht möglich, weil keine Mitarbeiter-Id vorhanden ist.";
         return;
      }

      _activating = true;
      ErrorMessage = null;
      _activateMessage = null;

      try {
         var ct = _cts.Token;
         var result = await EmployeeClient.PostActivateAsync(_employeeDto.Id, ct);

         if (result.IsFailure) {
            var err = result.Error!;
            if (err.Status is 403 or 409 or 422)
               ErrorMessage = err.Detail ?? err.Title;
            else
               HandleError(err);

            return;
         }

         _employeeDto.IsActive = true;
         _activateMessage = "Mitarbeiter wurde aktiviert.";
         await InvokeAsync(StateHasChanged);
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("EmployeeDetail: activation cancelled");
      }
      finally {
         _activating = false;
      }
   }
}

