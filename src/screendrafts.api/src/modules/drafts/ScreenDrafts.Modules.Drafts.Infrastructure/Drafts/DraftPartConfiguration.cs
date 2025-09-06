
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftPartConfiguration : IEntityTypeConfiguration<DraftPart>
{
  public void Configure(EntityTypeBuilder<DraftPart> builder)
  {
    builder.ToTable(Tables.DraftParts);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(
      x => x.Value,
      value => DraftPartId.Create(value));

    builder.Property(x => x.PartIndex)
      .IsRequired();

    builder.HasIndex(x => new { x.DraftId, x.PartIndex })
      .IsUnique();

    builder.Navigation(x => x.Releases)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(x => x.Releases)
      .WithOne(r => r.DraftPart)
      .HasForeignKey(r => r.PartId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
