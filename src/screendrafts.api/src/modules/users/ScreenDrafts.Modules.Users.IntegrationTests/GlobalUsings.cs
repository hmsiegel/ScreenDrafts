global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text.Json.Serialization;

global using Bogus;

global using FluentAssertions;

global using MediatR;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;

global using ScreenDrafts.Common.Application.Authorization;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Users.Application.Users.Commands.AddPermission;
global using ScreenDrafts.Modules.Users.Application.Users.Commands.AddPermissionToRole;
global using ScreenDrafts.Modules.Users.Application.Users.Commands.AddUserRole;
global using ScreenDrafts.Modules.Users.Application.Users.Commands.RegisterUser;
global using ScreenDrafts.Modules.Users.Application.Users.Commands.UpdateUser;
global using ScreenDrafts.Modules.Users.Application.Users.Queries.GetPermissionByCode;
global using ScreenDrafts.Modules.Users.Application.Users.Queries.GetUser;
global using ScreenDrafts.Modules.Users.Application.Users.Queries.GetUserPermissions;
global using ScreenDrafts.Modules.Users.Domain.Users.Errors;
global using ScreenDrafts.Modules.Users.Infrastructure.Database;
global using ScreenDrafts.Modules.Users.Infrastructure.Identity;
global using ScreenDrafts.Modules.Users.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Users.Presentation.Users;

global using Testcontainers.Keycloak;
global using Testcontainers.PostgreSql;
global using Testcontainers.Redis;
