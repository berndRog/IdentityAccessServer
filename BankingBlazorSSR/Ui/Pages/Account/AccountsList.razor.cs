using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
namespace BankingBlazorSsr.Ui.Pages.Account;

public partial class AccountsList(
   IAccountClient accountClient,
   NavigationManager navigationManager,
   ILogger<AccountsList> logger
) : BasePage, IDisposable {
   private readonly CancellationTokenSource _cts = new();

   private List<AccountDto> _accountDtos = [];

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   protected override async Task OnInitializedAsync() {
      Loading = true;
      ErrorMessage = null;

      try {
         var ct = _cts.Token;

         var result = await accountClient.GetAllAccountsAsync(ct);
         if (result.IsFailure) {
            ErrorMessage = result.Error?.Title ?? "Request failed";
            return;
         }

         logger.LogInformation("AccountsList: GetAll");

         _accountDtos = result.Value!
            .OrderBy(a => a.IbanString)
            .ToList();
      }
      catch (OperationCanceledException) {
         logger.LogDebug("AccountsList: request was cancelled");
      }
      catch (Exception ex) {
         ErrorMessage = "Unexpected error while loading accounts.";
         logger.LogError(ex, "AccountsList: unexpected error");
      }
      finally {
         Loading = false;
         ErrorMessage = null;
         await InvokeAsync(StateHasChanged);
      }
   }

   private void OpenAccount(Guid accountId) {
      logger.LogInformation("AccountsList: nav: /accounts/{AccountId}", accountId);
      navigationManager.NavigateTo($"/accounts/{accountId}");
   }
}