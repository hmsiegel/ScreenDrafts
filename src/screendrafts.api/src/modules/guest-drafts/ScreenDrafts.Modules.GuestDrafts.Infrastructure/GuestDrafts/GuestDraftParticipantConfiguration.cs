namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftParticipantConfiguration
  : IEntityTypeConfiguration<GuestDraftParticipant>
{
  public void Configure(EntityTypeBuilder<GuestDraftParticipant> builder)
  {
    builder.ToTable(Tables.GuestDraftParticipants);

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftParticipantIdConverter);

    builder.Property(p => p.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(p => p.PublicId)
      .IsUnique();

    builder.Property(p => p.GuestDraftId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftIdConverter);

    builder.Property(p => p.UserId)
      .IsRequired();

    builder.HasIndex(p => new { p.GuestDraftId, p.UserId })
      .IsUnique();

    builder.Property(p => p.IsOwner)
      .IsRequired();

    builder.Property(p => p.JoinedOnUtc)
      .IsRequired();

    // Veto / override / fungible-token economy. Runtime defaults come from the
    // entity's C# field initializers / InitializeVetoes -- the HasDefaultValue
    // calls below are just DB-level safety nets for any direct-SQL insert path,
    // not something the app relies on.
    builder.Property(p => p.StartingVetoes)
      .IsRequired()
      .HasDefaultValue(1);

    builder.Property(p => p.AwardedVetoes)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(p => p.AwardedVetoOverrides)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(p => p.CommissionerOverrides)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(p => p.FungibleTokens)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(p => p.AwardedFungibleTokens)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(p => p.VetoesUsed)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(p => p.VetoOverridesUsed)
      .IsRequired()
      .HasDefaultValue(0);

    builder.Property(p => p.FungibleTokensUsed)
      .IsRequired()
      .HasDefaultValue(0);
  }
}
