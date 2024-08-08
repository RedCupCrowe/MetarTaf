using MetarTaf.Components;
using MetarTaf.Components.Factories;
using MetarTaf.Components.Services;


var builder = WebApplication.CreateBuilder(args);
string apiKey = "pp7LF-kDXnpRRq6vGPtDO7Dij_7fAJuN3w9HpQtInYA";

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .Services
    .AddSingleton(new MetarService(new HttpClient(), apiKey))
    .AddSingleton(new TAFService(new HttpClient(), apiKey))
    .AddSingleton(new AirportInfoService(new HttpClient(), apiKey))
    .AddSingleton<AirportFactory>(); ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
