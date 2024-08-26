using KristofferStrube.Blazor.FileAPI;
using System.Text.Json.Serialization;

namespace KristofferStrube.Blazor.MediaStreamRecording;

/// <summary>
/// The options specific to initializing an <see cref="BlobEvent"/>. 
/// </summary>
/// <remarks><see href="https://www.w3.org/TR/mediastream-recording/#blobeventinit">See the API definition here</see>.</remarks>
public class BlobEventInit
{
    /// <summary>
    /// A <see cref="Blob"/> object containing the data to deliver via <see cref="BlobEvent"/>.
    /// </summary>
    [JsonPropertyName("data")]
    public required Blob Data { get; set; }

    /// <summary>
    /// The timecode to be used in initializing <see cref="BlobEvent"/>.
    /// </summary>
    [JsonPropertyName("timecode")]
    public double Timecode { get; set; }
}
