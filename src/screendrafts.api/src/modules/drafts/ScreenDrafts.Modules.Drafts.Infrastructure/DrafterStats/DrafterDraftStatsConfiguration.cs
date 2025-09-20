namespace ScreenDrafts.Modules.Drafts.Infrastructure.DrafterStats;

internal sealed class DrafterDraftStatsConfiguration : IEntityTypeConfiguration<DrafterDraftStats>
{
  public void Configure(EntityTypeBuilder<DrafterDraftStats> builder)
  {
    builder.ToTable(Tables.DrafterDraftStats);

    builder.HasKey(ds => new { ds.Id, ds.DraftPartId });

    builder.Property(ds => ds.Id)
      .ValueGeneratedNever()
      .HasConversion(
        id => id.Value,
        value => DrafterDraftStatsId.Create(value));

    // --- DraftId (required) ---
    builder.Property(ds => ds.DraftPartId)
      .HasConversion(
        id => id.Value,
        value => DraftPartId.Create(value));

    builder.HasOne(ds => ds.DraftPart)
      .WithMany(d => d.DrafterStats)
      .HasForeignKey(d => d.DraftPartId)
      .IsRequired();

    // --- DrafterId (optional) ---
    builder.Property(ds => ds.DrafterId)
      .IsRequired(false)
      .HasConversion(
        id => id!.Value,
        value => DrafterId.Create(value));

    builder.HasOne(ds => ds.Drafter)
      .WithMany()
      .HasForeignKey(builder => builder.DrafterId)
      .IsRequired(false);

    // --- DrafterTeamId (optional) ---
    builder.Property(ds => ds.DrafterTeamId)
      .IsRequired(false)
      .HasConversion(
        id => id!.Value,
        value => DrafterTeamId.Create(value));

    builder.HasOne(ds => ds.DrafterTeam)
      .WithMany()
      .HasForeignKey(builder => builder.DrafterTeamId)
      .IsRequired(false);

    // --- Scalar Properties ---
    builder.Property(ds => ds.StartingVetoes)
      .IsRequired()
      .HasDefaultValue(1);
    builder.Property(ds => ds.RolloverVeto).IsRequired();
    builder.Property(ds => ds.RolloverVetoOverride).IsRequired(false);
    builder.Property(ds => ds.TriviaVetoes).IsRequired();
    builder.Property(ds => ds.TriviaVetoOverrides).IsRequired(false);
    builder.Property(ds => ds.CommissionerOverrides).IsRequired();
    builder.Property(ds => ds.VetoesUsed).IsRequired();
    builder.Property(ds => ds.VetoOverridesUsed).IsRequired(false);

    // --- Ignore computed properties ---
    builder.Ignore(ds => ds.TotalVetoes);
    builder.Ignore(ds => ds.TotalVetoOverrides);
    builder.Ignore(ds => ds.VetoesRollingOver);
    builder.Ignore(ds => ds.VetoOverridesRollingOver);
  }
}
