using BankingBlazorSSR.Api.Dtos;
namespace BankingBlazorSSR.Api.Clients;

public sealed class OwnersClient(HttpClient http) {
   
   private const string Base = "bankingapi/v1/owners/me";

   public async Task<OwnerProvisionDto> PostProvisionAsync(CancellationToken ct = default) {
      // POST bankingapi/v1/owners/me/provisioned
      var response = await http.PostAsync($"{Base}/provisioned", content: null, ct);
      response.EnsureSuccessStatusCode();
      return await response.Content
         .ReadFromJsonAsync<OwnerProvisionDto>(cancellationToken: ct);
   }

   public async Task<OwnerProfileDto> GetProfileAsync(CancellationToken ct = default) {
      // GET bankingapi/v1/owners/me/profile
      var responseDto = await http.GetFromJsonAsync<OwnerProfileDto>($"{Base}/profile", ct);
      return responseDto!;
   }

   public async Task<OwnerProfileDto> UpdateProfileAsync(
      OwnerProfileDto dto, 
      CancellationToken ct = default
   ) {
      // PUT bankingapi/v1/owners/me/profile
      var response = await http.PutAsJsonAsync($"{Base}/profile", dto, ct);
      response.EnsureSuccessStatusCode();
      var responseDto = 
         await response.Content.ReadFromJsonAsync<OwnerProfileDto>(cancellationToken: ct);
      return responseDto!;
      
   }
}