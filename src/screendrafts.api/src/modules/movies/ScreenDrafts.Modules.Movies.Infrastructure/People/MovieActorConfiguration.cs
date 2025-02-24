namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class MovieActorConfiguration : IEntityTypeConfiguration<MovieActor>
{
  public void Configure(EntityTypeBuilder<MovieActor> builder)
  {
    builder.ToTable(Tables.MovieActors);

    builder.HasKey(t => new { t.MovieId, t.ActorId });

    builder.Property(d => d.MovieId)
      .HasColumnName("movie_id")
      .HasConversion(
        movieId => movieId.Value,
        value => MovieId.Create(value));

    builder.Property(d => d.ActorId)
      .HasColumnName("actor_id")
      .HasConversion(
        actorId => actorId.Value,
        value => PersonId.Create(value));

    builder.HasOne(d => d.Movie)
      .WithMany(m => m!.MovieActors)
      .HasForeignKey(d => d.MovieId);

    builder.HasOne(ma => ma.Actor)
      .WithMany(a => a.MovieActors)
      .HasForeignKey(ma => ma.ActorId);
  }
}
