global using MassTransit;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Modules.Communications.Domain.Email;
global using ScreenDrafts.Modules.Communications.Features;
global using ScreenDrafts.Modules.Communications.Features.Inbox;
global using ScreenDrafts.Modules.Communications.Features.Outbox;
global using ScreenDrafts.Modules.Communications.Infrastructure;
global using ScreenDrafts.Modules.Communications.Infrastructure.Email;
global using ScreenDrafts.Modules.Communications.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Communications.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Drafts.IntegrationEvents;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
