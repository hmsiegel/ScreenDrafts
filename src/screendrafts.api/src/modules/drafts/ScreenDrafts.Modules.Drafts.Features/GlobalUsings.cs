global using System.Collections.ObjectModel;
global using System.Reflection;

global using Ardalis.SmartEnum;

global using Dapper;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Inbox;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Outbox;
global using ScreenDrafts.Common.Application.Services;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Presentation.Http;
global using ScreenDrafts.Common.Presentation.Responses;
global using ScreenDrafts.Common.Presentation.Results;
global using ScreenDrafts.Modules.Drafts.Domain.Abstractions.Data;
global using ScreenDrafts.Modules.Drafts.Domain.Campaigns;
global using ScreenDrafts.Modules.Drafts.Domain.Categories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Helpers;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Lifecycles;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Participants;
global using ScreenDrafts.Modules.Drafts.Domain.People;
global using ScreenDrafts.Modules.Drafts.Domain.People.DomainEvents;
global using ScreenDrafts.Modules.Drafts.Domain.People.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.People.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;
global using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;
global using ScreenDrafts.Modules.Drafts.Features.Extensions;
global using ScreenDrafts.Modules.Drafts.Features.Helpers;
global using ScreenDrafts.Modules.Drafts.Features.People;
global using ScreenDrafts.Modules.Drafts.IntegrationEvents;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
global using ScreenDrafts.Modules.Users.PublicApi;
