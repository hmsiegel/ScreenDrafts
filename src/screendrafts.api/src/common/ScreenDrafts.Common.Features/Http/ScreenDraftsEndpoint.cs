namespace ScreenDrafts.Common.Features.Http;

public abstract class ScreenDraftsEndpoint<TRequest> : Endpoint<TRequest>
  where TRequest : notnull
{
  private ISender _sender = null!;

  protected ISender Sender => _sender ??= HttpContext.RequestServices.GetService<ISender>()!;
}

public abstract class ScreenDraftsEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
  where TRequest : notnull
  where TResponse : notnull
{
  private ISender _sender = null!;
  protected ISender Sender => _sender ??= HttpContext.RequestServices.GetService<ISender>()!;
}

public abstract class ScreenDraftsEndpointWithoutRequest<TResponse> : EndpointWithoutRequest<TResponse>
  where TResponse : notnull
{
  private ISender _sender = null!;

  protected ISender Sender => _sender ??= HttpContext.RequestServices.GetService<ISender>()!;
}
