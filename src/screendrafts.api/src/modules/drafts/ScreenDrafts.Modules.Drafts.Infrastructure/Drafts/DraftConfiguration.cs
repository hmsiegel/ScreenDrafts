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

    builder.Property(d => d.TotalPicks)
          .IsRequired();

    builder.Property(d => d.TotalDrafters)
          .IsRequired();

    builder.Property(d => d.TotalHosts)
          .IsRequired();

    builder.Property(d => d.DraftStatus)
          .HasConversion(
            draftStatus => draftStatus.Value,
            value => DraftStatus.FromValue(value));

    builder.Property(d => d.EpisodeType)
          .HasConversion(
            episodeType => episodeType.Value,
            value => EpisodeType.FromValue(value));

    builder.HasOne(d => d.GameBoard)
      .WithOne(gb => gb.Draft)
      .HasForeignKey<GameBoard>(gb => gb.Id);

    builder.HasIndex(d => d.ReadableId)
      .IsUnique();

    builder.Property(d => d.Description);

    builder.HasMany(d => d.Picks)
      .WithOne(p => p.Draft)
      .HasForeignKey(p => p.DraftId);

    builder
      .Metadata
      .FindNavigation(nameof(Draft.Picks))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}
