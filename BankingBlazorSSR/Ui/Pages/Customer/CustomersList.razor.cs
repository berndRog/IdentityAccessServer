using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
namespace BankingBlazorSsr.Ui.Pages.Customer;

public partial class CustomersList : BasePage, IDisposable {

   [Inject] private ICustomerClient CustomerClient { get; set; } = default!;
   [Inject] private NavigationManager NavigationManager { get; set; } = default!;
   [Inject] private ILogger<CustomersList> Logger { get; set; } = default!;

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

   private void OpenCustomer(Guid customerId) {
      Logger.LogInformation("CustomersList: nav: /customers/{CustomerId}", customerId);
      NavigationManager.NavigateTo($"/customers/{customerId}");
   }
}