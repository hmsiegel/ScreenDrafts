namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafters;

internal sealed class DrafterHonorificsHistoryConfiguration : IEntityTypeConfiguration<DrafterHonorificHistory>
{
  public void Configure(EntityTypeBuilder<DrafterHonorificHistory> builder)
  {
    builder.ToTable(Tables.DraftersHonorificsHistory);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DrafterHonorificHistoryIdConverter);

    builder.Property(x => x.DrafterIdValue)
      .IsRequired();

    builder.HasIndex(x => new { x.DrafterIdValue, x.AchievedAt })
      .HasDatabaseName("ix_drafter_honorifics_history_drafter_id_achieved_at");

    builder.Property(x => x.Honorific)
      .IsRequired()
      .HasConversion(
        h => h.Value,
        value => DrafterHonorific.FromValue(value));

    builder.Property(x => x.AppearanceCount)
      .IsRequired();

    builder.Property(x => x.AchievedAt)
      .IsRequired();
  }
}
