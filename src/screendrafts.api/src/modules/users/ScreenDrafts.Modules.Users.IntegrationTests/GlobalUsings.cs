global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text.Json.Serialization;

global using Bogus;

global using FluentAssertions;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;

global using ScreenDrafts.Common.Abstractions.Authorization;
global using ScreenDrafts.Common.Abstractions.Errors;
global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Users.Domain.Users.Errors;
global using ScreenDrafts.Modules.Users.Features.Admin.AddPermission;
global using ScreenDrafts.Modules.Users.Features.Admin.AddPermissionToRole;
global using ScreenDrafts.Modules.Users.Features.Admin.GetPermissionByCode;
global using ScreenDrafts.Modules.Users.Features.Users.GetByUserId;
global using ScreenDrafts.Modules.Users.Features.Users.Register;
global using ScreenDrafts.Modules.Users.Features.Users.Update;
global using ScreenDrafts.Modules.Users.Infrastructure.Database;
global using ScreenDrafts.Modules.Users.Infrastructure.Identity;
global using ScreenDrafts.Modules.Users.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Users.PublicApi;

global using Serilog;

global using Testcontainers.Keycloak;
