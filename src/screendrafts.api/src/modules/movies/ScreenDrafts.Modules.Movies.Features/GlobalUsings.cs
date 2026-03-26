global using System.Collections.ObjectModel;
global using System.Globalization;
global using System.Reflection;
global using System.Runtime.CompilerServices;

global using Dapper;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Exceptions;
global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Inbox;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Common.Application.Outbox;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Presentation.Http;
global using ScreenDrafts.Common.Presentation.Responses;
global using ScreenDrafts.Common.Presentation.Results;
global using ScreenDrafts.Modules.Integrations.IntegrationEvents;
global using ScreenDrafts.Modules.Integrations.PublicApi;
global using ScreenDrafts.Modules.Movies.Domain.Medias;
global using ScreenDrafts.Modules.Movies.Domain.Medias.DomainEvents;
global using ScreenDrafts.Modules.Movies.Domain.Medias.Entities;
global using ScreenDrafts.Modules.Movies.Domain.Medias.Errors;
global using ScreenDrafts.Modules.Movies.Domain.Medias.Repositories;
global using ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
