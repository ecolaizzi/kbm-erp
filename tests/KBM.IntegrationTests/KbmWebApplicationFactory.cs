using KBM.Infrastructure.Persistence;
using KBM.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KBM.IntegrationTests;

public class KbmWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "KbmTest_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<KbmDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<KbmDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KbmDbContext>();
            db.Database.EnsureCreated();
            var rbac = scope.ServiceProvider.GetRequiredService<RbacSeedService>();
            rbac.EnsureSeededAsync().GetAwaiter().GetResult();
        });
    }
}
