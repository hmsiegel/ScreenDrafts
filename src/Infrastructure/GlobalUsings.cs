global using Ardalis.Specification;
global using Ardalis.Specification.EntityFrameworkCore;

global using Finbuckle.MultiTenant;
global using Finbuckle.MultiTenant.EntityFrameworkCore;

global using Hangfire;
global using Hangfire.Client;
global using Hangfire.Logging;
global using Hangfire.Server;

global using Mapster;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Data.Sqlite;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Query;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.FileProviders;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using MySqlConnector;

global using Npgsql;

global using Oracle.ManagedDataAccess.Client;

global using OrchardCore.Localization;

global using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

global using ScreenDrafts.Application.Common.Caching;
global using ScreenDrafts.Application.Common.Events;
global using ScreenDrafts.Application.Common.Exceptions;
global using ScreenDrafts.Application.Common.FileStorage;
global using ScreenDrafts.Application.Common.Interfaces;
global using ScreenDrafts.Application.Common.Mailing;
global using ScreenDrafts.Application.Common.Models;
global using ScreenDrafts.Application.Common.Persistence;
global using ScreenDrafts.Application.Common.Specification;
global using ScreenDrafts.Application.Identity.Users;
global using ScreenDrafts.Domain.Catalog;
global using ScreenDrafts.Domain.Common.Contracts;
global using ScreenDrafts.Domain.DraftAggregate;
global using ScreenDrafts.Domain.DraftAggregate.ValueObjects;
global using ScreenDrafts.Domain.DrafterAggregate.ValueObjects;
global using ScreenDrafts.Domain.DraftersAggregate;
global using ScreenDrafts.Domain.HostAggregate.ValueObject;
global using ScreenDrafts.Domain.Identity;
global using ScreenDrafts.Domain.MovieAggregate;
global using ScreenDrafts.Domain.MovieAggregate.ValueObjects;
global using ScreenDrafts.Infrastructure.Auditing;
global using ScreenDrafts.Infrastructure.Auth;
global using ScreenDrafts.Infrastructure.BackgroundJobs;
global using ScreenDrafts.Infrastructure.Caching;
global using ScreenDrafts.Infrastructure.Common;
global using ScreenDrafts.Infrastructure.Common.Extensions;
global using ScreenDrafts.Infrastructure.Cors;
global using ScreenDrafts.Infrastructure.FileStorage;
global using ScreenDrafts.Infrastructure.Identity;
global using ScreenDrafts.Infrastructure.Localization;
global using ScreenDrafts.Infrastructure.Mailing;
global using ScreenDrafts.Infrastructure.Mapping;
global using ScreenDrafts.Infrastructure.Middleware;
global using ScreenDrafts.Infrastructure.Multitenancy;
global using ScreenDrafts.Infrastructure.Notifications;
global using ScreenDrafts.Infrastructure.OpenApi;
global using ScreenDrafts.Infrastructure.Persistence;
global using ScreenDrafts.Infrastructure.Persistence.Configuration;
global using ScreenDrafts.Infrastructure.Persistence.ConnectionString;
global using ScreenDrafts.Infrastructure.Persistence.Context;
global using ScreenDrafts.Infrastructure.Persistence.Initialization;
global using ScreenDrafts.Infrastructure.Persistence.Repository;
global using ScreenDrafts.Infrastructure.SecurityHeaders;
global using ScreenDrafts.Infrastructure.Validations;
global using ScreenDrafts.Shared.Authorization;
global using ScreenDrafts.Shared.Multitenancy;

global using Serilog;

global using SmartEnum.EFCore;

global using System.ComponentModel.DataAnnotations;
global using System.Data;
global using System.Data.SqlClient;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Security.Claims;
global using System.Text;
