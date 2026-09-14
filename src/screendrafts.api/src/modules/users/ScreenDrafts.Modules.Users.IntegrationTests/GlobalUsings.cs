global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json.Serialization;

global using Bogus;

global using Dapper;

global using FluentAssertions;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;

global using ScreenDrafts.Common.Abstractions.Authorization;
global using ScreenDrafts.Common.Abstractions.Errors;
global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.Application.Clock;
global using ScreenDrafts.Common.Application.EventBus;
global using ScreenDrafts.Common.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Users.Domain.Bootstraps;
global using ScreenDrafts.Modules.Users.Domain.EmailChange;
global using ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;
global using ScreenDrafts.Modules.Users.Domain.Users.Errors;
global using ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;
global using ScreenDrafts.Modules.Users.Features.Users.EmailChange;
global using ScreenDrafts.Modules.Users.Features.Users.EmailChange.ClaimEmailBootstrap;
global using ScreenDrafts.Modules.Users.Features.Users.EmailChange.Confirm;
global using ScreenDrafts.Modules.Users.Features.Users.EmailChange.GenerateEmailBootstrapTokens;
global using ScreenDrafts.Modules.Users.Features.Users.EmailChange.Request;
global using ScreenDrafts.Modules.Users.Features.Users.EmailChange.ValidateEmailBootstrapToken;
global using ScreenDrafts.Modules.Users.Features.Users.GetByPublicId;
global using ScreenDrafts.Modules.Users.Features.Users.GetByUserId;
global using ScreenDrafts.Modules.Users.Features.Users.GetUsersByIds;
global using ScreenDrafts.Modules.Users.Features.Users.Register;
global using ScreenDrafts.Modules.Users.Features.Users.RegisterSocialUser;
global using ScreenDrafts.Modules.Users.Domain.Abstractions.Identity;
global using ScreenDrafts.Modules.Users.Features.Users.ChangePassword;
global using ScreenDrafts.Modules.Users.Features.Users.Update;
global using ScreenDrafts.Modules.Users.Infrastructure.Database;
global using ScreenDrafts.Modules.Users.Infrastructure.Identity;
global using ScreenDrafts.Modules.Users.IntegrationEvents;
global using ScreenDrafts.Modules.Users.IntegrationTests.Abstractions;
global using ScreenDrafts.Modules.Users.PublicApi;

global using Testcontainers.Keycloak;

global using Xunit;
