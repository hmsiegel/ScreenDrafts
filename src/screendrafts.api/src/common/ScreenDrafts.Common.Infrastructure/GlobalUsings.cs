global using System.Buffers;
global using System.Collections.Concurrent;
global using System.Data;
global using System.Data.Common;
global using System.Globalization;
global using System.Reflection;
global using System.Security.Claims;
global using System.Text.Json;

global using CsvHelper;
global using CsvHelper.Configuration;

global using Dapper;

global using MassTransit;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Options;

global using MongoDB.Bson;
global using MongoDB.Bson.Serialization;
global using MongoDB.Bson.Serialization.Serializers;
global using MongoDB.Driver;
global using MongoDB.Driver.Core.Extensions.DiagnosticSources;

global using Newtonsoft.Json;

global using Npgsql;

global using OpenTelemetry.Resources;
global using OpenTelemetry.Trace;

global using Quartz;

global using ScreenDrafts.Common.Application.Authorization;
global using ScreenDrafts.Common.Application.Caching;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.CsvFiles;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Exceptions;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure.Authentication;
global using ScreenDrafts.Common.Infrastructure.Authorization;
global using ScreenDrafts.Common.Infrastructure.Caching;
global using ScreenDrafts.Common.Infrastructure.Clock;
global using ScreenDrafts.Common.Infrastructure.CsvFiles;
global using ScreenDrafts.Common.Infrastructure.Data;
global using ScreenDrafts.Common.Infrastructure.EventBus;
global using ScreenDrafts.Common.Infrastructure.Outbox;

global using StackExchange.Redis;
