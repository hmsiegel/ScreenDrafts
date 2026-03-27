namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafters;

internal sealed class DrafterCanonicalAppearanceConfiguration : IEntityTypeConfiguration<DrafterCanonicalAppearance>
{
  public void Configure(EntityTypeBuilder<DrafterCanonicalAppearance> builder)
  {
    builder.ToTable(Tables.DrafterCanonicalAppearances);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DrafterCanonicalAppearanceIdConverter);

    builder.Property(x => x.DrafterIdValue)
      .IsRequired();

    builder.Property(x => x.DraftPartPublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(x => new { x.DrafterIdValue, x.DraftPartPublicId })
      .IsUnique()
      .HasDatabaseName("ux_drafter_canonical_appearances_drafter_id_part_id");

    builder.HasIndex(x => x.DrafterIdValue)
      .HasDatabaseName("ix_drafter_canonical_appearances_drafter_id_value");

    builder.Property(x => x.HasMainFeedRelease)
      .IsRequired();

    builder.Property(x => x.AppearedAt)
      .IsRequired();
  }
}
