namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftPartConfiguration : IEntityTypeConfiguration<DraftPart>
{
  public void Configure(EntityTypeBuilder<DraftPart> builder)
  {
    builder.ToTable(Tables.DraftParts);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(
      x => x.Value,
      value => DraftPartId.Create(value));

    builder.Property(x => x.PartIndex)
      .IsRequired();

    builder.HasIndex(x => new { x.DraftId, x.PartIndex })
      .IsUnique();

    builder.Navigation(x => x.Releases)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(x => x.Releases)
      .WithOne(r => r.DraftPart)
      .HasForeignKey(r => r.PartId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(d => d.TotalPicks)
          .IsRequired();

    builder.Property(d => d.TotalDrafters)
          .IsRequired();

    builder.Property(d => d.TotalHosts)
          .IsRequired();

    builder.HasOne(d => d.GameBoard)
      .WithOne(gb => gb.DraftPart)
      .HasForeignKey<GameBoard>(gb => gb.Id);

    builder.HasMany(d => d.Picks)
      .WithOne(p => p.DraftPart)
      .HasForeignKey(p => p.DraftPartId);

    builder
      .Metadata
      .FindNavigation(nameof(DraftPart.Picks))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(d => d.DrafterStats)
      .WithOne(ds => ds.DraftPart)
      .HasForeignKey(ds => ds.DraftPartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(d => d.DrafterStats)
      .HasField("_drafterDraftStats")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Ignore(d => d.PrimaryHost);
    builder.Ignore(d => d.CoHosts);
  }
}
