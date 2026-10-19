namespace ScreenDrafts.Modules.Communications.Infrastructure.Database;

public sealed class CommunicationsDbContext(DbContextOptions<CommunicationsDbContext> options)
  : DbContext(options),
    IUnitOfWork
{
  internal DbSet<ContactMessage> ContactMessages { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureConfiguration).Assembly);

    modelBuilder.HasDefaultSchema(Schemas.Communications);
  }
}
