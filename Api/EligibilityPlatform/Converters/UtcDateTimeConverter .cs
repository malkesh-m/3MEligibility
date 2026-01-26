using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace MEligibilityPlatform.Converters
{
  public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
         DateTime utcValue = value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc) 
                : value.ToUniversalTime();
                
            writer.WriteStringValue(
                utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")
            );
        }
    }
}
