using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using TimeyWimey;
using TimeyWimey.Data;
using TimeyWimey.Infrastructure;
using TimeyWimey.Model;
using TimeyWimey.TimeRegistration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<Calendar>();
builder.Services.AddSingleton<TimeLineCalculator>();

builder.Services.AddSingleton<MouseService>();
builder.Services.AddSingleton<DataPersistence>();
builder.Services.AddSingleton<IMouseService>(sp => sp.GetRequiredService<MouseService>());

builder.Services.AddWimeyDbContext();

var host = builder.Build();

await host.InitializePersistence();

await host.RunAsync();
