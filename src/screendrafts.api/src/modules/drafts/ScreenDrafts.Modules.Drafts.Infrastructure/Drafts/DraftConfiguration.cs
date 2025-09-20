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
          .HasConversion(
            id => id.Value,
            value => DraftId.Create(value));

    builder.Property(d => d.ReadableId)
          .ValueGeneratedOnAdd();

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

    builder.HasIndex(d => d.ReadableId)
      .IsUnique();

    builder.Property(d => d.Description);

    builder.HasMany(d => d.DraftCategories)
      .WithOne(dc => dc.Draft)
      .HasForeignKey(dc => dc.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(d => d.Parts)
      .WithOne(dp => dp.Draft)
      .HasForeignKey(dp => dp.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(d => d.Campaigns)
      .WithMany()
      .UsingEntity<Dictionary<string, object>>(
      "drafts_campaigns",
        j => j
          .HasOne<Campaign>()
          .WithMany()
          .HasForeignKey("campaign_id")
          .HasConstraintName("fk_drafts_campaigns_campaign_id"),
        j => j
          .HasOne<Draft>()
          .WithMany()
          .HasForeignKey("draft_id")
          .HasConstraintName("fk_drafts_campaigns_draft_id"),
        j =>
        {
          j.ToTable(Tables.DraftCampaigns);
        });

    builder.HasOne(d => d.Series)
      .WithMany()
      .HasForeignKey(d => d.SeriesId);

    builder.Navigation(x => x.Parts)
      .HasField("_parts")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(x => x.Campaigns)
      .HasField("_campaigns")
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
