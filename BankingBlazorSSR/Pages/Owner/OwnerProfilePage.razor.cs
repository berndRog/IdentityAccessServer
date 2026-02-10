using BankingBlazorSSR.Api.Clients;
using BankingBlazorSSR.Api.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BankingBlazorSSR.Pages.Owner;

public partial class OwnerProfilePage(
   OwnersClient ownersClient,
   NavigationManager navigationManager,
   ILogger<OwnerProfilePage> logger
) : ComponentBase {

   private bool _loading = true;
   private bool _saving;
   private bool _showGlobalErrors;
   private string? _saveError;
   private string? _saveOk;

   // Always non-null so EditContext can be created safely
   private OwnerProfileDto _ownerProfileDto = new();

   private EditContext _editContext = default!;

   protected override async Task OnInitializedAsync() {

      // Create EditContext for the initial instance
      RebuildEditContext();

      try {
         // Load profile (read endpoint)
         _ownerProfileDto = await ownersClient.GetProfileAsync();
         logger.LogDebug("Loaded owner profile: {@Profile}", _ownerProfileDto);

         // Rebuild EditContext because model instance changed
         RebuildEditContext();
      }
      finally {
         _loading = false;
      }
   }

   private void RebuildEditContext() {
      _editContext = new EditContext(_ownerProfileDto);

      _editContext.OnValidationStateChanged += (_, __) =>
         _showGlobalErrors = _editContext.GetValidationMessages().Any();
   }

   private void Cancel()
      => navigationManager.NavigateTo("/");

   private async Task SaveAsync() {
      _saving = true;
      _saveError = null;
      _saveOk = null;

      try {
         logger.LogDebug("Update owner profile: {@Profile}", _ownerProfileDto);
         await ownersClient.UpdateProfileAsync(_ownerProfileDto);

         _saveOk = "Gespeichert.";
      }
      catch (Exception ex) {
         _saveError = ex.Message;
      }
      finally {
         _saving = false;
      }
   }
}