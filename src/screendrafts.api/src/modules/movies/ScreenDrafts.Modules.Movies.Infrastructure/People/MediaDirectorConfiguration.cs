namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class MediaDirectorConfiguration : IEntityTypeConfiguration<MediaDirector>
{
  public void Configure(EntityTypeBuilder<MediaDirector> builder)
  {
    builder.ToTable(Tables.MediaDirectors);
    builder.HasKey(md => new { md.MediaId, md.DirectorId });

    builder.Property(md => md.MediaId)
      .HasColumnName("media_id")
      .HasConversion(
      id => id.Value,
      value => MediaId.Create(value));

    builder.Property(md => md.DirectorId)
      .HasColumnName("director_id")
      .HasConversion(
      id => id.Value,
      value => PersonId.Create(value));

    builder.HasOne(d => d.Media)
      .WithMany(m => m.MediaDirectors)
      .HasForeignKey(d => d.MediaId);

    builder.HasOne(d => d.Director)
      .WithMany(d => d.MediaDirectors)
      .HasForeignKey(d => d.DirectorId);
  }
}
