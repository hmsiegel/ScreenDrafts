namespace ScreenDrafts.Modules.Drafts.Infrastructure.GameBoards;

internal sealed class GameBoardConfiguration : IEntityTypeConfiguration<GameBoard>
{
  public void Configure(EntityTypeBuilder<GameBoard> builder)
  {
    builder.ToTable(Tables.GameBoards);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever()
      .HasConversion(
        v => v.Value,
        v => GameBoardId.Create(v));

    builder.HasMany(gb => gb.DraftPositions)
      .WithOne(dp => dp.GameBoard)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(gb => gb.Draft)
      .WithOne(d => d.GameBoard)
      .HasForeignKey<GameBoard>(gb => gb.DraftId)
      .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
  }
}
