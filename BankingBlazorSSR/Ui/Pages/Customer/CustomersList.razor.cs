using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;

namespace BankingBlazorSsr.Ui.Pages.Customer;

public partial class CustomersList : BasePage, IDisposable {

   [Inject] private ICustomerClient CustomerClient { get; set; } = default!;
   [Inject] private NavigationManager NavigationManager { get; set; } = default!;
   [Inject] private ILogger<CustomersList> Logger { get; set; } = default!;

   [Parameter, SupplyParameterFromQuery]
   public string? ReturnUrl { get; set; }

   private readonly CancellationTokenSource _cts = new();

   private List<CustomerDto> _customerDtos = [];

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   protected override async Task OnInitializedAsync() {
      Loading = true;
      ErrorMessage = null;

      try {
         var ct = _cts.Token;

         var result = await CustomerClient.GetAllAsync(ct);
         if (result.IsFailure) {
            HandleError(result.Error!);
            return;
         }

         _customerDtos = (result.Value ?? [])
            .OrderBy(o => o.Lastname)
            .ThenBy(o => o.Firstname)
            .ToList();
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("CustomersList: request was cancelled");
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

   private void OpenCustomer(Guid customerId) {
      var target = $"/customers/{customerId}";
      if (!string.IsNullOrWhiteSpace(ReturnUrl))
         target += $"?returnUrl={Uri.EscapeDataString(NavigationManager.Uri)}";

      Logger.LogInformation("CustomersList: nav: {Target}", target);
      NavigationManager.NavigateTo(target);
   }
}