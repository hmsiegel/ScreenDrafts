namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.EndZoomSession;

internal sealed class EndZoomSessionCommandHandler(IDraftPartRepository draftPartRepository)
  : ICommandHandler<EndZoomSessionCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(EndZoomSessionCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var result = draftPart.ClearZoomSessionName();

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
