using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.FileAPI;
using KristofferStrube.Blazor.MediaCaptureStreams;
using KristofferStrube.Blazor.WebIDL.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KristofferStrube.Blazor.MediaStreamRecording.WasmExample.Pages;
public partial class VideoRecording
{
    private string? error;
    private MediaStream? mediaStream;
    private readonly List<(string label, string id)> videoOptions = [];
    private string? selectedVideoSource;

    private MediaRecorder? recorder;
    private EventListener<BlobEvent>? dataAvailableEventListener;
    private string combinedBlobURL = "";
    private readonly List<Blob> blobsRecorded = [];
    private bool playing = false;
    private ElementReference videoElement;

    private async Task OpenVideoStream()
    {
        try
        {
            var mediaTrackConstraints = new MediaTrackConstraints
            {
                DeviceId = selectedVideoSource is null ? null : new ConstrainDomString(selectedVideoSource)
            };

            MediaDevices mediaDevices = await MediaDevicesService.GetMediaDevicesAsync();
            mediaStream = await mediaDevices.GetUserMediaAsync(new MediaStreamConstraints() { Video = mediaTrackConstraints });

            MediaDeviceInfo[] deviceInfos = await mediaDevices.EnumerateDevicesAsync();
            videoOptions.Clear();
            foreach (MediaDeviceInfo device in deviceInfos)
            {
                if (await device.GetKindAsync() is MediaDeviceKind.VideoInput)
                {
                    videoOptions.Add((await device.GetLabelAsync(), await device.GetDeviceIdAsync()));
                }
            }

            StateHasChanged();
            // We don't have a wrapper for HtmlMediaElement's yet so we use simple JSInterop for this part.
            IJSObjectReference htmlMediaElement = await JSRuntime.InvokeAsync<IJSObjectReference>("getReference", videoElement);
            await JSRuntime.InvokeVoidAsync("setAttribute", htmlMediaElement, "srcObject", mediaStream);
        }
        catch (WebIDLException ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private async Task StartRecording()
    {
        if (mediaStream is null)
        {
            return;
        }

        try
        {
            recorder = await MediaRecorder.CreateAsync(JSRuntime, mediaStream);

            dataAvailableEventListener = await EventListener<BlobEvent>.CreateAsync(JSRuntime, async (BlobEvent e) =>
            {
                Blob blob = await e.GetDataAsync();
                blobsRecorded.Add(blob);
            });
            await recorder.AddOnDataAvailableEventListenerAsync(dataAvailableEventListener);

            await recorder.StartAsync();

            StateHasChanged();
            // We don't have a wrapper for HtmlMediaElement's yet so we use simple JSInterop for this part.
            IJSObjectReference htmlMediaElement = await JSRuntime.InvokeAsync<IJSObjectReference>("getReference", videoElement);
            await JSRuntime.InvokeVoidAsync("setAttribute", htmlMediaElement, "srcObject", mediaStream);
        }
        catch (WebIDLException ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private async Task StopRecording()
    {
        if (mediaStream is null || recorder is null || dataAvailableEventListener is null)
        {
            return;
        }

        try
        {
            MediaStreamTrack[] videoTracks = await mediaStream.GetVideoTracksAsync();
            foreach (MediaStreamTrack track in videoTracks)
            {
                await track.StopAsync();
                await track.DisposeAsync();
            }

            await recorder.StopAsync();
            await recorder.RemoveOnDataAvailableEventListenerAsync(dataAvailableEventListener);
            await dataAvailableEventListener.DisposeAsync();
            await recorder.DisposeAsync();

            await using Blob combinedBlob = await Blob.CreateAsync(JSRuntime, [.. blobsRecorded], new() { Type = await blobsRecorded.First().GetTypeAsync() });
            combinedBlobURL = await URLService.CreateObjectURLAsync(combinedBlob);

            foreach (Blob blob in blobsRecorded)
            {
                await blob.DisposeAsync();
            }
        }
        catch (WebIDLException ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private void PlayRecording()
    {
        playing = true;
    }

    private async Task StopPlayingRecording()
    {
        if (mediaStream is not null)
        {
            await mediaStream.DisposeAsync();
            mediaStream = null;
        }
        if (recorder is not null)
        {
            await recorder.DisposeAsync();
            recorder = null;
        }
        if (combinedBlobURL is not "")
        {
            await URLService.RevokeObjectURLAsync(combinedBlobURL);
        }

        playing = false;
    }

    public async ValueTask DisposeAsync()
    {
        await StopPlayingRecording();
    }
}