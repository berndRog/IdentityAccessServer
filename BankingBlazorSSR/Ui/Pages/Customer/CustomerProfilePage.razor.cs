using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
namespace BankingBlazorSsr.Ui.Pages.Customer;

/// <summary>
/// CustomerDto profile edit page.
/// Demonstrates form state handling, validation lifecycle,
/// navigation semantics (Back vs Cancel), and API error handling.
/// </summary>
public partial class CustomerProfilePage : IDisposable {

   // ---- Dependency Injection ------------------------------------------------
   [Inject] private ICustomerClient CustomerClient { get; set; } = default!;
   [Inject] private NavigationManager Navigation { get; set; } = default!;
   [Inject] private ILogger<CustomerProfilePage> Logger { get; set; } = default!;

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
   // Current editable model
   private CustomerDto _customerDto = new();
   // Snapshot of original state (used for Cancel)
   private CustomerDto _originalCustomerDto = new();
   // Blazor form state manager
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

         // Load profile from API (Result pattern)
         var result = await CustomerClient.GetProfileAsync(ct);
         if (result.IsFailure) {
            HandleError(result.Error!);
            return;
         }

         _customerDto = result.Value ?? new CustomerDto();

         // Store snapshot for Cancel
         _originalCustomerDto = Clone(_customerDto);

         Logger.LogDebug("Loaded customer profile: {c}", _customerDto);

         // Recreate EditContext because model instance changed
         RebuildEditContext();
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("CustomerProfilePage: request was cancelled");
      }
      finally {
         Loading = false;
      }
   }


   // -------------------------------------------------------------------------
   // Form Lifecycle
   // -------------------------------------------------------------------------
   /// <summary>
   /// Recreates EditContext when model instance changes.
   /// Important: Validation state belongs to EditContext, not the model.
   /// </summary>
   private void RebuildEditContext() {
      if (_editContext != null)
         _editContext.OnValidationStateChanged -= ValidationChanged;

      _editContext = new EditContext(_customerDto);
      _editContext.OnValidationStateChanged += ValidationChanged;
   }

   private void ValidationChanged(object? sender, ValidationStateChangedEventArgs e) {
      _showGlobalErrors = _editContext.GetValidationMessages().Any();
   }
   
   // -------------------------------------------------------------------------
   // Navigation semantics
   // -------------------------------------------------------------------------
   /// <summary>
   /// Cancel = discard changes and stay in application context.
   /// No persistence operation.
   /// </summary>
   private void Cancel() {
      _customerDto = Clone(_originalCustomerDto);
      RebuildEditContext();
      _saveError = null;
      _saveOk = null;
      _showGlobalErrors = false;

      StateHasChanged();
   }

   /// <summary>
   /// Leave = navigate away from page.
   /// Uses return URL if available.
   /// </summary>
   private void GoBack() => Navigation.NavigateTo(ReturnUrl ?? "/customers");


   // -------------------------------------------------------------------------
   // Save operation
   // -------------------------------------------------------------------------
   /// <summary>
   /// Validates form, sends update to API and handles domain/API errors.
   /// </summary>
   private async Task SaveAsync() {
      _saving = true;
      _saveError = null;
      _saveOk = null;

      Logger.LogDebug("Save customer profile: {@Profile}", _customerDto);

      // Prevent API call if invalid
      if (!_editContext.Validate()) {
         _showGlobalErrors = true;
         _saving = false;
         return;
      }

      try {
         var ct = _cts.Token;
         var result = await CustomerClient.UpdateProfileAsync(_customerDto, ct);

         if (result.IsFailure) {
            var err = result.Error!;
            Logger.LogWarning("Save failed {s}: {t}", err.Status, err.Title);

            // Business validation errors stay on page
            if (err.Status is 409 or 422) {
               _saveError = err.Detail ?? err.Title;
               return;
            }

            // Authentication / authorization / not found handled globally
            HandleError(err);
            return;
         }

         // Success: API returned updated entity
         _customerDto = result.Value ?? _customerDto;
         RebuildEditContext();

         _saveOk = "Saved.";

         var id = _customerDto.Id;
         if (id == Guid.Empty) {
            // If the API doesn't return Id, stay on page and show a helpful error.
            _saveError = "Saved, but server did not return an Id. Navigation to details is not possible.";
            Logger.LogWarning("Save succeeded but CustomerDto.Id is empty.");
            return;
         }

         var target = $"/customers/{id}";
         Logger.LogInformation("CustomerProfilePage: navigate to {Target}", target);

         // Let the UI render 'Saved.' before leaving (optional but helps debugging/UX)
         await InvokeAsync(StateHasChanged);

         // Navigate. If interactive routing is flaky (SSR + auth), force reload.
         Navigation.NavigateTo(target, forceLoad: true);
      }
      catch (OperationCanceledException) {
         Logger.LogDebug("CustomerProfilePage: save cancelled");
      }
      finally {
         _saving = false;
      }
   }


   // -------------------------------------------------------------------------
   // Helper
   // -------------------------------------------------------------------------
   /// <summary>
   /// DTO clone used to restore form state after Cancel.
   /// DTO cloning is acceptable because DTOs are data containers,
   /// not domain entities.
   /// </summary>
   private static CustomerDto Clone(CustomerDto src) => new() {
      Id = src.Id,
      Firstname = src.Firstname,
      Lastname = src.Lastname,
      EmailString = src.EmailString,
      Street = src.Street,
      PostalCode = src.PostalCode,
      City = src.City,
      Country = src.Country
   };
}
