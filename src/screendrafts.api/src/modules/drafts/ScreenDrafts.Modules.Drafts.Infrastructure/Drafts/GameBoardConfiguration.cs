namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

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

    builder.Property(e => e.DraftId)
      .IsRequired();

    builder.HasOne(e => e.Draft)
      .WithOne(g => g.GameBoard)
      .HasForeignKey<GameBoard>("draftId")
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(gb => gb.PickAssignments)
      .WithOne()
      .HasForeignKey("gameBoardId")
      .OnDelete(DeleteBehavior.Cascade);
  }
}
