namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafters;

internal sealed class DrafterHonorificConfiguration : IEntityTypeConfiguration<DrafterHonorificEntity>
{
  public void Configure(EntityTypeBuilder<DrafterHonorificEntity> builder)
  {
    builder.ToTable(Tables.DrafterHonorifics);

    builder.HasKey(t => t.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DrafterHonorificIdConverter);

    builder.Property(x => x.DrafterIdValue)
      .IsRequired();

    builder.HasIndex(x => x.DrafterIdValue)
      .IsUnique()
      .HasDatabaseName("ux_drafter_honorifics_drafter_id_value");

    builder.Property(x => x.Honorific)
      .IsRequired()
      .HasConversion(
      h => h.Value,
      value => DrafterHonorific.FromValue(value));

    builder.Property(x => x.AppearanceCount)
      .IsRequired();

    builder.Property(x => x.UpdateAtUtc)
      .IsRequired();
  }
}
