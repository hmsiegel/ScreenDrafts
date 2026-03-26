using SmartEnum.EFCore;

namespace ScreenDrafts.Modules.Movies.Infrastructure.Database;

public sealed class MoviesDbContext(DbContextOptions<MoviesDbContext> options)
  : DbContext(options), IUnitOfWork
{
  internal DbSet<Media> Media { get; set; }

  internal DbSet<Genre> Genres { get; set; }

  internal DbSet<ProductionCompany> ProductionCompanies { get; set; }

  internal DbSet<Person> People { get; set; }

  internal DbSet<MediaActor> MediaActors { get; set; }

  internal DbSet<MediaDirector> MediaDirectors { get; set; }

  internal DbSet<MediaWriter> MediaWriters { get; set; }
  internal DbSet<MediaProducer> MediaProducers { get; set; }

  internal DbSet<MediaGenre> MediaGenres { get; set; }

  internal DbSet<MediaProductionCompany> MediaProductionCompanies { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Movies);
  }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    ArgumentNullException.ThrowIfNull(configurationBuilder);
    configurationBuilder.ConfigureSmartEnum();
  }
}
