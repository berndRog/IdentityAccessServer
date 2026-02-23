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
   
   [Parameter, SupplyParameterFromQuery]
   public string? ReturnUrl { get; set; }
   
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
            HandleError(result.Error!);
            return;
         }

         logger.LogInformation("AccountsList: GetAll");

         _accountDtos = (result.Value ?? [])
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
         await InvokeAsync(StateHasChanged);
      }
   }

   private void GoBack() {
      if (!string.IsNullOrWhiteSpace(ReturnUrl)) {
         navigationManager.NavigateTo(ReturnUrl);
         return;
      }

      navigationManager.NavigateTo("javascript:history.back()", forceLoad: true);
   }

   private void OpenAccount(Guid accountId) {
      var target = $"/accounts/{accountId}";
      if (!string.IsNullOrWhiteSpace(ReturnUrl))
         target += $"?returnUrl={Uri.EscapeDataString(navigationManager.Uri)}";

      logger.LogInformation("AccountsList: nav: {Target}", target);
      navigationManager.NavigateTo(target);
   }
}