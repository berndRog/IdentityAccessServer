// using BankingBlazorSsr.Core;
// using BankingBlazorSsr.Core.Dto;
// using BankingBlazorSsr.Ui.Models;
// using Microsoft.AspNetCore.Components;
// namespace BankingBlazorSsr.Ui.Pages.Beneficiary;
//
// public partial class BeneficiaryCreate(
//    ICustomerClient ownerClient,
//    IAccountClient accountClient,
//    IBeneficiaryClient beneficiaryClient,
//    NavigationManager navigationManager,
//    ILogger<BeneficiaryCreate> logger
// ): ComponentBase {
//    
//    [Parameter] public required Guid AccountId { get; set; }
//    
//    private readonly BeneficiaryCreateModel _beneficiaryCreate = new();
//    private string? _errorMessage = null;
//
//    private CustomerDto? _customerDto = default!;
//    private AccountDto? _accountDto = null;
//    private BeneficiaryDto? _beneficiaryDto = null;
//    
//    private async Task HandleSubmit() {
//       
//       logger.LogInformation("BeneficiaryCreate: HandleSubmit() {1} {2} {3}", 
//          _beneficiaryCreate.Firstname, _beneficiaryCreate.Lastname, _beneficiaryCreate.IbanString);
//       
//       switch (await ownerClient.GetByName($"{_beneficiaryCreate.Firstname} {_beneficiaryCreate.Lastname}")) {
//          case ResultData<IEnumerable<CustomerDto?>>.Success success:
//             logger.LogInformation("BeneficiaryCreate: GetByName() success");
//             if(success.Data!.Count() == 1) {
//                _customerDto = success.Data!.FirstOrDefault();               
//             } 
//             else if(success.Data!.Count() > 1) 
//                _errorMessage = $"Es gibt mehr einen CustomerDto {success.Data}";
//             break;
//          case ResultData<IEnumerable<CustomerDto>?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }
//       
//       switch (await accountClient.GetByIban(_beneficiaryCreate.IbanString)) {
//          case ResultData<AccountDto?>.Success sucess:
//             logger.LogInformation("BeneficiaryCreate: GetAccountByIban: {1}", sucess.Data);
//             _accountDto = sucess.Data!;
//             break;
//          case ResultData<AccountDto?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }
//       _beneficiaryDto = new BeneficiaryDto(
//          Id: Guid.NewGuid(),
//          FirstName: _beneficiaryCreate.Firstname,
//          LastName: _beneficiaryCreate.Lastname,
//          IbanString: _beneficiaryCreate.IbanString,
//          AccountId: AccountId
//       );
//
//       switch (await beneficiaryClient.Post(AccountId, _beneficiaryDto)) {
//          case ResultData<BeneficiaryDto?>.Success sucess:
//             logger.LogInformation("BeneficiaryCreate: PostBeneficiary: {1}", _beneficiaryDto);
//             break;
//          case ResultData<BeneficiaryDto?>.Error error:
//             _errorMessage = error.Exception.Message;
//             return;
//       }
//       navigationManager.NavigateTo($"/accounts/{AccountId}");
//    
//    }
//
//    private void LeaveForm() {
//       // Implementiere die Navigation zurück
//       
//    }
//
//    private void CancelOperation() {
//       // Implementiere die Abbruch-Logik
//    }
//    
// }