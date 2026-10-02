global using System.Net;
global using System.Net.Http.Json;

global using Bogus;

global using FluentAssertions;

global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.DependencyInjection;

global using ScreenDrafts.Common.Application.Services;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Reporting.Domain.Drafters;
global using ScreenDrafts.Modules.Reporting.Domain.Drafts;
global using ScreenDrafts.Modules.Reporting.Domain.Movies;
global using ScreenDrafts.Modules.Reporting.Features.Drafters.UpdateDrafterHonorifics;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.ActivateSpotlight;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.CreateSpotlight;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.DeactivateSpotlight;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.DeleteSpotlight;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.GetActiveSpotlight;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.GetSiteStats;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.MarkDraftCompleted;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.RotateSpotlight;
global using ScreenDrafts.Modules.Reporting.Features;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.UpdateSpotlight;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.UpsertDraftPartRelease;
global using ScreenDrafts.Modules.Reporting.Features.Drafts.UpsertDraftSummary;
global using ScreenDrafts.Modules.Reporting.Features.Movies.RevertMovieHonorific;
global using ScreenDrafts.Modules.Reporting.Features.Movies.UpdateMovieHonorific;
global using ScreenDrafts.Modules.Reporting.Infrastructure.Database;
global using ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions;

global using Xunit;
