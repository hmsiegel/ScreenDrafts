using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Common.Application.Messaging;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IBaseCommand : IBaseRequest;
