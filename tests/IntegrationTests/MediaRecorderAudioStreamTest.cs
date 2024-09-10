using FluentAssertions;
using FluentAssertions.Execution;
using IntegrationTests.Infrastructure;
using KristofferStrube.Blazor.DOM;
using KristofferStrube.Blazor.FileAPI;
using KristofferStrube.Blazor.MediaCaptureStreams;
using KristofferStrube.Blazor.WebIDL.Exceptions;

namespace KristofferStrube.Blazor.MediaStreamRecording.IntegrationTests;

public class MediaRecorderAudioStreamTest : AudioMediaStreamBlazorTest
{
    [Test]
    public async Task CreateAsync_WithNoOptions_Succeeds()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();

            MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);
            return mediaRecorder;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        _ = EvaluationContext.Result.Should().BeOfType<MediaRecorder>();
    }

    [Test]
    public async Task CreateAsync_WitOptions_Succeeds()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();

            MediaRecorderOptions options = new()
            {
                AudioBitrateMode = BitrateMode.Constant,
                AudioBitsPerSecond = 1000,
                MimeType = "audio/webm"
            };
            MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream, options);
            return mediaRecorder;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        _ = EvaluationContext.Result.Should().BeOfType<MediaRecorder>();
    }

    [Test]
    public async Task CreateAsync_WithUnsupportedMimeType_Throws_NotSupportedErrorException()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();

            MediaRecorderOptions options = new()
            {
                MimeType = "audio/mp3"
            };
            MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream, options);
            return mediaRecorder;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        _ = EvaluationContext.Exception.Should().BeOfType<NotSupportedErrorException>();
    }

    [Test]
    public async Task GetStreamAsync_Returns_NewReferenceToOriginalStream()
    {
        // Arrange
        MediaStream? mediaStream = null;
        AfterRenderAsync = async () =>
        {
            mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            return await mediaRecorder.GetStreamAsync();
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        MediaStream returnedMediaStream = EvaluationContext.Result.Should().BeOfType<MediaStream>().Subject;
        string returnedMediaStreamId = await returnedMediaStream.GetIdAsync();
        string originalMediaStreamId = await mediaStream!.GetIdAsync();
        _ = returnedMediaStreamId.Should().Be(originalMediaStreamId);
    }

    [Test]
    public async Task GetMimeTypeAsync_Returns_UsedMimeType()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            MediaRecorderOptions options = new()
            {
                MimeType = "audio/webm"
            };
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream, options);

            return await mediaRecorder.GetMimeTypeAsync();
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        string mimeType = EvaluationContext.Result.Should().BeOfType<string>().Subject;
        _ = mimeType.Should().Be("audio/webm");
    }

    [Test]
    public async Task GetStateAsync_Returns_Inactive_WhenNotStarted()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            return await mediaRecorder.GetStateAsync();
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        RecordingState state = EvaluationContext.Result.Should().BeOfType<RecordingState>().Subject;
        _ = state.Should().Be(RecordingState.Inactive);
    }

    [Test]
    public async Task GetStateAsync_Returns_Recording_WhenStarted()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);
            await mediaRecorder.StartAsync();

            return await mediaRecorder.GetStateAsync();
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        RecordingState state = EvaluationContext.Result.Should().BeOfType<RecordingState>().Subject;
        _ = state.Should().Be(RecordingState.Recording);
    }

    [Test]
    public async Task OnStart_EmitsEvent_WhenMediaRecorderIsStarted()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<Event> onStartListener = await EventListener<Event>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnStartEventListenerAsync(onStartListener);
            await mediaRecorder.StartAsync();

            return await eventEmitted.Task;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        string eventType = EvaluationContext.Result.Should().BeOfType<string>().Subject;
        _ = eventType.Should().Be("start");
    }

    [Test]
    public async Task OnStart_DoesNotEmitEvent_WhenMediaRecorderIsStarted_IfListenerIsRemoved()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<Event> onStartListener = await EventListener<Event>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnStartEventListenerAsync(onStartListener);
            await mediaRecorder.RemoveOnStartEventListenerAsync(onStartListener);
            await mediaRecorder.StartAsync();

            await Task.Delay(1000);

            return eventEmitted.Task.Status;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        TaskStatus eventEmitted = EvaluationContext.Result.Should().BeOfType<TaskStatus>().Subject;
        _ = eventEmitted.Should().Be(TaskStatus.WaitingForActivation);
    }

    [Test]
    public async Task OnStop_EmitsEvent_WhenMediaRecorderIsStopped()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<Event> onStopListener = await EventListener<Event>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnStopEventListenerAsync(onStopListener);
            await mediaRecorder.StartAsync();
            await mediaRecorder.StopAsync();

            return await eventEmitted.Task;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        string eventType = EvaluationContext.Result.Should().BeOfType<string>().Subject;
        _ = eventType.Should().Be("stop");
    }

    [Test]
    public async Task OnStop_DoesNotEmitEvent_WhenMediaRecorderIsStopped_IfListenerIsRemoved()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<Event> onStopListener = await EventListener<Event>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnStopEventListenerAsync(onStopListener);
            await mediaRecorder.RemoveOnStopEventListenerAsync(onStopListener);
            await mediaRecorder.StartAsync();
            await mediaRecorder.StopAsync();

            await Task.Delay(1000);

            return eventEmitted.Task.Status;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        TaskStatus eventEmitted = EvaluationContext.Result.Should().BeOfType<TaskStatus>().Subject;
        _ = eventEmitted.Should().Be(TaskStatus.WaitingForActivation);
    }

    [Test]
    public async Task OnDataAvailable_EmitsEvent_WhenMediaRecorderHasDataAvailable()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<Blob> eventEmitted = new();
            await using EventListener<BlobEvent> onDataAvailableListener = await EventListener<BlobEvent>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetDataAsync());
            });
            await mediaRecorder.AddOnDataAvailableEventListenerAsync(onDataAvailableListener);
            await mediaRecorder.StartAsync();

            return await eventEmitted.Task;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        _ = EvaluationContext.Result.Should().BeOfType<Blob>();
    }

    [Test]
    public async Task OnDataAvailable_DoesNotEmitEvent_WhenMediaRecorderHasDataAvailable_IfListenerIsRemoved()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<BlobEvent> onDataAvailableListener = await EventListener<BlobEvent>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnDataAvailableEventListenerAsync(onDataAvailableListener);
            await mediaRecorder.RemoveOnDataAvailableEventListenerAsync(onDataAvailableListener);
            await mediaRecorder.StartAsync();

            await Task.Delay(1000);

            return eventEmitted.Task.Status;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        TaskStatus eventEmitted = EvaluationContext.Result.Should().BeOfType<TaskStatus>().Subject;
        _ = eventEmitted.Should().Be(TaskStatus.WaitingForActivation);
    }

    [Test]
    public async Task OnPause_EmitsEvent_WhenMediaRecorderIsPaused()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<Event> onPauseListener = await EventListener<Event>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnPauseEventListenerAsync(onPauseListener);
            await mediaRecorder.StartAsync();
            await mediaRecorder.PauseAsync();

            return await eventEmitted.Task;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        string eventType = EvaluationContext.Result.Should().BeOfType<string>().Subject;
        _ = eventType.Should().Be("pause");
    }

    [Test]
    public async Task OnPause_DoesNotEmitEvent_WhenMediaRecorderIsPaused_IfListenerIsRemoved()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<Event> onPauseListener = await EventListener<Event>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnPauseEventListenerAsync(onPauseListener);
            await mediaRecorder.RemoveOnPauseEventListenerAsync(onPauseListener);
            await mediaRecorder.StartAsync();
            await mediaRecorder.PauseAsync();

            await Task.Delay(1000);

            return eventEmitted.Task.Status;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        TaskStatus eventEmitted = EvaluationContext.Result.Should().BeOfType<TaskStatus>().Subject;
        _ = eventEmitted.Should().Be(TaskStatus.WaitingForActivation);
    }

    [Test]
    public async Task OnResume_EmitsEvent_WhenMediaRecorderIsResumed()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<Event> onResumeListener = await EventListener<Event>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnResumeEventListenerAsync(onResumeListener);
            await mediaRecorder.StartAsync();
            await mediaRecorder.PauseAsync();
            await mediaRecorder.ResumeAsync();

            return await eventEmitted.Task;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        string eventType = EvaluationContext.Result.Should().BeOfType<string>().Subject;
        _ = eventType.Should().Be("resume");
    }

    [Test]
    public async Task OnResume_DoesNotEmitEvent_WhenMediaRecorderIsResumed_IfListenerIsRemoved()
    {
        // Arrange
        AfterRenderAsync = async () =>
        {
            await using MediaStream mediaStream = await EvaluationContext.GetMediaStream();
            await using MediaRecorder mediaRecorder = await MediaRecorder.CreateAsync(EvaluationContext.JSRuntime, mediaStream);

            TaskCompletionSource<string> eventEmitted = new();
            await using EventListener<Event> onResumeListener = await EventListener<Event>.CreateAsync(EvaluationContext.JSRuntime, async e =>
            {
                eventEmitted.SetResult(await e.GetTypeAsync());
            });
            await mediaRecorder.AddOnResumeEventListenerAsync(onResumeListener);
            await mediaRecorder.RemoveOnResumeEventListenerAsync(onResumeListener);
            await mediaRecorder.StartAsync();
            await mediaRecorder.PauseAsync();
            await mediaRecorder.ResumeAsync();

            await Task.Delay(1000);

            return eventEmitted.Task.Status;
        };

        // Act
        await OnAfterRerenderAsync();

        // Assert
        using AssertionScope scope = new();

        TaskStatus eventEmitted = EvaluationContext.Result.Should().BeOfType<TaskStatus>().Subject;
        _ = eventEmitted.Should().Be(TaskStatus.WaitingForActivation);
    }
}
