namespace ScreenDrafts.Common.Features.Http.Results;

public static class ResultExtensions
{
  private static async Task SendProblemsAsync(IEndpoint endpoint, Result result)
  {
    var problem = ApiResults.Problem(result);
    await problem.ExecuteAsync(endpoint.HttpContext);

  }
  public static async Task SendOkAsync<T>(
      this IEndpoint endpoint,
      Result<T> result,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(result);

    if (result.IsFailure)
    {
      await SendProblemsAsync(endpoint, result);
      return;
    }

    await endpoint.HttpContext.Response.SendOkAsync(
      result.Value,
      cancellation: cancellationToken);
  }

  public static async Task SendNoContentAsync(
      this IEndpoint endpoint,
      Result result,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(result);

    if (result.IsFailure)
    {
      await SendProblemsAsync(endpoint, result);
      return;
    }

    await endpoint.HttpContext.Response.SendNoContentAsync(cancellation: cancellationToken);
  }

  public static async Task SendCreatedAsync<T>(
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
      await SendProblemsAsync(endpoint, result);
      return;
    }

    endpoint.HttpContext.Response.Headers.Location = locationFactory(result.Value);

    await endpoint.HttpContext.Response.SendAsync(
        result.Value,
        StatusCodes.Status201Created,
        cancellation: cancellationToken);
  }

  public static async Task SendOkOrNoContentAsync<T>(
    this IEndpoint endpoint,
    Result<T> result,
    Func<T, bool> shouldReturnBody,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(shouldReturnBody);


    if (result.IsFailure)
    {
      await SendProblemsAsync(endpoint, result);
      return;
    }

    if (shouldReturnBody(result.Value))
    {
      await endpoint.HttpContext.Response.SendAsync(
        result.Value,
        StatusCodes.Status200OK,
        cancellation: cancellationToken);
    }
    else
    {
      await endpoint.HttpContext.Response.SendNoContentAsync(cancellation: cancellationToken);
    }

  }
}
