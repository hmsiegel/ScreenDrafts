namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

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


    builder.HasOne(x => x.Drafter)
      .WithOne(d => d.RolloverVetoOverride)
      .HasForeignKey<RolloverVetoOverride>("drafterId")
      .OnDelete(DeleteBehavior.Cascade);
  }
}
