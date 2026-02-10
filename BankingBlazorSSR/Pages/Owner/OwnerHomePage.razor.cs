using BankingBlazorSSR.Api.Dtos;
using BankingBlazorSSR.UseCases.OwnerProfile;
using Microsoft.AspNetCore.Components;

namespace BankingBlazorSSR.Pages.Owner;

public partial class OwnerHomePage(
   PostOwnerProvision  postOwnerProvision,
   GetOwnerProfile getOwnerProfile,
   NavigationManager navigationManager,
   ILogger<OwnerHomePage> logger
) : ComponentBase {
   
   private OwnerProfileDto? _ownerProfileDto;
   private bool _loading = true;

   protected override async Task OnInitializedAsync(){
      // Provisioning  (idempotent)
      var ownerProvisionDto = await postOwnerProvision.ExecuteAsync(CancellationToken.None);
      if (ownerProvisionDto.ShowProfile) {
         logger.LogInformation("Owner just provisioned");
         navigationManager.NavigateTo("/owner/profile");
      }
      _ownerProfileDto = await getOwnerProfile.ExecuteAsync(CancellationToken.None);
      _loading = false;
   }

}
