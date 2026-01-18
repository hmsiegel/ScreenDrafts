namespace ScreenDrafts.Modules.Drafts.Infrastructure.Categories;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    builder.ToTable(Tables.Categories);

    builder.HasKey(c => c.Id);

    builder.Property(c => c.Id)
          .ValueGeneratedNever()
          .HasColumnName("id")
          .HasConversion(
            id => id.Value,
            value => CategoryId.Create(value));

    builder.Property(c => c.Name)
      .IsRequired()
      .HasMaxLength(Category.NameMaxLength);

    builder.Property(c => c.Description)
      .HasMaxLength(Category.DescriptionMaxLength);

    builder.HasMany(c => c.DraftCategories)
      .WithOne(dc => dc.Category)
      .HasForeignKey(dc => dc.CategoryId);
  }
}
