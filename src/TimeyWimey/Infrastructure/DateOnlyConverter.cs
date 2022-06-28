using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeyWimey.Infrastructure;

// from https://marcominerva.wordpress.com/2021/11/22/dateonly-and-timeonly-support-with-system-text-json/
public class DateOnlyConverter : JsonConverter<DateOnly>
{
    private readonly string serializationFormat;

    public DateOnlyConverter() : this(null)
    {
    }

    public DateOnlyConverter(string? serializationFormat)
    {
        this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
    }

    public override DateOnly Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateOnly.Parse(value!);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value,
        JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(serializationFormat));
}