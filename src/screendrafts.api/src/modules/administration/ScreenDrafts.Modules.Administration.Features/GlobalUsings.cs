global using System.Data;
global using System.Reflection;

global using Dapper;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Exceptions;
global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Inbox;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Common.Application.Outbox;
global using ScreenDrafts.Common.Application.Services;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Presentation.Http;
global using ScreenDrafts.Common.Presentation.Results;
global using ScreenDrafts.Modules.Administration.Domian;
global using ScreenDrafts.Modules.Administration.Features.Users.GetUserRoles;
global using ScreenDrafts.Modules.Administration.PublicApi;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
global using ScreenDrafts.Modules.Users.PublicApi;
