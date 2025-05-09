﻿global using System.Collections.ObjectModel;
global using System.Data;
global using System.Data.Common;
global using System.Reflection;

global using Dapper;

global using MassTransit;

global using MediatR;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Newtonsoft.Json;

global using Npgsql;

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
global using ScreenDrafts.Modules.Drafts.Application.Abstractions.Data;
global using ScreenDrafts.Modules.Drafts.Application.Behaviors;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPositionsByGameBoard;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Converters;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Data;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Outbox;
global using ScreenDrafts.Modules.Movies.IntegrationEvents;
global using ScreenDrafts.Modules.Users.IntegrationEvents;

global using SmartEnum.EFCore;
