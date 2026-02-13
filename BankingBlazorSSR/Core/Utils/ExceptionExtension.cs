namespace BankingBlazorSsr.Utils;

public static class ExceptionExtensions {
   public static void ThrowIfNull(this object? value, string paramName) {
      if (value is null) {
         throw new ArgumentNullException(paramName);
      }
   }
}