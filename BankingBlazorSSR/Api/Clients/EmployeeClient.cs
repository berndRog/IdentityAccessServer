using System.Text.Json;
using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Clients;

public sealed class EmployeeClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<EmployeeClient> logger
) : BaseApiClient<EmployeeClient>(factory, json, logger), IEmployeeClient
{
   private const string MeBase = "bankingapi/v1/employees/me";

   // POST /employees/me/provisioned 
   public Task<Result<ProvisionDto>> PostProvisionAsync(CancellationToken ct) 
      => SendAsync<ProvisionDto>(
         () => _http.PostAsync($"{MeBase}/provisioned", content: null, ct), ct);

   // GET /employees/me/profile
   public Task<Result<EmployeeDto>> GetProfileAsync(CancellationToken ct) 
      => SendAsync<EmployeeDto>(() => _http.GetAsync($"{MeBase}/profile", ct), ct);
   
   // PUT /employees/me/profile 
   public Task<Result<EmployeeDto>> UpdateProfileAsync(
      EmployeeDto dto,
      CancellationToken ct = default
   ) => SendAsync<EmployeeDto>(
         () => _http.PutAsJsonAsync($"{MeBase}/profile", dto, ct), ct);

   // GET /employees
   public Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(CancellationToken ct) 
      => SendAsync<IEnumerable<EmployeeDto>>(
         () => _http.GetAsync("employees", ct), ct);

   // GET /employees/{ownerId}
   public Task<Result<EmployeeDto>> GetByIdAsync(Guid Id, CancellationToken ct) 
      => SendAsync<EmployeeDto>(
         () => _http.GetAsync($"employees/{Id}", ct), ct);

   // GET /employees/username/?username={userName}
   public Task<Result<EmployeeDto>> GetByUserNameAsync(string userName, CancellationToken ct) 
      => SendAsync<EmployeeDto>(
         () => _http.GetAsync($"owners/username/?username={Uri.EscapeDataString(userName)}", ct), ct);

   // GET /employees/name/?name={name}
   public Task<Result<IEnumerable<EmployeeDto>>> GetByNameAsync(string name, CancellationToken ct) 
      => SendAsync<IEnumerable<EmployeeDto>>(
         () => _http.GetAsync($"owners/name/?name={Uri.EscapeDataString(name)}", ct),ct);

   // GET /employees/exists/?username={userName} -> bool body
   public Task<Result<bool>> ExistsByUserNameAsync(string userName, CancellationToken ct = default) 
      => SendAsync<bool>(
         () => _http.GetAsync($"owners/exists/?username={Uri.EscapeDataString(userName)}", ct), ct);
}
