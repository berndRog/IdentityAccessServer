using System.Text.Json;
using BankingBlazorSsr.Api.Contracts;
using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Clients;

public sealed class EmployeeClient(
   IHttpClientFactory factory,
   JsonSerializerOptions json,
   ILogger<EmployeeClient> logger
) : BaseApiClient<EmployeeClient>(factory, json, logger), IEmployeeClient {
   private const string Base = "bankingapi/v1";

   // POST /employees/me/provision
   public Task<Result<ProvisionDto>> PostProvisionAsync(
      CancellationToken ct
   ) => SendAsync<ProvisionDto>(
      () => _http.PostAsync($"{Base}/employees/me/provision", content: null, ct), ct);

   // GET /employees/me/profile
   public Task<Result<EmployeeDto>> GetProfileAsync(
      CancellationToken ct
   ) => SendAsync<EmployeeDto>(
      () => _http.GetAsync($"{Base}/employees/me/profile", ct), ct);

   // PUT /employees/me/profile 
   public Task<Result<EmployeeDto>> UpdateProfileAsync(
      EmployeeDto dto,
      CancellationToken ct
   ) => SendAsync<EmployeeDto>(
      () => _http.PutAsJsonAsync($"{Base}/employees/me/profile", dto, ct), ct);

   // GET /employees
   public Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(
      CancellationToken ct
   ) => SendAsync<IEnumerable<EmployeeDto>>(
      () => _http.GetAsync($"{Base}/employees", ct), ct);

   // GET /employees/{customerId}
   public Task<Result<EmployeeDto>> GetByIdAsync(
      Guid Id,
      CancellationToken ct
   ) => SendAsync<EmployeeDto>(
      () => _http.GetAsync($"{Base}/employees/{Id}", ct), ct);

   // GET /customers/email/?email={emailString}
   public Task<Result<EmployeeDto>> GetByEmailAsync(
      string emailString,
      CancellationToken ct
   ) => SendAsync<EmployeeDto>(
      () => _http.GetAsync($"{Base}/employees/email/?email={Uri.EscapeDataString(emailString)}", ct), ct);

   // GET /employees/name/?name={name}
   public Task<Result<IEnumerable<EmployeeDto>>> GetByNameAsync(
      string name,
      CancellationToken ct
   ) => SendAsync<IEnumerable<EmployeeDto>>(
      () => _http.GetAsync($"{Base}/employees/name/?name={Uri.EscapeDataString(name)}", ct), ct);
}