using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.MediaStreamRecording;

/// <summary>
/// Defines the state of a <see cref="MediaRecorder"/>.
/// </summary>
/// <remarks><see href="https://www.w3.org/TR/mediastream-recording/#enumdef-recordingstate">See the API definition here</see>.</remarks>
[JsonConverter(typeof(RecordingStateConverter))]
public enum RecordingState
{
    /// <summary>
    /// Recording is not occuring: Either it has not been started or it has been stopped.
    /// </summary>
    Inactive,
    /// <summary>
    /// Recording has been started and the User Agent is capturing data.
    /// </summary>
    Recording,
    /// <summary>
    /// Recording has been started, then paused, and not yet stopped or resumed.
    /// </summary>
    Paused,
}
