namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Drafts;

internal sealed class DraftConfiguration : IEntityTypeConfiguration<Draft>
{
  public void Configure(EntityTypeBuilder<Draft> builder)
  {
    builder.ToTable(Tables.Drafts);

    builder.HasKey(d => d.Id);

    builder
      .Property(d => d.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.DraftIdConverter);

    builder.Property(d => d.PublicId).IsRequired().HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(d => d.PublicId).IsUnique();

    builder.Property(d => d.OwnerUserId).IsRequired();

    builder.Property(d => d.Title).IsRequired().HasMaxLength(Draft.TitleMaxLength);

    builder
      .Property(d => d.GuestDraftType)
      .IsRequired()
      .HasConversion(t => t.Value, v => DraftType.FromValue(v));

    builder
      .Property(d => d.GuestDraftStatus)
      .IsRequired()
      .HasConversion(s => s.Value, v => DraftStatus.FromValue(v));

    builder.Property(d => d.ShareToken).HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(d => d.ShareToken).IsUnique().HasFilter("share_token IS NOT NULL");

    builder.Property(d => d.CreatedOnUtc).IsRequired();

    builder.Property(d => d.UpdatedOnUtc);

    builder.Property(d => d.DraftDate);

    // Participants -- containment, cascades.
    builder
      .HasMany(d => d.Participants)
      .WithOne(p => p.Draft)
      .HasForeignKey(p => p.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(d => d.Participants).UsePropertyAccessMode(PropertyAccessMode.Field);

    // GameBoard -- optional 1:1, containment, cascades. GuestDraftGameBoard has no
    // back-reference nav (unidirectional FK), so WithOne() takes no expression.
    builder
      .HasOne(d => d.GameBoard)
      .WithOne()
      .HasForeignKey<GameBoard>(gb => gb.DraftId)
      .OnDelete(DeleteBehavior.Cascade);

    // Picks -- containment, cascades. GuestDraftPick has no back-reference nav
    // either (unidirectional FK to keep the entity from needing a full GuestDraft
    // load just to exist), same reasoning as GameBoard above.
    builder
      .HasMany(d => d.Picks)
      .WithOne()
      .HasForeignKey(p => p.GuestDraftId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(d => d.Picks).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
