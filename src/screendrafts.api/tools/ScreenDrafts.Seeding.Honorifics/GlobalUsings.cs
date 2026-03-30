global using System.Data.Common;
global using System.Globalization;
global using System.Reflection;

global using Dapper;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Exceptions;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.Seeding;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Configuration;
global using ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;
global using ScreenDrafts.Seeding.Honorifics.Common;
global using ScreenDrafts.Seeding.Honorifics.Seeders;

global using Serilog;
