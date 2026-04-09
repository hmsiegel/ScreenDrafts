namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AssignSurrogate;

internal sealed class AssignSurrogateCommandHandler(
  IDraftPredictionSetRepository setRepository)
  : ICommandHandler<AssignSurrogateCommand>
{
  private readonly IDraftPredictionSetRepository _setRepository = setRepository;

  public async Task<Result> Handle(
    AssignSurrogateCommand request,
    CancellationToken cancellationToken)
  {
    var primarySet = await _setRepository.GetByPublicIdAsync(
      request.PrimarySetPublicId,
      cancellationToken);

    if (primarySet is null)
    {
      return Result.Failure(PredictionErrors.SetNotFound(request.PrimarySetPublicId));
    }

    var surrogateSet = await _setRepository.GetByPublicIdAsync(
      request.SurrogateSetPublicId,
      cancellationToken);

    if (surrogateSet is null)
    {
      return Result.Failure(PredictionErrors.SurrogateSetNotFound(request.SurrogateSetPublicId));
    }

    var policy = MergePolicy.FromValue(request.MergePolicy);

    var assignment = SurrogateAssignment.Create(
      primarySet: primarySet,
      surrogateSet: surrogateSet,
      mergePolicy: policy);

    var attachResult = primarySet.AttachSurrogate(assignment);

    if (attachResult.IsFailure)
    {
      return attachResult;
    }

    _setRepository.Update(primarySet);

    return Result.Success();
  }
}
