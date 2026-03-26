namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class MediaWriterConfiguration : IEntityTypeConfiguration<MediaWriter>
{
  public void Configure(EntityTypeBuilder<MediaWriter> builder)
  {
    builder.ToTable(Tables.MediaWriters);
    builder.HasKey(md => new { md.MediaId, md.WriterId });

    builder.Property(md => md.MediaId)
      .HasColumnName("media_id")
      .HasConversion(
      id => id.Value,
      value => MediaId.Create(value));

    builder.Property(md => md.WriterId)
      .HasColumnName("writer_id")
      .HasConversion(
      id => id.Value,
      value => PersonId.Create(value));

    builder.HasOne(d => d.Media)
      .WithMany(m => m.MediaWriters)
      .HasForeignKey(d => d.MediaId);

    builder.HasOne(d => d.Writer)
      .WithMany(d => d.MediaWriters)
      .HasForeignKey(d => d.WriterId);
  }
}
