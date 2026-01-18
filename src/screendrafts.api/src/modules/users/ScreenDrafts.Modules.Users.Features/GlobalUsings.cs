global using System.Reflection;

global using Dapper;

global using FastEndpoints;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Features.Abstractions.Authorization;
global using ScreenDrafts.Common.Features.Abstractions.Data;
global using ScreenDrafts.Common.Features.Abstractions.EventBus;
global using ScreenDrafts.Common.Features.Abstractions.Exceptions;
global using ScreenDrafts.Common.Features.Abstractions.Messaging;
global using ScreenDrafts.Common.Features.Abstractions.Services;
global using ScreenDrafts.Common.Features.Http;
global using ScreenDrafts.Common.Features.Http.Responses;
global using ScreenDrafts.Common.Features.Http.Results;
global using ScreenDrafts.Common.Infrastructure.Authentication;
global using ScreenDrafts.Modules.Drafts.IntegrationEvents;
global using ScreenDrafts.Modules.Users.Domain.Users;
global using ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;
global using ScreenDrafts.Modules.Users.Domain.Users.Errors;
global using ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;
global using ScreenDrafts.Modules.Users.Features.Abstractions.Data;
global using ScreenDrafts.Modules.Users.Features.Abstractions.Identity;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
