using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface IEmployeeClient {
    // POST /employees/me/provisioned
    Task<Result<ProvisionDto>> PostProvisionAsync(CancellationToken ct = default);

    // GET /employees/me/profil
    Task<Result<EmployeeDto>> GetProfileAsync(CancellationToken ct = default);

    // PUT /employees/me/profile
    Task<Result<EmployeeDto>> UpdateProfileAsync(
        EmployeeDto dto,
        CancellationToken ct = default
    );

    // GET /employees
    Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(CancellationToken ct = default);

    // GET /employees/{ownerId}
    Task<Result<EmployeeDto>> GetByIdAsync(Guid Id, CancellationToken ct = default);
    
    // GET /employees/username/?username={userName}
    Task<Result<EmployeeDto>> GetByUserNameAsync(string userName, CancellationToken ct = default); 

    // GET /employees/name/?name={name}
    Task<Result<IEnumerable<EmployeeDto>>> GetByNameAsync(string name, CancellationToken ct = default); 

    // GET /employees/exists/?username={userName} -> bool body
    Task<Result<bool>> ExistsByUserNameAsync(string userName, CancellationToken ct = default);
}