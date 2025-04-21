namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class PickConfiguration : IEntityTypeConfiguration<Pick>
{
  public void Configure(EntityTypeBuilder<Pick> builder)
  {
    builder.ToTable(Tables.Picks);

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Id)
      .ValueGeneratedNever()
      .HasConversion(
        pickId => pickId.Value,
        value => PickId.Create(value));

    builder.HasOne(p => p.Movie)
      .WithMany(m => m.Picks)
      .HasForeignKey(p => p.MovieId);

    builder.Property(p => p.DrafterId)
      .ValueGeneratedNever()
      .HasConversion(drafterId => drafterId!.Value,
      value => DrafterId.Create(value))
      .IsRequired(false);

    builder.Property(p => p.DrafterTeamId)
      .ValueGeneratedNever()
      .HasConversion(drafterTeamId => drafterTeamId!.Value,
      value => DrafterTeamId.Create(value))
      .IsRequired(false);

    builder.HasOne(p => p.Drafter)
      .WithMany(d => d.Picks)
      .HasForeignKey(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(p => p.DrafterTeam)
      .WithMany(dt => dt.Picks)
      .HasForeignKey(p => p.DrafterTeamId)
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

    builder.Property(p => p.PlayOrder)
      .IsRequired();

    builder.HasIndex(p => new { p.DraftId, p.Position, p.MovieId, p.PlayOrder })
      .IsUnique();

    builder.HasIndex(p => new { p.DraftId, p.PlayOrder });

  }
}
