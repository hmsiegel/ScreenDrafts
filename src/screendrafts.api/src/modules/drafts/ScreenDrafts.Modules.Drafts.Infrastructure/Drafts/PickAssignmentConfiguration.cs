namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class PickAssignmentConfiguration : IEntityTypeConfiguration<PickAssignment>
{
  public void Configure(EntityTypeBuilder<PickAssignment> builder)
  {
    builder.ToTable(Tables.PickAssignments);

    builder.HasKey(pa => new { pa.GameBoardId, pa.Position });

    builder.Property(pa => pa.Position)
      .IsRequired();

    builder.Property(pa => pa.DrafterId)
      .IsRequired();

    builder.Property(pa => pa.ExtraVeto)
      .IsRequired();

    builder.Property(pa => pa.ExtraVetoOverride)
      .IsRequired();
  }
}
