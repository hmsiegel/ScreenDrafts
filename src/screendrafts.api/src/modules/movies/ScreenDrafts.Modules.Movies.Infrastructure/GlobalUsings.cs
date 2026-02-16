global using System.Data;
global using System.Data.Common;
global using System.Reflection;
global using System.Runtime.CompilerServices;

global using Dapper;

global using MassTransit;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Newtonsoft.Json;

global using Quartz;

global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Database;
global using ScreenDrafts.Common.Infrastructure.Inbox;
global using ScreenDrafts.Common.Infrastructure.Outbox;
global using ScreenDrafts.Common.Infrastructure.Serialization;
global using ScreenDrafts.Modules.Movies.Domain.Abstractions.Data;
global using ScreenDrafts.Modules.Movies.Domain.Movies;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Entities;
global using ScreenDrafts.Modules.Movies.Domain.Movies.Repositories;
global using ScreenDrafts.Modules.Movies.Domain.Movies.ValueObjects;
global using ScreenDrafts.Modules.Movies.Infrastructure.Database;
global using ScreenDrafts.Modules.Movies.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Movies.Infrastructure.Outbox;
