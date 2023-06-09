global using Ardalis.Specification;

global using Contracts.Drafts;

global using ErrorOr;

global using FluentValidation;

global using MediatR;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Logging;

global using ScreenDrafts.Application.Common.Events;
global using ScreenDrafts.Application.Common.Exceptions;
global using ScreenDrafts.Application.Common.FileStorage;
global using ScreenDrafts.Application.Common.Interfaces;
global using ScreenDrafts.Application.Common.Models;
global using ScreenDrafts.Application.Common.Persistence;
global using ScreenDrafts.Application.Common.Specification;
global using ScreenDrafts.Application.Common.Validation;
global using ScreenDrafts.Domain.Catalog;
global using ScreenDrafts.Domain.Common;
global using ScreenDrafts.Domain.Common.Contracts;
global using ScreenDrafts.Domain.Common.DomainErrors;
global using ScreenDrafts.Domain.DraftAggregate;
global using ScreenDrafts.Domain.DraftAggregate.Enums;
global using ScreenDrafts.Domain.DrafterAggregate.ValueObjects;
global using ScreenDrafts.Domain.DraftersAggregate;
global using ScreenDrafts.Shared.Notifications;

global using System.Reflection;
