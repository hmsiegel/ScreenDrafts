namespace ScreenDrafts.Modules.Drafts.IntegrationTests.WikiExport;

/// <summary>
/// Golden-output tests for the pick lines under "The following films were drafted:".
/// </summary>
public sealed class ExportDraftsWikiPickTests(DraftsIntegrationTestWebAppFactory factory)
  : WikiExportIntegrationTest(factory)
{
  private const string PicksStart = "The following films were drafted:\n\n";

  private sealed record Scenario(
    DraftSeed Draft,
    PartSeed Part,
    ParticipantSeed Alice,
    ParticipantSeed Bob,
    ParticipantSeed Carol
  );

  private async Task<Scenario> NewScenarioAsync()
  {
    var draft = await Seed.DraftAsync("Pick Draft", episodeNumber: 1);
    var part = await Seed.PartAsync(draft, 1, mainFeed: Date(2024, 1, 1));
    var alice = await Seed.ParticipantAsync(part, await Seed.DrafterAsync("Alice"));
    var bob = await Seed.ParticipantAsync(part, await Seed.DrafterAsync("Bob"));
    var carol = await Seed.ParticipantAsync(part, await Seed.DrafterAsync("Carol"));
    return new Scenario(draft, part, alice, bob, carol);
  }

  private async Task<string> PicksAsync(Scenario scenario)
  {
    var page = await ExportDraftPageAsync(scenario.Draft);
    return Between(page, PicksStart, "==Predictions==")[PicksStart.Length..];
  }

  private async Task<PickSeed> PickAsync(
    Scenario s,
    ParticipantSeed by,
    string title,
    int position,
    int playOrder,
    SubDraftSeed? subDraft = null
  ) => await Seed.PickAsync(s.Part, by, await Seed.MovieAsync(title), position, playOrder, subDraft);

  [Fact]
  public async Task Export_ShouldRenderALandedPickAsPositionFilmAndDrafterAsync()
  {
    var s = await NewScenarioAsync();
    await PickAsync(s, s.Alice, "Film A", position: 7, playOrder: 1);

    (await PicksAsync(s)).Should().Be("7. [[Film A]] by [[Alice]]\n\n");
  }

  [Fact]
  public async Task Export_ShouldSeparateEveryPickLineWithABlankLineAsync()
  {
    var s = await NewScenarioAsync();
    await PickAsync(s, s.Alice, "Film A", position: 7, playOrder: 1);
    await PickAsync(s, s.Bob, "Film B", position: 6, playOrder: 2);
    await PickAsync(s, s.Carol, "Film C", position: 5, playOrder: 3);

    (await PicksAsync(s))
      .Should()
      .Be(
        "7. [[Film A]] by [[Alice]]\n\n"
          + "6. [[Film B]] by [[Bob]]\n\n"
          + "5. [[Film C]] by [[Carol]]\n\n"
      );
  }

  [Fact]
  public async Task Export_ShouldStrikeThroughAFinallyVetoedPickAndNameTheVetoerAsync()
  {
    var s = await NewScenarioAsync();
    var pick = await PickAsync(s, s.Alice, "Film B", position: 5, playOrder: 1);
    await Seed.VetoAsync(pick, s.Bob, sequence: 1);

    (await PicksAsync(s)).Should().Be("<s>5. [[Film B]] by [[Alice]]</s> vetoed by [[Bob]]\n\n");
  }

  [Fact]
  public async Task Export_ShouldRenderTwoLines_WhenAVetoedPickIsRedraftedAtTheSamePositionAsync()
  {
    var s = await NewScenarioAsync();
    var vetoed = await PickAsync(s, s.Alice, "Film B", position: 5, playOrder: 1);
    await Seed.VetoAsync(vetoed, s.Bob, sequence: 1);
    await PickAsync(s, s.Carol, "Film C", position: 5, playOrder: 2);

    (await PicksAsync(s))
      .Should()
      .Be(
        "<s>5. [[Film B]] by [[Alice]]</s> vetoed by [[Bob]]\n\n"
          + "5. [[Film C]] by [[Carol]]\n\n"
      );
  }

  [Fact]
  public async Task Export_ShouldKeepAnOverriddenVetoOnTheLandedLineAsync()
  {
    var s = await NewScenarioAsync();
    var pick = await PickAsync(s, s.Alice, "Film D", position: 6, playOrder: 1);
    await Seed.VetoAsync(pick, s.Bob, sequence: 1, overriddenBy: s.Carol);

    (await PicksAsync(s))
      .Should()
      .Be("6. [[Film D]] by [[Alice]] <s>vetoed by [[Bob]]</s> veto overridden by [[Carol]]\n\n");
  }

  [Fact]
  public async Task Export_ShouldRenderACommissionerOverrideAsRemovedAsync()
  {
    var s = await NewScenarioAsync();
    var pick = await PickAsync(s, s.Alice, "Film E", position: 5, playOrder: 1);
    await Seed.CommissionerOverrideAsync(pick);

    (await PicksAsync(s))
      .Should()
      .Be("<s>5. [[Film E]] by [[Alice]]</s> removed via [[Commissioner Override]]\n\n");
  }

  [Fact]
  public async Task Export_ShouldRenderSeveralVetoesOnOnePickInSequenceOrderAsync()
  {
    var s = await NewScenarioAsync();
    var pick = await PickAsync(s, s.Alice, "Film F", position: 4, playOrder: 1);

    // Sequence 2 is inserted first; the line must still read 1 then 2.
    await Seed.VetoAsync(pick, s.Carol, sequence: 2);
    await Seed.VetoAsync(pick, s.Bob, sequence: 1, overriddenBy: s.Alice);

    (await PicksAsync(s))
      .Should()
      .Be(
        "<s>4. [[Film F]] by [[Alice]]</s> "
          + "<s>vetoed by [[Bob]]</s> veto overridden by [[Alice]] "
          + "vetoed by [[Carol]]\n\n"
      );
  }

  [Fact]
  public async Task Export_ShouldNameACommunityVetoerAsPatreonMembersAsync()
  {
    var s = await NewScenarioAsync();
    var community = await Seed.CommunityAsync(s.Part);
    var pick = await PickAsync(s, s.Alice, "Film G", position: 3, playOrder: 1);
    await Seed.VetoAsync(pick, community, sequence: 1);

    (await PicksAsync(s))
      .Should()
      .Be("<s>3. [[Film G]] by [[Alice]]</s> vetoed by [[Patreon Members]]\n\n");
  }

  [Fact]
  public async Task Export_ShouldOrderPicksByPlayOrder_NotInsertionOrderAsync()
  {
    var s = await NewScenarioAsync();
    await PickAsync(s, s.Carol, "Third", position: 5, playOrder: 3);
    await PickAsync(s, s.Alice, "First", position: 7, playOrder: 1);
    await PickAsync(s, s.Bob, "Second", position: 6, playOrder: 2);

    (await PicksAsync(s))
      .Should()
      .Be(
        "7. [[First]] by [[Alice]]\n\n"
          + "6. [[Second]] by [[Bob]]\n\n"
          + "5. [[Third]] by [[Carol]]\n\n"
      );
  }

  [Fact]
  public async Task Export_ShouldOrderPicksBySubDraftIndexThenPlayOrderAsync()
  {
    var s = await NewScenarioAsync();
    var second = await Seed.SubDraftAsync(s.Part, index: 1);
    var first = await Seed.SubDraftAsync(s.Part, index: 0);

    // Inserted worst-first: sub-draft 1 before sub-draft 0, and play order descending.
    await PickAsync(s, s.Carol, "SD1 Pick 1", position: 3, playOrder: 1, second);
    await PickAsync(s, s.Bob, "SD0 Pick 2", position: 2, playOrder: 2, first);
    await PickAsync(s, s.Alice, "SD0 Pick 1", position: 1, playOrder: 1, first);

    (await PicksAsync(s))
      .Should()
      .Be(
        "1. [[SD0 Pick 1]] by [[Alice]]\n\n"
          + "2. [[SD0 Pick 2]] by [[Bob]]\n\n"
          + "3. [[SD1 Pick 1]] by [[Carol]]\n\n"
      );
  }

  [Fact]
  public async Task Export_ShouldAlwaysUseAPlainTitleLink_EvenWhenTwoFilmsShareATitleAsync()
  {
    var s = await NewScenarioAsync();
    var dune84 = await Seed.MovieAsync("Dune", "1984");
    var dune21 = await Seed.MovieAsync("Dune", "2021");
    await Seed.PickAsync(s.Part, s.Alice, dune84, position: 7, playOrder: 1);
    await Seed.PickAsync(s.Part, s.Bob, dune21, position: 6, playOrder: 2);

    var picks = await PicksAsync(s);

    picks.Should().Be("7. [[Dune]] by [[Alice]]\n\n6. [[Dune]] by [[Bob]]\n\n");
    picks.Should().NotContain("1984").And.NotContain("2021");
  }

  [Fact]
  public async Task Export_ShouldNeverEmitACaretAfterAVetoAsync()
  {
    var s = await NewScenarioAsync();
    var finallyVetoed = await PickAsync(s, s.Alice, "Vetoed", position: 7, playOrder: 1);
    var saved = await PickAsync(s, s.Alice, "Saved", position: 6, playOrder: 2);
    var removed = await PickAsync(s, s.Alice, "Removed", position: 5, playOrder: 3);
    await Seed.VetoAsync(finallyVetoed, s.Bob, sequence: 1);
    await Seed.VetoAsync(saved, s.Bob, sequence: 1, overriddenBy: s.Carol);
    await Seed.CommissionerOverrideAsync(removed);

    var page = await ExportDraftPageAsync(s.Draft);

    page.Should().NotContain("^");
  }
}
