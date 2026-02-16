namespace ScreenDrafts.Modules.Drafts.Infrastructure.TriviaResults;

internal sealed class TriviaResultConfiguration : IEntityTypeConfiguration<TriviaResult>
{
  public void Configure(EntityTypeBuilder<TriviaResult> builder)
  {
    builder.ToTable(Tables.TriviaResults);

    builder.HasKey(tr => tr.Id);

    builder.Property(tr => tr.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.TriviaResultsIdConverter);

    builder.Property(tr => tr.Position)
      .IsRequired();

    builder.Property(tr => tr.QuestionsWon)
      .IsRequired();

    builder.ComplexProperty(tr => tr.ParticipantId, participantBuilder =>
    {
      participantBuilder.Property(p => p.Value)
        .HasColumnName("participant_id")
        .IsRequired();

      participantBuilder.Property(p => p.Kind)
        .HasColumnName("participant_kind")
        .IsRequired()
        .HasConversion(IdConverters.ParticipantKindConverter);
    });


    builder.HasOne(tr => tr.DraftPart)
      .WithMany(d => d.TriviaResults)
      .HasForeignKey(tr => tr.DraftPartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(tr => tr.DraftPartId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPartIdConverter);
  }
}
