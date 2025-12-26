using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CommonShared.Observability;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonObservability(this IServiceCollection services,
        string serviceName, string otlpEndpoint)
    {
        var resource = ResourceBuilder.CreateDefault().AddService(serviceName);

        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(t => t
                .SetResourceBuilder(resource)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(m => m
                .SetResourceBuilder(resource)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation());

        services.AddLogging(l =>
        {
            l.ClearProviders();
            l.AddOpenTelemetry(o =>
            {
                o.SetResourceBuilder(resource);
                o.AddOtlpExporter(e => e.Endpoint = new Uri(otlpEndpoint));
            });
        });

        return services;
    }
}
