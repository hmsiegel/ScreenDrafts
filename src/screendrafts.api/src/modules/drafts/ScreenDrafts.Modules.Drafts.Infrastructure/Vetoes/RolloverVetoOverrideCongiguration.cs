namespace ScreenDrafts.Modules.Drafts.Infrastructure.Vetoes;

internal sealed class RolloverVetoOverrideCongiguration : IEntityTypeConfiguration<RolloverVetoOverride>
{
  public void Configure(EntityTypeBuilder<RolloverVetoOverride> builder)
  {
    builder.ToTable(Tables.RolloverVetoOverrides);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasConversion(
            x => x.Value,
            x => new RolloverVetoOverrideId(x));

    builder.HasOne(rv => rv.Drafter)
      .WithOne(d => d.RolloverVetoOverride)
      .HasForeignKey<RolloverVetoOverride>(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
