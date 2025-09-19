using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Shared;

public static class HangfireService
{
    public static IServiceCollection AddHangfire(
        [NotNull] this IServiceCollection services,
        string aspAppName
    )
    {
        string connectionString = "Host=localhost;Port=5432;Database=test;Username=postgres;Password=postgres;ApplicationName=Hangfire";
        string hangfireSchemaName = "_____hangfire";
        TimeSpan invisibilityTimeout = TimeSpan.FromMinutes(15);
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
                        InvisibilityTimeout = invisibilityTimeout,
                    });
            });

        for (var i = 1; i <= serverCount; ++i)
        {
            var localIndex = i;
            result.AddHangfireServer(serverOptions =>
            {
                serverOptions.ServerName = $"{aspAppName}_hgf{localIndex}";
                serverOptions.WorkerCount = 1;
                serverOptions.CancellationCheckInterval = TimeSpan.FromMilliseconds(500);
                serverOptions.Queues = [$"queue{localIndex}"];
                serverOptions.ServerTimeout = TimeSpan.FromSeconds(15);
            });
        }

        return result;
    }


}