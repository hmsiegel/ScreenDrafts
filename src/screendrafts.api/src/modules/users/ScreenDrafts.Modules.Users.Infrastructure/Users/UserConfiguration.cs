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

    builder.Property(u => u.IdentityId)
      .IsRequired()
      .HasMaxLength(255);

    builder.Property(x => x.PersonPublicId);

    builder.Property(x => x.PublicId)
      .IsRequired();

    builder.Property(u => u.FirstName)
      .HasMaxLength(FirstName.MaxLength)
      .HasConversion(
      u => u.Value,
      value => FirstName.Create(value!).Value);

    builder.Property(u => u.LastName)
      .HasMaxLength(LastName.MaxLength)
      .HasConversion(
      u => u.Value,
      value => LastName.Create(value!).Value);

    builder.Property(u => u.Email)
      .HasMaxLength(Email.MaxLength)
      .HasConversion(
      u => u.Value,
      value => Email.Create(value!).Value);

    builder.Property(u => u.ProfilePicturePath)
      .HasMaxLength(2048);

    builder.Property(u => u.TwitterHandle)
      .HasMaxLength(100);

    builder.Property(u => u.InstagramHandle)
      .HasMaxLength(100);

    builder.Property(u => u.LetterboxdHandle)
      .HasMaxLength(100);

    builder.Property(u => u.BlueskyHandle)
      .HasMaxLength(100);

    builder.HasIndex(u => u.Email).IsUnique();

    builder.HasIndex(u => u.IdentityId).IsUnique();

    builder.HasIndex(u => u.PersonId).IsUnique().HasFilter("\"person_id\" IS NOT NULL");

    builder.HasIndex(u => u.PersonPublicId).IsUnique().HasFilter("\"person_public_id\" IS NOT NULL");

    builder.HasIndex(u => u.PublicId).IsUnique();
  }
}
