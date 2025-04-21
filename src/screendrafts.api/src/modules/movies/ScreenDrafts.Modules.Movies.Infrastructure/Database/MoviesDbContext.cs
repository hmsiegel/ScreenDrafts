namespace ScreenDrafts.Modules.Movies.Infrastructure.Database;

public sealed class MoviesDbContext(DbContextOptions<MoviesDbContext> options)
  : DbContext(options), IUnitOfWork
{
  internal DbSet<Movie> Movies { get; set; }

  internal DbSet<Genre> Genres { get; set; }

  internal DbSet<ProductionCompany> ProductionCompanies { get; set; }

  internal DbSet<Person> People { get; set; }

  internal DbSet<MovieActor> MovieActors { get; set; }

  internal DbSet<MovieDirector> MovieDirectors { get; set; }

  internal DbSet<MovieWriter> MovieWriters { get; set; }

  internal DbSet<MovieProducer> MovieProducers { get; set; }

  internal DbSet<MovieGenre> MovieGenres { get; set; }

  internal DbSet<MovieProductionCompany> MovieProductionCompanies { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Movies);
  }
}
