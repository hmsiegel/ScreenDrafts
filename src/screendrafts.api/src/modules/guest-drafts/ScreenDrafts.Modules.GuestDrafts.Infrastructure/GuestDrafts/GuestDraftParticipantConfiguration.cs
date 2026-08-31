namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftParticipantConfiguration
  : IEntityTypeConfiguration<GuestDraftParticipant>
{
  public void Configure(EntityTypeBuilder<GuestDraftParticipant> builder)
  {
    builder.ToTable(Tables.GuestDraftParticipants);

    builder.HasKey(p => p.Id);

    builder
      .Property(p => p.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftParticipantIdConverter);

    builder.Property(p => p.PublicId).IsRequired().HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(p => p.PublicId).IsUnique();

    builder
      .Property(p => p.GuestDraftId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftIdConverter);

    builder.Property(p => p.UserId).IsRequired();

    builder.HasIndex(p => new { p.GuestDraftId, p.UserId }).IsUnique();

    builder.Property(p => p.IsOwner).IsRequired();

    builder.Property(p => p.JoinedOnUtc).IsRequired();
  }
}
