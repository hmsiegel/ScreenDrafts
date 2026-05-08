namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class DraftSpotlightConfiguration : IEntityTypeConfiguration<DraftSpotlight>
{
  public void Configure(EntityTypeBuilder<DraftSpotlight> builder)
  {
    builder.ToTable(Tables.DraftSpotlights);

    builder.HasKey(x => x.Id);

    builder
      .Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftSpotlightIdConverter);

    builder.Property(x => x.DraftPublicId).IsRequired();

    builder.Property(x => x.SpotlightDescription).IsRequired();

    builder.Property(x => x.SpotifyUrl);
    builder.Property(x => x.IsActive);
    builder.Property(x => x.IsPinned);
    builder.Property(x => x.ActivatedAtUtc);
    builder.Property(x => x.CreatedAtUtc);

    // Partial unique index — enforced in migration script, not expressible
    // purely via Fluent API for filtered indexes. Defined here for discoverability;
    // the migration will emit the correct WHERE clause.
    builder
      .HasIndex(x => x.IsActive)
      .HasDatabaseName("uix_draft_spotlights_active")
      .HasFilter("is_active = true")
      .IsUnique();
  }
}
