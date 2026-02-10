using BankingBlazorSSR.Api.Clients;
using BankingBlazorSSR.Api.Dtos;
using BankingBlazorSSR.Pages.Owner;
namespace BankingBlazorSSR.UseCases.OwnerProfile;

public sealed class UpdateOwnerProfile(
   OwnersClient ownersClient
){
   public Task ExecuteAsync(
      OwnerProfileDto dto, 
      CancellationToken ct
   ) => ownersClient.UpdateProfileAsync(dto, ct);
}
