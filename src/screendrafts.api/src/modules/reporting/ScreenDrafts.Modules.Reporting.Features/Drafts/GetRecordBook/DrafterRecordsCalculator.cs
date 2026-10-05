namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

/// <summary>Builds the Guest G.M. section of the Record Book from per-drafter, per-draft rows.</summary>
internal static class DrafterRecordsCalculator
{
  private static readonly int[] _allTiers = [5, 10, 15, 20];
  private static readonly int[] _upToFifteen = [5, 10, 15];
  private static readonly int[] _fromTen = [10, 15, 20];
  private static readonly int[] _five = [5];

  public static RecordBookSection Build(
    IReadOnlyList<DrafterDraftRow> rows,
    IReadOnlyList<PickSlotRow> slots,
    IReadOnlySet<Guid> copaceticDraftIds
  )
  {
    ArgumentNullException.ThrowIfNull(rows);
    ArgumentNullException.ThrowIfNull(slots);
    ArgumentNullException.ThrowIfNull(copaceticDraftIds);

    var appearedRows = rows.Where(r => r.Appeared).ToList();
    var drafters = Summarize(rows, copaceticDraftIds);

    return new RecordBookSection
    {
      Key = "guest-gm",
      Title = "Guest G.M. Records",
      Groups =
      [
        Group("appearances", "Appearances", Appearances(drafters)),
        Group("picks", "Picks", Picks(drafters, appearedRows)),
        Group("vetoes", "Vetoes", Vetoes(drafters)),
        Group("consecutive-vetoes", "Consecutive Vetoes", ConsecutiveVetoes(slots)),
        Group("veto-overrides", "Veto Overrides", VetoOverrides(drafters)),
        Group(
          "commissioner-overrides",
          "Commissioner Overrides",
          CommissionerOverrides(drafters, appearedRows)
        ),
        Group("copacetic", "Copacetic Drafts", Copacetic(drafters)),
      ],
    };
  }

  private static IEnumerable<RecordItem> Appearances(List<DrafterSummary> d)
  {
    return Compact(
      [
        RecordRanker.Build(
          "guest-gm.most-appearances",
          "Most Guest G.M. appearances",
          RecordRanker.Count,
          d,
          x => x.Appearances,
          true,
          Holder
        ),
        RecordRanker.Build(
          "guest-gm.most-drafts-with-a-title-vetoed",
          "Most drafts with a title vetoed",
          RecordRanker.Count,
          d,
          x => x.DraftsWithVetoAgainst,
          true,
          Holder
        ),
      ],
      RecordRanker.BuildTiered(
        "guest-gm.highest-pct-without-being-vetoed",
        "Highest percentage of drafts without being vetoed",
        RecordRanker.Percent,
        d,
        x => x.Appearances,
        x => 100m * (x.Appearances - x.DraftsWithVetoAgainst) / x.Appearances,
        true,
        HolderWithDrafts,
        _allTiers
      )
    );
  }

  private static IEnumerable<RecordItem> Picks(List<DrafterSummary> d, List<DrafterDraftRow> rows)
  {
    return Compact(
      [
        RecordRanker.Build(
          "guest-gm.most-titles-drafted",
          "Most titles drafted",
          RecordRanker.Count,
          d,
          x => x.TitlesDrafted,
          true,
          Holder
        ),
        RecordRanker.Build(
          "guest-gm.most-picks-vetoed",
          "Most picks vetoed",
          RecordRanker.Count,
          d,
          x => x.PicksVetoed,
          true,
          Holder
        ),
        RecordRanker.Build(
          "guest-gm.most-picks-vetoed-single-draft",
          "Most picks vetoed in a single draft",
          RecordRanker.Count,
          rows,
          x => x.PicksVetoed,
          true,
          DraftHolder
        ),
        RecordRanker.Build(
          "guest-gm.most-no1-picks-made",
          "Most No. 1 picks made",
          RecordRanker.Count,
          d,
          x => x.No1Landed,
          true,
          Holder
        ),
        RecordRanker.Build(
          "guest-gm.most-no1-picks-vetoed",
          "Most No. 1 picks vetoed",
          RecordRanker.Count,
          d,
          x => x.No1Vetoed,
          true,
          Holder
        ),
      ],
      RecordRanker.BuildTiered(
        "guest-gm.fewest-picks-vetoed",
        "Fewest picks vetoed",
        RecordRanker.Count,
        d,
        x => x.Appearances,
        x => x.PicksVetoed,
        false,
        HolderWithDrafts,
        _allTiers
      ),
      RecordRanker.BuildTiered(
        "guest-gm.most-picks-vetoed-per-draft",
        "Most picks vetoed per draft",
        RecordRanker.Ratio,
        d,
        x => x.Appearances,
        x => (decimal)x.PicksVetoed / x.Appearances,
        true,
        HolderWithDrafts,
        _five
      ),
      RecordRanker.BuildTiered(
        "guest-gm.fewest-picks-vetoed-per-draft",
        "Fewest picks vetoed per draft",
        RecordRanker.Ratio,
        d,
        x => x.Appearances,
        x => (decimal)x.PicksVetoed / x.Appearances,
        false,
        HolderWithDrafts,
        _allTiers
      )
    );
  }

  private static IEnumerable<RecordItem> Vetoes(List<DrafterSummary> d)
  {
    return Compact(
      [
        RecordRanker.Build(
          "guest-gm.most-vetoes-used",
          "Most vetoes used",
          RecordRanker.Count,
          d,
          x => x.VetoesUsed,
          true,
          Holder
        ),
        RecordRanker.Build(
          "guest-gm.most-self-vetoes",
          "Most self-vetoes",
          RecordRanker.Count,
          d,
          x => x.SelfVetoes,
          true,
          Holder
        ),
        RecordRanker.Build(
          "guest-gm.most-times-vetoing-a-no1-pick",
          "Most times vetoing a No. 1 pick",
          RecordRanker.Count,
          d,
          x => x.No1VetoesUsed,
          true,
          Holder
        ),
      ],
      RecordRanker.BuildTiered(
        "guest-gm.fewest-vetoes-used",
        "Fewest vetoes used",
        RecordRanker.Count,
        d,
        x => x.Appearances,
        x => x.VetoesUsed,
        false,
        HolderWithDrafts,
        _allTiers
      ),
      RecordRanker.BuildTiered(
        "guest-gm.most-vetoes-used-per-draft",
        "Most vetoes used per draft",
        RecordRanker.Ratio,
        d,
        x => x.Appearances,
        x => (decimal)x.VetoesUsed / x.Appearances,
        true,
        HolderWithDrafts,
        _upToFifteen
      ),
      RecordRanker.BuildTiered(
        "guest-gm.fewest-vetoes-used-per-draft",
        "Fewest vetoes used per draft",
        RecordRanker.Ratio,
        d,
        x => x.Appearances,
        x => (decimal)x.VetoesUsed / x.Appearances,
        false,
        HolderWithDrafts,
        _allTiers
      )
    );
  }

  /// <summary>
  /// A vetoed pick is replaced at the same board position, so the same drafter can be vetoed several
  /// times at one position. A chain is one drafter at one position of one draft part with at least one
  /// standing veto. "Consecutive" means a chain of two or more vetoes.
  /// </summary>
  private static IEnumerable<RecordItem> ConsecutiveVetoes(IReadOnlyList<PickSlotRow> slots)
  {
    var perDrafter = slots
      .GroupBy(s => s.DrafterId)
      .Select(g => new SlotTally(
        g.First(),
        g.Count(s => s.TimesVetoed >= 2),
        g.Count(s => s.TimesVetoed >= 3)
      ))
      .ToList();

    var perDraft = slots
      .GroupBy(s => (s.DrafterId, s.DraftId))
      .Select(g => new SlotTally(
        g.First(),
        g.Count(s => s.TimesVetoed >= 2),
        g.Count(s => s.TimesVetoed >= 3)
      ))
      .ToList();

    return Compact([
      RecordRanker.Build(
        "guest-gm.most-times-vetoed-at-a-single-pick",
        "Most times vetoed at a single pick",
        RecordRanker.Count,
        slots,
        x => x.TimesVetoed,
        true,
        SlotHolder
      ),
      RecordRanker.Build(
        "guest-gm.most-times-with-consecutive-vetoes-at-a-single-pick",
        "Most times with consecutive vetoes at a single pick",
        RecordRanker.Count,
        perDrafter,
        x => x.Chains2,
        true,
        x => TallyHolder(x, withDraft: false)
      ),
      RecordRanker.Build(
        "guest-gm.most-times-vetoed-consecutively-in-a-single-draft",
        "Most times vetoed consecutively in a single draft",
        RecordRanker.Count,
        perDraft,
        x => x.Chains2,
        true,
        x => TallyHolder(x, withDraft: true)
      ),
      RecordRanker.Build(
        "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick",
        "Most times vetoed 3 or more times at a single pick",
        RecordRanker.Count,
        perDrafter,
        x => x.Chains3,
        true,
        x => TallyHolder(x, withDraft: false)
      ),
      RecordRanker.Build(
        "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick-in-a-single-draft",
        "Most times vetoed 3 or more times at a single pick in a single draft",
        RecordRanker.Count,
        perDraft,
        x => x.Chains3,
        true,
        x => TallyHolder(x, withDraft: true)
      ),
    ]);
  }

  private static IEnumerable<RecordItem> VetoOverrides(List<DrafterSummary> d)
  {
    return Compact([
      RecordRanker.Build(
        "guest-gm.most-vetoes-overridden",
        "Most vetoes overridden",
        RecordRanker.Count,
        d,
        x => x.VetoesOverridden,
        true,
        Holder
      ),
      RecordRanker.Build(
        "guest-gm.most-veto-overrides-deployed",
        "Most veto overrides deployed",
        RecordRanker.Count,
        d,
        x => x.OverridesDeployed,
        true,
        Holder
      ),
      RecordRanker.Build(
        "guest-gm.most-times-rescued-by-a-veto-override",
        "Most times rescued by a veto override",
        RecordRanker.Count,
        d,
        x => x.PicksSaved,
        true,
        Holder
      ),
    ]);
  }

  private static IEnumerable<RecordItem> CommissionerOverrides(
    List<DrafterSummary> d,
    List<DrafterDraftRow> rows
  )
  {
    return Compact([
      RecordRanker.Build(
        "guest-gm.most-picks-removed-by-commissioner-override",
        "Most picks removed via commissioner override",
        RecordRanker.Count,
        d,
        x => x.PicksRemovedByCommissioner,
        true,
        Holder
      ),
      RecordRanker.Build(
        "guest-gm.most-picks-removed-by-commissioner-override-single-draft",
        "Most picks removed via commissioner override in the same draft",
        RecordRanker.Count,
        rows,
        x => x.PicksRemovedByCommissioner,
        true,
        DraftHolder
      ),
      RecordRanker.Build(
        "guest-gm.most-appearances-without-a-commissioner-override",
        "Most appearances without a commissioner override",
        RecordRanker.Count,
        d,
        x => x.Appearances - x.DraftsWithCommissionerOverride,
        true,
        Holder
      ),
    ]);
  }

  private static IEnumerable<RecordItem> Copacetic(List<DrafterSummary> d)
  {
    return Compact(
      [
        RecordRanker.Build(
          "guest-gm.most-copacetic-drafts",
          "Most copacetic drafts",
          RecordRanker.Count,
          d,
          x => x.CopaceticDrafts,
          true,
          Holder
        ),
        RecordRanker.Build(
          "guest-gm.most-drafts-without-a-copacetic-draft",
          "Most drafts without a copacetic draft",
          RecordRanker.Count,
          d.Where(x => x.CopaceticDrafts == 0),
          x => x.Appearances,
          true,
          Holder
        ),
      ],
      RecordRanker.BuildTiered(
        "guest-gm.highest-pct-copacetic-drafts",
        "Highest percentage of copacetic drafts",
        RecordRanker.Percent,
        d,
        x => x.Appearances,
        x => 100m * x.CopaceticDrafts / x.Appearances,
        true,
        HolderWithDrafts,
        _fromTen
      )
    );
  }

  private static List<DrafterSummary> Summarize(
    IReadOnlyList<DrafterDraftRow> rows,
    IReadOnlySet<Guid> copaceticDraftIds
  )
  {
    return
    [
      .. rows.GroupBy(r => r.DrafterId)
        .Select(g =>
        {
          var appeared = g.Where(r => r.Appeared).ToList();
          var first = g.FirstOrDefault(r => r.DrafterName.Length > 0) ?? g.First();

          return new DrafterSummary
          {
            Id = g.Key,
            PublicId = first.DrafterPublicId,
            Name = first.DrafterName,
            Appearances = appeared.Count,
            TitlesDrafted = g.Sum(r => r.PicksLanded),
            PicksVetoed = g.Sum(r => r.PicksVetoed),
            PicksSaved = g.Sum(r => r.PicksSaved),
            PicksRemovedByCommissioner = g.Sum(r => r.PicksRemovedByCommissioner),
            No1Landed = g.Sum(r => r.No1Landed),
            No1Vetoed = g.Sum(r => r.No1Vetoed),
            VetoesUsed = g.Sum(r => r.VetoesUsed),
            VetoesOverridden = g.Sum(r => r.VetoesOverridden),
            SelfVetoes = g.Sum(r => r.SelfVetoes),
            No1VetoesUsed = g.Sum(r => r.No1VetoesUsed),
            OverridesDeployed = g.Sum(r => r.OverridesDeployed),
            DraftsWithVetoAgainst = appeared.Count(r => r.PicksVetoed > 0),
            DraftsWithCommissionerOverride = appeared.Count(r => r.PicksRemovedByCommissioner > 0),
            CopaceticDrafts = appeared.Count(r => copaceticDraftIds.Contains(r.DraftId)),
          };
        })
        .Where(s => s.Appearances > 0),
    ];
  }

  private static RecordHolder SlotHolder(PickSlotRow s) =>
    new()
    {
      Kind = "drafter",
      Name = s.DrafterName,
      PublicId = s.DrafterPublicId,
      Context = $"No. {s.Position}, {s.DraftTitle}",
    };

  private static RecordHolder TallyHolder(SlotTally t, bool withDraft) =>
    new()
    {
      Kind = "drafter",
      Name = t.Sample.DrafterName,
      PublicId = t.Sample.DrafterPublicId,
      Context = withDraft ? t.Sample.DraftTitle : null,
    };

  private sealed record SlotTally(PickSlotRow Sample, int Chains2, int Chains3);

  private static RecordHolder Holder(DrafterSummary d) =>
    new()
    {
      Kind = "drafter",
      Name = d.Name,
      PublicId = d.PublicId,
    };

  private static RecordHolder HolderWithDrafts(DrafterSummary d) =>
    new()
    {
      Kind = "drafter",
      Name = d.Name,
      PublicId = d.PublicId,
      Context = d.Appearances == 1 ? "1 draft" : $"{d.Appearances} drafts",
    };

  private static RecordHolder DraftHolder(DrafterDraftRow r) =>
    new()
    {
      Kind = "drafter",
      Name = r.DrafterName,
      PublicId = r.DrafterPublicId,
      Context = r.DraftTitle,
    };

  private static RecordBookGroup Group(string key, string title, IEnumerable<RecordItem> records) =>
    new()
    {
      Key = key,
      Title = title,
      Records = [.. records],
    };

  private static IEnumerable<RecordItem> Compact(
    IEnumerable<RecordItem?> single,
    params IEnumerable<RecordItem>[] tiered
  ) => single.OfType<RecordItem>().Concat(tiered.SelectMany(t => t));
}
