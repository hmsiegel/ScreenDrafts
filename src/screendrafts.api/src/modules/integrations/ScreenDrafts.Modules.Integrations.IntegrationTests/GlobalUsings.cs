global using FluentAssertions;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging.Abstractions;

global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Drafts.IntegrationEvents;
global using ScreenDrafts.Modules.Integrations.Domain.Movies;
global using ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;
global using ScreenDrafts.Modules.Integrations.Domain.Zoom;
global using ScreenDrafts.Modules.Integrations.Features.Zoom;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Database;
global using ScreenDrafts.Modules.Integrations.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Integrations.IntegrationTests.Doubles;

global using Xunit;
