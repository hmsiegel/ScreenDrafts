global using System.Reflection;

global using Dapper;

global using FluentValidation;

global using MediatR;

global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Exceptions;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Users.Application.Abstractions.Data;
global using ScreenDrafts.Modules.Users.Application.Users.Queries.GetUser;
global using ScreenDrafts.Modules.Users.Domain.Users;
global using ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;
global using ScreenDrafts.Modules.Users.Domain.Users.Errors;
global using ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
