namespace ScreenDrafts.Common.Features.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>, IBaseRequest;
