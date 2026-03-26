namespace ScreenDrafts.Modules.Movies.Infrastructure.People;

internal sealed class MediaActorConfiguration : IEntityTypeConfiguration<MediaActor>
{
  public void Configure(EntityTypeBuilder<MediaActor> builder)
  {
    builder.ToTable(Tables.MediaActors);
    builder.HasKey(t => new { t.MediaId, t.ActorId });

    builder.Property(d => d.MediaId)
      .HasColumnName("media_id")
      .HasConversion(
        mediaId => mediaId.Value,
        value => MediaId.Create(value));

    builder.Property(d => d.ActorId)
      .HasColumnName("actor_id")
      .HasConversion(
        actorId => actorId.Value,
        value => PersonId.Create(value));

    builder.HasOne(d => d.Media)
      .WithMany(m => m!.MediaActors)
      .HasForeignKey(d => d.MediaId);

    builder.HasOne(ma => ma.Actor)
      .WithMany(a => a.MediaActors)
      .HasForeignKey(ma => ma.ActorId);
  }
}
