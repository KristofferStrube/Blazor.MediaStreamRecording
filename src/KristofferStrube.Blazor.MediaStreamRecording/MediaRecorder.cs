using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.MediaCaptureStreams;
using KristofferStrube.Blazor.WebIDL;
using KristofferStrube.Blazor.FileAPI;
using Microsoft.JSInterop;
using KristofferStrube.Blazor.MediaStreamRecording.Extensions;

namespace KristofferStrube.Blazor.MediaStreamRecording;

/// <summary>
/// This is the primary part of the MediaStream Recording API. It can be constructed from an existing <see cref="MediaStream"/> and then <see cref="StartAsync"/> and <see cref="StopAsync"/> methods can be used to begin and stop recording sound.
/// </summary>
/// <remarks><see href="https://www.w3.org/TR/mediastream-recording/#mediarecorder">See the API definition here</see>.</remarks>
[IJSWrapperConverter]
public class MediaRecorder : EventTarget, IJSCreatable<MediaRecorder>
{
    /// <summary>
    /// A lazily evaluated task that gives access to helper methods for the MediaStream Recording API.
    /// </summary>
    protected readonly Lazy<Task<IJSObjectReference>> mediaStreamHelperTask;

    /// <summary>
    /// Creates an <see cref="MediaRecorder"/> using the standard constructor.
    /// </summary>
    /// <param name="jSRuntime">An <see cref="IJSRuntime"/> instance.</param>
    /// <param name="mediaStream">The <see cref="MediaStream"/> to be recorded.</param>
    /// <param name="options">Optional parameter value for this <see cref="MediaRecorder"/>.</param>
    /// <returns>A new instance of an <see cref="MediaRecorder"/>.</returns>
    public static async Task<MediaRecorder> CreateAsync(IJSRuntime jSRuntime, MediaStream mediaStream, MediaRecorderOptions? options = null)
    {
        IJSObjectReference helper = await jSRuntime.GetHelperAsync();
        IJSObjectReference jSInstance = await helper.InvokeAsync<IJSObjectReference>("constructMediaRecorder", mediaStream.JSReference, options);
        return new MediaRecorder(jSRuntime, jSInstance, new() { DisposesJSReference = true });
    }

    /// <inheritdoc/>
    public static new Task<MediaRecorder> CreateAsync(IJSRuntime jSRuntime, IJSObjectReference jSReference)
    {
        return CreateAsync(jSRuntime, jSReference, new());
    }

    /// <inheritdoc/>
    public static new async Task<MediaRecorder> CreateAsync(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options)
    {
        return await Task.FromResult(new MediaRecorder(jSRuntime, jSReference, options));
    }

    /// <inheritdoc cref="CreateAsync(IJSRuntime, IJSObjectReference, CreationOptions)"/>
    protected MediaRecorder(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options) : base(jSRuntime, jSReference, options)
    {
        mediaStreamHelperTask = new(jSRuntime.GetHelperAsync);
    }

    /// <summary>
    /// Gets the <see cref="MediaStream"/> to be recorded.
    /// </summary>
    public async Task<MediaStream> GetStreamAsync()
    {
        IJSObjectReference helper = await mediaStreamHelperTask.Value;
        IJSObjectReference jSInstance = await helper.InvokeAsync<IJSObjectReference>("getAttribute", JSReference, "stream");
        return await MediaStream.CreateAsync(JSRuntime, jSInstance, new() { DisposesJSReference = true });
    }

    /// <summary>
    /// Gets the MIME type used by the <see cref="MediaRecorder"/> object.
    /// The User Agent will be able to play back any of the MIME types it supports for recording.
    /// For example, it should be able to display a video recording in the HTML <c>&lt;video&gt;</c> tag.
    /// </summary>
    /// <remarks>
    /// MIME type specifies the media type and container format for the recording via a type/subtype combination,
    /// with the codecs and/or profiles parameters specified where ambiguity might arise.
    /// Individual codecs might have further optional specific parameters.
    /// </remarks>
    public async Task<string> GetMimeTypeAsync()
    {
        IJSObjectReference helper = await mediaStreamHelperTask.Value;
        return await helper.InvokeAsync<string>("getAttribute", JSReference, "mimeType");
    }

    /// <summary>
    /// The current state of the <see cref="MediaRecorder"/> object.
    /// </summary>
    public async Task<RecordingState> GetStateAsync()
    {
        IJSObjectReference helper = await mediaStreamHelperTask.Value;
        return await helper.InvokeAsync<RecordingState>("getAttribute", JSReference, "state");
    }

    /// <summary>
    /// Adds an <see cref="EventListener{TEvent}"/> for when the User Agent returns data to the application.
    /// The data attribute of this event contains a <see cref="Blob"/> of recorded data.
    /// </summary>
    /// <param name="callback">Callback that will be invoked when the event is dispatched.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.AddEventListenerAsync{TEvent}(string, EventListener{TEvent}?, AddEventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task AddOnDataAvailableEventListenerAsync(EventListener<BlobEvent> callback, AddEventListenerOptions? options = null)
    {
        await AddEventListenerAsync("dataavailable", callback, options);
    }

    /// <summary>
    /// Removes the event listener from the event listener list if it has been parsed to <see cref="AddOnDataAvailableEventListenerAsync"/> previously.
    /// </summary>
    /// <param name="callback">The callback <see cref="EventListener{TEvent}"/> that you want to stop listening to events.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.RemoveEventListenerAsync{TEvent}(string, EventListener{TEvent}?, EventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task RemoveOnDataAvailableEventListenerAsync(EventListener<BlobEvent> callback, EventListenerOptions? options = null)
    {
        await RemoveEventListenerAsync("dataavailable", callback, options);
    }

    /// <summary>
    /// Starts a recording.
    /// </summary>
    /// <param name="timeslice">If set this defines the minimum of timeslice milliseconds of data have been collected, or some minimum time slice imposed by the User Agent, whichever is greater.</param>
    public async Task StartAsync(ulong? timeslice = null)
    {
        await JSReference.InvokeVoidAsync("start", timeslice);
    }

    /// <summary>
    /// Stops a recording.
    /// </summary>
    public async Task StopAsync()
    {
        await JSReference.InvokeVoidAsync("stop");
    }
}
