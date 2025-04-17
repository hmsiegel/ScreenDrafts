global using System.Reflection;

global using FastEndpoints;

global using MediatR;

global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVeto;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVetoOverride;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafter;
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
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.StartDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.UpdateReleaseDate;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPicks;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPositionsByGameBoard;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetTriviaResultsForDrafter;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;
global using ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;
global using ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHostWithoutUser;
global using ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.GetHost;
global using ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.ListHosts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
