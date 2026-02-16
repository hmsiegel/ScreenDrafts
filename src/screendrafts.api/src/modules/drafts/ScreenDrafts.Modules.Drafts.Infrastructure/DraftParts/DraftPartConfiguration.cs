namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftPartConfiguration : IEntityTypeConfiguration<DraftPart>
{
  public void Configure(EntityTypeBuilder<DraftPart> builder)
  {
    builder.ToTable(Tables.DraftParts);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartIdConverter);

    builder.Property(x => x.PartIndex)
      .IsRequired();

    builder.HasIndex(x => new { x.DraftId, x.PartIndex })
      .IsUnique();

    // Draft
    builder.Property(x => x.DraftId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftIdConverter);

    // Series
    builder.Property(x => x.SeriesId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.SeriesIdConverter);

    // Releases
    builder.Ignore(b => b.Releases);

    builder.HasMany<DraftRelease>("_releases")
      .WithOne(r => r.DraftPart)
      .HasForeignKey(r => r.PartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation("_releases")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    // Status
    builder.Property(d => d.Status)
      .HasConversion(
        dps => dps.Value,
        value => DraftPartStatus.FromValue(value));

    builder.Property(d => d.CreatedAtUtc)
      .IsRequired();

    builder.Property(d => d.UpdatedAtUtc);

    builder.HasOne(d => d.GameBoard)
      .WithOne(gb => gb.DraftPart)
      .HasForeignKey<GameBoard>(gb => gb.Id);

    // Picks
    builder.Ignore(builder => builder.Picks);

    builder.HasMany<Pick>("_picks")
      .WithOne(p => p.DraftPart)
      .HasForeignKey(p => p.DraftPartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation("_picks")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Property(dp => dp.MovieVersionPolicyType)
      .HasConversion<int>()
      .IsRequired();

    builder.OwnsMany<RequiredMovieVersion>("_requiredMovieVersions", b =>
    {
      b.ToTable(Tables.DraftPartRequiredMovieVersions);
      b.WithOwner().HasForeignKey("draft_part_id");
      b.Property<Guid>("id");
      b.HasKey("id");

      b.Property(x => x.MovieId)
        .HasColumnName("movie_id")
        .IsRequired();

      b.Property(x => x.VersionName)
        .HasColumnName("version_name")
        .HasMaxLength(100)
        .IsRequired();

      b.HasIndex("draft_part_id", nameof(RequiredMovieVersion.MovieId))
        .IsUnique();
    });

    builder.Navigation("_requiredMovieVersions")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Ignore(d => d.RequiredMovieVersions);

    // Participants

    builder.Ignore(d => d.Participants);

    builder.HasMany<DraftPartParticipant>("_draftPartParticipants")
      .WithOne(dpp => dpp.DraftPart)
      .HasForeignKey(dpp => dpp.DraftPartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation("_draftPartParticipants")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    // Draft Type
    builder.Property(d => d.DraftType)
      .IsRequired()
      .HasConversion(
        dt => dt.Value,
        value => DraftType.FromValue(value));

    // Hosts
    builder.Ignore(d => d.DraftHosts);

    builder.HasMany<DraftHost>("_draftHosts")
      .WithOne(dh => dh.DraftPart)
      .HasForeignKey(dh => dh.DraftPartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation("_draftHosts")
      .UsePropertyAccessMode (PropertyAccessMode.Field);

    // Trivia Results
    builder.HasMany(d => d.TriviaResults)
      .WithOne(tr => tr.DraftPart)
      .HasForeignKey(tr => tr.DraftPartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(x => x.ScheduledForUtc);

    builder.Ignore(d => d.PrimaryHost);
    builder.Ignore(d => d.CoHosts);
    builder.Ignore(d => d.TotalHosts);
    builder.Ignore(d => d.TotalDrafters);
    builder.Ignore(d => d.TotalDrafterTeams);
    builder.Ignore(d => d.TotalPicks);
    builder.Ignore(d => d.TotalParticipants);

  }
}
