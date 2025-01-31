global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Reflection;
global using System.Text.Json.Serialization;

global using MediatR;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using ScreenDrafts.Common.Application.Authorization;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure.Interceptors;
global using ScreenDrafts.Modules.Users.Application.Abstractions.Data;
global using ScreenDrafts.Modules.Users.Application.Abstractions.Identity;
global using ScreenDrafts.Modules.Users.Application.Users.Queries.GetUserPermissions;
global using ScreenDrafts.Modules.Users.Domain.Users;
global using ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;
global using ScreenDrafts.Modules.Users.Infrastructure.Authorization;
global using ScreenDrafts.Modules.Users.Infrastructure.Database;
global using ScreenDrafts.Modules.Users.Infrastructure.Identity;
global using ScreenDrafts.Modules.Users.Infrastructure.Users;
