namespace ScreenDrafts.Modules.Drafts.Infrastructure.Vetoes;

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

    builder.HasOne(rv => rv.Drafter)
      .WithOne(d => d.RolloverVeto)
      .HasForeignKey<RolloverVeto>(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
