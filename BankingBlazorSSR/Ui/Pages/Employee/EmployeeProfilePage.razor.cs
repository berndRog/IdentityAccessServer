using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace BankingBlazorSsr.Ui.Pages.Employee;

/// <summary>
/// EmployeeDto profile edit page.
/// Demonstrates form state handling, validation lifecycle,
/// navigation semantics (Back vs Cancel), and API error handling.
/// </summary>
public partial class EmployeeProfilePage : IDisposable {

   // ---- Dependency Injection ------------------------------------------------
   [Inject] private IEmployeeClient EmployeeClient { get; set; } = default!;
   [Inject] private NavigationManager Navigation { get; set; } = default!;
   [Inject] private ILogger<EmployeeProfilePage> Logger { get; set; } = default!;

   private readonly CancellationTokenSource _cts = new();

   public void Dispose() {
      _cts.Cancel();
      _cts.Dispose();
   }

   // ---- Navigation Context --------------------------------------------------
   // Optional return URL (passed via query string)
   // After Save or Leave navigation returns here instead of fixed route
   [Parameter, SupplyParameterFromQuery]
   public string? ReturnUrl { get; set; }

   // ---- UI State ------------------------------------------------------------
   private bool _saving;
   private bool _showGlobalErrors;
   private string? _saveError;
   private string? _saveOk;

   // ---- Form Model ----------------------------------------------------------
   private EmployeeDto _employeeDto = new();
   private EmployeeDto _originalEmployeeDto = new();
   private EditContext _editContext = default!;

   // -------------------------------------------------------------------------
   // Initialization
   // -------------------------------------------------------------------------
   protected override async Task OnInitializedAsync() {

      Loading = true;
      ErrorMessage = null;

      // Create initial EditContext so form can render immediately
      RebuildEditContext();

      try {
         var ct = _cts.Token;

         var result = await EmployeeClient.GetProfileAsync(ct);
         if (result.IsFailure) {
            HandleError(result.Error!);
            return;
         }

         _employeeDto = result.Value ?? new EmployeeDto();

         // Normalize loaded values so the form renders clean data
         _employeeDto.EmailString = _employeeDto.EmailString?.Trim() ?? string.Empty;
         _employeeDto.Firstname = _employeeDto.Firstname?.Trim() ?? string.Empty;
         _employeeDto.Lastname = _employeeDto.Lastname?.Trim() ?? string.Empty;
         _employeeDto.PersonnelNumber = _employeeDto.PersonnelNumber?.Trim() ?? string.Empty;

         _employeeDto.PhoneString = string.IsNullOrWhiteSpace(_employeeDto.PhoneString) ? null : _employeeDto.PhoneString.Trim();

         // Store snapshot for Cancel
         _originalEmployeeDto = Clone(_employeeDto);

         Logger.LogDebug("Loaded employee profile: {@e}", _employeeDto);

         // Recreate EditContext because model instance changed
         RebuildEditContext();
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("EmployeeProfilePage: request was cancelled");
      }
      finally {
         Loading = false;
      }
   }

   // -------------------------------------------------------------------------
   // Form Lifecycle
   // -------------------------------------------------------------------------
   private void RebuildEditContext() {
      if (_editContext != null)
         _editContext.OnValidationStateChanged -= ValidationChanged;

      _editContext = new EditContext(_employeeDto);
      _editContext.OnValidationStateChanged += ValidationChanged;
   }

   private void ValidationChanged(object? sender, ValidationStateChangedEventArgs e) {
      _showGlobalErrors = _editContext.GetValidationMessages().Any();
   }

   // -------------------------------------------------------------------------
   // Navigation semantics
   // -------------------------------------------------------------------------
   private void Cancel() {
      _employeeDto = Clone(_originalEmployeeDto);
      RebuildEditContext();
      _saveError = null;
      _saveOk = null;
      _showGlobalErrors = false;

      StateHasChanged();
   }

   private void GoBack() => Navigation.NavigateTo(ReturnUrl ?? "/employees");

   // -------------------------------------------------------------------------
   // Save operation
   // -------------------------------------------------------------------------
   private async Task SaveAsync() {
      _saving = true;
      _saveError = null;
      _saveOk = null;

      // Normalize required fields
      _employeeDto.EmailString = _employeeDto.EmailString?.Trim() ?? string.Empty;
      _employeeDto.Firstname = _employeeDto.Firstname?.Trim() ?? string.Empty;
      _employeeDto.Lastname = _employeeDto.Lastname?.Trim() ?? string.Empty;
      _employeeDto.PersonnelNumber = _employeeDto.PersonnelNumber?.Trim() ?? string.Empty;

      // Normalize optional fields
      _employeeDto.PhoneString = string.IsNullOrWhiteSpace(_employeeDto.PhoneString) ? null : _employeeDto.PhoneString.Trim();

      Logger.LogDebug("Save employee profile: {@Profile}", _employeeDto);

      if (!_editContext.Validate()) {
         _showGlobalErrors = true;
         _saving = false;
         return;
      }

      try {
         var ct = _cts.Token;
         var result = await EmployeeClient.UpdateProfileAsync(_employeeDto, ct);

         if (result.IsFailure) {
            var err = result.Error!;
            Logger.LogWarning("Save failed {s}: {t}", err.Status, err.Title);

            if (err.Status is 409 or 422) {
               _saveError = err.Detail ?? err.Title;
               return;
            }

            HandleError(err);
            return;
         }

         _employeeDto = result.Value ?? _employeeDto;
         RebuildEditContext();

         _saveOk = "Saved.";

         var id = _employeeDto.Id;
         if (id == Guid.Empty) {
            _saveError = "Saved, but server did not return an Id. Navigation to details is not possible.";
            Logger.LogWarning("Save succeeded but EmployeeDto.Id is empty.");
            return;
         }

         var target = $"/employees/{id}";
         Logger.LogInformation("EmployeeProfilePage: navigate to {Target}", target);

         await InvokeAsync(StateHasChanged);
         Navigation.NavigateTo(target, forceLoad: true);
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("EmployeeProfilePage: save cancelled");
      }
      finally {
         _saving = false;
      }
   }

   // -------------------------------------------------------------------------
   // Helper
   // -------------------------------------------------------------------------
   private static EmployeeDto Clone(EmployeeDto src) => new() {
      Id = src.Id,
      Firstname = src.Firstname,
      Lastname = src.Lastname,
      EmailString = src.EmailString,
      PhoneString = src.PhoneString,
      PersonnelNumber = src.PersonnelNumber,
      IsActive = src.IsActive,
      AdminRights = src.AdminRights
   };
}
