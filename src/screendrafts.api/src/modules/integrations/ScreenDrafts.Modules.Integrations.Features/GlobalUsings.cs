global using System.Reflection;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Inbox;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Common.Application.Outbox;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Presentation.Http;
global using ScreenDrafts.Common.Presentation.Results;
global using ScreenDrafts.Modules.Integrations.Domain.Imdb;
global using ScreenDrafts.Modules.Integrations.Domain.Movies;
global using ScreenDrafts.Modules.Integrations.Features.Movies.SearchFoMovie;
global using ScreenDrafts.Modules.Integrations.IntegrationEvents;
global using ScreenDrafts.Modules.Integrations.PublicApi;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
