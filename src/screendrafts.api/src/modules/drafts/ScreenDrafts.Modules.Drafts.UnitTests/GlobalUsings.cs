global using System.Collections.ObjectModel;
global using System.Reflection;

global using Bogus;

global using FluentAssertions;

global using ScreenDrafts.Common.Abstractions.Results;
global using ScreenDrafts.Common.UnitTests;
global using ScreenDrafts.Modules.Drafts.Domain.Campaigns;
global using ScreenDrafts.Modules.Drafts.Domain.Categories;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.Lifecycles;
global using ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.Hosts;
global using ScreenDrafts.Modules.Drafts.Domain.People.DomainEvents;
global using ScreenDrafts.Modules.Drafts.Domain.People.Errors;
global using ScreenDrafts.Modules.Drafts.Domain.People.ValueObjects;
global using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;
global using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;
global using ScreenDrafts.Modules.Drafts.UnitTests.Abstractions;
global using ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

global using Xunit;

