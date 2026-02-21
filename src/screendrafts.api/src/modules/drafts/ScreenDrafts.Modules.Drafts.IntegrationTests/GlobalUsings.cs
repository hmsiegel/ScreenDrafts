global using Bogus;

global using FluentAssertions;

global using MediatR;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Hosts;
global using ScreenDrafts.Modules.Drafts.Domain.Participants;
global using ScreenDrafts.Modules.Drafts.Domain.People.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;
global using ScreenDrafts.Modules.Drafts.Features.Drafters.Create;
global using ScreenDrafts.Modules.Drafts.Features.DraftParts.AddHostToDraftPart;
global using ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;
global using ScreenDrafts.Modules.Drafts.Features.DraftParts.PlayPick;
global using ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;
global using ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;
global using ScreenDrafts.Modules.Drafts.Features.Hosts.Create;
global using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;
global using ScreenDrafts.Modules.Users.Infrastructure.Database;
global using ScreenDrafts.Modules.Users.Infrastructure.Identity;

global using Testcontainers.Keycloak;
