using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using Radzen;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using TwitchManager.Auth;
using TwitchManager.Components;
using TwitchManager.Comunications.TwicthApi;
using TwitchManager.Comunications.TwitchGQL;
using TwitchManager.Data.DbContexts;
using TwitchManager.Helpers;
using TwitchManager.Models.General;
using TwitchManager.Services.Abstractions;
using TwitchManager.Services.Implementations;
using AutoMapper.Extensions.ExpressionMapping;
using Quartz;
using TwitchManager.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = null);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/keys"))
    .SetApplicationName("TwitchManager");

builder.Services.AddAutoMapper(cfg => 
{ 
    cfg.AddExpressionMapping(); 
}, Assembly.GetExecutingAssembly());

builder.Services.ConfigureWritable<ConfigData>(
    builder.Configuration.GetSection("Config"));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRadzenComponents();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<IStreamerService, StreamerService>();
builder.Services.AddScoped<IClipService, ClipService>();
builder.Services.AddHttpClient("TwitchApi")
    .ConfigureHttpClient((s, c) => c.BaseAddress = new Uri(s.GetRequiredService<IOptionsMonitor<ConfigData>>().CurrentValue.BaseUrl))
    .ConfigurePrimaryHttpMessageHandler((s) => new TwitchApiHttpClientHandler(s.GetRequiredService<IWritableOptions<ConfigData>>()));

builder.Services.AddHttpClient("TwitchGQL")
    .ConfigureHttpClient((s, c) => c.BaseAddress = new Uri("https://gql.twitch.tv/gql"))
    .ConfigurePrimaryHttpMessageHandler((s) => new TwitchGQLHttpClientHandler());


builder.Services.AddDbContextFactory<TwitchManagerDbContext, TwitchManagerDbContextFactory>();

builder.Services.AddTwitchManagerAuth();

builder.Services.AddLocalization(options => options.ResourcesPath = "");

builder.Services.AddControllers();

builder.Services.AddQuartz(q =>
{
    if (bool.TryParse(builder.Configuration["Jobs:ClipSyncronizerJob:Enabled"], out var enabled) && enabled)
    {
        var jobKey = new JobKey("ClipSyncronizerJob");
        q.AddJob<ClipSyncronizerJob>(opts => opts.WithIdentity(jobKey));

        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity("ClipSyncronizerJob-trigger")
            .WithCronSchedule(builder.Configuration["Jobs:ClipSyncronizerJob:Schedule"])
        );
    }
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


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

app.Lifetime.ApplicationStarted.Register(async () =>
{
    var dbFactory = app.Services.GetRequiredService<IDbContextFactory<TwitchManagerDbContext>>();
    var context = await dbFactory.CreateDbContextAsync();

    await context.Database.MigrateAsync();

    var config = app.Services.GetRequiredService<IOptionsMonitor<ConfigData>>().CurrentValue;
    if(config.ConfigType == ConfigType.StandAlone)
    {
        var server = app.Services.GetService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();

        foreach (var address in addressFeature.Addresses)
        {
            var uri = new Uri(address);
            if (uri.Scheme == "http")
            {
                OpenBrowser(uri.ToString());
                break;
            }
        }
    }
});

app.MapGet("/shutdown", async (context) => {

    var config = context.RequestServices.GetRequiredService<IOptionsMonitor<ConfigData>>().CurrentValue;

    if (config.ConfigType == ConfigType.StandAlone)
    {
        await app.StopAsync();

        Results.Ok();
    }
    else
    {
        await context.SignOutAsync(TwitchManagerAuthenticationOptions.AuthenticationScheme);

        context.Response.Redirect("/login");
    }
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