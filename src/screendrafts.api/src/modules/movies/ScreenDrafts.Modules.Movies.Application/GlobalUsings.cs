global using System.Collections.ObjectModel;
global using System.Reflection;

global using Dapper;

global using FluentValidation;

global using MediatR;

global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Exceptions;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Movies.Application.Abstractions.Data;
global using ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;
global using ScreenDrafts.Modules.Movies.Domain.Movies;
global using ScreenDrafts.Modules.Movies.Domain.Movies.DomainEvents;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Entities;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Errors;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Repositories;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
