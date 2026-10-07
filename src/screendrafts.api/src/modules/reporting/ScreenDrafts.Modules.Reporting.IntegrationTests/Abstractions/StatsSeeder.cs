namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions;

/// <summary>
/// Fluent seeding for the Record Book, query and title tests. Builds drafts, parts, picks, vetoes and credits in
/// memory, then writes the matching <c>pick_facts</c>, <c>veto_facts</c>, <c>pick_credit_facts</c>,
/// <c>draft_summaries</c> and <c>draft_part_releases</c> rows in one <see cref="SaveAsync"/>. Everything is
/// deterministic: fixed ids, names and dates, so order-sensitive assertions are stable.
/// </summary>
internal sealed class StatsSeeder(ReportingDbContext db, int idOffset = 0)
{
  public const int DrafterKind = 0;
  public const int TeamKind = 1;
  public const int CommunityKind = 2;

  public const string MainFeed = "MainFeed";

  private static readonly DateTime _recordedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  private readonly ReportingDbContext _db = db;
  private readonly List<DraftSeed> _drafts = [];
  private int _draftCounter = idOffset;
  private int _partCounter = idOffset;

  public static Guid DrafterId(int n) => new(n, 0, 0, new byte[8]);

  public static Guid TeamId(int n) => new(n, 1, 0, new byte[8]);

  public static string DrafterName(int n) => $"Drafter {n:00}";

  public static string PersonPublicId(int n) => $"p_drafter{n:00}";

  public DraftSeed Draft(
    string? title = null,
    string series = "Main Series",
    string type = "Standard",
    int policy = 0,
    int totalParts = 1)
  {
    _draftCounter++;

    var draft = new DraftSeed(this)
    {
      Id = new Guid(_draftCounter, 2, 0, new byte[8]),
      PublicId = $"d_seed{_draftCounter:000}",
      Title = title ?? $"Draft {_draftCounter:000}",
      Series = series,
      Type = type,
      Policy = policy,
      TotalParts = totalParts,
    };

    _drafts.Add(draft);
    return draft;
  }

  public StatsSeeder MovieHonorific(string moviePublicId, MovieHonorific appearance, int appearances)
  {
    _db.MovieHonorifics.Add(
      MovieHonorificEntity.Create(
        moviePublicId,
        $"Title {moviePublicId}",
        appearance,
        MoviePositionHonorific.None,
        appearances));
    return this;
  }

  public async Task SaveAsync(CancellationToken cancellationToken)
  {
    foreach (var draft in _drafts)
    {
      foreach (var part in draft.Parts)
      {
        AddPart(draft, part);
      }
    }

    await _db.SaveChangesAsync(cancellationToken);
    _db.ChangeTracker.Clear();
    _drafts.Clear();
  }

  private void AddPart(DraftSeed draft, PartSeed part)
  {
    _db.DraftSummaries.Add(
      DraftSummary.Create(
        draft.Id,
        draft.PublicId,
        part.PublicId,
        draft.Title,
        draft.Type,
        part.Index,
        draft.TotalParts,
        part.Picks.Count,
        isPatreon: false,
        part.Episode,
        isComplete: true,
        _recordedAt,
        _recordedAt));

    foreach (var release in part.Releases)
    {
      _db.DraftPartsReleases.Add(
        DraftPartRelease.Create(draft.Id, part.PublicId, release.Channel, release.Date));
    }

    foreach (var pick in part.Picks)
    {
      _db.PickFacts.Add(ToFact(draft, part, pick));

      foreach (var veto in pick.Vetoes)
      {
        _db.VetoFacts.Add(ToFact(draft, part, pick, veto));
      }

      foreach (var drafter in pick.Credits)
      {
        _db.PickCreditFacts.Add(
          PickCreditFact.Create(
            Guid.NewGuid(),
            pick.Id,
            draft.Id,
            part.PublicId,
            DrafterId(drafter),
            $"dr_drafter{drafter:00}",
            PersonPublicId(drafter),
            DrafterName(drafter),
            _recordedAt));
      }
    }
  }

  private static PickFact ToFact(DraftSeed draft, PartSeed part, PickSeed pick)
  {
    var last = pick.Vetoes.MaxBy(v => v.Sequence);

    return PickFact.Create(
      pick.Id,
      draft.Id,
      draft.PublicId,
      part.PublicId,
      part.Index,
      draft.Title,
      draft.Type,
      draft.Series,
      draft.Policy,
      part.SubDraftIndex,
      pick.Position,
      pick.PlayOrder,
      $"m_{pick.Media}",
      pick.Media,
      pick.PlayedByKind,
      pick.PlayedById,
      pick.PlayedByPublicId,
      pick.PlayedByName,
      pick.Vetoes.Count,
      pick.Vetoes.Count > 0,
      last?.IsOverridden ?? false,
      pick.RemovedByCommissioner,
      _recordedAt);
  }

  private static VetoFact ToFact(DraftSeed draft, PartSeed part, PickSeed pick, VetoSeed veto)
  {
    var issuedById = veto.IssuerKind switch
    {
      DrafterKind => DrafterId(veto.Issuer),
      TeamKind => TeamId(veto.Issuer),
      _ => Guid.Empty,
    };

    return VetoFact.Create(
      Guid.NewGuid(),
      pick.Id,
      draft.Id,
      part.PublicId,
      veto.Sequence,
      veto.IssuerKind,
      issuedById,
      veto.IssuerKind == DrafterKind ? $"dr_drafter{veto.Issuer:00}" : null,
      IssuerName(veto),
      veto.IsOverridden,
      veto.OverriddenByDrafter is null ? null : DrafterKind,
      veto.OverriddenByDrafter is { } o ? DrafterId(o) : null,
      veto.OverriddenByDrafter is { } o2 ? $"dr_drafter{o2:00}" : null,
      veto.OverriddenByDrafter is { } o3 ? DrafterName(o3) : null,
      veto.IssuerKind == pick.PlayedByKind && issuedById == pick.PlayedById,
      _recordedAt);
  }

  private static string IssuerName(VetoSeed veto) =>
    veto.IssuerKind switch
    {
      DrafterKind => DrafterName(veto.Issuer),
      TeamKind => $"Team {veto.Issuer}",
      _ => "Patreon Members",
    };

  internal string NextPartPublicId() => $"dp_seed{++_partCounter:000}";

  internal sealed class DraftSeed(StatsSeeder owner)
  {
    public Guid Id { get; init; }
    public string PublicId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Series { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int Policy { get; init; }
    public int TotalParts { get; init; }
    public List<PartSeed> Parts { get; } = [];

    /// <summary>Adds a part. A main-feed release is added when <paramref name="mainFeed"/> is supplied.</summary>
    public PartSeed Part(int index = 1, int? episode = null, DateOnly? mainFeed = null, int? subDraftIndex = null)
    {
      var part = new PartSeed(owner.NextPartPublicId(), index, episode, subDraftIndex);

      if (mainFeed is { } date)
      {
        part.Releases.Add((MainFeed, date));
      }

      Parts.Add(part);
      return part;
    }
  }

  internal sealed class PartSeed(string publicId, int index, int? episode, int? subDraftIndex)
  {
    private int _playOrder;

    public string PublicId { get; } = publicId;
    public int Index { get; } = index;
    public int? Episode { get; } = episode;
    public int? SubDraftIndex { get; } = subDraftIndex;
    public List<PickSeed> Picks { get; } = [];
    public List<(string Channel, DateOnly Date)> Releases { get; } = [];

    public PartSeed Release(string channel, DateOnly date)
    {
      Releases.Add((channel, date));
      return this;
    }

    /// <summary>Adds a pick played by drafter <paramref name="playedBy"/>, credited to that drafter.</summary>
    public PickSeed Pick(string media, int position, int playedBy, int? playOrder = null)
    {
      var pick = new PickSeed(media, position, playOrder ?? ++_playOrder)
      {
        PlayedByKind = DrafterKind,
        PlayedById = DrafterId(playedBy),
        PlayedByPublicId = $"dr_drafter{playedBy:00}",
        PlayedByName = DrafterName(playedBy),
      };

      pick.Credits.Add(playedBy);
      Picks.Add(pick);
      return pick;
    }

    /// <summary>Adds a team pick credited to each of <paramref name="members"/>.</summary>
    public PickSeed TeamPick(string media, int position, int team, int[] members, int? playOrder = null)
    {
      var pick = new PickSeed(media, position, playOrder ?? ++_playOrder)
      {
        PlayedByKind = TeamKind,
        PlayedById = TeamId(team),
        PlayedByPublicId = null,
        PlayedByName = $"Team {team}",
      };

      pick.Credits.AddRange(members);
      Picks.Add(pick);
      return pick;
    }

    /// <summary>Adds a community pick, which credits nobody.</summary>
    public PickSeed CommunityPick(string media, int position, int? playOrder = null)
    {
      var pick = new PickSeed(media, position, playOrder ?? ++_playOrder)
      {
        PlayedByKind = CommunityKind,
        PlayedById = Guid.Empty,
        PlayedByPublicId = null,
        PlayedByName = "Patreon Members",
      };

      Picks.Add(pick);
      return pick;
    }
  }

  internal sealed class PickSeed(string media, int position, int playOrder)
  {
    public Guid Id { get; } = Guid.NewGuid();
    public string Media { get; } = media;
    public int Position { get; } = position;
    public int PlayOrder { get; } = playOrder;
    public int PlayedByKind { get; init; }
    public Guid PlayedById { get; init; }
    public string? PlayedByPublicId { get; init; }
    public string PlayedByName { get; init; } = string.Empty;
    public bool RemovedByCommissioner { get; private set; }
    public List<int> Credits { get; } = [];
    public List<VetoSeed> Vetoes { get; } = [];

    /// <summary>Adds the next veto (sequence 1, 2, ...) issued by a drafter, team or the community.</summary>
    public PickSeed Veto(int issuerKind, int issuer = 0, bool overridden = false, int? overriddenBy = null)
    {
      Vetoes.Add(new VetoSeed(Vetoes.Count + 1, issuerKind, issuer, overridden, overriddenBy));
      return this;
    }

    public PickSeed Removed()
    {
      RemovedByCommissioner = true;
      return this;
    }
  }

  internal sealed record VetoSeed(int Sequence, int IssuerKind, int Issuer, bool IsOverridden, int? OverriddenByDrafter);
}
