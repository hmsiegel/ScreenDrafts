global using System.Collections.ObjectModel;
global using System.Data;
global using System.Data.Common;
global using System.Reflection;

global using Dapper;

global using MassTransit;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
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
global using ScreenDrafts.Common.Application.Services;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Infrastructure;
global using ScreenDrafts.Common.Infrastructure.Database;
global using ScreenDrafts.Common.Infrastructure.Inbox;
global using ScreenDrafts.Common.Infrastructure.Outbox;
global using ScreenDrafts.Common.Infrastructure.Serialization;
global using ScreenDrafts.Modules.Drafts.Domain.Abstractions.Data;
global using ScreenDrafts.Modules.Drafts.Domain.Campaigns;
global using ScreenDrafts.Modules.Drafts.Domain.Categories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Hosts;
global using ScreenDrafts.Modules.Drafts.Domain.People;
global using ScreenDrafts.Modules.Drafts.Domain.People.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.People.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;
global using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Converters;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Inbox;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Outbox;

global using SmartEnum.EFCore;
