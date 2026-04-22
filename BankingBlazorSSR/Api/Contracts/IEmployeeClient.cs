using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface IEmployeeClient {
   // POST /employees/me/provision
   Task<Result<ProvisionDto>> PostProvisionAsync(CancellationToken ct = default);

   // GET /employees/me/profil
   Task<Result<EmployeeDto>> GetProfileAsync(CancellationToken ct = default);

   // PUT /employees/me/profile
   Task<Result<EmployeeDto>> UpdateProfileAsync(
      EmployeeDto dto,
      CancellationToken ct = default
   );

   // POST /employees/{id}/activate
   Task<Result<bool>> PostActivateAsync(
      Guid id,
      CancellationToken ct = default
   );

   // GET /employees
   Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(
      CancellationToken ct = default
   );

   // GET /employees/{Id}
   Task<Result<EmployeeDto>> GetByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // GET /employees/email/?email={emailString}
   Task<Result<EmployeeDto>> GetByEmailAsync(
      string emailString,
      CancellationToken ct = default
   );

   // GET /employees/name/?name={name}
   Task<Result<IEnumerable<EmployeeDto>>> GetByNameAsync(string name, CancellationToken ct = default);
}