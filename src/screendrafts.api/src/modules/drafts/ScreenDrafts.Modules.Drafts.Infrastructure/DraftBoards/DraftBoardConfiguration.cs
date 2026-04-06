namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftBoards;

internal sealed class DraftBoardConfiguration : IEntityTypeConfiguration<DraftBoard>
{
  public void Configure(EntityTypeBuilder<DraftBoard> builder)
  {
    builder.ToTable(Tables.DraftBoards);

    builder.HasKey(t => t.Id);

    builder.Property(b => b.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftBoardIdConverter);

    builder.Property(b => b.DraftId)
      .IsRequired()
      .HasConversion(IdConverters.DraftIdConverter);

    builder.ComplexProperty(b => b.Participant, participantBuilder =>
    {
      participantBuilder.Property(p => p.Value)
        .HasColumnName("participant_id")
        .IsRequired();

      participantBuilder.Property(p => p.Kind)
        .HasColumnName("participant_kind")
        .IsRequired()
        .HasConversion(EnumConverters.ParticipantKindConverter);
    });

    builder.Property(b => b.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.Property(b => b.CreatedAtUtc)
      .IsRequired();

    builder.Property(b => b.UpdatedAtUtc);

    builder.HasIndex(b => b.DraftId);

    builder.HasIndex(b => b.PublicId)
      .IsUnique();

    builder.OwnsMany(b => b.Items, a =>
    {
      a.ToTable(Tables.DraftBoardItems);
      a.WithOwner().HasForeignKey("draft_board_id");
      a.Property(i => i.TmdbId)
        .IsRequired();
      a.Property(i => i.Notes)
        .HasMaxLength(500);
      a.Property(i => i.Priority);

      a.HasIndex("draft_board_id", nameof(DraftBoardItem.TmdbId))
        .IsUnique();
    });
  }
}
