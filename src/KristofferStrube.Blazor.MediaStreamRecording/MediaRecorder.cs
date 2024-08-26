using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.FileAPI;
using KristofferStrube.Blazor.MediaCaptureStreams;
using KristofferStrube.Blazor.MediaStreamRecording.Extensions;
using KristofferStrube.Blazor.WebIDL;
using KristofferStrube.Blazor.Window;
using Microsoft.JSInterop;

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
    /// Gets the <see cref="MediaStream"/> that is to be recorded.
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
    /// Adds an <see cref="EventListener{TEvent}"/> for when the User Agent has started recording data from the <see cref="MediaStream"/>.
    /// </summary>
    /// <param name="callback">Callback that will be invoked when the event is dispatched.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.AddEventListenerAsync{TEvent}(string, EventListener{TEvent}?, AddEventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task AddOnStartEventListenerAsync(EventListener<Event> callback, AddEventListenerOptions? options = null)
    {
        await AddEventListenerAsync("start", callback, options);
    }

    /// <summary>
    /// Removes the event listener from the event listener list if it has been parsed to <see cref="AddOnStartEventListenerAsync"/> previously.
    /// </summary>
    /// <param name="callback">The callback <see cref="EventListener{TEvent}"/> that you want to stop listening to events.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.RemoveEventListenerAsync{TEvent}(string, EventListener{TEvent}?, EventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task RemoveOnStartEventListenerAsync(EventListener<Event> callback, EventListenerOptions? options = null)
    {
        await RemoveEventListenerAsync("start", callback, options);
    }

    /// <summary>
    /// Adds an <see cref="EventListener{TEvent}"/> for when the User Agent has stopped recording data from the <see cref="MediaStream"/>.
    /// </summary>
    /// <param name="callback">Callback that will be invoked when the event is dispatched.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.AddEventListenerAsync{TEvent}(string, EventListener{TEvent}?, AddEventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task AddOnStopEventListenerAsync(EventListener<Event> callback, AddEventListenerOptions? options = null)
    {
        await AddEventListenerAsync("stop", callback, options);
    }

    /// <summary>
    /// Removes the event listener from the event listener list if it has been parsed to <see cref="AddOnStopEventListenerAsync"/> previously.
    /// </summary>
    /// <param name="callback">The callback <see cref="EventListener{TEvent}"/> that you want to stop listening to events.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.RemoveEventListenerAsync{TEvent}(string, EventListener{TEvent}?, EventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task RemoveOnStopEventListenerAsync(EventListener<Event> callback, EventListenerOptions? options = null)
    {
        await RemoveEventListenerAsync("stop", callback, options);
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
    /// Adds an <see cref="EventListener{TEvent}"/> for when the User Agent has paused recording data from the <see cref="MediaStream"/>.
    /// </summary>
    /// <param name="callback">Callback that will be invoked when the event is dispatched.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.AddEventListenerAsync{TEvent}(string, EventListener{TEvent}?, AddEventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task AddOnPauseEventListenerAsync(EventListener<Event> callback, AddEventListenerOptions? options = null)
    {
        await AddEventListenerAsync("pause", callback, options);
    }

    /// <summary>
    /// Removes the event listener from the event listener list if it has been parsed to <see cref="AddOnPauseEventListenerAsync"/> previously.
    /// </summary>
    /// <param name="callback">The callback <see cref="EventListener{TEvent}"/> that you want to stop listening to events.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.RemoveEventListenerAsync{TEvent}(string, EventListener{TEvent}?, EventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task RemoveOnPauseEventListenerAsync(EventListener<Event> callback, EventListenerOptions? options = null)
    {
        await RemoveEventListenerAsync("pause", callback, options);
    }

    /// <summary>
    /// Adds an <see cref="EventListener{TEvent}"/> for when the User Agent has resumed recording data from the <see cref="MediaStream"/>.
    /// </summary>
    /// <param name="callback">Callback that will be invoked when the event is dispatched.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.AddEventListenerAsync{TEvent}(string, EventListener{TEvent}?, AddEventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task AddOnResumeEventListenerAsync(EventListener<Event> callback, AddEventListenerOptions? options = null)
    {
        await AddEventListenerAsync("resume", callback, options);
    }

    /// <summary>
    /// Removes the event listener from the event listener list if it has been parsed to <see cref="AddOnResumeEventListenerAsync"/> previously.
    /// </summary>
    /// <param name="callback">The callback <see cref="EventListener{TEvent}"/> that you want to stop listening to events.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.RemoveEventListenerAsync{TEvent}(string, EventListener{TEvent}?, EventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task RemoveOnResumeEventListenerAsync(EventListener<Event> callback, EventListenerOptions? options = null)
    {
        await RemoveEventListenerAsync("resume", callback, options);
    }

    /// <summary>
    /// Adds an <see cref="EventListener{TEvent}"/> for an <see cref="ErrorEvent"/>.
    /// </summary>
    /// <param name="callback">Callback that will be invoked when the event is dispatched.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.AddEventListenerAsync{TEvent}(string, EventListener{TEvent}?, AddEventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task AddOnErrorEventListenerAsync(EventListener<ErrorEvent> callback, AddEventListenerOptions? options = null)
    {
        await AddEventListenerAsync("error", callback, options);
    }

    /// <summary>
    /// Removes the event listener from the event listener list if it has been parsed to <see cref="AddOnErrorEventListenerAsync"/> previously.
    /// </summary>
    /// <param name="callback">The callback <see cref="EventListener{TEvent}"/> that you want to stop listening to events.</param>
    /// <param name="options"><inheritdoc cref="EventTarget.RemoveEventListenerAsync{TEvent}(string, EventListener{TEvent}?, EventListenerOptions?)" path="/param[@name='options']"/></param>
    public async Task RemoveOnErrorEventListenerAsync(EventListener<ErrorEvent> callback, EventListenerOptions? options = null)
    {
        await RemoveEventListenerAsync("error", callback, options);
    }

    /// <summary>
    /// The target bitrate used to encode video tracks.
    /// </summary>
    public async Task<ulong> GetVideoBitsPerSecond()
    {
        IJSObjectReference helper = await mediaStreamHelperTask.Value;
        return await helper.InvokeAsync<ulong>("getAttribute", JSReference, "videoBitsPerSecond");
    }

    /// <summary>
    /// The target bitrate used to encode audio tracks.
    /// </summary>
    public async Task<ulong> GetAudioBitsPerSecond()
    {
        IJSObjectReference helper = await mediaStreamHelperTask.Value;
        return await helper.InvokeAsync<ulong>("getAttribute", JSReference, "audioBitsPerSecond");
    }

    /// <summary>
    /// The <see cref="BitrateMode"/> used to encode audio tracks.
    /// </summary>
    public async Task<BitrateMode> GetAudioBitrateMode()
    {
        IJSObjectReference helper = await mediaStreamHelperTask.Value;
        return await helper.InvokeAsync<BitrateMode>("getAttribute", JSReference, "audioBitrateMode");
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

    /// <summary>
    /// Pauses a recording.
    /// </summary>
    public async Task PauseAsync()
    {
        await JSReference.InvokeVoidAsync("pause");
    }

    /// <summary>
    /// Resumes a recording.
    /// </summary>
    public async Task ResumeAsync()
    {
        await JSReference.InvokeVoidAsync("resume");
    }

    /// <summary>
    /// Requests for the current data <see cref="Blob"/> to be collected and emited via the <see cref="AddOnDataAvailableEventListenerAsync(EventListener{BlobEvent}, AddEventListenerOptions?)"/> event.
    /// </summary>
    public async Task RequestDataAsync()
    {
        await JSReference.InvokeVoidAsync("requestData");
    }

    /// <summary>
    /// Check to see whether a <see cref="MediaRecorder"/> can record in a specified MIME type.
    /// If <see langword="true"/> is returned from this method, it only indicates that the <see cref="MediaRecorder"/> implementation is capable of recording <see cref="Blob"/> objects for the specified MIME type.
    /// Recording may still fail if sufficient resources are not available to support the concrete media encoding.
    /// </summary>
    /// <param name="jSRuntime">An <see cref="IJSRuntime"/> instance.</param>
    /// <param name="type">The type that should be checked for support.</param>
    public static async Task<bool> IsSupportedAsync(IJSRuntime jSRuntime, string type)
    {
        return await jSRuntime.InvokeAsync<bool>("MediaRecorder.isTypeSupported", type);
    }

    /// <summary>
    /// Check to see whether a <see cref="MediaRecorder"/> can record in a specified MIME type.
    /// If <see langword="true"/> is returned from this method, it only indicates that the <see cref="MediaRecorder"/> implementation is capable of recording <see cref="Blob"/> objects for the specified MIME type.
    /// Recording may still fail if sufficient resources are not available to support the concrete media encoding.
    /// </summary>
    /// <param name="type">The type that should be checked for support.</param>
    public async Task<bool> IsSupportedAsync(string type)
    {
        return await JSRuntime.InvokeAsync<bool>("MediaRecorder.isTypeSupported", type);
    }
}
