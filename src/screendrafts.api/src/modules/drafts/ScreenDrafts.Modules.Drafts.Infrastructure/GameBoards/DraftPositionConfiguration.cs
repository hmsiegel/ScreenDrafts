using ScreenDrafts.Modules.Drafts.Infrastructure.Converters;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.GameBoards;

internal sealed class DraftPositionConfiguration : IEntityTypeConfiguration<DraftPosition>
{
  public void Configure(EntityTypeBuilder<DraftPosition> builder)
  {
    builder.ToTable(Tables.DraftPositions);

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever()
      .HasConversion(
        v => v.Value,
        v => DraftPositionId.Create(v));

    builder.Property(e => e.Name)
      .IsRequired()
      .HasMaxLength(DraftPosition.NameMaxLength);

    builder.Property(dp => dp.HasBonusVeto)
      .IsRequired();

    builder.Property(dp => dp.HasBonusVetoOverride)
      .IsRequired();

    builder.Property(dp => dp.Picks)
      .HasListOfPicksConverter();
  }
}
