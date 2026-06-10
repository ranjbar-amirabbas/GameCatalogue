using Microsoft.Extensions.Hosting;
using Serilog;

namespace GameCatalogue.Infrastructure.Logging;

/// <summary>
/// Centralized Serilog configuration.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog for the host: structured console output enriched from
    /// the log context, at a minimum level of Information.
    /// </summary>
    /// <param name="context">The host builder context.</param>
    /// <param name="configuration">The logger configuration to populate.</param>
    public static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration configuration)
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
    }
}
