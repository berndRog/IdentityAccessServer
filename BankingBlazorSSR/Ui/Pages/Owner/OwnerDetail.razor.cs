// using BankingBlazorSsr.Api;
// using BankingBlazorSsr.Core;
// using BankingBlazorSsr.Core.Dto;
// using Microsoft.AspNetCore.Components;
// namespace BankingBlazorSsr.Ui.Pages.Owner;
//
// public partial class OwnerDetail(
//    IOwnerClient ownerClient,
//    IAccountClient accountClient,
//    UserStateHolder userStateHolder,
//    NavigationManager navigationManager,
//    ILogger<OwnerDetail> logger
// ){
//    [Parameter] public Guid Id { get; set; }
//
//    private OwnerDto? _ownerDto;
//    private List<AccountDto>? _accountDtos;
//    private string? _errorMessage = null;
//    
//    protected override async Task OnInitializedAsync() {
//       
//       logger.LogInformation("OwnerDetail: OnInitializedAsync Id: {1}",Id);
//       if (!userStateHolder.IsAuthenticated) {
//          _errorMessage = "Kontoinhaber ist nicht angemeldet!";
//          return;
//       }
//       if(userStateHolder.OwnerDto?.Id != Id) {
//          _errorMessage = "Id Kontoinhaber stimmt nicht überein!";
//          return;
//       }
//
//       switch (await ownerClient.GetById(Id)) {
//          case ResultData<OwnerDto?>.Success sucess:
//             logger.LogInformation("OwnerDetail: GetById: {1}", sucess.Data);
//             _ownerDto = sucess.Data!;
//             break;
//          case ResultData<OwnerDto?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }
//
//       switch (await accountClient.GetAllByOwner(Id)) {
//          case ResultData<IEnumerable<AccountDto>?>.Success sucess:
//             logger.LogInformation("OwnerDetail: GetAccoutsByOwnerId: {1}", sucess.Data);
//             _accountDtos = sucess.Data!.ToList();
//             break;
//          case ResultData<IEnumerable<AccountDto>?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }
//    }
//
//    private void OpenAccount(Guid accountId) {
//       var iban = _accountDtos?.FirstOrDefault(a => a.Id == accountId)?.Iban;
//       logger.LogInformation("OwnerDetail: nav: /accounts/iban/{1}", iban);
//       navigationManager.NavigateTo($"/accounts/iban/{iban}");
//    }
//    
//    private void LeaveForm() {
//       navigationManager.NavigateTo("/home");
//    }
//
//
// }