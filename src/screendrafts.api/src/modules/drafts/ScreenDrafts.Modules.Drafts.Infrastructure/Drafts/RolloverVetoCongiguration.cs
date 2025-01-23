namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class RolloverVetoCongiguration : IEntityTypeConfiguration<RolloverVeto>
{
  public void Configure(EntityTypeBuilder<RolloverVeto> builder)
  {
    builder.ToTable(Tables.RolloverVetoes);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasConversion(
            x => x.Value,
            x => new RolloverVetoId(x));


    builder.HasOne(x => x.Drafter)
      .WithOne(d => d.RolloverVeto)
      .HasForeignKey<RolloverVeto>("drafterId")
      .OnDelete(DeleteBehavior.Cascade);
  }
}
