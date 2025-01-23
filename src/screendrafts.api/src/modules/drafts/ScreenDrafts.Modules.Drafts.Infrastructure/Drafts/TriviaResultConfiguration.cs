namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class TriviaResultConfiguration : IEntityTypeConfiguration<TriviaResult>
{
  public void Configure(EntityTypeBuilder<TriviaResult> builder)
  {
    builder.ToTable(Tables.TriviaResults);

    builder.HasKey(tr => new {tr.Id, tr.DraftId, tr.DrafterId, tr.Position });

    builder.Property(tr => tr.Id)
      .ValueGeneratedNever()
      .HasConversion(
        id => id.Value,
        value => new TriviaResultId(value));

    builder.Property(tr => tr.AwardIsVeto)
      .IsRequired();

    builder.Property(tr => tr.Position)
      .IsRequired();
  }
}
