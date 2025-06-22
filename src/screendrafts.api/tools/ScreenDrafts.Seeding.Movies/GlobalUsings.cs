global using System.ComponentModel.DataAnnotations.Schema;
global using System.Globalization;
global using System.Reflection;

global using IMDbApiLib;
global using IMDbApiLib.Models;

global using MediatR;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using OMDbApiNet;
global using OMDbApiNet.Model;

global using ScreenDrafts.Common.Application;
global using ScreenDrafts.Common.Application.CsvFiles;
global using ScreenDrafts.Common.Application.Exceptions;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Seeding;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Configuration;
global using ScreenDrafts.Common.Infrastructure.Database;
global using ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;
global using ScreenDrafts.Modules.Movies.Application;
global using ScreenDrafts.Modules.Movies.Application.Movies.Commands.AddMovie;
global using ScreenDrafts.Modules.Movies.Domain.Movies;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Entities;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Errors;
global using ScreenDrafts.Modules.Movies.Domain.Movies.ValueObjects;
global using ScreenDrafts.Modules.Movies.Infrastructure;
global using ScreenDrafts.Modules.Movies.Infrastructure.Database;
global using ScreenDrafts.Seeding.Movies.Common;
global using ScreenDrafts.Seeding.Movies.Imdb;
global using ScreenDrafts.Seeding.Movies.Imdb.Enums;
global using ScreenDrafts.Seeding.Movies.Imdb.GetOnlineMovie;
global using ScreenDrafts.Seeding.Movies.Seeders;
global using ScreenDrafts.Seeding.Movies.Seeders.Models;

global using Serilog;
