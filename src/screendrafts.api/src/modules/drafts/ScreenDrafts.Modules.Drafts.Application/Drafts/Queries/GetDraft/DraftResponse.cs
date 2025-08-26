namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

public sealed record DraftResponse(
    Guid Id,
    string Title,
    int? EpisodeNumber,
    int DraftType,
    int TotalPicks,
    int TotalDrafters,
    int TotalHosts,
    int EpisodeType,
    int DraftStatus,
    string? Description,
    Guid? PreviousDraftId,
    string? PreviousDraftTitle,
    Guid? NextDraftId, 
    string? NextDraftTitle)
{
  private readonly List<DrafterDraftResponse> _drafters = [];
  private HostDraftResponse? _primaryHost;
  private readonly List<HostDraftResponse> _coHosts = [];
  private readonly List<ReleaseDateResponse>? _releaseDates = [];
  private readonly List<DraftPickResponse>? _draftPicks = [];
  private readonly List<VetoResponse>? _vetoes = [];
  private readonly List<VetoOverrideResponse>? _vetoOverrides = [];
  private readonly List<CommissionerOverrideResponse>? _commissionerOverrides = [];

  public DraftResponse()
    : this(
        Guid.Empty,
        string.Empty,
        null!,
        default,
        default,
        default,
        default,
        default,
        default,
        null,
        null,
        null,
        null,
        null)
  {
  }

  public DateTime[]? RawReleaseDates { get; set; }

  public ReadOnlyCollection<DrafterDraftResponse> Drafters => _drafters.AsReadOnly();
  public HostDraftResponse? PrimaryHost => _primaryHost;
  public ReadOnlyCollection<HostDraftResponse> CoHosts => _coHosts.AsReadOnly();
  public ReadOnlyCollection<ReleaseDateResponse>? ReleaseDates => _releaseDates!.AsReadOnly();
  public ReadOnlyCollection<DraftPickResponse>? DraftPicks => _draftPicks!.AsReadOnly();
  public ReadOnlyCollection<VetoResponse>? Vetoes => _vetoes?.AsReadOnly();
  public ReadOnlyCollection<VetoOverrideResponse>? VetoOverrides => _vetoOverrides?.AsReadOnly();
  public ReadOnlyCollection<CommissionerOverrideResponse>? CommissionerOverrides => _commissionerOverrides?.AsReadOnly();


  public void AddDrafter(DrafterDraftResponse drafter) => _drafters.Add(drafter);
  public void SetPrimaryHost(HostDraftResponse primaryHost) => _primaryHost = primaryHost;
  public void AddCoHost(HostDraftResponse host) => _coHosts.Add(host);
  public void AddDraftPick(DraftPickResponse draftPick) => _draftPicks!.Add(draftPick);
  public void AddReleaseDate(ReleaseDateResponse releaseDate) => _releaseDates!.Add(releaseDate);
  public void AddVeto(VetoResponse veto) => _vetoes!.Add(veto);
  public void AddVetoOverride(VetoOverrideResponse vetoOverride) => _vetoOverrides!.Add(vetoOverride);
  public void AddCommissionerOverride(CommissionerOverrideResponse commissionerOverride) => _commissionerOverrides!.Add(commissionerOverride);


  public void PopulateReleaseDatesFromRaw()
  {
    if (RawReleaseDates is not null)
    {
      _releaseDates?.Clear();
      _releaseDates?.AddRange(RawReleaseDates!.Select(rd => new ReleaseDateResponse(DateOnly.FromDateTime(rd))));
    }
  }
}
