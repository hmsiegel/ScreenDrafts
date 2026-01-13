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

    builder.Property(d => d.Description);

    builder.HasMany(d => d.DraftCategories)
      .WithOne(dc => dc.Draft)
      .HasForeignKey(dc => dc.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(d => d.Parts)
      .WithOne(dp => dp.Draft)
      .HasForeignKey(dp => dp.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(d => d.Campaign)
      .WithMany(c => c.Drafts)
      .HasForeignKey(d => d.CampaignId);

    builder.HasOne(d => d.Series)
      .WithMany()
      .HasForeignKey(d => d.SeriesId);

    builder.Navigation(x => x.Parts)
      .HasField("_parts")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(x => x.SeriesId).HasDatabaseName("ix_drafts_series_id");
  }
}
