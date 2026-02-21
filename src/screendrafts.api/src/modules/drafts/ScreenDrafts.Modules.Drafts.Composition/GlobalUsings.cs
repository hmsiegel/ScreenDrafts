global using Dapper;

global using MassTransit;

global using MediatR;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Infrastructure.Data;
global using ScreenDrafts.Modules.Drafts.Features.Behaviors;
global using ScreenDrafts.Modules.Drafts.Features.Common;
global using ScreenDrafts.Modules.Drafts.Features.GameBoards;
global using ScreenDrafts.Modules.Drafts.Features.Inbox;
global using ScreenDrafts.Modules.Drafts.Features.Outbox;
global using ScreenDrafts.Modules.Drafts.Infrastructure;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
