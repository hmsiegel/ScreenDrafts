global using System.Collections.ObjectModel;
global using System.Data.Common;
global using System.Diagnostics;
global using System.Reflection;

global using FastEndpoints;

global using FluentValidation;
global using FluentValidation.Results;

global using MediatR;

global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Common.Domain;
global using ScreenDrafts.Common.Features.Abstractions.Exceptions;
global using ScreenDrafts.Common.Features.Abstractions.Logging;
global using ScreenDrafts.Common.Features.Abstractions.Messaging;
global using ScreenDrafts.Common.Features.Mediation;
global using ScreenDrafts.Common.Features.Mediation.Behaviors;

global using Serilog.Context;
