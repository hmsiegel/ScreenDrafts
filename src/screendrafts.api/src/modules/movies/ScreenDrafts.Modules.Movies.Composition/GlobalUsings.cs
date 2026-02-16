global using FluentValidation;

global using MassTransit;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Modules.Integrations.IntegrationEvents;
global using ScreenDrafts.Modules.Movies.Features;
global using ScreenDrafts.Modules.Movies.Features.Inbox;
global using ScreenDrafts.Modules.Movies.Features.Outbox;
global using ScreenDrafts.Modules.Movies.Infrastructure;
global using ScreenDrafts.Modules.Movies.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Movies.Infrastructure.Outbox;
