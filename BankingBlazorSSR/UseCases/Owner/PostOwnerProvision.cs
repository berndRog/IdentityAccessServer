using BankingBlazorSSR.Api.Clients;
using BankingBlazorSSR.Api.Dtos;
namespace BankingBlazorSSR.UseCases.OwnerProfile;

public sealed class PostOwnerProvision(
   OwnersClient ownersClient
) {
   public Task<OwnerProvisionDto> ExecuteAsync(CancellationToken ct)
      => ownersClient.PostProvisionAsync(ct);
}