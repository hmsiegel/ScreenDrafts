namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftReleaseDateConfiguration : IEntityTypeConfiguration<DraftReleaseDate>
{
  public void Configure(EntityTypeBuilder<DraftReleaseDate> builder)
  {
    builder.HasKey(dr => new { dr.DraftId, dr.ReleaseDate });

    builder.HasOne(dr => dr.Draft)
      .WithMany(d => d.ReleaseDates)
      .HasForeignKey(dr => dr.DraftId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
