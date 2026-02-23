using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Api.Errors;
using BankingBlazorSsr.Core;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
namespace BankingBlazorSsr.Ui.Pages.Account;

public partial class AccountById(
   IAccountClient accountClient,
   NavigationManager navigationManager,
   ILogger<AccountById> logger
) : BasePage, IDisposable {
   [Parameter] public required Guid Id { get; set; }

   private readonly CancellationTokenSource _cts = new();

   private CustomerDto? _customerDto;
   private BankingBlazorSsr.Api.Dtos.AccountDto? _accountDto;
   private List<BeneficiaryDto> _beneficiaryDtos = [];

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   protected override async Task OnInitializedAsync() {
      Loading = true;
      ErrorMessage = null;

      try {
         var ct = _cts.Token;

         _customerDto = CommonUser.CustomerDto;
         
         logger.LogInformation("AccountDetail: OnInitializedAsync Id: {Id}", Id);
         var resultAccount = await accountClient.GetAccountByIdAsync(Id, ct);
         if (resultAccount.IsFailure) {
            SetErrorAndLog("GetAccountByIdAsync failed", resultAccount.Error);
            return;
         }

         _accountDto = resultAccount.Value!;
         logger.LogInformation("AccountDetail loaded account {AccountId}", _accountDto.Id);

         var resultBeneficiaries =
            await accountClient.GetBeneficiariesByAcountIdAsync(_accountDto.Id, ct);
         if (resultBeneficiaries.IsFailure) {
            SetErrorAndLog("GetBeneficiariesByAcountIdAsync failed", resultBeneficiaries.Error);
            return;
         }

         _beneficiaryDtos = resultBeneficiaries.Value?.ToList() ?? [];
      }
      catch (OperationCanceledException) {
         // expected when navigating away / disposing
         logger.LogDebug("AccountDetail: request was cancelled");
      }
      catch (Exception ex) {
         ErrorMessage = "Unexpected error while loading account data.";
         logger.LogError(ex, "AccountDetail: unexpected error for Id {Id}", Id);
      }
      finally {
         Loading = false;
         await InvokeAsync(StateHasChanged);
      }
   }

   private void SetErrorAndLog(string message, ApiError? error) {
      Loading = false;

      var title = error?.Title ?? "Request failed";
      var detail = error?.Detail ?? "No further details.";
      ErrorMessage = $"{title}\n{detail}";

      logger.LogWarning("AccountDetail: {Message}. Title={Title}; Detail={Detail}", message, title, detail);
   }

   private void LeaveForm() => navigationManager.NavigateTo("/home");
}