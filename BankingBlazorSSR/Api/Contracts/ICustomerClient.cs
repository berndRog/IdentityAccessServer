using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface ICustomerClient {
    // POST /customers/me/provision
    Task<Result<ProvisionDto>> PostProvisionAsync(
        CancellationToken ct = default
    );

    // GET /customers/me/profile
    Task<Result<CustomerDto>> GetProfileAsync(
        CancellationToken ct = default
    );

    // PUT /customers/me/profile
    Task<Result<CustomerDto>> UpdateProfileAsync(
        CustomerDto dto,
        CancellationToken ct = default
    );

    // GET /customers
    Task<Result<IEnumerable<CustomerDto>>> GetAllAsync(CancellationToken ct = default);

    // GET /customers/{id}
    Task<Result<CustomerDto>> GetByIdAsync(
        Guid id, 
        CancellationToken ct = default
    );
    
    // GET /customers/email/?email={emailString}
    Task<Result<CustomerDto>> GetByEmailAsync(
        string emailString,
        CancellationToken ct = default
    );

    
    // GET /customers/name/?name={name}
    Task<Result<IEnumerable<CustomerDto>>> GetByNameAsync(
        string name, 
        CancellationToken ct = default
    ); 
}