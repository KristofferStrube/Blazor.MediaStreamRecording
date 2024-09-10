using FluentAssertions;
using IntegrationTests.Infrastructure;
using KristofferStrube.Blazor.MediaCaptureStreams;

namespace KristofferStrube.Blazor.MediaStreamRecording.IntegrationTests;

public class MediaRecorderTest : AudioMediaStreamBlazorTest
{
    [Test]
    public async Task CreateAsync_Simple()
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
}
