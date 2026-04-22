using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Pages.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

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
   [Parameter, SupplyParameterFromQuery]
   public bool Onboarding { get; set; }

   // ---- UI State ------------------------------------------------------------
   private bool _activating;
   private bool _saving;
   private bool _showGlobalErrors;
   private bool _onboardingCompleted;
   private string? _saveError;
   private string? _saveOk;
   private string? _activationInfo;

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
      _onboardingCompleted = false;
      _saveError = null;
      _saveOk = null;
      _activationInfo = null;
      _showGlobalErrors = false;

      StateHasChanged();
   }

   private void GoBack() => Navigation.NavigateTo(ReturnUrl ?? "/employees");

   private void FinishOnboarding() {
      var target = _employeeDto.IsActive ? "/employee" : "/";
      Navigation.NavigateTo(target, forceLoad: true);
   }

   private bool CanSelfActivate
      => _onboardingCompleted
         && !_employeeDto.IsActive
         && AdminRightsHelper.HasManageEmployeesRight(_employeeDto.AdminRights);

   // -------------------------------------------------------------------------
   // Save operation
   // -------------------------------------------------------------------------
   private async Task SaveAsync() {
      _saving = true;
      _onboardingCompleted = false;
      _saveError = null;
      _saveOk = null;
      _activationInfo = null;

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
          _originalEmployeeDto = Clone(_employeeDto);
         RebuildEditContext();

          _saveOk = Onboarding
             ? "Profil gespeichert."
             : "Saved.";

          if (Onboarding) {
             _onboardingCompleted = true;
             _activationInfo = BuildActivationInfo(_employeeDto);
             Logger.LogInformation("EmployeeProfilePage: onboarding completed, active={IsActive}, rights={AdminRights}",
                _employeeDto.IsActive, _employeeDto.AdminRights);
             await InvokeAsync(StateHasChanged);
             return;
          }

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

   private async Task ActivateAsync() {
      if (_employeeDto.Id == Guid.Empty) {
         _saveError = "Aktivierung nicht möglich, weil keine Mitarbeiter-Id vorhanden ist.";
         return;
      }

      _activating = true;
      _saveError = null;
      _saveOk = null;

      try {
         var ct = _cts.Token;
         var result = await EmployeeClient.PostActivateAsync(_employeeDto.Id, ct);

         if (result.IsFailure) {
            var err = result.Error!;
            if (err.Status is 409 or 422 or 403)
               _saveError = err.Detail ?? err.Title;
            else
               HandleError(err);

            return;
         }

         _employeeDto.IsActive = true;
         _originalEmployeeDto = Clone(_employeeDto);
         _saveOk = "Mitarbeiter aktiviert.";
         _activationInfo = BuildActivationInfo(_employeeDto);
         await InvokeAsync(StateHasChanged);
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("EmployeeProfilePage: activation cancelled");
      }
      finally {
         _activating = false;
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

   private static string BuildActivationInfo(EmployeeDto employee) {
      if (employee.IsActive)
         return "Der Mitarbeiter ist aktiviert. Das Onboarding ist abgeschlossen und der Mitarbeiterbereich kann jetzt genutzt werden.";

      return AdminRightsHelper.HasManageEmployeesRight(employee.AdminRights)
         ? "Das Profil ist gespeichert. Dieser Mitarbeiter verfügt über Mitarbeiterverwaltungsrechte. Die fachliche Aktivierung muss jetzt noch durchgeführt werden, bevor der Mitarbeiterbereich nutzbar ist."
         : "Das Profil ist gespeichert. Die Aktivierung muss nun durch einen anderen berechtigten Mitarbeiter mit Mitarbeiterverwaltungsrechten vorgenommen werden.";
   }
}
