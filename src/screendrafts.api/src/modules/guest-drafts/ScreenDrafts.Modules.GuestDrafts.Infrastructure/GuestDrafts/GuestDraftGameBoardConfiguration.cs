namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftGameBoardConfiguration
  : IEntityTypeConfiguration<GuestDraftGameBoard>
{
  public void Configure(EntityTypeBuilder<GuestDraftGameBoard> builder)
  {
    builder.ToTable(Tables.GuestDraftGameBoards);

    builder.HasKey(gb => gb.Id);

    builder.Property(gb => gb.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftGameBoardIdConverter);

    builder.Property(gb => gb.GuestDraftId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftIdConverter);

    builder.HasIndex(gb => gb.GuestDraftId)
      .IsUnique();

    // Positions -- containment, cascades. GuestDraftPosition has no back-reference
    // nav (unidirectional FK), same pattern as GuestDraft's GameBoard/Picks.
    builder.HasMany(gb => gb.Positions)
      .WithOne()
      .HasForeignKey(p => p.GameBoardId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(gb => gb.Positions)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
