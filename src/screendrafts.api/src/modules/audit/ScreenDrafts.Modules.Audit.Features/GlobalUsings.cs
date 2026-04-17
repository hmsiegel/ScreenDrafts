global using System.Globalization;
global using System.Reflection;
global using System.Text;

global using CsvHelper;
global using CsvHelper.Configuration;

global using Dapper;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.Application.EventBus.Dispatchers;
global using ScreenDrafts.Common.Application.Inbox;
global using ScreenDrafts.Common.Application.Messaging;
global using ScreenDrafts.Common.Application.Messaging.Dispatchers;
global using ScreenDrafts.Common.Application.Outbox;
global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Presentation.Http;
global using ScreenDrafts.Common.Presentation.Results;
global using ScreenDrafts.Modules.Audit.Features.Common;
