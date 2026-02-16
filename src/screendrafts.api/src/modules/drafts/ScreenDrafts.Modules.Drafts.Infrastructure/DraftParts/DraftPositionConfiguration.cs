namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftPositionConfiguration : IEntityTypeConfiguration<DraftPosition>
{
  public void Configure(EntityTypeBuilder<DraftPosition> builder)
  {
    builder.ToTable(Tables.DraftPositions);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPositionIdConverter);


    builder.Property(e => e.Name)
      .IsRequired()
      .HasMaxLength(DraftPosition.NameMaxLength);

    builder.Property(dp => dp.HasBonusVeto)
      .IsRequired();

    builder.Property(dp => dp.HasBonusVetoOverride)
      .IsRequired();

    builder.Property(dp => dp.Picks)
      .HasListOfPicksConverter();

    builder.HasOne(dp => dp.GameBoard)
      .WithMany(gb => gb.DraftPositions)
      .HasForeignKey(dp => dp.GameBoardId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
