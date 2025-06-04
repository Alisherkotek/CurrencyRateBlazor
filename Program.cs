using Radzen;
using BlazorTestApp.Components;
using BlazorTestApp.Data;
using BlazorTestApp.Services;
using BlazorTestApp.Configuration;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

var currencySettings = builder.Configuration.GetSection("CurrencySettings").Get<CurrencySettings>() 
    ?? new CurrencySettings();
builder.Services.AddSingleton(currencySettings);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();

builder.Services.AddHttpClient("CurrencyApi", httpClient =>
{
    httpClient.BaseAddress = new Uri(currencySettings.ApiUrl);
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddDbContext<CurrencyContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddHostedService<CurrencyUpdateService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CurrencyContext>();
    await context.Database.EnsureCreatedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();