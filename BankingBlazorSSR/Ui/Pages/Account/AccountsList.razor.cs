// using BankingBlazorSsr.Api;
// using BankingBlazorSsr.Core;
// using BankingBlazorSsr.Core.Dto;
// using Microsoft.AspNetCore.Components;
// namespace BankingBlazorSsr.Ui.Pages.Account;
//
// public partial class AccountsList(
//    IAccountClient accountClient,
//    UserStateHolder userStateHolder,
//    NavigationManager navigationManager,
//    ILogger<AccountsList> logger
// ) : ComponentBase {
//    private List<AccountDto> _accountDtos = [];
//    private string? _errorMessage = null;
//
//    protected override async Task OnInitializedAsync() {
//       if (!userStateHolder.IsAuthenticated) {
//          _errorMessage = "Admin ist nicht angemeldet!";
//          return;
//       }
//       var result = await accountClient.GetAll();
//       if (result is null) {
//          _errorMessage = "No response.";
//          return;
//       }
//       if (result.IsSuccess) {
//          logger.LogInformation("OwnerList: GetAll");
//          _accountDtos = result.Value
//             .OrderBy(a => a.Iban)
//             .ToList();
//          return;
//       }
//       _errorMessage = result.Error.Title;
//    }
//
//    private void OpenAccount(Guid accountId) {
//       logger.LogInformation("OwnerList: nav: /accounts/{1}", accountId);
//       navigationManager.NavigateTo($"/accounts/{accountId}");
//    }
// }