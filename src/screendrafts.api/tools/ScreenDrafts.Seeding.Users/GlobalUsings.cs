global using System.ComponentModel.DataAnnotations.Schema;
global using System.Globalization;
global using System.Reflection;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Abstractions.Exceptions;
global using ScreenDrafts.Common.Application.CsvFiles;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Seeding;
global using ScreenDrafts.Common.Application.Services;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Authorization;
global using ScreenDrafts.Common.Infrastructure.Configuration;
global using ScreenDrafts.Common.Infrastructure.Database;
global using ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;
global using ScreenDrafts.Modules.Drafts.Composition;
global using ScreenDrafts.Modules.Drafts.Domain.People.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Modules.Users.Composition;
global using ScreenDrafts.Modules.Users.Domain.Abstractions.Identity;
global using ScreenDrafts.Modules.Users.Domain.Users;
global using ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;
global using ScreenDrafts.Modules.Users.Infrastructure.Database;
global using ScreenDrafts.Seeding.Users.Common;
global using ScreenDrafts.Seeding.Users.Seeders;

global using Serilog;
