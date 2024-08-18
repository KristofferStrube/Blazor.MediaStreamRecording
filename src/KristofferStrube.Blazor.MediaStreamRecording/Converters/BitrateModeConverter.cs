using System.Text.Json;
using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.MediaStreamRecording;

internal class BitrateModeConverter : JsonConverter<BitrateMode>
{
    public override BitrateMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, BitrateMode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            BitrateMode.Constant => "constant",
            BitrateMode.Variable => "variable",
            _ => throw new ArgumentException($"Value '{value}' was not a valid {nameof(BitrateMode)}.")
        });
    }
}
