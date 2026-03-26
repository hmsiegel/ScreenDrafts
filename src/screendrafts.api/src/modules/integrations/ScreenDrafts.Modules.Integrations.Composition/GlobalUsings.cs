global using System.Net.Http.Headers;

global using MassTransit;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Options;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Modules.Integrations.Domain.Services.Igdb;
global using ScreenDrafts.Modules.Integrations.Domain.Services.Imdb;
global using ScreenDrafts.Modules.Integrations.Domain.Services.Omb;
global using ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;
global using ScreenDrafts.Modules.Integrations.Features;
global using ScreenDrafts.Modules.Integrations.Features.Inbox;
global using ScreenDrafts.Modules.Integrations.Features.Outbox;
global using ScreenDrafts.Modules.Integrations.Features.PublicApi;
global using ScreenDrafts.Modules.Integrations.Infrastructure;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Services.Igdb;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Services.Imdb;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Services.Omdb;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Services.Tmdb;
global using ScreenDrafts.Modules.Integrations.PublicApi;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
