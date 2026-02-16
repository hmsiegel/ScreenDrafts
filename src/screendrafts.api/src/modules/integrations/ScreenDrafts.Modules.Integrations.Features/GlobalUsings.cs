global using System.Reflection;

global using MediatR;

global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Inbox;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Outbox;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Integrations.Domain.Enums;
global using ScreenDrafts.Modules.Integrations.Domain.Imdb;
global using ScreenDrafts.Modules.Integrations.Domain.Movies;
global using ScreenDrafts.Modules.Integrations.IntegrationEvents;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
