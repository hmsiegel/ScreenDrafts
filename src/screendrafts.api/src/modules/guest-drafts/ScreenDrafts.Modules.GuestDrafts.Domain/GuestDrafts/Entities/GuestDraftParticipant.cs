namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;

public sealed class GuestDraftParticipant : Entity<GuestDraftParticipantId>
{
  private GuestDraftParticipant(
    string publicId,
    GuestDraftId guestDraftId,
    Guid userId,
    bool isOwner,
    DateTime joinedOnUtc,
    GuestDraftParticipantId? id = null)
    : base(id ?? GuestDraftParticipantId.CreateUnique())
  {
    PublicId = publicId;
    GuestDraftId = guestDraftId;
    UserId = userId;
    IsOwner = isOwner;
    JoinedOnUtc = joinedOnUtc;
  }

  private GuestDraftParticipant()
  {
  }

  public string PublicId { get; private set; } = default!;
  public GuestDraftId GuestDraftId { get; private set; } = default!;
  public Guid UserId { get; private set; }
  public bool IsOwner { get; private set; }
  public DateTime JoinedOnUtc { get; private set; }

  public GuestDraft GuestDraft { get; private set; } = default!;

  // ── Veto / override / fungible-token economy ────────────────────────────
  // Mirrors DraftPartParticipant's inventory model, minus the *RollingIn fields
  // and *RollingOut properties -- a guest draft is always single-part, so there's
  // no cross-part carryover to model.

  public int StartingVetoes { get; private set; } = 1;

  /// <summary>Extra vetoes granted via a position's bonus flag, post-assignment.</summary>
  public int AwardedVetoes { get; private set; }

  /// <summary>Extra veto overrides granted via a position's bonus flag.</summary>
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

  internal static GuestDraftParticipant Create(
    string publicId,
    GuestDraftId guestDraftId,
    Guid userId,
    bool isOwner)
  {
    return new GuestDraftParticipant(
      publicId: publicId,
      guestDraftId: guestDraftId,
      userId: userId,
      isOwner: isOwner,
      joinedOnUtc: DateTime.UtcNow);
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
