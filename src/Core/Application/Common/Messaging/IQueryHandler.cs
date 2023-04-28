using ScreenDrafts.Domain.Common.Shared;

namespace ScreenDrafts.Application.Common.Messaging;

/// <summary>
/// Represents the query handler interface.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The query response type.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
