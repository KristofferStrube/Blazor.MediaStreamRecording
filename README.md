[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/KristofferStrube/Blazor.MediaStreamRecording)](https://github.com/KristofferStrube/Blazor.MediaStreamRecording/issues)
[![GitHub forks](https://img.shields.io/github/forks/KristofferStrube/Blazor.MediaStreamRecording)](https://github.com/KristofferStrube/Blazor.MediaStreamRecording/network/members)
[![GitHub stars](https://img.shields.io/github/stars/KristofferStrube/Blazor.MediaStreamRecording)](https://github.com/KristofferStrube/Blazor.MediaStreamRecording/stargazers)
[![NuGet Downloads (official NuGet)](https://img.shields.io/nuget/dt/KristofferStrube.Blazor.MediaStreamRecording?label=NuGet%20Downloads)](https://www.nuget.org/packages/KristofferStrube.Blazor.MediaStreamRecording/)

# Blazor.MediaStreamRecording
A Blazor wrapper for the [MediaStream Recording browser API.](https://www.w3.org/TR/mediastream-recording/)

This Web API makes it easy to record sound from a `MediaStream`.
It additionally makes it possible to query which encodings the current platform has available for recording.
This project implements a wrapper around the API for Blazor so that we can easily and safely record sound in the browser.

# Demo
The sample project can be demoed at https://kristofferstrube.github.io/Blazor.MediaStreamRecording/

On each page, you can find the corresponding code for the example in the top right corner.

On the [API Coverage Status](https://kristofferstrube.github.io/Blazor.MediaStreamRecording/Status) page, you can see how much of the WebIDL specs this wrapper has covered.

# Getting Started
## Prerequisites
You need to install .NET 7.0 or newer to use the library.

[Download .NET 7](https://dotnet.microsoft.com/download/dotnet/7.0)

## Installation
You can install the package via NuGet with the Package Manager in your IDE or alternatively using the command line:
```bash
dotnet add package KristofferStrube.Blazor.MediaStreamRecording
```

# Usage
The package can be used in Blazor WebAssembly and Blazor Server projects.
## Import
You need to reference the package in order to use it in your pages. This can be done in `_Import.razor` by adding the following.
```razor
@using KristofferStrube.Blazor.MediaStreamRecording
```

## Recording `MediaStream`
The primary purpose of the API is to record some `MediaStream`. You can get a `MediaStream` using the [Blazor.MediaCaptureStreams](https://github.com/KristofferStrube/Blazor.MediaCaptureStreams) library. After this you can use the following minimal sample for recording a `MediaStream`.

```csharp
// List to collect each recording part.
List<Blob> blobsRecorded = new();

// Create new MediaRecorder from some existing MediaStream.
await using MediaRecorder recorder = await MediaRecorder.CreateAsync(JSRuntime, mediaStream);

// Add event listener for when each data part is available.
await using var dataAvailableEventListener =
    await EventListener<BlobEvent>.CreateAsync(JSRuntime, async (BlobEvent e) =>
    {
        Blob blob = await e.GetDataAsync();
        blobsRecorded.Add(blob);
    });
await recorder.AddOnDataAvailableEventListenerAsync(dataAvailableEventListener);

// Starts Recording
await recorder.StartAsync();

// Records for 5 seconds.
await Task.Delay(5000);

// Stops recording
await recorder.StopAsync();

// Combines and collects the total audio data.
Blob combinedBlob = await Blob.CreateAsync(JSRuntime, [.. blobsRecorded]);
byte[] audioData = await combinedBlob.ArrayBufferAsync();
await combinedBlob.JSReference.DisposeAsync();

// Dispose of blob parts created while recording.
foreach (Blob blob in blobsRecorded)
    await blob.JSReference.DisposeAsync();
```


# Related repositories
The library uses the following other packages to support its features:
- https://github.com/KristofferStrube/Blazor.WebIDL (To make error handling JSInterop)
- https://github.com/KristofferStrube/Blazor.DOM (To implement `MediaRecorder` which extends `EventTarget`)
- https://github.com/KristofferStrube/Blazor.Window (For the `ErrorEvent` type that is also exposed via the `onError` event handler on `MediaRecorder`)
- https://github.com/KristofferStrube/Blazor.MediaCaptureStreams (To enable the capturing of the `MediaStream`s that should be recorded)

# Related articles
This repository was built with inspiration and help from the following series of articles:

- [Typed exceptions for JSInterop in Blazor](https://kristoffer-strube.dk/post/typed-exceptions-for-jsinterop-in-blazor/)
- [Blazor WASM 404 error and fix for GitHub Pages](https://blog.elmah.io/blazor-wasm-404-error-and-fix-for-github-pages/)
- [How to fix Blazor WASM base path problems](https://blog.elmah.io/how-to-fix-blazor-wasm-base-path-problems/)
