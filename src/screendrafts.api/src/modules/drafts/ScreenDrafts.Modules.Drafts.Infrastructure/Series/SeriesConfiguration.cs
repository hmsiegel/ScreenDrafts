namespace ScreenDrafts.Modules.Drafts.Infrastructure.Series;

using Series = Domain.Drafts.Entities.Series;

internal sealed class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
  public void Configure(EntityTypeBuilder<Series> builder)
  {
    builder.ToTable(Tables.Series);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .ValueGeneratedNever()
        .HasConversion(IdConverters.SeriesIdConverter);

    builder.Property(x => x.Name)
      .IsRequired()
      .HasMaxLength(Series.MaxNameLength);

    builder.Property(x => x.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);
    builder.HasIndex(x => x.PublicId)
      .IsUnique();

    builder.Property(x => x.Kind)
      .IsRequired()
      .HasConversion(
      x => x.Value,
      value => SeriesKind.FromValue(value));

    builder.Property(x => x.CanonicalPolicy)
      .IsRequired()
      .HasConversion(
      x => x.Value,
      value => CanonicalPolicy.FromValue(value));

    builder.Property(x => x.ContinuityScope)
      .IsRequired()
      .HasConversion(
      x => x.Value,
      value => ContinuityScope.FromValue(value));

    builder.Property(x => x.ContinuityDateRule)
      .IsRequired()
      .HasConversion(
      x => x.Value,
      value => ContinuityDateRule.FromValue(value));

    builder.Property(x => x.DefaultDraftType)
      .HasConversion(
        x => x != null ? x.Value : (int?)null,
        value => value.HasValue ? DraftType.FromValue(value.Value) : null);

    builder.Property(x => x.AllowedDraftTypes)
      .IsRequired()
      .HasConversion(
        x => (int)x,
        value => (DraftTypeMask)value);

    builder.Property(x => x.CreatedAtUtc)
      .IsRequired();

    builder.Property(x => x.UpdatedAtUtc);
  }
}
