global using System.Collections.ObjectModel;

global using Bogus;

global using FluentAssertions;

global using MediatR;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.DependencyInjection;

global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVeto;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVetoOverride;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafter;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVeto;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVetoOverride;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.ListDrafters;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterToDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddHostToDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddMovie;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPick;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AssignDraftPosition;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AssignTriviaResults;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CompleteDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateGameBoard;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.PauseDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveDrafterFromDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveHostFromDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.StartDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.UpdateReleaseDate;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPicks;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPosition;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPositionsByGameBoard;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetGameBoard;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetTriviaResultsForDrafter;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;
global using ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;
global using ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHostWithoutUser;
global using ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.GetHost;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;
global using ScreenDrafts.Modules.Drafts.Presentation.Drafts;

global using Testcontainers.PostgreSql;
global using Testcontainers.Redis;
