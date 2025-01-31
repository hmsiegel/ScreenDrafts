global using System.Buffers;
global using System.Data.Common;
global using System.Security.Claims;
global using System.Text.Json;

global using MassTransit;

global using MediatR;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Options;

global using Npgsql;

global using ScreenDrafts.Common.Application.Authorization;
global using ScreenDrafts.Common.Application.Caching;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Exceptions;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure.Authentication;
global using ScreenDrafts.Common.Infrastructure.Authorization;
global using ScreenDrafts.Common.Infrastructure.Caching;
global using ScreenDrafts.Common.Infrastructure.Clock;
global using ScreenDrafts.Common.Infrastructure.Data;
global using ScreenDrafts.Common.Infrastructure.Interceptors;

global using StackExchange.Redis;
