using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using TimeyWimey;
using TimeyWimey.Data;
using TimeyWimey.Infrastructure;
using TimeyWimey.Model;
using TimeyWimey.TimeRegistration;
using TimeyWimey.TimeReports;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<Calendar>();
builder.Services.AddSingleton<TimeLineCalculator>();
builder.Services.AddSingleton<EventAggregator>();
builder.Services.AddSingleton<MouseService>();
builder.Services.AddSingleton<DataPersistence>();
builder.Services.AddSingleton<SchemaMigrations>();
builder.Services.AddSingleton<WeekReportGenerator>();
builder.Services.AddMudServices();

builder.Services.AddSingleton<IMouseService>(sp => sp.GetRequiredService<MouseService>());

builder.Services.AddWimeyDbContext();

var host = builder.Build();

await host.InitializePersistence();

await host.RunAsync();
