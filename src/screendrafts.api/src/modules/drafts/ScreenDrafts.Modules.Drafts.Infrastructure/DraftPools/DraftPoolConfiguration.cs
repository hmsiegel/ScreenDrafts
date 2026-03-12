namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftPools;

internal sealed class DraftPoolConfiguration : IEntityTypeConfiguration<DraftPool>
{
  public void Configure(EntityTypeBuilder<DraftPool> builder)
  {
    builder.ToTable(Tables.DraftPools);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPoolIdConverter);

    builder.Property(x => x.DraftId)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftIdConverter);

    builder.Property(x => x.PublicId)
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength)
      .IsRequired();

    builder.Property(x => x.IsLocked).IsRequired();
    builder.Property(x => x.CreatedAtUtc).IsRequired();
    builder.Property(x => x.UpdatedAtUtc);

    builder.HasIndex(p => p.DraftId).IsUnique();
    builder.HasIndex(p => p.PublicId).IsUnique();

    builder.OwnsMany(x => x.TmdbIds, a =>
    {
      a.ToTable(Tables.DraftPoolItems);
      a.WithOwner().HasForeignKey("DraftPoolId");
      a.Property(i => i.TmdbId)
        .HasColumnName("tmdb_id")
        .IsRequired()
        .ValueGeneratedNever();
      a.HasKey("DraftPoolId", nameof(DraftPoolItem.TmdbId));
    });
  }
}
