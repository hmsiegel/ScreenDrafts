﻿using ScreenDrafts.Domain.Common.Shared;

using Result = ScreenDrafts.Domain.Common.Shared.Result;

namespace ScreenDrafts.Application.Common.Messaging;

/// <summary>
/// Represents the command interface.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Represents the command interface.
/// </summary>
/// <typeparam name="TResponse">The command response type.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
