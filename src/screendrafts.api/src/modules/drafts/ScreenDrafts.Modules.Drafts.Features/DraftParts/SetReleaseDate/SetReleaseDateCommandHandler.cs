namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

internal sealed class SetReleaseDateCommandHandler(IDraftPartRepository draftPartRepository)
  : ICommandHandler<SetReleaseDateCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(SetReleaseDateCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    draftPart.AddRelease(request.ReleaseChannel, request.ReleaseDate);

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
