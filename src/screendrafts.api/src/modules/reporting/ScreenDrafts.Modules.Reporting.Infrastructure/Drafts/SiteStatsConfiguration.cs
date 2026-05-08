namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class SiteStatsConfiguration : IEntityTypeConfiguration<SiteStats>
{
  public void Configure(EntityTypeBuilder<SiteStats> builder)
  {
    builder.ToTable(Tables.SiteStats);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id);

    builder.Property(x => x.VetoesCount);

    builder.Property(x => x.UpdatedAt);
  }
}
