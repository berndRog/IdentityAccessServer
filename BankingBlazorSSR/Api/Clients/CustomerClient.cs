using System.Net.Http.Json;
using System.Text.Json;
using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Clients;

public sealed class CustomerClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<CustomerClient> logger
) : BaseApiClient<CustomerClient>(factory, json, logger), ICustomerClient
{
   private const string Base = "banking/v2";

   // POST banking/v2/customers/me/provision  -> 200 OK + OwnerProvisionDto
   public Task<Result<ProvisionDto>> PostProvisionAsync(CancellationToken ct = default) 
      => SendAsync<ProvisionDto>(
         () => _http.PostAsJsonAsync($"{Base}/customers/me/provision", new { }, ct), ct);

   // GET bankingapi/v1/customers/me/profile -> 200 OK + OwnerProfileDto
   public Task<Result<CustomerDto>> GetProfileAsync(CancellationToken ct = default) 
      => SendAsync<CustomerDto>(() => _http.GetAsync($"{Base}/customers/me/profile", ct), ct);

   // PUT bankingapi/v1/customers/me/profile -> 200 OK + OwnerProfileDto
   public Task<Result<CustomerDto>> UpdateProfileAsync(
      CustomerDto dto,
      CancellationToken ct = default
   ) => SendAsync<CustomerDto>(
         () => _http.PutAsJsonAsync($"{Base}/customers/me/profile", dto, ct), ct);

   // GET /customers
   public Task<Result<IEnumerable<CustomerDto>>> GetAllAsync(CancellationToken ct = default) 
      => SendAsync<IEnumerable<CustomerDto>>(
         () => _http.GetAsync($"{Base}/customers", ct), ct);

   // GET /customers/{customerId}
   public Task<Result<CustomerDto>> GetByIdAsync(Guid customerId, CancellationToken ct = default) 
      => SendAsync<CustomerDto>(
         () => _http.GetAsync($"{Base}/customers/{customerId}", ct), ct);

   // GET /customers/email/?email={emailString}
   public Task<Result<CustomerDto>> GetByEmailAsync(string emailString, CancellationToken ct = default) 
      => SendAsync<CustomerDto>(
         () => _http.GetAsync($"{Base}/customers/email/?email={Uri.EscapeDataString(emailString)}", ct),ct);
   
   // GET /customers/name/?name={name}
   public Task<Result<IEnumerable<CustomerDto>>> GetByNameAsync(string name, CancellationToken ct = default) 
      => SendAsync<IEnumerable<CustomerDto>>(
         () => _http.GetAsync($"{Base}/customers/name/?name={Uri.EscapeDataString(name)}", ct),ct);
   
}
