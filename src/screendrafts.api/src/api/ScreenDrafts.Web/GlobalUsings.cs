global using System.Diagnostics;
global using System.Reflection;

global using FastEndpoints;

global using HealthChecks.UI.Client;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.AspNetCore.OpenApi;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.OpenApi.Models;

global using MongoDB.Driver;

global using Npgsql;

global using RabbitMQ.Client;

global using Scalar.AspNetCore;

global using ScreenDrafts.Common.Application;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Configuration;
global using ScreenDrafts.Common.Infrastructure.Database;
global using ScreenDrafts.Common.Infrastructure.EventBus;
global using ScreenDrafts.Modules.Administration.Infrastructure;
global using ScreenDrafts.Modules.Administration.Infrastructure.Database;
global using ScreenDrafts.Modules.Audit.Infrastructure;
global using ScreenDrafts.Modules.Audit.Infrastructure.Database;
global using ScreenDrafts.Modules.Communications.Infrastructure;
global using ScreenDrafts.Modules.Communications.Infrastructure.Database;
global using ScreenDrafts.Modules.Drafts.Infrastructure;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Modules.Integrations.Infrastructure;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Database;
global using ScreenDrafts.Modules.Movies.Infrastructure;
global using ScreenDrafts.Modules.Movies.Infrastructure.Database;
global using ScreenDrafts.Modules.RealTimeUpdates.Infrastructure;
global using ScreenDrafts.Modules.RealTimeUpdates.Infrastructure.Database;
global using ScreenDrafts.Modules.Reporting.Infrastructure;
global using ScreenDrafts.Modules.Reporting.Infrastructure.Database;
global using ScreenDrafts.Modules.Users.Infrastructure;
global using ScreenDrafts.Modules.Users.Infrastructure.Database;
global using ScreenDrafts.Web.Abstractions;
global using ScreenDrafts.Web.Extensions;
global using ScreenDrafts.Web.Logging;
global using ScreenDrafts.Web.Middleware;
global using ScreenDrafts.Web.OpenTelemetry;

global using Serilog;
global using Serilog.Context;
