using BankingBlazorSSR.Api.Clients;
using BankingBlazorSSR.Api.Dtos;
namespace BankingBlazorSSR.UseCases.OwnerProfile;

public sealed class GetOwnerProfile(
   OwnersClient ownersClient
) {
   public async Task<OwnerProfileDto> ExecuteAsync(CancellationToken ct) 
      => await ownersClient.GetProfileAsync(ct);   
}