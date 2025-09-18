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
        string connectionString = "Host=localhost;Port=5432;Database=visiology;Username=postgres;Password=postgres;ApplicationName=Hangfire";
        string hangfireSchemaName = "_____hangfire";
        TimeSpan шnvisibilityTimeout = TimeSpan.FromMinutes(15);
        var connFactory = new NpgsqlConnectionFactory(connectionString, new PostgreSqlStorageOptions() { SchemaName = hangfireSchemaName });
        var serverCount = Consts.ServerCount;

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
                //serverOptions.Queues = [$"queue{localIndex}"];
                serverOptions.Queues = [$"queue1"];
                serverOptions.ServerTimeout = TimeSpan.FromSeconds(15);
            });
        }

        return result;
    }


}