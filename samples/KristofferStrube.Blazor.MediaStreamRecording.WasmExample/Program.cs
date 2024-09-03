using KristofferStrube.Blazor.FileAPI;
using KristofferStrube.Blazor.MediaCaptureStreams;
using KristofferStrube.Blazor.MediaStreamRecording.WasmExample;
using KristofferStrube.Blazor.WebIDL;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Adding IMediaDevicesService to service collection.
builder.Services.AddMediaDevicesService();

builder.Services.AddURLService();

WebAssemblyHost app = builder.Build();

// For Blazor WASM you need to call this to make Error Handling JS Interop.
await app.Services.SetupErrorHandlingJSInterop();

await app.RunAsync();