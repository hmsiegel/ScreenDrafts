namespace ScreenDrafts.Modules.Users.Infrastructure.Users;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
  public void Configure(EntityTypeBuilder<Role> builder)
  {
    builder.ToTable(Tables.Roles);

    builder.HasKey(x => x.Name);

    builder.Property(x => x.Name)
      .HasMaxLength(50)
      .IsRequired();

    builder.HasMany<User>()
      .WithMany(u => u.Roles)
      .UsingEntity(jb =>
      {
        jb.ToTable(Tables.UserRoles);
        jb.Property("RolesName").HasColumnName("role_name");
      });

    builder.HasData(
      Role.SuperAdministrator,
      Role.Administrator,
      Role.Guest,
      Role.Host,
      Role.Drafter,
      Role.Patreon
    );
  }
}
