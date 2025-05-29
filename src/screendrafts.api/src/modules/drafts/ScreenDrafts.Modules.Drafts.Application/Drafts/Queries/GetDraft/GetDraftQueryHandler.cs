namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

internal sealed class GetDraftQueryHandler(IDraftsRepository draftsRepository, IPicksRepository picksRepository)
  : IQueryHandler<GetDraftQuery, DraftResponse>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;

  public async Task<Result<DraftResponse>> Handle(GetDraftQuery request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftWithDetailsAsync(DraftId.Create(request.DraftId), cancellationToken);

    if (draft is null)
    {
      return Result.Failure<DraftResponse>(DraftErrors.NotFound(request.DraftId));
    }

    var picks = await _picksRepository.GetByDraftIdAsync(DraftId.Create(request.DraftId), cancellationToken);

    List<DrafterResponse> drafters = [.. draft.Drafters.Select(d => new DrafterResponse(d.Id.Value, d.Name))];
    List<HostResponse> hosts = [.. draft.Hosts.Select(h => new HostResponse(h.Id.Value, h.HostName))];
    List<ReleaseDateResponse> releaseDates = [.. draft.ReleaseDates.Select(r => new ReleaseDateResponse(r.ReleaseDate))];
    List<DraftPickResponse> draftPickResponses = [.. picks.Select(p => new DraftPickResponse(
      p.Position,
      p.MovieId,
      p.Movie.MovieTitle,
      p.DrafterId!.Value,
      p.Drafter!.Name))];

    var draftResponse = new DraftResponse(
      draft.Id.Value,
      draft.Title.Value,
      draft.EpisodeNumber!,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus);

    drafters.ForEach(d => draftResponse.AddDrafter(d));
    hosts.ForEach(h => draftResponse.AddHost(h));
    releaseDates.ForEach(rd => draftResponse.AddReleaseDate(rd));
    draftPickResponses.ForEach(p => draftResponse.AddDraftPick(p));

    return draftResponse;
  }
}
