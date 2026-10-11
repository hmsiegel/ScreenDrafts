namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Invitations;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
  public void Configure(EntityTypeBuilder<Invitation> builder)
  {
    builder.ToTable(Tables.DraftInvitations);

    builder.HasKey(i => i.Id);

    builder
      .Property(i => i.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.InvitationIdConverter);

    builder.Property(i => i.PublicId).IsRequired().HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(i => i.PublicId).IsUnique();

    builder
      .Property(i => i.DraftId)
      .IsRequired()
      .HasConversion(IdConverters.DraftIdConverter);

    builder.Property(i => i.Email).IsRequired().HasMaxLength(Invitation.EmailMaxLength);

    builder.Property(i => i.Name).IsRequired().HasMaxLength(Invitation.NameMaxLength);

    builder.Property(i => i.InvitedByUserId).IsRequired();

    builder.Property(i => i.CreatedOnUtc).IsRequired();

    builder.Property(i => i.LastSentOnUtc).IsRequired();

    builder.Property(i => i.AcceptedOnUtc);

    builder.Property(i => i.AcceptedByUserId);

    // One invitation per email per draft.
    builder.HasIndex(i => new { i.DraftId, i.Email }).IsUnique();

    // Registration lookup: all pending invites for an email.
    builder.HasIndex(i => i.Email);

    builder
      .HasOne<Draft>()
      .WithMany()
      .HasForeignKey(i => i.DraftId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
