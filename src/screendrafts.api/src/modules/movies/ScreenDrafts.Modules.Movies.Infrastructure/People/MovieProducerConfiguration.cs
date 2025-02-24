namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class MovieProducerConfiguration : IEntityTypeConfiguration<MovieProducer>
{
  public void Configure(EntityTypeBuilder<MovieProducer> builder)
  {
    builder.ToTable(Tables.MovieProducers);

    builder.HasKey(md => new { md.MovieId, md.ProducerId });

    builder.Property(md => md.MovieId)
      .HasColumnName("movie_id")
      .HasConversion(
      id => id.Value,
      value => MovieId.Create(value));

    builder.Property(md => md.ProducerId)
      .HasColumnName("producer_id")
      .HasConversion(
      id => id.Value,
      value => PersonId.Create(value));

    builder.HasOne(d => d.Movie)
      .WithMany(m => m.MovieProducers)
      .HasForeignKey(d => d.MovieId);

    builder.HasOne(d => d.Producer)
      .WithMany(d => d.MovieProducers)
      .HasForeignKey(d => d.ProducerId);
  }
}
