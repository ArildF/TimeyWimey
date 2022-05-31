using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;

namespace TimeyWimey.Data;

public static class DataExtensions
{
    public static IServiceCollection AddWimeyDbContext(this IServiceCollection self)
    {
        self.AddDbContextFactory<WimeyDataContext>(opts => 
            opts.UseSqlite($"Filename={DataPersistence.DatabaseFilePath}"));

        return self;
    }

    public static async Task<WebAssemblyHost> InitializePersistence(this WebAssemblyHost host)
    {
        var dp = host.Services.GetRequiredService<DataPersistence>();
        await dp.Initialize();

        return host;
    }
}