global using System.Globalization;

global using Bogus;

global using FluentAssertions;

global using Microsoft.EntityFrameworkCore;

global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Movies.Application.Movies.Commands.AddMovie;
global using ScreenDrafts.Modules.Movies.Application.Movies.Queries.GetMovie;
global using ScreenDrafts.Modules.Movies.Domain.Movies;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Entities;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Errors;
global using ScreenDrafts.Modules.Movies.Infrastructure.Database;
global using ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

global using Xunit;
