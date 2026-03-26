global using System.Globalization;

global using Bogus;

global using FluentAssertions;

global using Microsoft.EntityFrameworkCore;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Movies.Domain.Medias;
global using ScreenDrafts.Modules.Movies.Domain.Medias.Entities;
global using ScreenDrafts.Modules.Movies.Domain.Medias.Errors;
global using ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;
global using ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;
global using ScreenDrafts.Modules.Movies.Infrastructure.Database;
global using ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Movies.IntegrationTests.TestUtils;

global using Xunit;
