// using BankingBlazorSsr.Api;
// using BankingBlazorSsr.Core;
// using BankingBlazorSsr.Core.Dto;
// using BankingBlazorSsr.Ui.Pages.Account;
// using Microsoft.AspNetCore.Components;
// namespace BankingBlazorSsr.Ui.Pages.Account;
//
// public partial class AccountById(
//    IAccountClient accountClient,
//    IBeneficiaryClient beneficiaryClient,
//    UserStateHolder userStateHolder,
//    NavigationManager navigationManager,
//    ILogger<AccountByIban> logger
// ): ComponentBase {
//    [Parameter] public required Guid Id { get; set; }
//
//    private OwnerDto? _ownerDto = null;
//    private AccountDto? _accountDto = null;
//    private List<BeneficiaryDto> _beneficiaryDtos = [];
//    private string? _errorMessage = null;
//
//    protected override async Task OnInitializedAsync() {
//       
//       logger.LogInformation("AccountbyId: OnInitializedAsync Id: {1}",Id);
//       if (!userStateHolder.IsAuthenticated) {
//          _errorMessage = "Kontoinhaber ist nicht angemeldet!";
//          return;
//       }
//       if(userStateHolder.OwnerDto == null) {
//          _errorMessage = "Kontoinhaber ist nicht korrekt angemeldet!";
//          return;
//       }
//       _ownerDto = userStateHolder.OwnerDto!;
//       
//       switch (await accountClient.GetById(Id)) {
//          case ResultData<AccountDto?>.Success sucess:
//             logger.LogInformation("AccountDetail: GetByIban: {1}", sucess.Data);
//             _accountDto = sucess.Data!;
//             break;
//          case ResultData<AccountDto?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }
//       
//       switch (await beneficiaryClient.GetByAccount(_accountDto!.Id)) {
//          case ResultData<IEnumerable<BeneficiaryDto>?>.Success sucess:
//             logger.LogInformation("AccountDetail: GetBeneficiariesByAccountId: {1}", sucess.Data);
//             _beneficiaryDtos = sucess.Data!.ToList();
//             break;
//          case ResultData<IEnumerable<BeneficiaryDto>?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }
//    }
//    
//    private void LeaveForm() {
//       navigationManager.NavigateTo("/home");
//    }
//    
// }