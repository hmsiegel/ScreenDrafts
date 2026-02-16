namespace ScreenDrafts.Modules.Drafts.Infrastructure.GameBoards;

internal sealed class GameBoardConfiguration : IEntityTypeConfiguration<GameBoard>
{
  public void Configure(EntityTypeBuilder<GameBoard> builder)
  {
    builder.ToTable(Tables.GameBoards);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.GameBoardIdConverter);

    builder.HasMany(gb => gb.DraftPositions)
      .WithOne(dp => dp.GameBoard)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(gb => gb.DraftPart)
      .WithOne(d => d.GameBoard)
      .HasForeignKey<GameBoard>(gb => gb.DraftPartId)
      .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

    builder.Property(gb => gb.DraftPartId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPartIdConverter);
  }
}
