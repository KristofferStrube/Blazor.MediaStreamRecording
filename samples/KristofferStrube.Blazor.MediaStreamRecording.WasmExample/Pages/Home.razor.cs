using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.FileAPI;
using KristofferStrube.Blazor.MediaCaptureStreams;
using KristofferStrube.Blazor.MediaStreamRecording.WasmExample.Shared;
using KristofferStrube.Blazor.WebAudio;
using KristofferStrube.Blazor.WebIDL.Exceptions;

namespace KristofferStrube.Blazor.MediaStreamRecording.WasmExample.Pages;
public partial class Home
{
    private string? error;
    private MediaStream? mediaStream;
    private readonly List<(string label, string id)> audioOptions = [];
    private string? selectedAudioSource;
    private MediaStreamAudioSourceNode? liveSourceNoce;
    private AnalyserNode? liveAnalyzer;

    private MediaRecorder? recorder;
    private EventListener<BlobEvent>? dataAvailableEventListener;
    private string combinedBlobURL = "";
    private AudioBuffer? audioBuffer;
    private readonly List<Blob> blobsRecorded = [];
    private AudioContext? context;
    private AudioBufferSourceNode? audioSourceNode;
    private AnalyserNode? bufferAnalyzer;

    private AmplitudePlot plot = default!;

    private async Task OpenAudioStream()
    {
        try
        {
            var mediaTrackConstraints = new MediaTrackConstraints
            {
                EchoCancellation = true,
                NoiseSuppression = true,
                AutoGainControl = false,
            };
            if (selectedAudioSource is not null)
            {
                mediaTrackConstraints.DeviceId = new ConstrainDOMStringParameters() { Exact = selectedAudioSource };
            }

            MediaDevices mediaDevices = await MediaDevicesService.GetMediaDevicesAsync();
            mediaStream = await mediaDevices.GetUserMediaAsync(new MediaStreamConstraints() { Audio = mediaTrackConstraints });

            MediaDeviceInfo[] deviceInfos = await mediaDevices.EnumerateDevicesAsync();
            audioOptions.Clear();
            foreach (MediaDeviceInfo device in deviceInfos)
            {
                if (await device.GetKindAsync() is MediaDeviceKind.AudioInput)
                {
                    audioOptions.Add((await device.GetLabelAsync(), await device.GetDeviceIdAsync()));
                }
            }

            context = await AudioContext.CreateAsync(JSRuntime);

            MediaStreamAudioSourceOptions options = new()
            {
                MediaStream = mediaStream
            };
            liveSourceNoce = await context.CreateMediaStreamSourceAsync(mediaStream);
            liveAnalyzer = await AnalyserNode.CreateAsync(JSRuntime, context);
            await liveSourceNoce.ConnectAsync(liveAnalyzer);
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
        }
        catch (WebIDLException ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private async Task StopRecording()
    {
        if (mediaStream is null || recorder is null || dataAvailableEventListener is null || context is null)
        {
            return;
        }

        try
        {
            // Event listener for when it actually stopped.
            EventListener<Event> stopEventListener = default!;
            stopEventListener = await EventListener<Event>.CreateAsync(JSRuntime, async _ =>
            {
                MediaStreamTrack[] audioTracks = await mediaStream.GetAudioTracksAsync();
                foreach (MediaStreamTrack track in audioTracks)
                {
                    await track.StopAsync();
                    await track.DisposeAsync();
                }

                await recorder.RemoveOnStopEventListenerAsync(stopEventListener);
                await stopEventListener.DisposeAsync();

                await recorder.RemoveOnDataAvailableEventListenerAsync(dataAvailableEventListener);
                await dataAvailableEventListener.DisposeAsync();
                await recorder.DisposeAsync();

                await using Blob combinedBlob = await Blob.CreateAsync(JSRuntime, [.. blobsRecorded], new() { Type = await blobsRecorded.First().GetTypeAsync() });
                combinedBlobURL = await URLService.CreateObjectURLAsync(combinedBlob);

                foreach (Blob blob in blobsRecorded)
                {
                    await blob.DisposeAsync();
                }

                byte[] audioData = await combinedBlob.ArrayBufferAsync();

                audioBuffer = await context.DecodeAudioDataAsync(audioData);
                StateHasChanged();
            });
            await recorder.AddOnStopEventListenerAsync(stopEventListener);

            // Queue stop
            await recorder.StopAsync();
        }
        catch (WebIDLException ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private async Task PlayRecording()
    {
        if (context is null)
        {
            return;
        }

        await using AudioDestinationNode destination = await context.GetDestinationAsync();

        while (context is not null && audioBuffer is not null)
        {
            audioSourceNode = await AudioBufferSourceNode.CreateAsync(
                JSRuntime,
                context,
                new() { Buffer = audioBuffer }
            );

            TaskCompletionSource ended = new();
            await using EventListener<Event> replayEnded = await EventListener<Event>.CreateAsync(JSRuntime, async _ =>
            {
                if (audioSourceNode is not null)
                {
                    await plot.Reset();
                }
                ended.SetResult();
            });
            await audioSourceNode.AddOnEndedEventListenerAsync(replayEnded);

            await audioSourceNode.ConnectAsync(destination);

            if (bufferAnalyzer is not null)
            {
                await bufferAnalyzer.DisposeAsync();
            }

            bufferAnalyzer = await AnalyserNode.CreateAsync(JSRuntime, context);
            await audioSourceNode.ConnectAsync(bufferAnalyzer);

            await audioSourceNode.StartAsync();

            StateHasChanged();

            await ended.Task;

            await audioSourceNode.RemoveOnEndedEventListenerAsync(replayEnded);
            await audioSourceNode.DisposeAsync();
            audioSourceNode = null;
        }
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
        if (audioBuffer is not null)
        {
            await audioBuffer.DisposeAsync();
            audioBuffer = null;
        }
        blobsRecorded.Clear();
        if (context is not null)
        {
            await context.DisposeAsync();
            context = null;
        }
        if (audioSourceNode is not null)
        {
            await audioSourceNode.StopAsync();
        }
        if (combinedBlobURL is not "")
        {
            await URLService.RevokeObjectURLAsync(combinedBlobURL);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopPlayingRecording();
    }
}