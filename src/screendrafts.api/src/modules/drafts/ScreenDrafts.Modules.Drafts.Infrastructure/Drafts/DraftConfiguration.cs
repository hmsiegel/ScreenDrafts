namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftConfiguration : IEntityTypeConfiguration<Draft>
{
  public void Configure(EntityTypeBuilder<Draft> builder)
  {
    builder.ToTable(Tables.Drafts);

    builder.HasKey(d => d.Id);

    builder.Property(d => d.Id)
          .ValueGeneratedNever()
          .HasColumnName("id")
          .HasConversion(IdConverters.DraftIdConverter);

    builder.Property(d => d.PublicId)
          .IsRequired()
          .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(d => d.PublicId)
          .IsUnique()
          .HasDatabaseName("ix_drafts_public_id");

    builder.Property(d => d.Title)
          .HasMaxLength(Title.MaxLength)
          .HasConversion(
            title => title.Value,
            value => new Title(value));

    builder.Property(d => d.DraftType)
          .HasConversion(
            draftType => draftType.Value,
            value => DraftType.FromValue(value));

    builder.Property(d => d.DraftStatus)
          .HasConversion(
            draftStatus => draftStatus.Value,
            value => DraftStatus.FromValue(value));

    builder.Property(d => d.Description);

    builder.HasMany(d => d.DraftCategories)
      .WithOne(dc => dc.Draft)
      .HasForeignKey(dc => dc.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(d => d.SeriesId)
      .HasColumnName("series_id")
      .ValueGeneratedNever()
      .HasConversion(IdConverters.SeriesIdConverter);

    builder.Property(d => d.CampaignId);

    builder.HasOne(d => d.Campaign)
      .WithMany("_drafts")
      .HasForeignKey(d => d.CampaignId)
      .OnDelete(DeleteBehavior.SetNull);

    builder.HasOne(d => d.Series)
      .WithMany()
      .HasForeignKey(d => d.SeriesId)
      .HasPrincipalKey(s => s.Id)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Navigation(x => x.Parts)
      .HasField("_parts")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.DraftCategories)
      .HasField("_draftCategories")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.ChannelReleases)
      .HasField("_channelReleases")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.SeriesId).HasDatabaseName("ix_drafts_series_id");

    builder.Property(x => x.Version)
      .HasColumnName("xmin")
      .HasColumnType("xid")
      .ValueGeneratedOnAddOrUpdate()
      .IsConcurrencyToken();
  }
}
