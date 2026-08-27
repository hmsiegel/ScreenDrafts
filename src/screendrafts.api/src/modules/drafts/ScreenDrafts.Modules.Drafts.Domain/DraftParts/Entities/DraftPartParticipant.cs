namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class DraftPartParticipant : Entity<DraftPartParticipantId>
{
  private DraftPartParticipant(
    DraftPart draftPart,
    Participant participantId,
    DraftPartParticipantId? id = null
  )
    : base(id ?? DraftPartParticipantId.CreateUnique())
  {
    DraftPartId = draftPart.Id;
    DraftPart = draftPart;
    ParticipantIdValue = participantId.Value;
    ParticipantKindValue = participantId.Kind;
  }

  private DraftPartParticipant()
  {
    // EF Core
  }

  public DraftPartId DraftPartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;

  public Guid ParticipantIdValue { get; private set; }
  public ParticipantKind ParticipantKindValue { get; private set; } = default!;

  public Participant ParticipantId => new(ParticipantIdValue, ParticipantKindValue);

  // Inventory inputs

  /// <summary>
  /// 1 for part 1 (or any part on a draft that grants a veto per part), otherwise 0.
  /// Set at part-start time.
  /// </summary>
  public int StartingVetoes { get; private set; } = 1;

  /// <summary>
  /// Vetoes carries in from the previous part/ draft. Set at part-start time based on the previous part's ending inventory.
  /// </summary>
  public int VetoesRollingIn { get; private set; }

  /// <summary>
  /// Veto overrides carries in from the previous part/ draft. Set at part-start time based on the previous part's ending inventory.
  /// </summary>
  public int VetoOverridesRollingIn { get; private set; }

  /// <summary>
  /// Extra vetoes granted via draft position award (post-trivia)
  /// </summary>
  public int AwardedVetoes { get; private set; }

  /// <summary>
  /// Extra veto overrides granted via draft position award (post-trivia)
  /// </summary>
  public int AwardedVetoOverrides { get; private set; }

  public int CommissionerOverrides { get; private set; }

  // Fungible Tokens
  public int FungibleTokens { get; private set; }
  public int FungibleTokensRollingIn { get; private set; }
  public int AwardedFungibleTokens { get; private set; }

  private int RemainingFungibleTokens => Math.Max(0, TotalFungibleTokens - FungibleTokensUsed);

  // Usage counters
  public int VetoesUsed { get; private set; }
  public int VetoOverridesUsed { get; private set; }
  public int FungibleTokensUsed { get; private set; }

  // Computed Totals

  public int TotalVetoes => StartingVetoes + VetoesRollingIn + AwardedVetoes;
  public int TotalVetoOverrides => VetoOverridesRollingIn + AwardedVetoOverrides;
  public int TotalFungibleTokens =>
    FungibleTokens + FungibleTokensRollingIn + AwardedFungibleTokens;

  /// <summary>
  /// How many vetoes carry forward to the next part (capped at 1)
  /// </summary>
  public int VetoesRollingOut => TotalVetoes - VetoesUsed >= 1 ? 1 : 0;

  /// <summary>
  /// How many veto overrides carry forward to the next part (capped at 1)
  /// </summary>
  public int VetoOverridesRollingOut => TotalVetoOverrides - VetoOverridesUsed >= 1 ? 1 : 0;

  public int FungibleTokensRollingOut => TotalFungibleTokens - FungibleTokensUsed >= 1 ? 1 : 0;

  // Guards
  public bool CanUseVeto() => (TotalVetoes - VetoesUsed) >= 1 || RemainingFungibleTokens >= 1;

  public bool CanUseVetoOverride(int maxOverrides) =>
    maxOverrides > 0 && (TotalVetoOverrides - VetoOverridesUsed) >= 1
    || RemainingFungibleTokens >= 1;

  // Factory
  public static DraftPartParticipant Create(DraftPart draftPart, Participant participantId)
  {
    ArgumentNullException.ThrowIfNull(draftPart);

    return new DraftPartParticipant(draftPart, participantId);
  }

  // Initialization and state mutation methods
  internal void InitializeVetoes(
    int startingVetoes,
    int vetoesRollingIn,
    int vetoOverridesRollingIn,
    int fungibleTokens = 0,
    int fungibleTokensRollingIn = 0
  )
  {
    StartingVetoes = startingVetoes;
    VetoesRollingIn = vetoesRollingIn;
    VetoOverridesRollingIn = vetoOverridesRollingIn;
    FungibleTokens = fungibleTokens;
    FungibleTokensRollingIn = fungibleTokensRollingIn;
    VetoesUsed = 0;
    VetoOverridesUsed = 0;
    FungibleTokensUsed = 0;
  }

  // Awards (granted via draft position post-trivia)
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

  // Commissioner override
  public void AddCommissionerOverride() => CommissionerOverrides++;

  // Spending
  public bool SpendVeto()
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

  public bool SpendVetoOverride(int maxOverrides)
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

  /// <summary>
  /// Refunds a spent veto token. Used when a veto is undone by a commissioner.
  /// </summary>
  public void RefundVeto(bool fromFungiblePool = false)
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

  /// <summary>
  /// Refunds a spent veto override token. Used when a veto override is undone by a commissioner.
  /// </summary>
  public void RefundVetoOverride(bool fromFungiblePool = false)
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

  // Seeding (historical data import only)
  internal void SeedSetState(
    int startingVetoes,
    int rolloverVeto,
    int rolloverVetoOverride,
    int triviaVetoes,
    int triviaVetoOverrides,
    int commissionerOverrides,
    int vetoesUsed,
    int vetoOverridesUsed,
    int fungibleTokens = 0,
    int fungibleTokensRollingIn = 0,
    int awardedFungibleTokens = 0,
    int fungibleTokensUsed = 0
  )
  {
    StartingVetoes = startingVetoes;

    VetoesRollingIn = rolloverVeto;
    VetoOverridesRollingIn = rolloverVetoOverride;

    AwardedVetoes = triviaVetoes;
    AwardedVetoOverrides = triviaVetoOverrides;

    CommissionerOverrides = commissionerOverrides;

    VetoesUsed = vetoesUsed;
    VetoOverridesUsed = vetoOverridesUsed;

    FungibleTokens = fungibleTokens;
    FungibleTokensRollingIn = fungibleTokensRollingIn;
    AwardedFungibleTokens = awardedFungibleTokens;
    FungibleTokensUsed = fungibleTokensUsed;
  }
}
