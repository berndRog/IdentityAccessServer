using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
namespace BankingBlazorSsr.Ui.Pages.Customer;

public partial class CustomerDetail : IDisposable {

   [Inject] private IAccountClient AccountClient { get; set; } = default!;
   [Inject] private ICustomerClient CustomerClient { get; set; } = default!;
   [Inject] private NavigationManager NavigationManager { get; set; } = default!;
   [Inject] private ILogger<CustomerDetail> logger { get; set; } = default!;

   [Parameter] public Guid Id { get; set; }

   private readonly CancellationTokenSource _cts = new();

   private CustomerDto _customerDto = new();
   private List<BankingBlazorSsr.Api.Dtos.AccountDto> _accountDtos = [];

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   protected override async Task OnInitializedAsync() {
      logger.LogInformation("CustomerDetail: OnInitializedAsync Id: {Id}", Id);

      Loading = true;
      ErrorMessage = null;

      try {
         var ct = _cts.Token;

         var resultCustomer = await CustomerClient.GetByIdAsync(Id, ct);
         if (resultCustomer.IsFailure) {
            HandleError(resultCustomer.Error!);
            return;
         }

         _customerDto = resultCustomer.Value!;
         CommonUser.CustomerDto = _customerDto;
         logger.LogDebug("Loaded customer: {@Customer}", _customerDto);

         // Note: method name says OwnerId but we pass the customer id here; keep as-is until API is renamed.
         var resultAccounts = await AccountClient.GetAccountsByOwnerIdAsync(customerId: Id, ct);
         if (resultAccounts.IsFailure) {
            HandleError(resultAccounts.Error!);
            return;
         }

         _accountDtos = (resultAccounts.Value ?? []).ToList();
         logger.LogDebug("Loaded accounts: {@Accounts}", _accountDtos);
      }
      catch (OperationCanceledException) {
         logger.LogDebug("CustomerDetail: request was cancelled");
      }
      catch (Exception ex) {
         ErrorMessage = "Unexpected error while loading customer details.";
         logger.LogError(ex, "CustomerDetail: unexpected error for Id {Id}", Id);
      }
      finally {
         Loading = false;
         await InvokeAsync(StateHasChanged);
      }
   }

   private void OpenAccount(Guid accountId) {
      var iban = _accountDtos.FirstOrDefault(a => a.Id == accountId)?.IbanString;
      if (string.IsNullOrWhiteSpace(iban)) {
         logger.LogWarning("CustomerDetail: OpenAccount({AccountId}) but IBAN not found in loaded accounts.", accountId);
         return;
      }

      logger.LogInformation("CustomerDetail: nav: /accounts/iban/{Iban}", iban);
      NavigationManager.NavigateTo($"/accounts/iban/{iban}");
   }

   private void LeaveForm() {
      NavigationManager.NavigateTo("/home");
   }

}