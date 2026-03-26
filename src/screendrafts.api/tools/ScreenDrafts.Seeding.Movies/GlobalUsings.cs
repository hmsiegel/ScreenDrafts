global using System.ComponentModel.DataAnnotations.Schema;
global using System.Globalization;
global using System.Reflection;

global using Dapper;

global using MediatR;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Exceptions;
global using ScreenDrafts.Common.Application;
global using ScreenDrafts.Common.Application.CsvFiles;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Seeding;
global using ScreenDrafts.Common.Application.Services;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Configuration;
global using ScreenDrafts.Common.Infrastructure.Database;
global using ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;
global using ScreenDrafts.Modules.Integrations.Composition;
global using ScreenDrafts.Modules.Integrations.Features.Movies.GetOnlineMedia;
global using ScreenDrafts.Modules.Movies.Composition;
global using ScreenDrafts.Modules.Movies.Domain.Medias;
global using ScreenDrafts.Modules.Movies.Domain.Medias.Entities;
global using ScreenDrafts.Modules.Movies.Domain.Medias.ValueObjects;
global using ScreenDrafts.Modules.Movies.Features;
global using ScreenDrafts.Modules.Movies.Features.Movies.AddMedia;
global using ScreenDrafts.Modules.Movies.Infrastructure;
global using ScreenDrafts.Modules.Movies.Infrastructure.Database;
global using ScreenDrafts.Seeding.Movies.Common;
global using ScreenDrafts.Seeding.Movies.Seeders;
global using ScreenDrafts.Seeding.Movies.Seeders.Models;

global using Serilog;
