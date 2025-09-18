using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Shared;

public static class HangfireService
{
    public static IServiceCollection AddHangfireVisiology(
        [NotNull] this IServiceCollection services,
        string aspAppName
    )
    {
        string connectionString = "Host=localhost;Port=5432;Database=visiology;Username=postgres;Password=postgres";
        string hangfireSchemaName = "hangfire";
        TimeSpan шnvisibilityTimeout = TimeSpan.FromMinutes(15);
        var connFactory = new NpgsqlConnectionFactory(connectionString, new PostgreSqlStorageOptions() { SchemaName = hangfireSchemaName });
        var serverCount = 2;

        var result = services
            .AddHangfire((provider, configuration) =>
            {
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(connFactory,
                    new PostgreSqlStorageOptions()
                    {
                        SchemaName = hangfireSchemaName,
                        InvisibilityTimeout = шnvisibilityTimeout,
                    });
            });

        for (var i = 1; i <= serverCount; ++i)
        {
            var localIndex = i;
            result.AddHangfireServer(serverOptions =>
            {
                // Так мы сможем настроить, сколько параллельных загрузок может быть, что прям круто
                serverOptions.ServerName = $"{aspAppName}_hgf{localIndex}";
                serverOptions.WorkerCount = 1;
                serverOptions.CancellationCheckInterval = TimeSpan.FromMilliseconds(500);
                serverOptions.Queues = [$"queue{localIndex}"];
                serverOptions.ServerTimeout = TimeSpan.FromSeconds(15);
            });
        }

        return result;
    }

    [AutomaticRetry(Attempts = 3)]
    public static void Job_Wait(Guid guid, int delay, int dataset)
    {
        Console.WriteLine($"Start.Job_Wait: dataset = {dataset}");
        Thread.Sleep(delay * 1000);
        Console.WriteLine($"End.Job_Wait: dataset = {dataset}");
    }
}