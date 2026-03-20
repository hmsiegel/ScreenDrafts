namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftPartParticipantConfiguration : IEntityTypeConfiguration<DraftPartParticipant>
{
  public void Configure(EntityTypeBuilder<DraftPartParticipant> builder)
  {
    builder.ToTable(Tables.DraftPartParticipants);

    // Id
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartParticipantIdConverter);

    // DraftPart FK
    builder.Property(dpp => dpp.DraftPartId)
        .IsRequired()
        .ValueGeneratedNever()
        .HasConversion(IdConverters.DraftPartIdConverter);

    builder.HasOne(dpp => dpp.DraftPart)
        .WithMany("_draftPartParticipants")
        .HasForeignKey(dpp => dpp.DraftPartId)
        .OnDelete(DeleteBehavior.Cascade);

    // Participant Polymorphic Identity
    builder.Property(dpp => dpp.ParticipantIdValue)
      .IsRequired();

    builder.Property(dpp => dpp.ParticipantKindValue)
      .IsRequired()
      .HasConversion(IdConverters.ParticipantKindConverter);

    builder.Ignore(dpp => dpp.ParticipantId);

    // Inventory Inputs
    builder.Property(x => x.StartingVetoes).IsRequired();

    builder.Property(x => x.VetoesRollingIn)
      .HasColumnName("vetoes_rolling_in")
      .IsRequired();

    builder.Property(x => x.VetoOverridesRollingIn)
      .HasColumnName("veto_overrides_rolling_in")
      .IsRequired();

    builder.Property(x => x.AwardedVetoes)
      .HasColumnName("awarded_vetoes")
      .IsRequired();

    builder.Property(x => x.AwardedVetoOverrides)
      .HasColumnName("awarded_veto_overrides")
      .IsRequired();

    builder.Property(x => x.CommissionerOverrides)
      .HasColumnName("commissioner_overrides")
      .IsRequired();

    // Usage Counters
    builder.Property(x => x.VetoesUsed).IsRequired();
    builder.Property(x => x.VetoOverridesUsed).IsRequired();


    builder.HasIndex(dpp => new { dpp.DraftPartId, dpp.ParticipantIdValue, dpp.ParticipantKindValue })
      .IsUnique()
      .HasDatabaseName("ux_draft_part_participants_unique");

    // Computed properties - not stored
    builder.Ignore(x => x.TotalVetoes);
    builder.Ignore(x => x.TotalVetoOverrides);
    builder.Ignore(x => x.VetoesRollingOut);
    builder.Ignore(x => x.VetoOverridesRollingOut);
  }

}
