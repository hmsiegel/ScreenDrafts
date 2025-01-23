namespace ScreenDrafts.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
