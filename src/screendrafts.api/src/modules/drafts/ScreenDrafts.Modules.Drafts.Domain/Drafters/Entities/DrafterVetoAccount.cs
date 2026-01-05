namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

/// <summary>
/// Tracks a participant's veto/veto override usage across the entire draft
/// while enforcing the per-part maximums supplied by the series policy.
/// </summary>
public sealed class DrafterVetoAccount(ParticipantId owner)
{
  private readonly Dictionary<(Guid SeriesId, int PartNumber), PartSpend> _spend = [];

  public ParticipantId Owner { get; } = owner;

  public bool CanUseVeto(
    Guid seriesId,
    int partNumber,
    int perPartMax)
  {
    var key = (seriesId, partNumber);
    if (!_spend.TryGetValue(key, out var spend)) spend = PartSpend.Empty;
    return spend.VetoesUsed < perPartMax;
  }

  public bool CanUseVetoOverride(
    Guid seriesId,
    int partNumber,
    int perPartMax)
  {
    var key = (seriesId, partNumber);
    if (!_spend.TryGetValue(key, out var spend)) spend = PartSpend.Empty;
    return spend.VetoOverridesUsed < perPartMax;
  }

  public void RecordVetoUse(Guid seriesId, int partNumber)
  {
    var key = (seriesId, partNumber);
    if (!_spend.TryGetValue(key, out var spend)) spend = PartSpend.Empty;
    _spend[key] = new(spend.VetoesUsed + 1, spend.VetoOverridesUsed);
  }

  public void RecordVetoOverrideUse(Guid seriesId, int partNumber)
  {
    var key = (seriesId, partNumber);
    if (!_spend.TryGetValue(key, out var spend)) spend = PartSpend.Empty;
    _spend[key] = new(spend.VetoesUsed, spend.VetoOverridesUsed + 1);
  }

  private readonly record struct PartSpend(int VetoesUsed, int VetoOverridesUsed)
  {
    public static PartSpend Empty => new(0, 0);
  }
}
