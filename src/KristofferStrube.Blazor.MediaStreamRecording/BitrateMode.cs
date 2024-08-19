using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.MediaStreamRecording;

/// <summary>
/// Defines the different ways that audio can encoded with a <see cref="MediaRecorder"/>.
/// </summary>
/// <remarks><see href="https://www.w3.org/TR/mediastream-recording/#enumdef-bitratemode">See the API definition here</see>.</remarks>
[JsonConverter(typeof(BitrateModeConverter))]
public enum BitrateMode
{
    /// <summary>
    /// Encode at a constant bitrate.
    /// </summary>
    Constant,

    /// <summary>
    /// Encode using a variable bitrate, allowing more space to be used for complex signals and less space for less complex signals.
    /// </summary>
    Variable,
}
