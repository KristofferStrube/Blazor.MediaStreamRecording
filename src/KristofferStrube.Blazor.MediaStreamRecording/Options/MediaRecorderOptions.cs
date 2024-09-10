using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.MediaStreamRecording;

/// <summary>
/// The options used for creating a <see cref="MediaRecorder"/>.
/// </summary>
/// <remarks><see href="https://www.w3.org/TR/mediastream-recording/#dictdef-mediarecorderoptions">See the API definition here</see>.</remarks>
public class MediaRecorderOptions
{
    /// <summary>
    /// The container and codec format(s) for the recording, which may include any parameters that are defined for the format.
    /// </summary>
    /// <remarks>
    /// It specifies the media type and container format for the recording via a type/subtype combination, with the codecs and/or profiles parameters specified where ambiguity might arise.
    /// Individual codecs might have further optional or mandatory specific parameters.
    /// </remarks>
    [JsonPropertyName("mimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MimeType { get; set; }

    /// <summary>
    /// Aggregate target bits per second for encoding of the Audio track(s), if any.
    /// </summary>
    [JsonPropertyName("audioBitsPerSecond")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? AudioBitsPerSecond { get; set; }

    /// <summary>
    /// Aggregate target bits per second for encoding of the Video track(s), if any.
    /// </summary>
    [JsonPropertyName("videoBitsPerSecond")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? VideoBitsPerSecond { get; set; }

    /// <summary>
    /// Aggregate target bits per second for encoding of all Video and Audio Track(s) present.
    /// This member overrides either <see cref="AudioBitsPerSecond"/> or <see cref="VideoBitsPerSecond"/> if present,
    /// and might be distributed among the present track encoders as the UA sees fit.
    /// </summary>
    [JsonPropertyName("bitsPerSecond")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? BitsPerSecond { get; set; }

    /// <summary>
    /// Specifes the <see cref="BitrateMode"/> that should be used to encode the Audio track(s).
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="BitrateMode.Variable"/>.
    /// </remarks>
    [JsonPropertyName("audioBitrateMode")]
    public BitrateMode AudioBitrateMode { get; set; } = BitrateMode.Variable;

    /// <summary>
    /// Specifies the nominal interval in time between key frames in the encoded video stream.
    /// The User Agent controls key frame generation considering this property as well as <see cref="VideoKeyFrameIntervalCount"/>.
    /// </summary>
    [JsonPropertyName("videoKeyFrameIntervalDuration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? VideoKeyFrameIntervalDuration { get; set; }

    /// <summary>
    /// Specifies the interval in number of frames between key frames in the encoded video stream.
    /// The User Agent controls key frame generation considering this property as well as <see cref="VideoKeyFrameIntervalDuration"/>.
    /// </summary>
    [JsonPropertyName("videoKeyFrameIntervalCount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? VideoKeyFrameIntervalCount { get; set; }
}
