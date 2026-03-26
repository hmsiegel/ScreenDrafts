namespace ScreenDrafts.Modules.Movies.Infrastructure.ProductionCompanies;

internal sealed class MediaProductionCompanyConfiguration : IEntityTypeConfiguration<MediaProductionCompany>
{
  public void Configure(EntityTypeBuilder<MediaProductionCompany> builder)
  {
    builder.ToTable(Tables.MediaProductionCompanies);

    builder.HasKey(md => new { md.MediaId, md.ProductionCompanyId });

    builder.Property(md => md.MediaId)
      .HasColumnName("media_id")
      .HasConversion(
      id => id.Value,
      value => MediaId.Create(value));

    builder.HasOne(d => d.Media)
      .WithMany(m => m.MediaProductionCompanies)
      .HasForeignKey(d => d.MediaId);

    builder.HasOne(d => d.ProductionCompany)
      .WithMany(d => d.MediaProductionCompanies)
      .HasForeignKey(d => d.ProductionCompanyId);
  }
}
