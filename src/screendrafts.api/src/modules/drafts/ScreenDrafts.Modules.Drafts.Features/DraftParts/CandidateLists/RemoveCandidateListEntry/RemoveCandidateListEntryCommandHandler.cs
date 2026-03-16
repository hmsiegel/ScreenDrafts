namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.RemoveCandidateListEntry;

internal sealed class RemoveCandidateListEntryCommandHandler(
  IDraftPartRepository draftPartRepository,
  ICandidateListRepository candidateListRepository)
  : ICommandHandler<RemoveCandidateListEntryCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ICandidateListRepository _candidateListRepository = candidateListRepository;

  public async Task<Result> Handle(RemoveCandidateListEntryCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(CandidateListErrors.DraftPartNotFound(request.DraftPartId));
    }

    var entry = await _candidateListRepository.FindByTmdbIdAsync(
      draftPartId: draftPart.Id,
      tmdbId: request.TmdbId,
      cancellationToken: cancellationToken);

    if (entry is null)
    {
      return Result.Failure(CandidateListErrors.EntryNotFound(request.TmdbId));
    }

    _candidateListRepository.Delete(entry);

    return Result.Success();
  }
}
