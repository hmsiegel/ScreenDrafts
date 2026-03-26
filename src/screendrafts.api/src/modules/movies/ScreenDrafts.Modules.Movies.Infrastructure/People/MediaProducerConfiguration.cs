namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class MediaProducerConfiguration : IEntityTypeConfiguration<MediaProducer>
{
  public void Configure(EntityTypeBuilder<MediaProducer> builder)
  {
    builder.ToTable(Tables.MediaProducers);
    builder.HasKey(md => new { md.MediaId, md.ProducerId });

    builder.Property(md => md.MediaId)
      .HasColumnName("media_id")
      .HasConversion(
      id => id.Value,
      value => MediaId.Create(value));

    builder.Property(md => md.ProducerId)
      .HasColumnName("producer_id")
      .HasConversion(
      id => id.Value,
      value => PersonId.Create(value));

    builder.HasOne(d => d.Media)
      .WithMany(m => m.MediaProducers)
      .HasForeignKey(d => d.MediaId);

    builder.HasOne(d => d.Producer)
      .WithMany(d => d.MediaProducers)
      .HasForeignKey(d => d.ProducerId);
  }
}
