namespace ScreenDrafts.Host.DependencyInjection;
public interface IServiceInstaller
{
    void Install(IServiceCollection services, IConfiguration configuration);
}
