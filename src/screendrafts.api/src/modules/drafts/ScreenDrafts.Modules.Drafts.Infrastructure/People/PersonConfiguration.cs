namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
  public void Configure(EntityTypeBuilder<Person> builder)
  {
    builder.ToTable(Tables.People);

    builder.HasKey(person => person.Id);

    builder
      .Property(person => person.Id)
      .ValueGeneratedNever()
      .HasConversion(id => id.Value, value => PersonId.Create(value));

    builder.Property(p => p.PublicId).ValueGeneratedOnAdd();

    builder.Property(p => p.Biography).HasColumnName("biography");

    builder.Property(p => p.Location).HasMaxLength(200).HasColumnName("location");

    builder
      .Property(p => p.ProfilePicturePath)
      .HasMaxLength(500)
      .HasColumnName("profile_picture_path");

    builder.Property(p => p.TwitterHandle).HasMaxLength(100).HasColumnName("twitter_handle");

    builder.Property(p => p.InstagramHandle).HasMaxLength(100).HasColumnName("instagram_handle");

    builder.Property(p => p.LetterboxdHandle).HasMaxLength(100).HasColumnName("letterboxd_handle");

    builder.Property(p => p.BlueskyHandle).HasMaxLength(100).HasColumnName("bluesky_handle");

    builder.HasIndex(p => p.PublicId).IsUnique().HasDatabaseName("ix_people_public_id");

    builder
      .HasOne(p => p.DrafterProfile)
      .WithOne(d => d.Person)
      .HasForeignKey<Drafter>(d => d.PersonId);

    builder.HasOne(p => p.HostProfile).WithOne(h => h.Person).HasForeignKey<Host>(h => h.PersonId);
  }
}
