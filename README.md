[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/KristofferStrube/Blazor.MediaStreamRecording)](https://github.com/KristofferStrube/Blazor.MediaStreamRecording/issues)
[![GitHub forks](https://img.shields.io/github/forks/KristofferStrube/Blazor.MediaStreamRecording)](https://github.com/KristofferStrube/Blazor.MediaStreamRecording/network/members)
[![GitHub stars](https://img.shields.io/github/stars/KristofferStrube/Blazor.MediaStreamRecording)](https://github.com/KristofferStrube/Blazor.MediaStreamRecording/stargazers)
<!--
[![NuGet Downloads (official NuGet)](https://img.shields.io/nuget/dt/KristofferStrube.Blazor.MediaStreamRecording?label=NuGet%20Downloads)](https://www.nuget.org/packages/KristofferStrube.Blazor.MediaStreamRecording/)
-->

# Blazor.MediaStreamRecording
A Blazor wrapper for the [MediaStream Recording browser API.](https://www.w3.org/TR/mediastream-recording/)

This Web API makes it easy to record sound from a `MediaStream`.
It additionally makes it possible to query which encodings the current platform has available for recording.
This project implements a wrapper around the API for Blazor so that we can easily and safely record sound in the browser.

**This wrapper is still under development.**
# Demo
The sample project can be demoed at https://kristofferstrube.github.io/Blazor.MediaStreamRecording/

On each page, you can find the corresponding code for the example in the top right corner.

On the [API Coverage Status](https://kristofferstrube.github.io/Blazor.MediaStreamRecording/Status) page, you can see how much of the WebIDL specs this wrapper has covered.

# Related repositories
The library uses the following other packages to support its features:
- https://github.com/KristofferStrube/Blazor.WebIDL (To make error handling JSInterop)
- https://github.com/KristofferStrube/Blazor.DOM (To implement `MediaRecorder` which extends `EventTarget`)
- https://github.com/KristofferStrube/Blazor.MediaCaptureStreams (To enable the capturing of `MediaStream`s)

# Related articles
This repository was built with inspiration and help from the following series of articles:

- [Typed exceptions for JSInterop in Blazor](https://kristoffer-strube.dk/post/typed-exceptions-for-jsinterop-in-blazor/)
- [Blazor WASM 404 error and fix for GitHub Pages](https://blog.elmah.io/blazor-wasm-404-error-and-fix-for-github-pages/)
- [How to fix Blazor WASM base path problems](https://blog.elmah.io/how-to-fix-blazor-wasm-base-path-problems/)
