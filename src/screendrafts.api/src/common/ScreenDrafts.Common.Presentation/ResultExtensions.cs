namespace ScreenDrafts.Common.Presentation;
public static class ResultExtensions
{
  public static async Task MapOkResultsAsync<T>(
      this IEndpoint endpoint,
      Result<T> result,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(result);

    if (result.IsFailure)
    {
      await endpoint.HttpContext.Response.SendErrorsAsync(
          endpoint.ValidationFailures,
          StatusCodes.Status400BadRequest,
          null,
          cancellationToken);
    }
    else
    {
      await endpoint.HttpContext.Response.SendOkAsync(result.Value, cancellation: cancellationToken);
    }
  }
  public static async Task MapResultsAsync(
      this IEndpoint endpoint,
      Result result,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(result);

    if (result.IsFailure)
    {
      await endpoint.HttpContext.Response.SendErrorsAsync(
          endpoint.ValidationFailures,
          StatusCodes.Status400BadRequest,
          null,
          cancellationToken);
    }
    else
    {
      await endpoint.HttpContext.Response.SendOkAsync(cancellation: cancellationToken);
    }
  }
  public static async Task MapNoContentResultsAsync(
      this IEndpoint endpoint,
      Result result,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(result);

    if (result.IsFailure)
    {
      await endpoint.HttpContext.Response.SendErrorsAsync(
          endpoint.ValidationFailures,
          StatusCodes.Status400BadRequest,
          null,
          cancellationToken);
    }
    else
    {
      await endpoint.HttpContext.Response.SendNoContentAsync(cancellation: cancellationToken);
    }
  }

  public static async Task MapCreatedResultsAsync<T>(
      this IEndpoint endpoint,
      Result<T> result,
      Func<T, string> locationFactory,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(locationFactory);

    if (result.IsFailure)
    {
      await endpoint.HttpContext.Response.SendErrorsAsync(
          endpoint.ValidationFailures,
          StatusCodes.Status400BadRequest,
          null,
          cancellationToken);
      return;
    }

    endpoint.HttpContext.Response.Headers.Location = locationFactory(result.Value);

    await endpoint.HttpContext.Response.SendAsync(
        result.Value,
        StatusCodes.Status201Created,
        cancellation: cancellationToken);
  }
}
