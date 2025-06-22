global using System.Collections.ObjectModel;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Globalization;
global using System.Reflection;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;

global using Newtonsoft.Json;

global using ScreenDrafts.Common.Application.CsvFiles;
global using ScreenDrafts.Common.Application.Exceptions;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Seeding;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Configuration;
global using ScreenDrafts.Common.Infrastructure.Converters;
global using ScreenDrafts.Common.Infrastructure.Database;
global using ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;
global using ScreenDrafts.Common.Infrastructure.EventBus;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Infrastructure;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Seeding.Drafts.Common;
global using ScreenDrafts.Seeding.Drafts.OpenTelemetry;
global using ScreenDrafts.Seeding.Drafts.Seeders.Drafts;
global using ScreenDrafts.Seeding.Drafts.Seeders.Models;

global using Serilog;
