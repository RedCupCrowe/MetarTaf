using MetarTaf.Components;
using MetarTaf.Components.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .Services.AddSingleton(new MetarService(new HttpClient(), "pp7LF-kDXnpRRq6vGPtDO7Dij_7fAJuN3w9HpQtInYA"))
    .AddSingleton(new AirportInfoService(new HttpClient(), "pp7LF-kDXnpRRq6vGPtDO7Dij_7fAJuN3w9HpQtInYA"));

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
