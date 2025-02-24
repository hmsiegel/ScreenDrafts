namespace ScreenDrafts.Modules.Drafts.Infrastructure.Picks;

internal sealed class PickConfiguration : IEntityTypeConfiguration<Pick>
{
  public void Configure(EntityTypeBuilder<Pick> builder)
  {
    builder.ToTable(Tables.Picks);

    builder.HasKey(p => p.Id);

    builder.HasOne(p => p.Movie)
      .WithOne(m => m.Pick)
      .HasForeignKey<Pick>(p => p.MovieId);

    builder.HasOne(p => p.Veto)
      .WithOne(v => v.Pick)
      .HasForeignKey<Pick>(p => p.VetoId);

    builder.HasOne(p => p.Drafter)
      .WithOne(d => d.Pick)
      .HasForeignKey<Pick>(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(p => p.Position)
      .IsRequired();
  }
}
