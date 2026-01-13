namespace ScreenDrafts.Common.Application.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
  IEnumerable<IValidator<TRequest>> validators)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IBaseCommand
{
  private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    ValidationFailure[] validationFailures = await ValidateAsync(request);

    if (validationFailures.Length == 0)
    {
      return await next(cancellationToken);
    }

    if (typeof(TResponse).IsGenericType &&
      typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
    {
      Type resultType = typeof(TResponse).GetGenericArguments()[0];

      MethodInfo? failureMethod = typeof(Result<>)
        .MakeGenericType(resultType)
        .GetMethod(nameof(Result<>.ValidationFailure));

      if (failureMethod is not null)
      {
        return (TResponse)failureMethod.Invoke(null, [CreateValidationError(validationFailures)])!;
      }
    }
    else if (typeof(TResponse) == typeof(Result))
    {
      return (TResponse)(object)Result.Failure(CreateValidationError(validationFailures));
    }

    throw new ValidationException(validationFailures);
  }

  private async Task<ValidationFailure[]> ValidateAsync(TRequest request)
  {
    if (!_validators.Any())
    {
      return [];
    }

    var context = new ValidationContext<TRequest>(request);

    var validationResults = await Task.WhenAll(
      _validators.Select(v => v.ValidateAsync(context)));

    var validationFailures = validationResults
      .Where(v => !v.IsValid)
      .SelectMany(vr => vr.Errors)
      .ToArray();

    return validationFailures;
  }

  private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
    new([.. validationFailures.Select(f => SDError.Problem(f.ErrorCode, f.ErrorMessage))]);
}
