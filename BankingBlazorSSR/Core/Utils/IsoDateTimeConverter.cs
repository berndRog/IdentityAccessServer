using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BankingBlazorSsr.Utils;

public class IsoDateTimeConverter : JsonConverter<DateTime> {
   
   public override DateTime Read(
      ref Utf8JsonReader reader, 
      Type typeToConvert, 
      JsonSerializerOptions options
   ) {
      var dateTimeString = reader.GetString();
      
      if (DateTime.TryParse(
          dateTimeString,               // string s, 
          null,                         // IFormatProvider provider,
          DateTimeStyles.RoundtripKind, // DateTimeStyles styles,
          out var dateTime              // out DateTime result
      )) {
         return dateTime;
      }

      // if TryParse fails: Handle variable-length milliseconds manually
      if (dateTimeString != null) {
         if (dateTimeString.EndsWith("Z"))
            throw new JsonException($"Unable to convert \"{dateTimeString}\" to DateTime.");
         dateTimeString = dateTimeString.TrimEnd('Z');
      }
      if (DateTime.TryParse(
             dateTimeString, 
             null, 
             DateTimeStyles.RoundtripKind, 
             out dateTime
          )) {
         return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
      }

      // If TryParse fauls again: Attempt parsing with different formats
      string[] formats = [
         "yyyy-MM-ddTHH:mm:ssZ",
         "yyyy-MM-ddTHH:mm:ss.fZ",
         "yyyy-MM-ddTHH:mm:ss.ffZ",
         "yyyy-MM-ddTHH:mm:ss.fffZ",
         "yyyy-MM-ddTHH:mm:ss.ffffZ",
         "yyyy-MM-ddTHH:mm:ss.fffffZ",
         "yyyy-MM-ddTHH:mm:ss.ffffffZ"
      ];

      foreach (var format in formats) {
         if (DateTime.TryParseExact(
             dateTimeString + "Z", 
             format, 
             null,
             DateTimeStyles.RoundtripKind, 
             out dateTime
         )) {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
         }
      }
      throw new JsonException($"Unable to convert \"{dateTimeString}\" to DateTime.");
   }

   public override void Write(
      Utf8JsonWriter writer, 
      DateTime value, 
      JsonSerializerOptions options
   ) {
      // Always output in ISO 8601 with milliseconds
      writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
   }
   
   public DateTime? ParseIsoToUtc(string iso8860) {
      
      DateTime dateTime;
      if (DateTime.TryParse(
          iso8860, 
          null, 
          DateTimeStyles.RoundtripKind, 
          out dateTime
       )) {
         return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
      }
      return null;
   }
}