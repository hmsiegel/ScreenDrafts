namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DrafterDraftStatsConfiguration : IEntityTypeConfiguration<DrafterDraftStats>
{
  public void Configure(EntityTypeBuilder<DrafterDraftStats> builder)
  {
    builder.ToTable(Tables.DrafterDraftStats);

    builder.HasKey(ds => new { ds.Id, ds.DrafterId, ds.DraftId });

    builder.Property(ds => ds.Id)
      .ValueGeneratedNever()
      .HasConversion(
        id => id.Value,
        value => DrafterDraftStatsId.Create(value));

    builder.Property(ds => ds.DrafterId)
      .IsRequired();

    builder.Property(ds => ds.DraftId)
      .IsRequired();

    builder.Property(ds => ds.StartingVetoes)
      .IsRequired()
      .HasDefaultValue(1);

    builder.Property(ds => ds.RolloversApplied)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(ds => ds.TriviaVetoes)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(ds => ds.StartingVetoOverrides)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(ds => ds.RolloversApplied)
      .IsRequired()
      .HasDefaultValue(0);
  }
}
