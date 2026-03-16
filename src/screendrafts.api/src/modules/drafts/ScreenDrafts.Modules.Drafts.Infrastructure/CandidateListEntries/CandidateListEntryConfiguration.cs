namespace ScreenDrafts.Modules.Drafts.Infrastructure.CandidateListEntries;

internal sealed class CandidateListEntryConfiguration : IEntityTypeConfiguration<CandidateListEntry>
{
  public void Configure(EntityTypeBuilder<CandidateListEntry> builder)
  {
    builder.ToTable(Tables.CandidateListEntries);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.CandidateListEntryIdConverter);

    builder.Property(x => x.DraftPartId)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartIdConverter);

    builder.Property(x => x.TmdbId)
      .IsRequired();

    builder.Property(x => x.MovieId);

    builder.Property(x => x.AddedByPublicId)
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength)
      .IsRequired();

    builder.Property(x => x.Notes)
      .HasMaxLength(1000);

    builder.Property(x => x.CreatedOnUtc)
      .IsRequired();

    builder.Property(x => x.IsPending)
      .IsRequired();

    builder.HasOne<DraftPart>()
      .WithMany()
      .HasForeignKey(x => x.DraftPartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(x => new { x.DraftPartId, x.TmdbId })
      .IsUnique();

    builder.HasIndex(x => new { x.TmdbId, x.IsPending });
  }
}
