﻿using ScreenDrafts.Domain.Common.Shared;

namespace ScreenDrafts.Application.Common.Messaging;

/// <summary>
/// Represents the query interface.
/// </summary>
/// <typeparam name="TResponse">The query response type.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
