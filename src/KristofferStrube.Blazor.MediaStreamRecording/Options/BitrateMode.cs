using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.MediaStreamRecording;

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
