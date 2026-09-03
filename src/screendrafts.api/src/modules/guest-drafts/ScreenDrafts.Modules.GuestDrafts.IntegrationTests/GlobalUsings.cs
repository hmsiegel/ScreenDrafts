global using Bogus;

global using FluentAssertions;

global using MediatR;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts;
global using ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;
global using ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Enums;
global using ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Errors;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Create;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.InviteParticipant;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyCommissionerOverride;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVeto;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.PlayPick;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.RevealPick;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoPick;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoVeto;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.AssignParticipantToPosition;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetCustomPositions;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetFixedBoardLayout;
global using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;
global using ScreenDrafts.Modules.GuestDrafts.Infrastructure.Database;
global using ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Movies.PublicApi;
global using ScreenDrafts.Modules.Users.PublicApi;

global using Xunit;
