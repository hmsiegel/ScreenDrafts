namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftPickConfiguration : IEntityTypeConfiguration<GuestDraftPick>
{
  public void Configure(EntityTypeBuilder<GuestDraftPick> builder)
  {
    builder.ToTable(Tables.GuestDraftPicks);

    builder.HasKey(p => p.Id);

    builder
      .Property(p => p.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftPickIdConverter);

    builder
      .Property(p => p.GuestDraftId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftIdConverter);

    builder.Property(p => p.Position).IsRequired();

    builder.Property(p => p.PlayOrder).IsRequired();

    // Not made unique here even though every play order should in practice be
    // distinct within a draft -- I haven't seen this enforced as a hard DB
    // constraint in canonical, so a plain index for query performance only.
    // Tighten to unique once confirmed against real gameplay data.
    builder.HasIndex(p => new { p.GuestDraftId, p.PlayOrder });

    builder
      .Property(p => p.MoviePublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    // MovieId -- FK into the new guest_drafts.movies local cache. Restrict, not
    // Cascade: a movie is shared cache data, never owned by any single pick, so
    // deleting a pick must never delete the movie row underneath it.
    builder.Property(p => p.MovieId).IsRequired();

    builder
      .HasOne<GuestDraftMovie>()
      .WithMany()
      .HasForeignKey(p => p.MovieId)
      .OnDelete(DeleteBehavior.Restrict);

    // PlayedByParticipant -- required reference, NOT containment (the participant
    // lives on regardless of this pick), so Restrict rather than Cascade.
    builder
      .Property(p => p.PlayedByParticipantId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftParticipantIdConverter);

    builder
      .HasOne(p => p.PlayedByParticipant)
      .WithMany()
      .HasForeignKey(p => p.PlayedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    // RevealAuthorizedParticipant -- optional reference, same Restrict reasoning.
    builder
      .Property(p => p.RevealAuthorizedParticipantId)
      .HasConversion(IdConverters.NullableGuestDraftParticipantIdConverter);

    builder
      .HasOne(p => p.RevealAuthorizedParticipant)
      .WithMany()
      .HasForeignKey(p => p.RevealAuthorizedParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(p => p.ActedByPublicId).HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    // Vetoes -- containment, cascades.
    builder
      .HasMany(p => p.Vetoes)
      .WithOne(v => v.TargetPick)
      .HasForeignKey(v => v.TargetPickId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(p => p.Vetoes).UsePropertyAccessMode(PropertyAccessMode.Field);

    // CommissionerOverride -- optional 1:1, containment, cascades.
    builder
      .HasOne(p => p.CommissionerOverride)
      .WithOne(co => co.Pick)
      .HasForeignKey<GuestDraftCommissionerOverride>(co => co.PickId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(p => p.RevealedAt);

    // History -- owned collection, no domain identity of its own (GuestDraftPickEvent
    // is a plain record), so a shadow int identity column rather than a typed id.
    builder.OwnsMany(
      p => p.History,
      history =>
      {
        history.ToTable(Tables.GuestDraftPickHistory);
        history.WithOwner().HasForeignKey("GuestDraftPickId");
        history.Property<int>("Id").ValueGeneratedOnAdd();
        history.HasKey("Id");

        history.Property(h => h.Kind).IsRequired().HasMaxLength(50);

        history.Property(h => h.IssuerParticipantId);

        history.Property(h => h.Note).HasMaxLength(500);

        history.Property(h => h.OccurredOnUtc).IsRequired();
      }
    );

    builder.Navigation(p => p.History).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
