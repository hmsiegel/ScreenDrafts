namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions;

/// <summary>Builders for <see cref="DraftPartStatsRecordedIntegrationEvent"/> and its record types.</summary>
internal static class StatsEvents
{
  private static readonly DateTime _occurredOn = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  public static DraftPartStatsRecordedIntegrationEvent Event(
    string draftPartPublicId,
    Guid draftId,
    IEnumerable<StatsPickRecord> picks,
    Action<EventOptions>? configure = null)
  {
    var o = new EventOptions();
    configure?.Invoke(o);

    return new DraftPartStatsRecordedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: _occurredOn,
      draftId: draftId,
      draftPublicId: $"d_{draftId.ToString("N")[..12]}",
      draftPartPublicId: draftPartPublicId,
      partIndex: o.PartIndex,
      draftTitle: o.DraftTitle,
      draftType: o.DraftType,
      seriesName: o.SeriesName,
      canonicalPolicyValue: o.CanonicalPolicy,
      picks: [.. picks],
      hasMainFeedRelease: o.HasMainFeedRelease);
  }

  public static StatsPickRecord Pick(Action<PickOptions>? configure = null)
  {
    var o = new PickOptions();
    configure?.Invoke(o);

    return new StatsPickRecord(
      PickId: o.PickId,
      Position: o.Position,
      PlayOrder: o.PlayOrder,
      SubDraftIndex: o.SubDraftIndex,
      MediaPublicId: o.MediaPublicId,
      MediaTitle: o.MediaTitle,
      PlayedByKind: o.PlayedByKind,
      PlayedByIdValue: o.PlayedById,
      PlayedByPublicId: o.PlayedByPublicId,
      PlayedByName: o.PlayedByName,
      IsCommissionerOverridden: o.Removed,
      Vetoes: o.Vetoes,
      Credits: o.Credits);
  }

  public static StatsVetoRecord Veto(
    int sequence,
    int issuedByKind,
    Guid issuedById,
    bool overridden = false,
    Guid? overriddenBy = null) =>
    new(
      VetoId: Guid.NewGuid(),
      Sequence: sequence,
      IssuedByKind: issuedByKind,
      IssuedByIdValue: issuedById,
      IssuedByPublicId: null,
      IssuedByName: "Issuer",
      IsOverridden: overridden,
      OverriddenByKind: overriddenBy is null ? null : 0,
      OverriddenByIdValue: overriddenBy,
      OverriddenByPublicId: null,
      OverriddenByName: overriddenBy is null ? null : "Overrider");

  public static StatsCreditRecord Credit(int drafter) =>
    new(
      StatsSeeder.DrafterId(drafter),
      $"dr_drafter{drafter:00}",
      StatsSeeder.PersonPublicId(drafter),
      StatsSeeder.DrafterName(drafter));

  /// <summary>A pick played and credited by a single drafter.</summary>
  public static StatsPickRecord SoloPick(int drafter, Action<PickOptions>? configure = null) =>
    Pick(o =>
    {
      o.PlayedById = StatsSeeder.DrafterId(drafter);
      o.PlayedByName = StatsSeeder.DrafterName(drafter);
      o.Credits = [Credit(drafter)];
      configure?.Invoke(o);
    });

  internal sealed class EventOptions
  {
    public int PartIndex { get; set; } = 1;
    public string DraftTitle { get; set; } = "Seed Draft";
    public string DraftType { get; set; } = "Standard";
    public string SeriesName { get; set; } = "Main Series";
    public int CanonicalPolicy { get; set; }
    public bool HasMainFeedRelease { get; set; }
  }

  internal sealed class PickOptions
  {
    public Guid PickId { get; set; } = Guid.NewGuid();
    public int Position { get; set; } = 1;
    public int PlayOrder { get; set; } = 1;
    public int? SubDraftIndex { get; set; }
    public string MediaPublicId { get; set; } = "m_heat";
    public string MediaTitle { get; set; } = "Heat";
    public int PlayedByKind { get; set; }
    public Guid PlayedById { get; set; } = StatsSeeder.DrafterId(1);
    public string? PlayedByPublicId { get; set; }
    public string PlayedByName { get; set; } = StatsSeeder.DrafterName(1);
    public bool Removed { get; set; }
    public IReadOnlyList<StatsVetoRecord> Vetoes { get; set; } = [];
    public IReadOnlyList<StatsCreditRecord> Credits { get; set; } = [];
  }
}
