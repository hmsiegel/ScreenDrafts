
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
  public void Configure(EntityTypeBuilder<Series> builder)
  {
    builder.ToTable(Tables.Series);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .ValueGeneratedNever()
        .HasConversion(
            id => id.Value,
            value => SeriesId.Create(value));

    builder.Property(x => x.Name)
      .IsRequired();

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

    builder.Property(x => x.RequiredDraftType)
        .HasConversion(
        x => x != null ? x.Value : (int?)null,
        value => value.HasValue ? DraftType.FromValue(value.Value) : null);

    builder.Property(x => x.DefaultDraftType)
      .HasConversion(
        x => x != null ? x.Value : (int?)null,
        value => value.HasValue ? DraftType.FromValue(value.Value) : null);
  }
}
