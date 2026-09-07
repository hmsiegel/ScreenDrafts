namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftPositionConfiguration : IEntityTypeConfiguration<GuestDraftPosition>
{
  public void Configure(EntityTypeBuilder<GuestDraftPosition> builder)
  {
    builder.ToTable(Tables.GuestDraftPositions);

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftPositionIdConverter);

    builder.Property(p => p.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(p => p.PublicId)
      .IsUnique();

    builder.Property(p => p.GameBoardId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftGameBoardIdConverter);

    builder.Property(p => p.Name)
      .IsRequired()
      .HasMaxLength(GuestDraftPosition.NameMaxLength);

    // Picks is a plain int[] -- the specific board slot numbers assigned to this
    // position, e.g. Standard's "A" gets [7, 6, 4, 2]. Postgres array column, not
    // a relational collection -- there's no per-slot entity, just numbers.
    builder.Property(p => p.Picks)
      .IsRequired()
      .HasColumnType("integer[]");

    builder.Property(p => p.HasBonusVeto)
      .IsRequired();

    builder.Property(p => p.HasBonusVetoOverride)
      .IsRequired();

    builder.Property(p => p.HasBonusFungibleToken)
      .IsRequired();

    // AssignedToParticipantId is intentionally a bare Guid, not a typed FK/nav --
    // mirrors canonical DraftPosition.AssignedToId's loose-coupling pattern rather
    // than a formal EF relationship.
    builder.Property(p => p.AssignedToParticipantId);
  }
}
