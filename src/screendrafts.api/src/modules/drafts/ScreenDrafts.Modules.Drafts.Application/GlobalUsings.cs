global using System.Collections.ObjectModel;
global using System.Data.Common;
global using System.Reflection;

global using Dapper;

global using FluentValidation;

global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Modules.Drafts.Application.Abstractions.Data;
global using ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafter;
global using ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;
global using ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.GetHost;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Users.PublicApi;
