
using System.Net.Http.Json;
using System.Text.Json;
using BankingBlazorSsr.Core;
using BankingBlazorSsr.Core.Dto;
using Microsoft.Extensions.Logging;

namespace BankingBlazorSsr.Api.Clients;

public sealed class BeneficiaryClient(
    IHttpClientFactory factory,
    JsonSerializerOptions json,
    ILogger<BeneficiaryClient> logger
) : BaseApiClient<BeneficiaryClient>(factory, json, logger)
{
    // GET /accounts/{accountId}/beneficiaries
    public Task<Result<IEnumerable<BeneficiaryDto>>> GetAllAsync(
        Guid accountId,
        CancellationToken ct = default) =>
        SendAsync<IEnumerable<BeneficiaryDto>>(
            () => _http.GetAsync($"accounts/{accountId}/beneficiaries", ct),
            ct
        );

    // GET /accounts/{accountId}/beneficiaries/{beneficiaryId}
    public Task<Result<BeneficiaryDto>> GetByIdAsync(
        Guid accountId,
        Guid beneficiaryId,
        CancellationToken ct = default) =>
        SendAsync<BeneficiaryDto>(
            () => _http.GetAsync(
                $"accounts/{accountId}/beneficiaries/{beneficiaryId}", ct),
            ct
        );

    // POST /accounts/{accountId}/beneficiaries
    public Task<Result<BeneficiaryDto>> PostAsync(
        Guid accountId,
        BeneficiaryDto dto,
        CancellationToken ct = default) =>
        SendAsync<BeneficiaryDto>(
            () => _http.PostAsJsonAsync(
                $"accounts/{accountId}/beneficiaries", dto, _json, ct),
            ct
        );

    // DELETE /accounts/{accountId}/beneficiaries/{beneficiaryId}
    // API returns 204 NoContent -> Result<bool>
    public Task<Result<bool>> DeleteAsync(
        Guid accountId,
        Guid beneficiaryId,
        CancellationToken ct = default) =>
        SendAsync<bool>(
            () => _http.DeleteAsync(
                $"accounts/{accountId}/beneficiaries/{beneficiaryId}", ct),
            ct
        );
}
