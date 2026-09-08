using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftParticipantConfiguration
  : IEntityTypeConfiguration<DraftParticipant>
{
  public void Configure(EntityTypeBuilder<DraftParticipant> builder)
  {
    builder.ToTable(Tables.GuestDraftParticipants);

    builder.HasKey(p => p.Id);

    builder
      .Property(p => p.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftParticipantIdConverter);

    builder.Property(p => p.DraftId).IsRequired().HasConversion(IdConverters.GuestDraftIdConverter);

    builder.Property(p => p.ParticipantIdValue).IsRequired();

    builder
      .Property(p => p.ParticipantKindValue)
      .IsRequired()
      .HasConversion(k => k.Value, v => ParticipantKind.FromValue(v));

    builder
      .HasIndex(p => new
      {
        p.DraftId,
        p.ParticipantIdValue,
        p.ParticipantKindValue,
      })
      .IsUnique();

    builder.Property(p => p.IsOwner).IsRequired();

    builder.Property(p => p.JoinedOnUtc).IsRequired();

    builder.Property(p => p.StartingVetoes).IsRequired().HasDefaultValue(1);

    builder.Property(p => p.AwardedVetoes).IsRequired().HasDefaultValue(0);

    builder.Property(p => p.AwardedVetoOverrides).IsRequired().HasDefaultValue(0);

    builder.Property(p => p.CommissionerOverrides).IsRequired().HasDefaultValue(0);

    builder.Property(p => p.FungibleTokens).IsRequired().HasDefaultValue(0);

    builder.Property(p => p.AwardedFungibleTokens).IsRequired().HasDefaultValue(0);

    builder.Property(p => p.VetoesUsed).IsRequired().HasDefaultValue(0);

    builder.Property(p => p.VetoOverridesUsed).IsRequired().HasDefaultValue(0);

    builder.Property(p => p.FungibleTokensUsed).IsRequired().HasDefaultValue(0);
  }
}
