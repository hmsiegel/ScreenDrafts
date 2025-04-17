namespace ScreenDrafts.Modules.Drafts.Infrastructure.TriviaResults;

internal sealed class TriviaResultConfiguration : IEntityTypeConfiguration<TriviaResult>
{
  public void Configure(EntityTypeBuilder<TriviaResult> builder)
  {
    builder.ToTable(Tables.TriviaResults);

    builder.HasKey(tr => tr.Id);

    builder.Property(tr => tr.Id)
      .ValueGeneratedNever()
      .HasConversion(
        id => id.Value,
        value => TriviaResultId.Create(value));

    builder.Property(tr => tr.Position)
      .IsRequired();

    builder.HasOne(tr => tr.Drafter)
      .WithMany()
      .HasForeignKey(tr => tr.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(tr => tr.DrafterTeam)
      .WithMany()
      .HasForeignKey(tr => tr.DrafterTeamId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(tr => tr.Draft)
      .WithMany(d => d.TriviaResults)
      .HasForeignKey(tr => tr.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(tr => tr.DrafterId)
      .IsRequired(false)
      .HasConversion(
        id => id!.Value,
        value => DrafterId.Create(value));

    builder.Property(tr => tr.DrafterTeamId)
      .IsRequired(false)
      .HasConversion(
        id => id!.Value,
        value => DrafterTeamId.Create(value));
  }
}
