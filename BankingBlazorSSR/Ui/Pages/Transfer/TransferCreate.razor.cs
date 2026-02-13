// using BankingBlazorSsr.Api;
// using BankingBlazorSsr.Core;
// using BankingBlazorSsr.Core.Dto;
// using Microsoft.AspNetCore.Components;
// namespace BankingBlazorSsr.Ui.Pages.Transfer;
//
// public partial class TransferCreate(
//    IAccountClient accountClient,
//    IBeneficiaryClient beneficiaryClient,
//    UserStateHolder userStateHolder,
//    NavigationManager navigationManager,
//    ILogger<TransferCreate> logger
// ): ComponentBase {
//
//    [Parameter] public required Guid AccountId { get; set; }
//
//    private OwnerDto? _ownerDto = null;
//    private AccountDto? _accountDto = null;
//    private List<AccountDto> _accountDtos = [];
//    private List<BeneficiaryDto>? _beneficiaryDtos = null;
//    private string? _errorMessage = null;
//    
//    
//    protected override async Task OnInitializedAsync() {
//
//       //logger.LogInformation("OwnerDetail: OnInitializedAsync Id: {1}", Id);
//       if (!userStateHolder.IsAuthenticated) {
//          _errorMessage = "Kontoinhaber ist nicht angemeldet!";
//          return;
//       }
//       _ownerDto = userStateHolder.OwnerDto!;
//
//       switch (await accountClient.GetById(AccountId)) {
//          case ResultData<AccountDto?>.Success sucess:
//             logger.LogInformation("TransferCreate: GetAccountById: {1}", sucess.Data);
//             _accountDto = sucess.Data!;
//             break;
//          case ResultData<AccountDto?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }  
//       switch (await beneficiaryClient.GetByAccount(_accountDto!.Id)) {
//          case ResultData<IEnumerable<BeneficiaryDto>?>.Success success:
//             logger.LogInformation("TransferCreate: GetBeneficiariesByAccountId() success");
//             _beneficiaryDtos = success.Data!.ToList();
//             break;
//          case ResultData<IEnumerable<BeneficiaryDto>?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }
//       
//    }
//    
//    private void CreateTransfer(Guid id) {
//       logger.LogInformation("TransferCreate: nav: accounts/{1}/beneficiaries/{2}/transfers/create", _accountDto!.Id, id);
//       navigationManager.NavigateTo($"/accounts/{_accountDto!.Id}/beneficiaries/{id}/transfers/create"); 
//    }
//
//    private void CreateBeneficiary() {
//       logger.LogInformation("TransferCreate: nav: /accounts/{1}/beneficiaries/create", _accountDto!.Id);
//       navigationManager.NavigateTo($"/accounts/{_accountDto!.Id}/beneficiaries/create");
//    }
// }