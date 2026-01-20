global using System.Collections.ObjectModel;
global using System.Reflection;

global using Ardalis.SmartEnum;

global using Dapper;

global using FastEndpoints;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Features.Abstractions.Clock;
global using ScreenDrafts.Common.Features.Abstractions.Data;
global using ScreenDrafts.Common.Features.Abstractions.EventBus;
global using ScreenDrafts.Common.Features.Abstractions.Logging;
global using ScreenDrafts.Common.Features.Abstractions.Messaging;
global using ScreenDrafts.Common.Features.Abstractions.Services;
global using ScreenDrafts.Common.Features.Http;
global using ScreenDrafts.Common.Features.Http.Responses;
global using ScreenDrafts.Common.Features.Http.Results;
global using ScreenDrafts.Common.Infrastructure.Authentication;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Helpers;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Lifecycles;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.People;
global using ScreenDrafts.Modules.Drafts.Domain.People.DomainEvents;
global using ScreenDrafts.Modules.Drafts.Domain.People.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.People.Repositories;
global using ScreenDrafts.Modules.Drafts.Features.Abstractions.Data;
global using ScreenDrafts.Modules.Drafts.Features.Extensions;
global using ScreenDrafts.Modules.Drafts.Features.Helpers;
global using ScreenDrafts.Modules.Drafts.Features.Series;
global using ScreenDrafts.Modules.Drafts.IntegrationEvents;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
global using ScreenDrafts.Modules.Users.PublicApi;
