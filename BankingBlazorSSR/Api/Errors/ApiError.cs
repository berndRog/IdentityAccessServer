namespace BankingBlazorSsr.Core;

public sealed record ApiError(
   int Status,              // HTTP status code (409, 422, ...)
   string Title,            // ProblemDetails.Title
   string? Detail = null,   // ProblemDetails.Detail
   string? ErrorCode = null // optional: Domain ErrorCode
);