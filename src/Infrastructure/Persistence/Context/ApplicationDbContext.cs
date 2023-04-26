using Host = ScreenDrafts.Domain.CommissionerAggregate.Host;

namespace ScreenDrafts.Infrastructure.Persistence.Context;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(
        ITenantInfo currentTenant,
        DbContextOptions options,
        ICurrentUser currentUser,
        ISerializerService serializer,
        IOptions<DatabaseSettings> dbSettings,
        IEventPublisher events)
        : base(
            currentTenant,
            options,
            currentUser,
            serializer,
            dbSettings,
            events)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Draft> Drafts => Set<Draft>();
    public DbSet<Host> Hosts => Set<Host>();
    public DbSet<Drafter> Drafters => Set<Drafter>();
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaNames.Catalog);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureSmartEnum();
        base.ConfigureConventions(configurationBuilder);
    }
}