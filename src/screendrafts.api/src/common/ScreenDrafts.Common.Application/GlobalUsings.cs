global using System.Collections.Concurrent;
global using System.Data.Common;
global using System.Diagnostics;
global using System.Reflection;

global using FluentValidation;
global using FluentValidation.Results;

global using MediatR;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Errors;
global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Behaviors;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Domain;

global using Serilog.Context;
