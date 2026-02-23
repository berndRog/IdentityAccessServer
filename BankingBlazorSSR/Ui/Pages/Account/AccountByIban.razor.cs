using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
namespace BankingBlazorSsr.Ui.Pages.Account;

public partial class AccountByIban(
   IAccountClient accountClient,
   NavigationManager navigationManager,
   ILogger<AccountByIban> logger
): BasePage, IDisposable {
   [Parameter] public required string IbanString { get; set; }

   private readonly CancellationTokenSource _cts = new();

   private CustomerDto? _customerDto;
   private BankingBlazorSsr.Api.Dtos.AccountDto? _accountDto;

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   protected override async Task OnInitializedAsync() {
      // BasePage state
      Loading = true;
      ErrorMessage = null;

      try {
         var ct = _cts.Token;

         _customerDto = CommonUser.CustomerDto;

         logger.LogInformation("AccountDetail: OnInitializedAsync IbanString: {Iban}", IbanString);

         var resultAccount = await accountClient.GetAccountByIbanAsync(IbanString, ct);
         if (resultAccount.IsFailure) {
            HandleError(resultAccount.Error!);
            return;
         }

         _accountDto = resultAccount.Value!;
         logger.LogInformation("AccountDetail loaded account {AccountId}", _accountDto.Id);
      }
      catch (OperationCanceledException) {
         // expected when navigating away / disposing
         logger.LogDebug("AccountDetail: request was cancelled");
      }
      catch (Exception ex) {
         ErrorMessage = "Unexpected error while loading account data.";
         logger.LogError(ex, "AccountDetail: unexpected error for IBAN {Iban}", IbanString);
      }
      finally {
         Loading = false;
         await InvokeAsync(StateHasChanged);
      }
   }

   private void HandleTransfers() {
      if (_accountDto is null) return;

      logger.LogInformation("AccountDetail: HandleTransfers navigate to /accounts/{AccountId}/transfers/create", _accountDto.Id);
      navigationManager.NavigateTo($"/accounts/{_accountDto.Id}/transfers/create");
   }

   private void HandleTransactions() {
      if (_accountDto is null) return;

      logger.LogInformation("AccountDetail: HandleTransactions navigate to /accounts/{AccountId}/transactions/create", _accountDto.Id);
      navigationManager.NavigateTo($"/accounts/{_accountDto.Id}/transactions/create");
   }

   private void LeaveForm() {
      navigationManager.NavigateTo("/home");
   }
}