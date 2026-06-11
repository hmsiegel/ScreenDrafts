namespace ScreenDrafts.Modules.Reporting.Features.Drafts.RotateSpotlight;

internal sealed class RotateSpotlightCommandHandler(ISpotlightRotationService rotationService)
  : ICommandHandler<RotateSpotlightCommand>
{
  private readonly ISpotlightRotationService _rotationService = rotationService;

  public async Task<Result> Handle(
    RotateSpotlightCommand request,
    CancellationToken cancellationToken
  )
  {
    try
    {
      await _rotationService.TriggerRotationJobAsync(cancellationToken);
      return Result.Success();
    }
    catch (InvalidOperationException)
    {
      return Result.Failure(DraftReportingErrors.RotationJobNotRegistered);
    }
  }
}
