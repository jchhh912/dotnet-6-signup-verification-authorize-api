

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

public static class DataStorageExtensions
{
    public static IServiceCollection AddDataStorage<T>(this IServiceCollection services, IConfiguration config)
        where T : BaseDbContext
    {
        services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));
        var appSettings = services.GetOptions<AppSettings>(nameof(AppSettings));
        if (string.IsNullOrEmpty(appSettings.DBProvider)) throw new ArgumentNullException("DB Provider is not configured(数据库未配置).");
        switch (appSettings.DBProvider.ToLower())
        {
            case "mssql":
                services.AddDbContext<T>(m => m.UseSqlServer(appSettings.ConnectionString, options =>
                {
                    options.MigrationsAssembly(typeof(BaseDbContext).Assembly.FullName);
                    options.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                }).LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));//打印EF执行的SQL语句
                break;
            case "mysql":
                services.AddDbContext<T>(m => m.UseMySql(appSettings.ConnectionString, ServerVersion.AutoDetect(appSettings.ConnectionString), options =>
                {
                    options.MigrationsAssembly(typeof(BaseDbContext).Assembly.FullName);
                    options.SchemaBehavior(MySqlSchemaBehavior.Ignore);
                }));
                break;
            default:
                throw new ArgumentNullException($"DB Provider {appSettings.DBProvider} is not supported.");
        }
        return services;
    }
    public static T GetOptions<T>(this IServiceCollection services, string sectionName)
      where T : new()
    {
        using var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var section = configuration.GetSection(sectionName);
        var options = new T();
        section.Bind(options);
        return options;
    }
}

