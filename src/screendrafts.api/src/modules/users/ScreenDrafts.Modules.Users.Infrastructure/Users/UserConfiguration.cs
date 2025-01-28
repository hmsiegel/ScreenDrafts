namespace ScreenDrafts.Modules.Users.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(
      x => x.Value,
      value => UserId.Create(value));

    builder.Property(u => u.FirstName)
      .HasMaxLength( FirstName.MaxLength)
      .HasConversion(
      u => u.Value,
      value => FirstName.Create(value!).Value);

    builder.Property(u => u.LastName)
      .HasMaxLength( LastName.MaxLength)
      .HasConversion(
      u => u.Value,
      value => LastName.Create(value!).Value);

    builder.Property(u => u.Email)
      .HasMaxLength( Email.MaxLength)
      .HasConversion(
      u => u.Value,
      value => Email.Create(value!).Value);

    builder.HasIndex(u => u.Email).IsUnique();
  }
}
