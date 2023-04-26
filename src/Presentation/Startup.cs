namespace ScreenDrafts.Presentation;
public static class Startup
{
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(_assembly);
        services.AddMappings();

        return services;
    }

    private static IServiceCollection AddMappings(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(_assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}
