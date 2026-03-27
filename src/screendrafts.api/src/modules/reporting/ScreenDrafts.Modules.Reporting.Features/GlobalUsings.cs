global using System.Reflection;

global using Dapper;

global using MediatR;

global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Inbox;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Common.Application.Outbox;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Drafts.IntegrationEvents;
global using ScreenDrafts.Modules.Reporting.Domain.Abstractions.Data;
global using ScreenDrafts.Modules.Reporting.Domain.Drafters;
global using ScreenDrafts.Modules.Reporting.Domain.Movies;
global using ScreenDrafts.Modules.Reporting.Features.Drafters.UpdateDrafterHonorifics;
global using ScreenDrafts.Modules.Reporting.Features.Movies.UpdateMovieHonorific;
global using ScreenDrafts.Modules.Reporting.IntegrationEvents;
