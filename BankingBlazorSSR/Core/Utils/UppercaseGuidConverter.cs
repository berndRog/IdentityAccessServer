using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingBlazorSsr.Core.Utils;

/// <summary>
/// Serializes GUIDs using an uppercase "D" format (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).
/// Reading is case-insensitive by default.
/// </summary>
public sealed class UppercaseGuidConverter : JsonConverter<Guid>
{
   public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      // Handles both JSON string and null (for nullable GUIDs a separate converter is needed).
      var s = reader.GetString();
      if (string.IsNullOrWhiteSpace(s))
         return Guid.Empty;

      return Guid.Parse(s);
   }

   public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
   {
      // "D" is the default with hyphens; ToUpperInvariant forces A-F to uppercase.
      writer.WriteStringValue(value.ToString("D").ToUpperInvariant());
   }
}

/// <summary>
/// Serializes nullable GUIDs using uppercase format.
/// </summary>
public sealed class UppercaseNullableGuidConverter : JsonConverter<Guid?>
{
   public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      if (reader.TokenType == JsonTokenType.Null)
         return null;

      var s = reader.GetString();
      if (string.IsNullOrWhiteSpace(s))
         return null;

      return Guid.Parse(s);
   }

   public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
   {
      if (value is null)
      {
         writer.WriteNullValue();
         return;
      }

      writer.WriteStringValue(value.Value.ToString("D").ToUpperInvariant());
   }
}
