global using System.Net;
global using System.Net.Http.Json;

global using Bogus;

global using FluentAssertions;

global using MediatR;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.DependencyInjection;

global using ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

global using Testcontainers.PostgreSql;
global using Testcontainers.Redis;
