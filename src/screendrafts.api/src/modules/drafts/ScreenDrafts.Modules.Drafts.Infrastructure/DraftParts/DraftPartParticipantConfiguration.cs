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
    builder.Property(x => x.RolloverVeto).IsRequired();
    builder.Property(x => x.RolloverVetoOverride).IsRequired();
    builder.Property(x => x.TriviaVetoes).IsRequired();
    builder.Property(x => x.TriviaVetoOverrides).IsRequired();
    builder.Property(x => x.CommissionerOverrides).IsRequired();

    // Usage Counters
    builder.Property(x => x.VetoesUsed).IsRequired();
    builder.Property(x => x.VetoOverridesUsed).IsRequired();


    builder.HasIndex(dpp => new { dpp.DraftPartId, dpp.ParticipantIdValue, dpp.ParticipantKindValue })
      .IsUnique()
      .HasDatabaseName("ux_draft_part_participants_unique");

    // Collections (private backing fields)
    builder.HasMany(x => x.Picks)
      .WithOne(p => p.PlayedByParticipant)
      .HasForeignKey(p => p.PlayedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(x => x.Vetoes)
      .WithOne(v => v.IssuedByParticipant)
      .HasForeignKey(v => v.IssuedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(x => x.VetoOverrides)
      .WithOne(vo => vo.IssuedByParticipant)
      .HasForeignKey(vo => vo.IssuedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    // Make EF use field access for collections
    builder.Navigation(dpp => dpp.Picks)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(dpp => dpp.Vetoes)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(dpp => dpp.VetoOverrides)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
