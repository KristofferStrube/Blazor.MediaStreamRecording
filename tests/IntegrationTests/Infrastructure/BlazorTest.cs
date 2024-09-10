using BlazorServer;
using KristofferStrube.Blazor.MediaCaptureStreams;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;

namespace KristofferStrube.Blazor.MediaStreamRecording.IntegrationTests.Infrastructure;

[TestFixture]
public class BlazorTest<TEvaluationContext> where TEvaluationContext : EvaluationContext, IEvaluationContext<TEvaluationContext>
{
    private IHost? _host;

    protected Uri RootUri;
    protected virtual string[] Args => ["--use-fake-device-for-media-stream", "--use-fake-ui-for-media-stream"];


    protected TEvaluationContext EvaluationContext { get; set; } = default!;
    protected TEvaluationContext EvaluationContextCreator(IServiceProvider sp)
    {
        EvaluationContext = TEvaluationContext.Create(sp);
        EvaluationContext.AfterRenderAsync = AfterRenderAsync;
        return EvaluationContext;
    }
    protected Func<Task<object?>>? AfterRenderAsync { get; set; }

    protected IPlaywright PlaywrightInstance { get; set; }
    protected IBrowserContext Context { get; set; }
    protected IPage Page { get; set; }

    [SetUp]
    public async Task Setup()
    {
        PlaywrightInstance = await Playwright.CreateAsync();
        IBrowser browser = await PlaywrightInstance.Chromium.LaunchAsync(new()
        {
            Args = Args,
        });
        // Create a new incognito browser context
        Context = await browser.NewContextAsync();
        // Create a new page inside context.
        Page = await Context.NewPageAsync();

        _host = BlazorServer.Program.BuildWebHost([],
            serviceBuilder =>
            {
                _ = serviceBuilder
                    .AddScoped(typeof(EvaluationContext), EvaluationContextCreator)
                    .AddMediaDevicesService();
            }
        );

        await _host.StartAsync();

        RootUri = new(_host.Services.GetRequiredService<IServer>().Features
            .GetRequiredFeature<IServerAddressesFeature>()
            .Addresses.Single());
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    protected async Task OnAfterRerenderAsync()
    {
        await Page.GotoAsync(RootUri.AbsoluteUri);
        await Assertions.Expect(Page.GetByTestId("result")).ToHaveTextAsync($"done");
    }
}