using Microsoft.Extensions.Logging.Abstractions;

using ScreenDrafts.Common.Application.Clock;
using ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;
using ScreenDrafts.Modules.Drafts.IntegrationEvents;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

/// <summary>
/// Drives <c>DraftPartStatsDomainEventHandler</c> against a seeded draft and reads back the integration event it
/// publishes. Running the real SQL also proves every Dapper alias matches its positional record parameter.
/// </summary>
public sealed class DraftPartStatsDomainEventHandlerTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  private WikiSeeder? _seed;
  private WikiSeeder Seed => _seed ??= new WikiSeeder(DbContext);

  private sealed record Scenario(
    DraftSeed Draft,
    PartSeed Part,
    string PartPublicId,
    DrafterSeed D1,
    DrafterSeed D2,
    DrafterSeed D3,
    Guid TeamId,
    string TeamPublicId,
    Dictionary<int, PickSeed> Picks);

  private Task<int> ExecuteAsync(FormattableString sql) =>
    DbContext.Database.ExecuteSqlAsync(sql, TestContext.Current.CancellationToken);

  /// <summary>
  /// Part 3 of "Stats Draft", series "Stats Series" (policy 2), part type Speed Draft, MainFeed release. Picks:
  ///  1 D1 solo, clean | 2 D2 vetoed by D1 | 3 D2 vetoed by the community | 4 D1 vetoed by D2, overridden by D3 |
  ///  5 D3 removed by the commissioner | 6 team pick (credits D1 + D2), vetoed by its own team |
  ///  7 D3 in Speed sub-draft 2 | 8 community pick.
  /// </summary>
  private async Task<Scenario> SeedScenarioAsync(bool mainFeedRelease = true)
  {
    var draft = await Seed.DraftAsync("Stats Draft", episodeNumber: 12);
    await ExecuteAsync($"UPDATE drafts.series SET name = {"Stats Series"}, canonical_policy = 2");

    var part = await Seed.PartAsync(
      draft,
      index: 3,
      mainFeed: mainFeedRelease ? new DateOnly(2026, 3, 1) : null,
      patreon: new DateOnly(2026, 3, 2));
    await ExecuteAsync($"UPDATE drafts.draft_parts SET draft_type = {5} WHERE id = {part.Id}");

    var d1 = await Seed.DrafterAsync("Dana One");
    var d2 = await Seed.DrafterAsync("Dev Two");
    var d3 = await Seed.DrafterAsync("Dot Three");

    var teamId = Guid.NewGuid();
    var teamPublicId = "dt_statsteam0001";
    await ExecuteAsync(
      $"INSERT INTO drafts.drafter_teams (id, name, public_id) VALUES ({teamId}, {"Team Alpha"}, {teamPublicId})");

    var p1 = await Seed.ParticipantAsync(part, d1);
    var p2 = await Seed.ParticipantAsync(part, d2);
    var p3 = await Seed.ParticipantAsync(part, d3);
    var team = await Seed.ParticipantAsync(part, teamId, WikiSeeder.TeamKind);
    var community = await Seed.CommunityAsync(part);
    var sub = await Seed.SubDraftAsync(part, index: 2);

    var picks = new Dictionary<int, PickSeed>
    {
      [1] = await Seed.PickAsync(part, p1, await Seed.MovieAsync("Heat"), position: 1, playOrder: 1),
      [2] = await Seed.PickAsync(part, p2, await Seed.MovieAsync("Alien"), position: 2, playOrder: 2),
      [3] = await Seed.PickAsync(part, p2, await Seed.MovieAsync("Brazil"), position: 3, playOrder: 3),
      [4] = await Seed.PickAsync(part, p1, await Seed.MovieAsync("Casino"), position: 4, playOrder: 4),
      [5] = await Seed.PickAsync(part, p3, await Seed.MovieAsync("Dune"), position: 5, playOrder: 5),
      [6] = await Seed.PickAsync(part, team, await Seed.MovieAsync("Elf"), position: 6, playOrder: 6),
      [7] = await Seed.PickAsync(part, p3, await Seed.MovieAsync("Fargo"), position: 1, playOrder: 7, sub),
      [8] = await Seed.PickAsync(part, community, await Seed.MovieAsync("Ghost"), position: 7, playOrder: 8),
    };

    await Seed.VetoAsync(picks[2], p1, sequence: 1);
    await Seed.VetoAsync(picks[3], community, sequence: 1);
    await Seed.VetoAsync(picks[4], p2, sequence: 1, overriddenBy: p3);
    await Seed.CommissionerOverrideAsync(picks[5]);
    await Seed.VetoAsync(picks[6], team, sequence: 1);

    await ExecuteAsync($"INSERT INTO drafts.team_pick_credits (id, drafter_id_value, target_pick_id) VALUES ({Guid.NewGuid()}, {d1.Id}, {picks[6].Id})");
    await ExecuteAsync($"INSERT INTO drafts.team_pick_credits (id, drafter_id_value, target_pick_id) VALUES ({Guid.NewGuid()}, {d2.Id}, {picks[6].Id})");

    var partPublicId = await PartPublicIdAsync(part.Id);

    return new Scenario(draft, part, partPublicId, d1, d2, d3, teamId, teamPublicId, picks);
  }

  private async Task<string> PartPublicIdAsync(Guid partId)
  {
    await using var connection = await GetService<IDbConnectionFactory>()
      .OpenConnectionAsync(TestContext.Current.CancellationToken);

    return await connection.ExecuteScalarAsync<string>(
      "SELECT public_id FROM drafts.draft_parts WHERE id = @partId", new { partId })
      ?? throw new InvalidOperationException("Part not found.");
  }

  private async Task<IReadOnlyList<DraftPartStatsRecordedIntegrationEvent>> RunAsync(
    Guid draftId,
    Guid partId,
    string draftPublicId,
    string partPublicId)
  {
    var bus = new CapturingEventBus();
    var handler = new DraftPartStatsDomainEventHandler(
      GetService<IDbConnectionFactory>(),
      bus,
      GetService<IDateTimeProvider>(),
      NullLogger<DraftPartStatsDomainEventHandler>.Instance);

    await handler.Handle(
      new DraftPartCompletedDomainEvent(draftId, partId, 3, draftPublicId, partPublicId, []),
      TestContext.Current.CancellationToken);

    return [.. bus.CapturedEvents.OfType<DraftPartStatsRecordedIntegrationEvent>()];
  }

  private async Task<DraftPartStatsRecordedIntegrationEvent> PublishedAsync(Scenario s)
  {
    var events = await RunAsync(s.Draft.Id, s.Part.Id, s.Draft.PublicId, s.PartPublicId);
    return events.Should().ContainSingle().Subject;
  }

  private static StatsPickRecord PickOf(DraftPartStatsRecordedIntegrationEvent e, Scenario s, int n) =>
    e.Picks.Single(p => p.PickId == s.Picks[n].Id);

  // -------------------------------------------------------------------------
  // Header
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldPublishTheDraftPartHeaderAsync()
  {
    var s = await SeedScenarioAsync();

    var published = await PublishedAsync(s);

    published.DraftId.Should().Be(s.Draft.Id);
    published.DraftPublicId.Should().Be(s.Draft.PublicId);
    published.DraftPartPublicId.Should().Be(s.PartPublicId);
    published.DraftTitle.Should().Be("Stats Draft");
    published.PartIndex.Should().Be(3);
    published.SeriesName.Should().Be("Stats Series");
    published.CanonicalPolicyValue.Should().Be(2);
    published.DraftType.Should().Be("SpeedDraft");
  }

  [Fact]
  public async Task Handle_ShouldSetHasMainFeedRelease_FromTheMainFeedChannelAsync()
  {
    var s = await SeedScenarioAsync(mainFeedRelease: true);

    (await PublishedAsync(s)).HasMainFeedRelease.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldNotSetHasMainFeedRelease_WhenOnlyAPatreonReleaseExistsAsync()
  {
    var s = await SeedScenarioAsync(mainFeedRelease: false);

    (await PublishedAsync(s)).HasMainFeedRelease.Should().BeFalse();
  }

  [Fact]
  public async Task Handle_ShouldPublishNothing_WhenTheDraftAndPartCannotBeResolvedAsync()
  {
    var s = await SeedScenarioAsync();

    // The series column is NOT NULL with a foreign key, so a draft without a series cannot be stored. The same
    // "header not found" path is reached when the part does not belong to the draft or does not exist.
    var otherDraft = await Seed.DraftAsync("Other Draft");

    var wrongDraft = await RunAsync(otherDraft.Id, s.Part.Id, otherDraft.PublicId, s.PartPublicId);
    var missingPart = await RunAsync(s.Draft.Id, Guid.NewGuid(), s.Draft.PublicId, "dp_missing");

    wrongDraft.Should().BeEmpty();
    missingPart.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Picks
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldPublishEveryPick_IncludingVetoedAndRemovedOnes_InPlayableOrderAsync()
  {
    var s = await SeedScenarioAsync();

    var published = await PublishedAsync(s);

    published.Picks.Should().HaveCount(8);
    // Main board first by position, then the sub-draft pick (index NULLS FIRST).
    published.Picks.Select(p => p.MediaTitle).Should().Equal(
      "Heat", "Alien", "Brazil", "Casino", "Dune", "Elf", "Ghost", "Fargo");
  }

  [Fact]
  public async Task Handle_ShouldPublishPlayOrderPositionAndSubDraftIndexAsync()
  {
    var s = await SeedScenarioAsync();

    var published = await PublishedAsync(s);

    var solo = PickOf(published, s, 1);
    solo.Position.Should().Be(1);
    solo.PlayOrder.Should().Be(1);
    solo.SubDraftIndex.Should().BeNull();

    var speed = PickOf(published, s, 7);
    speed.MediaTitle.Should().Be("Fargo");
    speed.SubDraftIndex.Should().Be(2);
    speed.PlayOrder.Should().Be(7);
    speed.Position.Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldPublishTheMediaPublicIdAndTitleAsync()
  {
    var s = await SeedScenarioAsync();

    var pick = PickOf(await PublishedAsync(s), s, 1);

    pick.MediaTitle.Should().Be("Heat");
    pick.MediaPublicId.Should().StartWith("m_");
  }

  // -------------------------------------------------------------------------
  // Vetoes
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldPublishAVetoIssuedByADrafter_WithIssuerDetailsAsync()
  {
    var s = await SeedScenarioAsync();

    var veto = PickOf(await PublishedAsync(s), s, 2).Vetoes.Should().ContainSingle().Subject;

    veto.Sequence.Should().Be(1);
    veto.IssuedByKind.Should().Be(0);
    veto.IssuedByIdValue.Should().Be(s.D1.Id);
    veto.IssuedByPublicId.Should().Be(s.D1.PublicId);
    veto.IssuedByName.Should().Be("Dana One");
    veto.IsOverridden.Should().BeFalse();
    veto.OverriddenByKind.Should().BeNull();
    veto.OverriddenByName.Should().BeNull();
  }

  [Fact]
  public async Task Handle_ShouldPublishACommunityVeto_WithNoPublicIdAsync()
  {
    var s = await SeedScenarioAsync();

    var pick = PickOf(await PublishedAsync(s), s, 3);

    var veto = pick.Vetoes.Should().ContainSingle().Subject;
    veto.IssuedByKind.Should().Be(2);
    veto.IssuedByPublicId.Should().BeNull();
    veto.IssuedByName.Should().Be("Community");
  }

  [Fact]
  public async Task Handle_ShouldPublishWhoOverrodeAnOverriddenVetoAsync()
  {
    var s = await SeedScenarioAsync();

    var veto = PickOf(await PublishedAsync(s), s, 4).Vetoes.Should().ContainSingle().Subject;

    veto.IssuedByIdValue.Should().Be(s.D2.Id);
    veto.IsOverridden.Should().BeTrue();
    veto.OverriddenByKind.Should().Be(0);
    veto.OverriddenByIdValue.Should().Be(s.D3.Id);
    veto.OverriddenByPublicId.Should().Be(s.D3.PublicId);
    veto.OverriddenByName.Should().Be("Dot Three");
  }

  [Fact]
  public async Task Handle_ShouldPublishATeamAsAVetoIssuerAsync()
  {
    var s = await SeedScenarioAsync();

    var veto = PickOf(await PublishedAsync(s), s, 6).Vetoes.Should().ContainSingle().Subject;

    veto.IssuedByKind.Should().Be(1);
    veto.IssuedByIdValue.Should().Be(s.TeamId);
    veto.IssuedByPublicId.Should().Be(s.TeamPublicId);
    veto.IssuedByName.Should().Be("Team Alpha");
  }

  [Fact]
  public async Task Handle_ShouldPublishNoVetoes_WhenAPickWasNeverVetoedAsync()
  {
    var s = await SeedScenarioAsync();

    PickOf(await PublishedAsync(s), s, 1).Vetoes.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Commissioner override
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldFlagACommissionerRemovedPickAsync()
  {
    var s = await SeedScenarioAsync();

    var published = await PublishedAsync(s);

    PickOf(published, s, 5).IsCommissionerOverridden.Should().BeTrue();
    published.Picks.Count(p => p.IsCommissionerOverridden).Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Credits
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldCreditThePlayer_ForASoloPick_WithThePersonPublicIdAsync()
  {
    var s = await SeedScenarioAsync();

    var pick = PickOf(await PublishedAsync(s), s, 1);

    var credit = pick.Credits.Should().ContainSingle().Subject;
    credit.DrafterIdValue.Should().Be(s.D1.Id);
    credit.DrafterPublicId.Should().Be(s.D1.PublicId);
    credit.PersonPublicId.Should().Be(s.D1.Person.PublicId);
    credit.DrafterName.Should().Be("Dana One");
    pick.PlayedByKind.Should().Be(0);
    pick.PlayedByIdValue.Should().Be(s.D1.Id);
    pick.PlayedByName.Should().Be("Dana One");
    pick.PlayedByPublicId.Should().Be(s.D1.PublicId);
  }

  [Fact]
  public async Task Handle_ShouldCreditEachSnapshottedMember_ForATeamPickAsync()
  {
    var s = await SeedScenarioAsync();

    var pick = PickOf(await PublishedAsync(s), s, 6);

    pick.PlayedByKind.Should().Be(1);
    pick.PlayedByName.Should().Be("Team Alpha");
    pick.PlayedByPublicId.Should().Be(s.TeamPublicId);
    pick.Credits.Select(c => c.DrafterIdValue).Should().BeEquivalentTo(new[] { s.D1.Id, s.D2.Id });
    pick.Credits.Select(c => c.PersonPublicId).Should().BeEquivalentTo(s.D1.Person.PublicId, s.D2.Person.PublicId);
  }

  [Fact]
  public async Task Handle_ShouldCreditNoOne_ForACommunityPickAsync()
  {
    var s = await SeedScenarioAsync();

    var pick = PickOf(await PublishedAsync(s), s, 8);

    pick.PlayedByKind.Should().Be(2);
    pick.PlayedByName.Should().Be("Community");
    pick.Credits.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldCreditTheSubDraftPlayerAsync()
  {
    var s = await SeedScenarioAsync();

    PickOf(await PublishedAsync(s), s, 7).Credits.Should().ContainSingle().Which.DrafterIdValue.Should().Be(s.D3.Id);
  }
}
