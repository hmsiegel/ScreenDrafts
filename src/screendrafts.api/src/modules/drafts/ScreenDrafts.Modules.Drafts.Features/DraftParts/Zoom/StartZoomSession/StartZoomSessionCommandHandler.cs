namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomSession;

internal sealed class StartZoomSessionCommandHandler(
  IDraftPartRepository draftPartRepository,
  IZoomSessionTokenService zoomSessionTokenService)
  : ICommandHandler<StartZoomSessionCommand, StartZoomSessionResult>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IZoomSessionTokenService _zoomSessionTokenService = zoomSessionTokenService;

  public async Task<Result<StartZoomSessionResult>> Handle(
    StartZoomSessionCommand request,
    CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(request.DraftPartPublicId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<StartZoomSessionResult>(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var sessionName = $"screendrafts-{request.DraftPartPublicId}";

    var setResult = draftPart.SetZoomSessionName(sessionName);

    if (setResult.IsFailure)
    {
      return Result.Failure<StartZoomSessionResult>(setResult.Errors[0]);
    }

    var token = _zoomSessionTokenService.GenerateToken(
      userIdentity: request.HostPublicId,
      sessionName: sessionName,
      role: ZoomSessionRole.Host);

    _draftPartRepository.Update(draftPart);

    return Result.Success(new StartZoomSessionResult
    {
      SessionName = sessionName,
      Token = token
    });
  }
}
