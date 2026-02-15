using BankingBlazorSsr.Api.Errors;
namespace BankingBlazorSsr.Core;

public sealed class Result<T> {
   public bool IsFailure { get; }
   public bool IsSuccess => !IsFailure;

   public ApiError? Error { get; }
   public T? Value { get; }

   private Result(bool isFailure, T? value, ApiError? error) {
      IsFailure = isFailure;
      Value = value;
      Error = error;
   }

   public static Result<T> Success(T value) => new(false, value, null);

   public static Result<T> Failure(ApiError error) => new(true, default, error);

   public TResult Fold<TResult>(
      Func<T, TResult> onSuccess,
      Func<ApiError, TResult> onFailure
   ) =>
      IsSuccess && Value is not null
         ? onSuccess(Value)
         : onFailure(Error!);
}
/*
public Result<TResult> Map<TResult>(Func<T, TResult> mapper) {
   return IsSuccess && Value is not null
      ? Result<TResult>.Success(mapper(Value))
      : Result<TResult>.Failure(Error!);
}

public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) {
   return IsSuccess && Value is not null
      ? binder(Value)
      : Result<TResult>.Failure(Error!);
}
*/