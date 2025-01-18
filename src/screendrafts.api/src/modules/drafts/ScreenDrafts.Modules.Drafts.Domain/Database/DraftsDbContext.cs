namespace ScreenDrafts.Modules.Drafts.Domain.Database;

public sealed class DraftsDbContext(DbContextOptions<DraftsDbContext> options) 
  : DbContext(options)
{
  internal DbSet<Draft> Drafts { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(Schemas.Drafts);
  }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    configurationBuilder.ConfigureSmartEnum();
  }
}
