namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Drafts;

internal sealed class GameBoardConfiguration : IEntityTypeConfiguration<GameBoard>
{
  public void Configure(EntityTypeBuilder<GameBoard> builder)
  {
    builder.ToTable(Tables.GameBoards);

    builder.HasKey(gb => gb.Id);

    builder
      .Property(gb => gb.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GameBoardIdConverter);

    builder.Property(gb => gb.DraftId).IsRequired().HasConversion(IdConverters.DraftIdConverter);

    builder.HasIndex(gb => gb.DraftId).IsUnique();

    // Positions -- containment, cascades. Position has no back-reference
    // nav (unidirectional FK), same pattern as Draft's GameBoard/Picks.
    builder
      .HasMany(gb => gb.Positions)
      .WithOne()
      .HasForeignKey(p => p.GameBoardId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(gb => gb.Positions).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
