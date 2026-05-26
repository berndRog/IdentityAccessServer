using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using Microsoft.AspNetCore.Components;
namespace BankingBlazorSsr.Ui.Pages.Employee;

public partial class EmployeesList : IDisposable {

   [Inject] private IEmployeeClient EmployeeClient { get; set; } = default!;
   [Inject] private NavigationManager NavigationManager { get; set; } = default!;
   [Inject] private IConfiguration Configuration { get; set; } = default!;
   [Inject] private ILogger<EmployeesList> Logger { get; set; } = default!;

   [Parameter, SupplyParameterFromQuery]
   public string? ReturnUrl { get; set; }

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

   private void GoBack() {
      if (!string.IsNullOrWhiteSpace(ReturnUrl)) {
         NavigationManager.NavigateTo(ReturnUrl);
         return;
      }

      // Fallback: browser history
      NavigationManager.NavigateTo("javascript:history.back()", forceLoad: true);
   }

   private void OpenEmployee(Guid employeeId) {
      // if (employeeId == Guid.Empty) {
      //    ErrorMessage = "Employee Id is missing. Add `Id` to EmployeeDto and pass it from the API.";
      //    Logger.LogWarning("EmployeesList: OpenEmployee called with empty id. EmployeeDto currently has no Id.");
      //    return;
      // }

      var target = $"/employees/{employeeId}";
      if (!string.IsNullOrWhiteSpace(ReturnUrl))
         target += $"?returnUrl={Uri.EscapeDataString(NavigationManager.Uri)}";

      Logger.LogInformation("EmployeesList: nav: {Target}", target);
      NavigationManager.NavigateTo(target);
   }

   private void OpenRegisterEmployee() {
      var authority = Configuration["IdentityAccessServer:Authority"]?.TrimEnd('/');
      if (string.IsNullOrWhiteSpace(authority)) {
         ErrorMessage = "IdentityAccessServer:Authority is not configured.";
         Logger.LogWarning("EmployeesList: IdentityAccessServer:Authority is missing.");
         return;
      }

      var target = $"{authority}/Identity/Account/RegisterEmployee";
      Logger.LogInformation("EmployeesList: nav: {Target}", target);
      NavigationManager.NavigateTo(target, forceLoad: true);
   }
}
