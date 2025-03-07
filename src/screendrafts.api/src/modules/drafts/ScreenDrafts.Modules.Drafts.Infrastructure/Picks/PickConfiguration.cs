namespace ScreenDrafts.Modules.Drafts.Infrastructure.Picks;

internal sealed class PickConfiguration : IEntityTypeConfiguration<Pick>
{
  public void Configure(EntityTypeBuilder<Pick> builder)
  {
    builder.ToTable(Tables.Picks);

    builder.HasKey(p => p.Id);

    builder.HasOne(p => p.Movie)
      .WithMany(m => m.Picks)
      .HasForeignKey(p => p.MovieId);

    builder.HasOne(p => p.Veto)
      .WithOne(v => v.Pick)
      .HasForeignKey<Pick>(p => p.VetoId);

    builder.HasOne(p => p.Drafter)
      .WithMany(d => d.Picks)
      .HasForeignKey(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(p => p.Draft)
      .WithMany(d => d.Picks)
      .HasForeignKey(p => p.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(p => p.DraftId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(
        draftId => draftId.Value,
        value => DraftId.Create(value));

    builder.Property(p => p.Position)
      .IsRequired();
  }
}
