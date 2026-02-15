using BankingBlazorSsr.Api.Dtos;
using BankingBlazorSsr.Core;
namespace BankingBlazorSsr.Api.Contracts;

public interface IOwnerClient {
    // POST /owners/me/provisioned
    Task<Result<ProvisionDto>> PostProvisionAsync(CancellationToken ct = default);

    // GET /owners/me/profil
    Task<Result<OwnerDto>> GetProfileAsync(CancellationToken ct = default);

    // PUT /owners/me/profile
    Task<Result<OwnerDto>> UpdateProfileAsync(
        OwnerDto dto,
        CancellationToken ct = default
    );

    // GET /owners
    Task<Result<IEnumerable<OwnerDto>>> GetAllAsync(CancellationToken ct = default);

    // GET /owners/{ownerId}
    Task<Result<OwnerDto>> GetByIdAsync(Guid ownerId, CancellationToken ct = default);
    
    // GET /owners/username/?username={userName}
    Task<Result<OwnerDto>> GetByUserNameAsync(string userName, CancellationToken ct = default); 

    // GET /owners/name/?name={name}
    Task<Result<IEnumerable<OwnerDto>>> GetByNameAsync(string name, CancellationToken ct = default); 

    // GET /owners/exists/?username={userName} -> bool body
    Task<Result<bool>> ExistsByUserNameAsync(string userName, CancellationToken ct = default);
}