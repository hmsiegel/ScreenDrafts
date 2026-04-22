global using System.Collections.Generic;

global using Bogus;

global using Dapper;

global using FluentAssertions;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Administration.Domian;
global using ScreenDrafts.Modules.Administration.Features.Users.AddPermission;
global using ScreenDrafts.Modules.Administration.Features.Users.AddPermissionToRole;
global using ScreenDrafts.Modules.Administration.Features.Users.AddRole;
global using ScreenDrafts.Modules.Administration.Features.Users.AddRoleToUser;
global using ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;
global using ScreenDrafts.Modules.Administration.Features.Users.GetUserRoles;
global using ScreenDrafts.Modules.Administration.Features.Users.ListPermissions;
global using ScreenDrafts.Modules.Administration.Features.Users.RemoveRoleFromUser;
global using ScreenDrafts.Modules.Administration.Infrastructure.Database;
global using ScreenDrafts.Modules.Administration.IntegrationTests.Abstractions;

global using Testcontainers.Keycloak;

global using Xunit;
