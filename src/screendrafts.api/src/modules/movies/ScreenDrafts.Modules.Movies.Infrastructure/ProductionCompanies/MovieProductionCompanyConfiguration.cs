namespace ScreenDrafts.Modules.Movies.Infrastructure.ProductionCompanies;

internal sealed class MovieProductionCompanyConfiguration : IEntityTypeConfiguration<MovieProductionCompany>
{
  public void Configure(EntityTypeBuilder<MovieProductionCompany> builder)
  {
    builder.ToTable(Tables.MovieProductionCompanies);

    builder.HasKey(md => new { md.MovieId, md.ProductionCompanyId });

    builder.Property(md => md.MovieId)
      .HasColumnName("movie_id")
      .HasConversion(
      id => id.Value,
      value => MovieId.Create(value));

    builder.HasOne(d => d.Movie)
      .WithMany(m => m.MovieProductionCompanies)
      .HasForeignKey(d => d.MovieId);

    builder.HasOne(d => d.ProductionCompany)
      .WithMany(d => d.MovieProductionCompanies)
      .HasForeignKey(d => d.ProductionCompanyId);
  }
}
