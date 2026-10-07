namespace ScreenDrafts.Modules.Reporting.UnitTests.Builders;

internal enum PickOutcome
{
  Landed,
  Vetoed,
  Saved,
  Removed,
}

/// <summary>Fluent builder for the in-memory dataset the stats query engine runs over.</summary>
internal sealed class QueryDataBuilder
{
  public const int DrafterKind = 0;
  public const int TeamKind = 1;
  public const int CommunityKind = 2;

  private readonly Dictionary<int, (string Series, string Type, int? Episode)> _drafts = [];
  private readonly List<QueryPickRow> _picks = [];
  private readonly List<QueryCreditRow> _credits = [];
  private readonly List<QueryVetoRow> _vetoes = [];

  public static Guid Id(int n) => new(n, 0, 0, new byte[8]);

  public QueryDataBuilder Draft(int number, string series = "Main", string type = "Standard", int? episode = null)
  {
    _drafts[number] = (series, type, episode);
    return this;
  }

  /// <summary>Adds a pick to a draft and returns its id. A draft not declared with <see cref="Draft"/> gets defaults.</summary>
  public Guid Pick(int draft, string media, PickOutcome outcome = PickOutcome.Landed)
  {
    var meta = _drafts.TryGetValue(draft, out var declared) ? declared : ("Main", "Standard", (int?)draft);
    var id = Guid.NewGuid();

    _picks.Add(new QueryPickRow
    {
      PickId = id,
      DraftId = Id(draft),
      DraftPublicId = $"d_{draft}",
      DraftTitle = $"Draft {draft}",
      DraftType = meta.Item2,
      SeriesName = meta.Item1,
      EpisodeNumber = meta.Item3,
      MediaPublicId = $"m_{media}",
      MediaTitle = media,
      Landed = outcome is PickOutcome.Landed or PickOutcome.Saved,
      VetoStanding = outcome == PickOutcome.Vetoed,
      CommissionerOverridden = outcome == PickOutcome.Removed,
    });

    return id;
  }

  public QueryDataBuilder Credit(Guid pickId, params int[] drafters)
  {
    foreach (var d in drafters)
    {
      _credits.Add(new QueryCreditRow
      {
        PickId = pickId,
        DrafterId = Id(1000 + d),
        DrafterPersonPublicId = $"p_{d}",
        DrafterPublicId = $"dr_{d}",
        DrafterName = $"Drafter {d}",
      });
    }

    return this;
  }

  public QueryDataBuilder Veto(Guid pickId, int kind, int issuer, bool overridden = false, bool self = false)
  {
    _vetoes.Add(new QueryVetoRow
    {
      VetoId = Guid.NewGuid(),
      PickId = pickId,
      IssuedByKind = kind,
      IssuedByIdValue = kind == DrafterKind ? Id(1000 + issuer) : Id(5000 + issuer),
      IssuedByName = $"Issuer {issuer}",
      IsOverridden = overridden,
      IsSelfVeto = self,
    });

    return this;
  }

  public QueryDataset Build() =>
    new() { Picks = _picks, Credits = _credits, Vetoes = _vetoes };

  public static StatsQuerySpec Spec(string metric, string groupBy, Action<SpecOptions>? configure = null)
  {
    var o = new SpecOptions();
    configure?.Invoke(o);

    return new StatsQuerySpec
    {
      Metric = metric,
      GroupBy = groupBy,
      Series = new HashSet<string>(o.Series, StringComparer.OrdinalIgnoreCase),
      DraftTypes = new HashSet<string>(o.DraftTypes, StringComparer.OrdinalIgnoreCase),
      EpisodeFrom = o.EpisodeFrom,
      EpisodeTo = o.EpisodeTo,
      MinAppearances = o.MinAppearances,
      Ascending = o.Ascending,
      Limit = o.Limit,
    };
  }

  internal sealed class SpecOptions
  {
    public List<string> Series { get; } = [];
    public List<string> DraftTypes { get; } = [];
    public int? EpisodeFrom { get; set; }
    public int? EpisodeTo { get; set; }
    public int? MinAppearances { get; set; }
    public bool Ascending { get; set; }
    public int Limit { get; set; } = StatsQueryValidator.DefaultLimit;
  }
}
