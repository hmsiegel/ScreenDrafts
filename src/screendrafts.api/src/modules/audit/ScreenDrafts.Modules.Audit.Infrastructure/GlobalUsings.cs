global using System.Data;
global using System.Data.Common;
global using System.Globalization;
global using System.Net.Http.Headers;
global using System.Reflection;
global using System.Security.Claims;
global using System.Text.Json;

global using Dapper;

global using FastEndpoints;

global using MassTransit;

global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using MongoDB.Bson;
global using MongoDB.Bson.Serialization.Attributes;
global using MongoDB.Driver;

global using Newtonsoft.Json;

global using Quartz;

global using ScreenDrafts.Common.Abstractions.Exceptions;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Database;
global using ScreenDrafts.Common.Infrastructure.Inbox;
global using ScreenDrafts.Common.Infrastructure.Outbox;
global using ScreenDrafts.Common.Infrastructure.Serialization;
global using ScreenDrafts.Modules.Audit.Domain;
global using ScreenDrafts.Modules.Audit.Domain.Abstractions.Data;
global using ScreenDrafts.Modules.Audit.Infrastructure.Database;
global using ScreenDrafts.Modules.Audit.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Audit.Infrastructure.Mongo;
global using ScreenDrafts.Modules.Audit.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Audit.Infrastructure.Persistence;
global using ScreenDrafts.Modules.Audit.Infrastructure.Processors;
