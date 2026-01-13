global using System.Reflection;

global using Dapper;

global using FastEndpoints;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;

global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Features.Abstractions.Data;
global using ScreenDrafts.Common.Features.Abstractions.Messaging;
global using ScreenDrafts.Common.Features.Abstractions.Services;
global using ScreenDrafts.Common.Features.Http;
global using ScreenDrafts.Common.Features.Http.Responses;
global using ScreenDrafts.Common.Features.Http.Results;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Features.Abstractions.Data;
