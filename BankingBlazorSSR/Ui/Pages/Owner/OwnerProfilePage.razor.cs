using BankingBlazorSsr.Api.Clients;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Ui.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BankingBlazorSsr.Ui.Pages.Owner;

public partial class OwnerProfilePage {  // dont't use : BasePage here 
   // Property injection for Blazor components
   [Inject] private OwnerClient OwnerClient { get; set; } = default!;
   [Inject] private NavigationManager Navigation { get; set; } = default!;
   [Inject] private ILogger<OwnerProfilePage> Logger { get; set; } = default!;

   private bool _saving;
   private bool _showGlobalErrors;

   private string? _saveError;
   private string? _saveOk;

   private OwnerProfileDto _ownerProfileDto = new();
   private EditContext _editContext = default!;

   protected override async Task OnInitializedAsync() {
      // BasePage state
      Loading = true; 
      ErrorMessage = null; 
      
      // Create EditContext for the initial instance
      RebuildEditContext();

      // Load profile (Result pattern)
      var result = await OwnerClient.GetProfileAsync();

      if (result.IsFailure) {
         HandleError(result.Error!); // BasePage navigation + ErrorMessage
         return;
      }

      _ownerProfileDto = result.Value ?? new OwnerProfileDto();
      Logger.LogDebug("Loaded owner profile: {@Profile}", _ownerProfileDto);

      RebuildEditContext();
      Loading = false;
   }

   private void RebuildEditContext() {
      _editContext = new EditContext(_ownerProfileDto);

      _editContext.OnValidationStateChanged += (_, __) =>
         _showGlobalErrors = _editContext.GetValidationMessages().Any();
   }

   private void Cancel() => Navigation.NavigateTo("/");

   private async Task SaveAsync() {
      _saving = true;
      _saveError = null;
      _saveOk = null;

      // Optional: don’t call API if form invalid
      if (!_editContext.Validate()) {
         _showGlobalErrors = true;
         _saving = false;
         return;
      }

      Logger.LogDebug("Update owner profile: {@Profile}", _ownerProfileDto);

      var result = await OwnerClient.UpdateProfileAsync(_ownerProfileDto);

      if (result.IsFailure) {
         // For 401/403/404 this will navigate away; for 409/422 it sets ErrorMessage
         // Here we want a save-specific message on the same page:
         var err = result.Error!;
         Logger.LogWarning("Save failed {Status}: {Title}", err.Status, err.Title);

         // If you want *only* inline save error (no redirect), handle 409/422 here:
         if (err.Status is 409 or 422) {
            _saveError = err.Detail ?? err.Title;
            _saving = false;
            return;
         }

         // Otherwise use global handler (may redirect)
         HandleError(err);
         _saving = false;
         return;
      }

      // Success: API returned updated profile in body
      _ownerProfileDto = result.Value ?? _ownerProfileDto;
      RebuildEditContext();

      _saveOk = "Gespeichert.";
      _saving = false;
   }
}