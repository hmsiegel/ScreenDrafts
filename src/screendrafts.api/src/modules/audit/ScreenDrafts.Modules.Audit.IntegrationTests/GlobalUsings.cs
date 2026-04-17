global using Bogus;

global using Dapper;

global using FluentAssertions;

global using MediatR;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Data;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Audit.Domain;
global using ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;
global using ScreenDrafts.Modules.Audit.Features.AuditLogs.GetDomainEventAuditLogs;
global using ScreenDrafts.Modules.Audit.Features.AuditLogs.GetHttpAuditLogs;
global using ScreenDrafts.Modules.Audit.Infrastructure.Database;
global using ScreenDrafts.Modules.Audit.IntegrationTests.Abstractions;
