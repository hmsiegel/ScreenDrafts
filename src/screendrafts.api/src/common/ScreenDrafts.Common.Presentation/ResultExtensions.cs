namespace ScreenDrafts.Common.Presentation;
public static class ResultExtensions
{
  public static async Task MapResultsAsync<T>(
      this IEndpoint endpoint,
      Result<T> result,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(result);

    if (result.IsFailure)
    {
      // Fix: Use the HttpContext.Response to call SendErrorsAsync
      await endpoint.HttpContext.Response.SendErrorsAsync(
          endpoint.ValidationFailures,
          StatusCodes.Status400BadRequest,
          null,
          cancellationToken);
    }
    else
    {
      // Fix: Use the HttpContext.Response to call SendOkAsync
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
      // Fix: Use the HttpContext.Response to call SendErrorsAsync
      await endpoint.HttpContext.Response.SendErrorsAsync(
          endpoint.ValidationFailures,
          StatusCodes.Status400BadRequest,
          null,
          cancellationToken);
    }
    else
    {
      // Fix: Use the HttpContext.Response to call SendOkAsync
      await endpoint.HttpContext.Response.SendOkAsync(cancellation: cancellationToken);
    }
  }
}
