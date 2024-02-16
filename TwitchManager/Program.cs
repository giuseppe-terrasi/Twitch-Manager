using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using Radzen;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using TwitchManager.Auth;
using TwitchManager.Components;
using TwitchManager.Comunications.TwicthApi;
using TwitchManager.Data;
using TwitchManager.Helpers;
using TwitchManager.Models.General;
using TwitchManager.Services.Abstractions;
using TwitchManager.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.ConfigureWritable<ConfigData>(
    builder.Configuration.GetSection("Config"));

builder.Services.AddRadzenComponents();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<IStreamerService, StreamerService>();
builder.Services.AddScoped<IClipService, ClipService>();
builder.Services.AddHttpClient("TwitchApi")
    .ConfigureHttpClient((s, c) => c.BaseAddress = new Uri(s.GetRequiredService<IOptionsMonitor<ConfigData>>().CurrentValue.BaseUrl))
    .ConfigurePrimaryHttpMessageHandler((s) => new TwitchApiHttpClientHandler(s.GetRequiredService<IWritableOptions<ConfigData>>()));


builder.Services.AddDbContextFactory<TwitchManagerDbContext>();

builder.Services.AddTwitchManagerAuth();

builder.Services.AddLocalization(options => options.ResourcesPath = "");

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

var supportedCultures = new[]
{
    "it-IT",
    "en-US"
};

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

if(app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
}
else
{
    var provider = new EmbeddedFileProvider(Assembly.GetAssembly(type: typeof(Program)));
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = provider,
        RequestPath = "",
    });
}

app.UseTwitchManagerAuth();

app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStarted.Register(() =>
{
    if (app.Environment.IsDevelopment())
        return;

    var server = app.Services.GetService<IServer>();
    var addressFeature = server.Features.Get<IServerAddressesFeature>();

    foreach ( var address in addressFeature.Addresses)
    {
        var uri = new Uri(address); 
        if(uri.Scheme == "http")
        {
            OpenBrowser(uri.ToString());
            break;
        }
    }
});

app.MapGet("/shutdown", () => { 
    app.StopAsync().ConfigureAwait(false);
});

app.Run();

static void OpenBrowser(string url)
{
    switch (RuntimeInformation.OSDescription)
    {
        case var os when os.Contains("Windows"):
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // Works ok on windows
            break;
        case var os when os.Contains("Linux"):
            Process.Start("xdg-open", url);  // Works ok on linux
            break;
        case var os when os.Contains("OSX"):
            Process.Start("open", url); // Not tested
            break;
    }

}