namespace ScreenDrafts.Modules.Users.Infrastructure.Users;

internal sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermissions>
{
  public void Configure(EntityTypeBuilder<UserPermissions> builder)
  {
    builder.ToTable(Tables.UserPermissions);

    builder.HasKey(x => new { x.UserId, x.PermissionCode });

    builder.Property(x => x.UserId)
      .HasColumnName("user_id")
      .IsRequired();

    builder.Property(x => x.PermissionCode)
      .HasColumnName("permission_code")
      .IsRequired();
  }
}
