namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions.PredictionContestants;

internal sealed class PredictionContestantConfiguration : IEntityTypeConfiguration<PredictionContestant>
{
  public void Configure(EntityTypeBuilder<PredictionContestant> builder)
  {
    builder.ToTable(Tables.PredictionContestants);

    builder.HasKey(t => t.Id);

    builder.Property(s => s.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.ContestantIdConverter);

    builder.Property(c => c.PersonId)
      .HasConversion(IdConverters.NullablePersonIdConverter);

    builder.Property(c => c.DisplayName)
      .IsRequired()
      .HasMaxLength(100);

    builder.Property(c => c.IsActive)
      .IsRequired();

    builder.HasIndex(c => c.PersonId)
      .IsUnique();

    builder.Property(x => x.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(s => s.PublicId)
      .IsUnique();
  }
}
