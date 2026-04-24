namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.GetZoomSessionToken;

internal sealed class GetZoomSessionTokenQueryHandler(
  IDraftPartRepository draftPartRepository,
  IZoomSessionTokenService tokenService)
  : IQueryHandler<GetZoomSessionTokenQuery, ZoomSessionTokenResult>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IZoomSessionTokenService _tokenService = tokenService;

  public async Task<Result<ZoomSessionTokenResult>> Handle(GetZoomSessionTokenQuery request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartPublicId,
      cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<ZoomSessionTokenResult>(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    if (draftPart.ZoomSessionName is null)
    {
      return Result.Failure<ZoomSessionTokenResult>(DraftPartErrors.NoActiveZoomSession);
    }

    var token = _tokenService.GenerateToken(
      draftPart.ZoomSessionName,
      request.ParticipantPublicId,
      ZoomSessionRole.Participant);

    return Result.Success(new ZoomSessionTokenResult
    {
      SessionName = draftPart.ZoomSessionName,
      Token = token
    });
  }
}
