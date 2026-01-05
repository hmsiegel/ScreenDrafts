namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftCategoryConfiguration : IEntityTypeConfiguration<DraftCategory>
{
  public void Configure(EntityTypeBuilder<DraftCategory> builder)
  {
    builder.ToTable(Tables.DraftCategories);

    builder.HasKey(dc => new { dc.DraftId, dc.CategoryId });

    builder.HasOne(dc => dc.Draft)
      .WithMany(d => d.DraftCategories)
      .HasForeignKey(dc => dc.DraftId);

    builder.HasOne(dc => dc.Category)
      .WithMany(c => c.DraftCategories)
      .HasForeignKey(dc => dc.CategoryId);
  }
}
