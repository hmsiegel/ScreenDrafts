global using MassTransit;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Modules.Audit.Features;
global using ScreenDrafts.Modules.Audit.Features.Inbox;
global using ScreenDrafts.Modules.Audit.Features.Outbox;
global using ScreenDrafts.Modules.Audit.Infrastructure;
global using ScreenDrafts.Modules.Audit.Infrastructure.Consumers;
global using ScreenDrafts.Modules.Audit.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Audit.Infrastructure.Outbox;
