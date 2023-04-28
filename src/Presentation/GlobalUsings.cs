global using Contracts.Drafter;
global using Contracts.Drafts;

global using ErrorOr;

global using Mapster;

global using MapsterMapper;

global using MediatR;

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.ApiExplorer;
global using Microsoft.AspNetCore.Mvc.ModelBinding;
global using Microsoft.Extensions.DependencyInjection;

global using NSwag.Annotations;

global using ScreenDrafts.Application.Auditing;
global using ScreenDrafts.Application.Catalog.Brands;
global using ScreenDrafts.Application.Catalog.Products;
global using ScreenDrafts.Application.Common.Models;
global using ScreenDrafts.Application.Dashboard;
global using ScreenDrafts.Application.Drafters.Commands;
global using ScreenDrafts.Application.Drafts.Commands.AddDraftersToDraft;
global using ScreenDrafts.Application.Drafts.Commands.CreateDraft;
global using ScreenDrafts.Application.Identity.Roles;
global using ScreenDrafts.Application.Identity.Tokens;
global using ScreenDrafts.Application.Identity.Users;
global using ScreenDrafts.Application.Identity.Users.Password;
global using ScreenDrafts.Application.Multitenancy;
global using ScreenDrafts.Infrastructure.Auth.Permissions;
global using ScreenDrafts.Infrastructure.Middleware;
global using ScreenDrafts.Infrastructure.OpenApi;
global using ScreenDrafts.Presentation.Common.Http;
global using ScreenDrafts.Shared.Authorization;

global using System.Reflection;
global using System.Security.Claims;
