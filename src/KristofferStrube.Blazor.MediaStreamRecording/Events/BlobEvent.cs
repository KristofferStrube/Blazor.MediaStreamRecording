using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.WebIDL;
using Microsoft.JSInterop;
using KristofferStrube.Blazor.FileAPI;
using KristofferStrube.Blazor.MediaStreamRecording.Extensions;

namespace KristofferStrube.Blazor.MediaStreamRecording;

/// <summary>
/// The data attribute of this event contains a <see cref="Blob"/> of recorded data.
/// </summary>
/// <remarks><see href="https://www.w3.org/TR/mediastream-recording/#mediarecorder">See the API definition here</see>.</remarks>
[IJSWrapperConverter]
public class BlobEvent : Event, IJSCreatable<BlobEvent>
{
    /// <summary>
    /// A lazily evaluated task that gives access to helper methods for the MediaStream Recording API.
    /// </summary>
    protected readonly Lazy<Task<IJSObjectReference>> mediaStreamHelperTask;

    /// <inheritdoc/>
    public static new Task<BlobEvent> CreateAsync(IJSRuntime jSRuntime, IJSObjectReference jSReference)
    {
        return CreateAsync(jSRuntime, jSReference, new());
    }

    /// <inheritdoc/>
    public static new async Task<BlobEvent> CreateAsync(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options)
    {
        return await Task.FromResult(new BlobEvent(jSRuntime, jSReference, options));
    }

    /// <inheritdoc cref="CreateAsync(IJSRuntime, IJSObjectReference, CreationOptions)"/>
    protected BlobEvent(IJSRuntime jSRuntime, IJSObjectReference jSReference, CreationOptions options) : base(jSRuntime, jSReference, options)
    {
        mediaStreamHelperTask = new(jSRuntime.GetHelperAsync);
    }

    /// <summary>
    /// Gets the encoded Blob whose type attribute indicates the encoding of the blob data.
    /// </summary>
    public async Task<Blob> GetDataAsync()
    {
        await using IJSObjectReference helper = await mediaStreamHelperTask.Value;
        IJSObjectReference jSIntance = await helper.InvokeAsync<IJSObjectReference>("getAttribute", JSReference, "data");
        return Blob.Create(JSRuntime, jSIntance);
    }

    /// <summary>
    /// Disposes this wrappers helpers and its references to JS objects.
    /// </summary>
    public new async ValueTask DisposeAsync()
    {
        if (mediaStreamHelperTask.IsValueCreated)
        {
            IJSObjectReference helper = await mediaStreamHelperTask.Value;
            await helper.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}
