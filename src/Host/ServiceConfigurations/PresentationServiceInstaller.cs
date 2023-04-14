namespace ScreenDrafts.Host.DependencyInjection;
public class PresentationServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddApplicationPart(Presentation.PresentationAssemblyReference.Assembly);
    }
}
