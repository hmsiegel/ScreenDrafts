namespace ScreenDrafts.Modules.Drafts.Infrastructure.DrafterStats;

internal sealed class DrafterDraftStatsConfiguration : IEntityTypeConfiguration<DrafterDraftStats>
{
  public void Configure(EntityTypeBuilder<DrafterDraftStats> builder)
  {
    builder.ToTable(Tables.DrafterDraftStats);

    builder.HasKey(ds => new { ds.Id, ds.DraftId });

    builder.Property(ds => ds.Id)
      .ValueGeneratedNever()
      .HasConversion(
        id => id.Value,
        value => DrafterDraftStatsId.Create(value));

    builder.HasOne(ds => ds.Drafter)
      .WithMany(d => d.DraftStats)
      .IsRequired(false);

    builder.HasOne(ds => ds.DrafterTeam)
      .WithMany(dt => dt.DraftStats)
      .IsRequired(false);

    builder.Property(ds => ds.DrafterId)
      .IsRequired(false)
      .HasConversion(
        id => id!.Value,
        value => DrafterId.Create(value));

    builder.Property(ds => ds.DrafterTeamId)
      .IsRequired(false)
      .HasConversion(
        id => id!.Value,
        value => DrafterTeamId.Create(value));

    builder.HasOne(ds => ds.Draft)
      .WithMany(d => d.DrafterStats);

    builder.Property(ds => ds.StartingVetoes)
      .IsRequired()
      .HasDefaultValue(1);

    builder.Property(ds => ds.RolloversApplied)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(ds => ds.TriviaVetoes)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(ds => ds.StartingVetoOverrides)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(ds => ds.RolloversApplied)
      .IsRequired()
      .HasDefaultValue(0);
  }
}
