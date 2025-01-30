global using System.Reflection;

global using FastEndpoints;

global using MassTransit;

global using MediatR;

global using Microsoft.AspNetCore.Http;

global using ScreenDrafts.Common.Application.Exceptions;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateGameBoard;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
