using MetarTaf.Components;
using MetarTaf.Components.Services;
using MetarTaf.Components.Factories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .Services
    .AddSingleton(new HttpClient())
    .AddSingleton(sp => new MetarService(sp.GetRequiredService<HttpClient>(), GetApiKey(builder.Configuration)))
    .AddSingleton(sp => new TAFService(sp.GetRequiredService<HttpClient>(), GetApiKey(builder.Configuration)))
    .AddSingleton(sp => new AirportInfoService(sp.GetRequiredService<HttpClient>(), GetApiKey(builder.Configuration)));

var app = builder.Build();

// Initialize AirportFactory with the required services
var metarService = app.Services.GetRequiredService<MetarService>();
var tafService = app.Services.GetRequiredService<TAFService>();
var airportInfoService = app.Services.GetRequiredService<AirportInfoService>();
AirportFactory.Initialize(metarService, tafService, airportInfoService);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string GetApiKey(IConfiguration configuration)
{
    // Try to get the API key from environment variables first
    var apiKey = Environment.GetEnvironmentVariable("API_KEY");

    // If not found, fall back to the configuration file
    if (string.IsNullOrEmpty(apiKey))
    {
        apiKey = configuration["ApiSettings:ApiKey"];
    }

    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("API key is not set.");
    }

    return apiKey;
}
