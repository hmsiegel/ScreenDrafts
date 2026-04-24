namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class DraftPartRecordingConfiguration : IEntityTypeConfiguration<DraftPartRecording>
{
  public void Configure(EntityTypeBuilder<DraftPartRecording> builder)
  {
    builder.ToTable(Tables.DraftPartRecordings);

    builder.HasKey(r => r.Id);

    builder.Property(r => r.Id)
        .HasConversion(IdConverters.DraftPartRecordingIdConverter);

    builder.Property(r => r.PublicId)
        .IsRequired()
        .HasMaxLength(50);

    builder.HasIndex(r => r.PublicId)
        .IsUnique();

    builder.Property(r => r.DraftPartId)
        .HasConversion(IdConverters.DraftPartIdConverter);

    builder.Property(r => r.ZoomSessionName)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(r => r.ZoomFileId)
        .IsRequired()
        .HasMaxLength(100);

    builder.HasIndex(r => r.ZoomFileId)
        .IsUnique();

    builder.Property(r => r.FileType)
        .HasConversion(EnumConverters.ZoomRecordingFileTypeConverter)
        .IsRequired();

    builder.Property(r => r.PlayUrl)
        .HasConversion(UriConverters.UriConverter)
        .HasMaxLength(2048);

    builder.Property(r => r.DownloadUrl)
        .HasConversion(UriConverters.UriConverter)
        .HasMaxLength(2048);

    builder.Property(r => r.RecordingStart)
        .IsRequired();

    builder.Property(r => r.RecordingEnd)
        .IsRequired();

    builder.Property(r => r.FileSizeBytes)
        .IsRequired();

    builder.Property(r => r.SequenceNumber)
        .IsRequired();

    builder.HasOne<DraftPart>()
        .WithMany()
        .HasForeignKey(r => r.DraftPartId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
