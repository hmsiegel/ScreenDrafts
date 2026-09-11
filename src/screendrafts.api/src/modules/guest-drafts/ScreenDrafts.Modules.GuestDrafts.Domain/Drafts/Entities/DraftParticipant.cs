using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

/// <summary>
/// Redesigned to mirror canonical DraftPartParticipant exactly: participants are
/// referenced by a raw (Guid Value, Kind) pair, not a direct navigation to
/// GuestDrafter/GuestDrafterTeam (those are separate aggregates -- no cross-
/// aggregate EF navigation, same as DraftPartParticipant has none to Drafter).
/// No PublicId of its own either, matching DraftPartParticipant -- "which
/// participant" is addressed via the underlying GuestDrafter's/GuestDrafterTeam's
/// own PublicId, resolved through GuestParticipantResolver at the Features layer,
/// not stored redundantly here.
/// </summary>
public sealed class DraftParticipant : Entity<DraftParticipantId>
{
  private DraftParticipant(
    DraftId draftId,
    Participant participantId,
    bool isOwner,
    DateTime joinedOnUtc,
    DraftParticipantId? id = null
  )
    : base(id ?? DraftParticipantId.CreateUnique())
  {
    DraftId = draftId;
    ParticipantIdValue = participantId.Value;
    ParticipantKindValue = participantId.Kind;
    IsOwner = isOwner;
    JoinedOnUtc = joinedOnUtc;
  }

  private DraftParticipant() { }

  public DraftId DraftId { get; private set; } = default!;

  public Guid ParticipantIdValue { get; private set; }
  public ParticipantKind ParticipantKindValue { get; private set; } = default!;

  public Participant ParticipantId => new(ParticipantIdValue, ParticipantKindValue);

  /// <summary>
  /// Set at add-time by the handler (caller.UserId == guestDraft.OwnerUserId at
  /// the moment this participant is added) -- not implied automatically the way
  /// it used to be. The owner is a commissioner/manager role independent of
  /// playing; they only get this flag on their own participant row if and when
  /// they add themselves like anyone else.
  /// </summary>
  public bool IsOwner { get; private set; }

  public DateTime JoinedOnUtc { get; private set; }

  public Draft Draft { get; private set; } = default!;

  // ── Veto / override / fungible-token economy -- unchanged from the prior
  // design; none of this depended on how a participant's identity was stored. ──

  public int StartingVetoes { get; private set; } = 1;
  public int AwardedVetoes { get; private set; }
  public int AwardedVetoOverrides { get; private set; }
  public int CommissionerOverrides { get; private set; }

  public int FungibleTokens { get; private set; }
  public int AwardedFungibleTokens { get; private set; }

  public int VetoesUsed { get; private set; }
  public int VetoOverridesUsed { get; private set; }
  public int FungibleTokensUsed { get; private set; }

  public int TotalVetoes => StartingVetoes + AwardedVetoes;
  public int TotalVetoOverrides => AwardedVetoOverrides;
  public int TotalFungibleTokens => FungibleTokens + AwardedFungibleTokens;

  private int RemainingFungibleTokens => Math.Max(0, TotalFungibleTokens - FungibleTokensUsed);

  public bool CanUseVeto() => (TotalVetoes - VetoesUsed) >= 1 || RemainingFungibleTokens >= 1;

  public bool CanUseVetoOverride(int maxOverrides) =>
    (maxOverrides > 0 && (TotalVetoOverrides - VetoOverridesUsed) >= 1)
    || RemainingFungibleTokens >= 1;

  internal static DraftParticipant Create(
    DraftId guestDraftId,
    Participant participantId,
    bool isOwner
  )
  {
    return new DraftParticipant(
      draftId: guestDraftId,
      participantId: participantId,
      isOwner: isOwner,
      joinedOnUtc: DateTime.UtcNow
    );
  }

  internal void InitializeVetoes(int startingVetoes, int fungibleTokens = 0)
  {
    StartingVetoes = startingVetoes;
    FungibleTokens = fungibleTokens;
    VetoesUsed = 0;
    VetoOverridesUsed = 0;
    FungibleTokensUsed = 0;
  }

  internal void GrantAward(bool isVeto)
  {
    if (isVeto)
    {
      AwardedVetoes++;
    }
    else
    {
      AwardedVetoOverrides++;
    }
  }

  internal void RevokeAward(bool isVeto)
  {
    if (isVeto)
    {
      AwardedVetoes = Math.Max(0, AwardedVetoes - 1);
    }
    else
    {
      AwardedVetoOverrides = Math.Max(0, AwardedVetoOverrides - 1);
    }
  }

  internal void GrantFungibleTokenAward() => AwardedFungibleTokens++;

  internal void RevokeFungibleTokenAward() =>
    AwardedFungibleTokens = Math.Max(0, AwardedFungibleTokens - 1);

  internal void AddCommissionerOverride() => CommissionerOverrides++;

  internal bool SpendVeto()
  {
    if (!CanUseVeto())
    {
      throw new InvalidOperationException("No remaining vetoes.");
    }

    if (TotalVetoes - VetoesUsed >= 1)
    {
      VetoesUsed++;
      return false;
    }

    FungibleTokensUsed++;
    return true;
  }

  internal bool SpendVetoOverride(int maxOverrides)
  {
    if (!CanUseVetoOverride(maxOverrides))
    {
      throw new InvalidOperationException("No remaining veto overrides.");
    }

    if (TotalVetoOverrides - VetoOverridesUsed >= 1)
    {
      VetoOverridesUsed++;
      return false;
    }

    FungibleTokensUsed++;
    return true;
  }

  internal void RefundVeto(bool fromFungiblePool = false)
  {
    if (fromFungiblePool)
    {
      FungibleTokensUsed = Math.Max(0, FungibleTokensUsed - 1);
    }
    else
    {
      VetoesUsed = Math.Max(0, VetoesUsed - 1);
    }
  }

  internal void RefundVetoOverride(bool fromFungiblePool = false)
  {
    if (fromFungiblePool)
    {
      FungibleTokensUsed = Math.Max(0, FungibleTokensUsed - 1);
    }
    else
    {
      VetoOverridesUsed = Math.Max(0, VetoOverridesUsed - 1);
    }
  }
}
