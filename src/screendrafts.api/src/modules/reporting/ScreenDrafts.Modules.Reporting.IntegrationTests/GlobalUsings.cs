global using Bogus;

global using FluentAssertions;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;

global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Reporting.Domain.Drafters;
global using ScreenDrafts.Modules.Reporting.Domain.Drafts;
global using ScreenDrafts.Modules.Reporting.Domain.Movies;
global using ScreenDrafts.Modules.Reporting.Features.Drafters.UpdateDrafterHonorifics;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.CreateSpotlight;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.GetActiveSpotlight;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.GetSiteStats;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.MarkDraftCompleted;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.UpsertDraftPartRelease;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.UpsertDraftSummary;
global using ScreenDrafts.Modules.Reporting.Features.Movies.UpdateMovieHonorific;
global using ScreenDrafts.Modules.Reporting.Infrastructure.Database;
global using ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions;

global using Xunit;
