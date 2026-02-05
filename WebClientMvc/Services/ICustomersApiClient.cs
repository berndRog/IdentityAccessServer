using WebClientMvc.Models.Dtos;
namespace WebClientMvc.Services;

/// <summary>
/// Client-side abstraction for calling the Customer endpoints
/// of the CarRentalApi from an MVC application.
///
/// This client is responsible for:
/// - attaching the OAuth access token
/// - translating HTTP responses into ApiResult<T>
/// - hiding HTTP details from MVC controllers
/// </summary>
public interface ICustomersApiClient {
   Task<ApiResult<Guid>> EnsureProvisionedAsync(CancellationToken ct);
   Task<ApiResult<CustomerProfileDto>> GetProfileAsync(CancellationToken ct);
   Task<ApiResult<CustomerProfileDto>> UpdateProfileAsync(CustomerProfileDto dto, CancellationToken ct);
}
