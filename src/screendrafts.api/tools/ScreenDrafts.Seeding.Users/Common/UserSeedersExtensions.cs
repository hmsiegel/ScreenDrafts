using ScreenDrafts.Common.Features.Abstractions.Seeding;

namespace ScreenDrafts.Seeding.Users.Common;
internal static class UserSeedersExtensions
{
  public static IServiceCollection AddUserSeeders(this IServiceCollection services)
  {
    services.Scan(scan => scan
        .FromAssemblyOf<UserSeeder>()
        .AddClasses(classes => classes.AssignableTo<ICustomSeeder>(), false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

    services.AddScoped<SeederExecutor>();

    return services;
  }
}
