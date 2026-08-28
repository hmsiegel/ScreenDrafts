namespace ScreenDrafts.Modules.Drafts.Infrastructure.Picks;

internal sealed class TeamPickCreditConfiguration : IEntityTypeConfiguration<TeamPickCredit>
{
  public void Configure(EntityTypeBuilder<TeamPickCredit> builder)
  {
    builder.ToTable(Tables.TeamPickCredits);

    builder.HasKey(x => x.Id);

    builder
      .Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.TeamPickCreditIdConverter);

    // Configured from this side as the "many" end, mirroring VetoConfiguration's
    // Pick-to-Veto relationship exactly — a pick can have multiple credited drafters.
    builder
      .Property(x => x.TargetPickId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPickIdConverter);

    builder
      .HasOne(x => x.TargetPick)
      .WithMany("_teamPickCredits")
      .HasForeignKey(x => x.TargetPickId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(x => x.DrafterIdValue).IsRequired();

    // One credit row per drafter per pick — a drafter can't be double-credited for the
    // same team pick even if something re-runs the snapshot logic.
    builder.HasIndex(x => new { x.TargetPickId, x.DrafterIdValue }).IsUnique();
  }
}
