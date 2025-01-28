namespace ScreenDrafts.Modules.Users.Infrastructure.Database;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options)
  : DbContext(options), IUnitOfWork
{
  internal DbSet<User> Users { get; set; } = default!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    modelBuilder.HasDefaultSchema(Schemas.Users);

  }
}
