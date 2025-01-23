namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;

internal sealed class DrafterConfiguration : IEntityTypeConfiguration<Drafter>
{
  public void Configure(EntityTypeBuilder<Drafter> builder)
  {
    builder.ToTable(Tables.Drafters);

    builder.HasKey(drafter => drafter.Id);

    builder.Property(drafter => drafter.Id)
      .ValueGeneratedNever()
      .HasConversion(
      id => id.Value,
      value => DrafterId.Create(value));

    builder.Property(drafter => drafter.Name)
      .IsRequired()
      .HasMaxLength(100);

    builder.HasOne(d => d.Draft)
      .WithMany(d => d.Drafters)
      .HasForeignKey("draftId")
      .OnDelete(DeleteBehavior.Cascade);
  }
}
