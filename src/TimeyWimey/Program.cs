using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TimeyWimey;
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
builder.Services.AddSingleton<IMouseService>(sp => sp.GetRequiredService<MouseService>());

await builder.Build().RunAsync();
