global using MediatR;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Options;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Modules.Users.Domain.Abstractions.Identity;
global using ScreenDrafts.Modules.Users.Features.Authorization;
global using ScreenDrafts.Modules.Users.Features.Behaviors;
global using ScreenDrafts.Modules.Users.Features.Inbox;
global using ScreenDrafts.Modules.Users.Features.Outbox;
global using ScreenDrafts.Modules.Users.Features.PublicApi;
global using ScreenDrafts.Modules.Users.Infrastructure;
global using ScreenDrafts.Modules.Users.Infrastructure.Identity;
global using ScreenDrafts.Modules.Users.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Users.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Users.PublicApi;

global using Serilog;
