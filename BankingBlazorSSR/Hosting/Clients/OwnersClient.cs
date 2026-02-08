using System.Net.Http.Json;
using BankingBlazorSSR.Hosting.Dtos;

namespace BankingBlazorSSR.Hosting.Clients;

public sealed class OwnersClient(HttpClient http) {
   private const string Base = "bankingapi/v1/owners/me";

   public async Task<Guid> ProvisionMeAsync(CancellationToken ct = default) {
      // POST bankingapi/v1/owners/me/provisioned
      var res = await http.PostAsync($"{Base}/provisioned", content: null, ct);
      res.EnsureSuccessStatusCode();
      return await res.Content.ReadFromJsonAsync<Guid>(cancellationToken: ct);
   }

   public async Task<OwnerProfileDto> GetMeProfileAsync(CancellationToken ct = default) {
      // GET bankingapi/v1/owners/me/profile
      var dto = await http.GetFromJsonAsync<OwnerProfileDto>($"{Base}/profile", ct);
      return dto!;
   }

   public async Task<OwnerProfileDto> UpdateMeProfileAsync(OwnerProfileDto dto, CancellationToken ct = default) {
      // PUT bankingapi/v1/owners/me/profile
      var res = await http.PutAsJsonAsync($"{Base}/profile", dto, ct);
      res.EnsureSuccessStatusCode();
      return (await res.Content.ReadFromJsonAsync<OwnerProfileDto>(cancellationToken: ct))!;
   }
}