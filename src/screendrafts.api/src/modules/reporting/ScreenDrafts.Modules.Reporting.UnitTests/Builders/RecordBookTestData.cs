namespace ScreenDrafts.Modules.Reporting.UnitTests.Builders;

/// <summary>Deterministic builders for the Record Book calculator inputs.</summary>
internal static class RecordBookTestData
{
  public static Guid Id(int n) => new(n, 0, 0, new byte[8]);

  public static DraftPartRow Part(int draftNumber, Action<DraftPartRow>? configure = null)
  {
    var row = new DraftPartRow
    {
      DraftId = Id(draftNumber),
      DraftPublicId = $"d_{draftNumber}",
      DraftTitle = $"Draft {draftNumber}",
      DraftType = "Standard",
      PartPublicId = $"dp_{draftNumber}_1",
      PartIndex = 1,
    };

    configure?.Invoke(row);
    return row;
  }

  public static DrafterDraftRow DrafterDraft(
    int drafterNumber,
    int draftNumber,
    Action<DrafterDraftRow>? configure = null
  )
  {
    var row = new DrafterDraftRow
    {
      DrafterId = Id(1000 + drafterNumber),
      DrafterPublicId = $"dr_{drafterNumber}",
      DrafterPersonPublicId = $"p_{drafterNumber}",
      DrafterName = $"Drafter {drafterNumber}",
      DraftId = Id(draftNumber),
      DraftPublicId = $"d_{draftNumber}",
      DraftTitle = $"Draft {draftNumber}",
      Appeared = true,
      EpisodeNumber = draftNumber,
    };

    configure?.Invoke(row);
    return row;
  }

  /// <summary>One drafter appearing in drafts 1..count, letting the caller adjust each row.</summary>
  public static List<DrafterDraftRow> Appearances(
    int drafterNumber,
    int count,
    Action<int, DrafterDraftRow>? configure = null
  ) =>
    [
      .. Enumerable
        .Range(1, count)
        .Select(draft => DrafterDraft(drafterNumber, draft, r => configure?.Invoke(draft, r))),
    ];

  public static PickSlotRow Slot(
    int drafterNumber,
    int draftNumber,
    int position,
    int timesVetoed
  ) =>
    new()
    {
      DrafterId = Id(1000 + drafterNumber),
      DrafterPublicId = $"dr_{drafterNumber}",
      DrafterPersonPublicId = $"p_{drafterNumber}",
      DrafterName = $"Drafter {drafterNumber}",
      DraftId = Id(draftNumber),
      DraftPublicId = $"d_{draftNumber}",
      DraftTitle = $"Draft {draftNumber}",
      PartPublicId = $"dp_{draftNumber}_1",
      Position = position,
      TimesVetoed = timesVetoed,
    };

  public static MediaRow Media(int n, int timesDrafted, int timesDraftedNo1 = 0) =>
    new()
    {
      MediaPublicId = $"m_{n}",
      MediaTitle = $"Movie {n}",
      TimesDrafted = timesDrafted,
      TimesDraftedNo1 = timesDraftedNo1,
    };

  public static RecordBookData Data(
    IReadOnlyList<DraftPartRow>? parts = null,
    IReadOnlyList<DrafterDraftRow>? drafterDrafts = null,
    IReadOnlyList<MediaRow>? media = null,
    IReadOnlyList<PickSlotRow>? slots = null,
    IReadOnlyDictionary<Guid, int>? uniqueTitles = null,
    IReadOnlyList<TitleAppearanceRow>? titleAppearances = null,
    Action<DataOverrides>? overrides = null
  )
  {
    var o = new DataOverrides();
    overrides?.Invoke(o);

    return new RecordBookData
    {
      Parts = parts ?? [],
      DrafterDrafts = drafterDrafts ?? [],
      Media = media ?? [],
      TitleAppearances = titleAppearances ?? [],
      PickSlots = slots ?? [],
      UniqueTitlesPlayedByDraft = uniqueTitles ?? new Dictionary<Guid, int>(),
      VetoesStood = o.VetoesStood,
      VetoesOverridden = o.VetoesOverridden,
      SelfVetoesStood = o.SelfVetoesStood,
      MarqueeOfFameTitles = o.Marquee,
      HatTrickTitles = o.HatTrick,
      GrandSlamTitles = o.GrandSlam,
    };
  }

  public static RecordItem? Find(RecordBookSection section, string code) =>
    section.Groups.SelectMany(g => g.Records).FirstOrDefault(r => r.Code == code);

  public static RecordHolder Holder(string name) => new() { Kind = "drafter", Name = name };

  internal sealed class DataOverrides
  {
    public int VetoesStood { get; set; }
    public int VetoesOverridden { get; set; }
    public int SelfVetoesStood { get; set; }
    public int Marquee { get; set; }
    public int HatTrick { get; set; }
    public int GrandSlam { get; set; }
  }
}
