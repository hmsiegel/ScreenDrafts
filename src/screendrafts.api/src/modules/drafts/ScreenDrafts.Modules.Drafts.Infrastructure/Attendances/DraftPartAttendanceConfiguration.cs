namespace ScreenDrafts.Modules.Drafts.Infrastructure.Attendances;

internal sealed class DraftPartAttendanceConfiguration
  : IEntityTypeConfiguration<DraftPartAttendance>
{
  public void Configure(EntityTypeBuilder<DraftPartAttendance> builder)
  {
    builder.ToTable(Tables.DraftPartAttendance);

    builder.HasKey(a => a.Id);

    builder.Property(a => a.Id).HasConversion(IdConverters.DraftPartAttendanceIdConverter);

    builder.Property(a => a.PublicId).HasMaxLength(50).IsRequired();

    builder.HasIndex(a => a.PublicId).IsUnique();

    builder
      .Property(a => a.DraftPartId)
      .HasConversion(id => id.Value, value => new DraftPartId(value))
      .IsRequired();

    builder.Property(a => a.PersonPublicId).HasMaxLength(50).IsRequired();

    builder.HasIndex(a => new { a.DraftPartId, a.PersonPublicId }).IsUnique();

    builder
      .Property(a => a.Status)
      .HasConversion(EnumConverters.AttendanceStatusConverter)
      .IsRequired();

    builder.Property(a => a.CreatedAtUtc).IsRequired();
    builder.Property(a => a.UpdatedAtUtc);
  }
}
