using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using PlayerPortal.Data.Infrastructure;
using PlayerPortal.Data.SqlDb;
using PlayerPortal.Services;
using Shard.Commons;
using Shared.Broker;
using Shared.Broker.Internal;

var builder = WebApplication.CreateBuilder(args);

// Load environment-specific JSON files only
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: false)
#if DEBUG
        .AddUserSecrets<Program>()
#endif
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ITypeScanner>(new TypeScanner(config => config.Register(DependencyContext.Default.GetDefaultAssemblyNames())));
builder.Services.AddSingleton<IBroker, IocBrokerWrapper>();

builder.Services.AddSingleton<IPlayerServices, PlayerService>();
builder.Services.AddSingleton<SqlPlayerRepository>();

//Configure SQL repositories
var conString = builder.Configuration.GetConnectionString("DefaultConnection").Replace("|ROOT|", builder.Environment.ContentRootPath);
builder.Services.AddDbContextPool<EMDboDBContext>(options =>
{
    options.UseSqlServer(conString);
    options.EnableSensitiveDataLogging();
});

var app = builder.Build();

// Apply database migrations automatically
app.MigrateDb();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Player}/{action=Index}/{id?}");

app.Run();


public static class WebHostExtensions
{
    public static WebApplication MigrateDb(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var db = scope.ServiceProvider.GetRequiredService<EMDboDBContext>();
                db.Database.SetCommandTimeout(7200); // 2 hours
                db.Database.Migrate();
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                Console.WriteLine($"Database migration failed: {ex.Message}");
            }
        }

        return app;
    }
}

