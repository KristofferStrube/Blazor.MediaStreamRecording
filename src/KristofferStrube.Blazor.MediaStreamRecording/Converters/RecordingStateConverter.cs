using System.Text.Json;
using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.MediaStreamRecording;

internal class RecordingStateConverter : JsonConverter<RecordingState>
{
    public override RecordingState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "inactive" => RecordingState.Inactive,
            "recording" => RecordingState.Recording,
            "paused" => RecordingState.Paused,
            var value => throw new ArgumentException($"Value '{value}' was not a valid {nameof(RecordingState)}.")
        };
    }

    public override void Write(Utf8JsonWriter writer, RecordingState value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            RecordingState.Inactive => "inactive",
            RecordingState.Recording => "recording",
            RecordingState.Paused => "paused",
            _ => throw new ArgumentException($"Value '{value}' was not a valid {nameof(RecordingState)}.")
        });
    }
}
