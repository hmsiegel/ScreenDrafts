namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeleteSpotlight;

internal sealed class DeleteSpotlightCommandHandler(IDraftReportingRepository repository)
  : ICommandHandler<DeleteSpotlightCommand>
{
  private readonly IDraftReportingRepository _repository = repository;

  public async Task<Result> Handle(
    DeleteSpotlightCommand request,
    CancellationToken cancellationToken
  )
  {
    var spotlight = await _repository.GetSpotlightByPublicIdAsync(
      request.PublicId,
      cancellationToken
    );

    if (spotlight is null)
    {
      return Result.Failure(DraftReportingErrors.SpotlightNotFound(request.PublicId));
    }

    if (spotlight.IsActive)
    {
      return Result.Failure(DraftReportingErrors.CannotDeleteActiveSpotlight);
    }

    _repository.RemoveSpotlight(spotlight);

    return Result.Success();
  }
}
