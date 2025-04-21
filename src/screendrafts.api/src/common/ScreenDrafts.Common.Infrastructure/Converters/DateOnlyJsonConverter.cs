namespace ScreenDrafts.Common.Infrastructure.Converters;

public sealed class DateOnlyJsonConverter : System.Text.Json.Serialization.JsonConverter<DateOnly>
{
  private const string DateFormat = "yyyy-MM-dd";

  public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return DateOnly.ParseExact(reader.GetString()!, DateFormat, CultureInfo.InvariantCulture);
  }

  public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
  {
    ArgumentNullException.ThrowIfNull(writer);

    writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
  }
}
