﻿global using System.Data;
global using System.Data.Common;
global using System.Reflection;

global using Dapper;

global using IMDbApiLib;
global using IMDbApiLib.Models;

global using MassTransit;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Newtonsoft.Json;

global using OMDbApiNet;
global using OMDbApiNet.Model;

global using Quartz;

global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.Logging;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Inbox;
global using ScreenDrafts.Common.Infrastructure.Outbox;
global using ScreenDrafts.Common.Infrastructure.Serialization;
global using ScreenDrafts.Modules.Integrations.Application.Abstractions.Data;
global using ScreenDrafts.Modules.Integrations.Application.Imdb;
global using ScreenDrafts.Modules.Integrations.Domain.Enums;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Database;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Imdb;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Integrations.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
