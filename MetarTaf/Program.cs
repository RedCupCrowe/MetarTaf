using MetarTaf.Components;
using MetarTaf.Components.Services;
using MetarTaf.Components.Factories;

var builder = WebApplication.CreateBuilder(args);
string apiKey = "pp7LF-kDXnpRRq6vGPtDO7Dij_7fAJuN3w9HpQtInYA";

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .Services
    .AddSingleton(new HttpClient())
    .AddSingleton(sp => new MetarService(sp.GetRequiredService<HttpClient>(), apiKey))
    .AddSingleton(sp => new TAFService(sp.GetRequiredService<HttpClient>(), apiKey))
    .AddSingleton(sp => new AirportInfoService(sp.GetRequiredService<HttpClient>(), apiKey));

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
