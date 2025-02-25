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

    builder.Property(d => d.ReadableId)
      .ValueGeneratedOnAdd();

    builder.Property(drafter => drafter.Name)
      .IsRequired()
      .HasMaxLength(100);

    builder.HasMany(d => d.Picks)
      .WithOne(p => p.Drafter)
      .HasForeignKey(d => d.DrafterId);

    builder.HasOne(d => d.RolloverVeto)
      .WithOne(rv => rv.Drafter)
      .HasForeignKey<RolloverVeto>(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(d => d.RolloverVetoOverride)
      .WithOne(rv => rv.Drafter)
      .HasForeignKey<RolloverVetoOverride>(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(d => d.Drafts)
      .WithMany(d => d.Drafters);
  }
}
