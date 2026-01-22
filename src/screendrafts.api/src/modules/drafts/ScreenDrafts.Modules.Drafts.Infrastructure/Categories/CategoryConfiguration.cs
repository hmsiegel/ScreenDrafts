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
          .HasConversion(IdConverters.CategoryIdConverter);

    builder.Property(c => c.Name)
      .IsRequired()
      .HasMaxLength(Category.NameMaxLength);

    builder.Property(c => c.Description)
      .HasMaxLength(Category.DescriptionMaxLength);

    builder.HasMany(c => c.DraftCategories)
      .WithOne(dc => dc.Category)
      .HasForeignKey(dc => dc.CategoryId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(c => c.DraftCategories)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Property(x => x.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(x => x.PublicId)
      .IsUnique();

    builder.Property(x => x.CreatedOnUtc)
      .IsRequired();

    builder.Property(x => x.ModifiedOnUtc);

    builder.Property(x => x.IsDeleted)
      .IsRequired();
  }
}
