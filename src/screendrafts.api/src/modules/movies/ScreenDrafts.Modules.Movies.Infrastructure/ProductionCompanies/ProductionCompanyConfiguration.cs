namespace ScreenDrafts.Modules.Movies.Infrastructure.ProductionCompanies;

internal sealed class ProductionCompanyConfiguration : IEntityTypeConfiguration<ProductionCompany>
{
  public void Configure(EntityTypeBuilder<ProductionCompany> builder)
  {
    builder.ToTable(Tables.ProductionCompanies);

    builder.HasKey(pc => pc.Id);

    builder.HasIndex(pc => pc.ImdbId);

    builder.Property(pc => pc.Name)
      .IsRequired()
      .HasMaxLength(ProductionCompany.MaxLength);
  }
}
