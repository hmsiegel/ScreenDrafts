global using System.Globalization;

global using Bogus;

global using FluentAssertions;

global using MediatR;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Movies.Application.Movies.Commands.AddMovie;
global using ScreenDrafts.Modules.Movies.Domain.Movies;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Entities;
global using ScreenDrafts.Modules.Movies.Infrastructure.Database;
global using ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

global using Testcontainers.PostgreSql;
global using Testcontainers.Redis;

global using Xunit;
