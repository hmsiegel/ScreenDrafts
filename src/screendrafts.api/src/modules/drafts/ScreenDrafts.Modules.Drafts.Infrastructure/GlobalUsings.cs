global using System.Collections.ObjectModel;
global using System.Data.Common;
global using System.Reflection;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

global using Npgsql;

global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Infrastructure.Interceptors;
global using ScreenDrafts.Modules.Drafts.Application.Abstractions.Data;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Database;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;
global using ScreenDrafts.Modules.Drafts.Infrastructure.GameBoards;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Hosts;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Picks;
global using ScreenDrafts.Modules.Drafts.Infrastructure.Vetoes;

global using SmartEnum.EFCore;
