namespace ScreenDrafts.Modules.Drafts.Infrastructure.Blessings;

internal sealed class RolloverVetoConfiguration : IEntityTypeConfiguration<RolloverVeto>
{
  public void Configure(EntityTypeBuilder<RolloverVeto> builder)
  {
    builder.ToTable(Tables.RolloverVetoes);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasConversion(
            x => x.Value,
            x => new RolloverVetoId(x));

    builder.Property(x => x.DrafterId)
        .HasConversion(
            x => x!.Value,
            x => DrafterId.Create(x));

    builder.Property(x => x.DrafterTeamId)
        .HasConversion(
            x => x!.Value,
            x => DrafterTeamId.Create(x));

    builder.HasOne(rv => rv.Drafter)
      .WithOne(d => d.RolloverVeto)
      .HasForeignKey<RolloverVeto>(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(rv => rv.DrafterTeam)
      .WithOne(d => d.RolloverVeto)
      .HasForeignKey<RolloverVeto>(rv => rv.DrafterTeamId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
