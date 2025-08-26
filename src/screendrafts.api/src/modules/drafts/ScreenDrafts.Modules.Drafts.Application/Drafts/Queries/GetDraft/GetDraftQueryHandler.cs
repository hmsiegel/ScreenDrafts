namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

internal sealed class GetDraftQueryHandler(
  IDraftsRepository draftsRepository,
  IPicksRepository picksRepository,
  IVetoRepository vetoRepository)
  : IQueryHandler<GetDraftQuery, DraftResponse>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;
  private readonly IVetoRepository _vetoRepository = vetoRepository;


  public async Task<Result<DraftResponse>> Handle(GetDraftQuery request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftWithDetailsAsync(DraftId.Create(request.DraftId), cancellationToken);

    if (draft is null)
    {
      return Result.Failure<DraftResponse>(DraftErrors.NotFound(request.DraftId));
    }

    var episodeNumber = draft.EpisodeNumber;
    var episodeType = draft.EpisodeType;

    var picks = await _picksRepository.GetByDraftIdAsync(DraftId.Create(request.DraftId), cancellationToken);
    var vetoes = await _vetoRepository.GetVetoesByDraftId(DraftId.Create(request.DraftId), cancellationToken);
    var vetoOverrides = await _vetoRepository.GetVetoOverridesByDraftId(DraftId.Create(request.DraftId), cancellationToken);
    var commissionerOverrides = await _draftsRepository.GetCommissionerOverridesByDraftIdAsync(DraftId.Create(request.DraftId), cancellationToken);

    List<DrafterDraftResponse> drafters = [.. draft.Drafters.Select(d => new DrafterDraftResponse(d.Id.Value, d.PersonId.Value, d.Person.DisplayName!))];
    var primaryHostLink = draft.DraftHosts.FirstOrDefault(h => h.Role.Value == 0);
    var coHostLinks = draft.DraftHosts
      .Where(h => h.Role.Value == 1)
      .OrderBy(h => h.Host.Person.DisplayName);
    List<ReleaseDateResponse> releaseDates = [.. draft.ReleaseDates.Select(r => new ReleaseDateResponse(r.ReleaseDate))];
    List<DraftPickResponse> draftPickResponses = [.. picks.Select(p => new DraftPickResponse(
      p.Position,
      p.PlayOrder,
      p.MovieId,
      p.Movie.MovieTitle,
      p.DrafterId?.Value,
      p.Drafter?.Person.DisplayName!,
      p.IsVetoed,
      p.DrafterTeamId?.Value,
      p.DrafterTeam?.Name))];

    List<VetoResponse> vetoResponses = [.. vetoes.Select(v => new VetoResponse(
      v!.Id.Value,
      v.PickId.Value,
      v.Pick.Position,
      v.Pick.PlayOrder,
      v.Pick.Movie.MovieTitle,
      v.DrafterId?.Value,
      v.Drafter?.Person.DisplayName!,
      v.DrafterTeamId?.Value,
      v.DrafterTeam?.Name))];
    List<VetoOverrideResponse> vetoOverrideResponses = [.. vetoOverrides.Select(vo => new VetoOverrideResponse(
      vo!.Id.Value,
      new VetoResponse(
        vo.Veto.Id.Value,
        vo.Veto.PickId.Value,
        vo.Veto.Pick.Position,
        vo.Veto.Pick.PlayOrder,
        vo.Veto.Pick.Movie.MovieTitle,
        vo.Veto.DrafterId?.Value,
        vo.Veto.Drafter?.Person.DisplayName!,
        vo.Veto.DrafterTeamId?.Value,
        vo.Veto.DrafterTeam?.Name),
      vo.DrafterId?.Value,
      vo.Drafter ?.Person.DisplayName!))];
    List<CommissionerOverrideResponse> commissionerOverrideResponses = [.. commissionerOverrides.Select(co => new CommissionerOverrideResponse(
      co!.Id,
      co.PickId.Value,
      co.Pick.Position,
      co.Pick.PlayOrder,
      co.Pick.MovieId,
      co.Pick.Movie.MovieTitle,
      co.Pick.DrafterId?.Value,
      co.Pick.Drafter?.Person.DisplayName!,
      co.Pick.DrafterTeamId?.Value,
      co.Pick.DrafterTeam?.Name))];

    var previousDraft = await _draftsRepository.GetPreviousDraftAsync(episodeNumber, episodeType, cancellationToken);
    var nextDraft = await _draftsRepository.GetNextDraftAsync(episodeNumber, episodeType, cancellationToken);

    var draftResponse = new DraftResponse(
      draft.Id.Value,
      draft.Title.Value,
      draft.EpisodeNumber!,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus,
      draft?.Description,
      previousDraft?.Id.Value,
      previousDraft?.Title.Value,
      nextDraft?.Id.Value,
      nextDraft?.Title.Value)
    {
      RawReleaseDates = [.. draft!.ReleaseDates.Select(r => r.ReleaseDate.ToDateTime(TimeOnly.MinValue))]
    };

    drafters.ForEach(d => draftResponse.AddDrafter(d));

    if (primaryHostLink is not null)
    {
      var primaryHost = new HostDraftResponse(
        primaryHostLink.Host.Id.Value,
        primaryHostLink.Host.PersonId.Value,
        primaryHostLink.Host.Person.DisplayName!);
      draftResponse.SetPrimaryHost(primaryHost);
    }

    coHostLinks.ToList().ForEach(h => draftResponse.AddCoHost(new HostDraftResponse(
      h.Host.Id.Value,
      h.Host.PersonId.Value,
      h.Host.Person.DisplayName!)));

    releaseDates.ForEach(rd => draftResponse.AddReleaseDate(rd));
    draftPickResponses.ForEach(p => draftResponse.AddDraftPick(p));
    vetoResponses.ForEach(v => draftResponse.AddVeto(v));
    vetoOverrideResponses.ForEach(vo => draftResponse.AddVetoOverride(vo));
    commissionerOverrideResponses.ForEach(co => draftResponse.AddCommissionerOverride(co));

    return draftResponse;
  }
}
