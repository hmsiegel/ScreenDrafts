global using MassTransit;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Modules.Integrations.Domain.Imdb;
global using ScreenDrafts.Modules.Integrations.Features;
global using ScreenDrafts.Modules.Integrations.Features.Inbox;
global using ScreenDrafts.Modules.Integrations.Features.Outbox;
global using ScreenDrafts.Modules.Integrations.Infrastructure;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Imdb;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
