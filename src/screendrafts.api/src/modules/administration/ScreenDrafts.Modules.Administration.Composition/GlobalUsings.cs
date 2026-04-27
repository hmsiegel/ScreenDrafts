global using MassTransit;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Modules.Administration.Features;
global using ScreenDrafts.Modules.Administration.Features.Inbox;
global using ScreenDrafts.Modules.Administration.Features.Outbox;
global using ScreenDrafts.Modules.Administration.Features.PublicApi;
global using ScreenDrafts.Modules.Administration.Infrastructure;
global using ScreenDrafts.Modules.Administration.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Administration.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Administration.PublicApi;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
