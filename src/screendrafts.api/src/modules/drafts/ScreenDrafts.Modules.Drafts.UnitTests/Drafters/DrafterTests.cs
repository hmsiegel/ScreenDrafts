namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters;

public class DrafterTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;

    // Act
    var result = Drafter.Create(person);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Person.Should().Be(person);
    result.Value.Person.FirstName.Should().Be(person.FirstName);
    result.Value.Person.LastName.Should().Be(person.LastName);
  }

  [Fact]
  public void AddVeto_ShouldAddVetoToList()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var movie = MovieFactory.CreateMovie().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var pick = Pick.Create(
      Faker.Random.Number(1, 7),
      movie,
      drafter,
      null,
      draft,
      Faker.Random.Number(1, 9)).Value;

    var veto = Veto.Create(pick, drafter, null).Value;

    // Act
    drafter.AddVeto(veto);

    // Assert
    drafter.Vetoes.Should().ContainSingle();
  }

  [Fact]
  public void AddVetoOverride_ShouldAddVetoOverrideToList()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var movie = MovieFactory.CreateMovie().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var pick = Pick.Create(
      Faker.Random.Number(1, 7),
      movie,
      drafter,
      null,
      draft,
      Faker.Random.Number(1, 9)).Value;

    var veto = Veto.Create(pick, drafter, null).Value;

    var vetoOverride = VetoOverride.Create(veto, drafter, null);

    // Act
    drafter.AddVetoOverride(vetoOverride.Value);

    // Assert
    drafter.VetoOverrides.Should().ContainSingle();
  }

  [Fact]
  public void SetRolloverVeto_ShouldReturnSuccess_WhenRolloverVetoIsSet()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;

    // Act
    var result = drafter.SetRolloverVeto(RolloverVeto.Create(
      drafter,
      null,
      draft.Id.Value).Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetRolloverVeto_ShouldReturnFailure_WhenRolloverVetoIsAlreadySet()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    drafter.SetRolloverVeto(RolloverVeto.Create(
          drafter,
          null,
          draft.Id.Value).Value);

    // Act
    var result = drafter.SetRolloverVeto(RolloverVeto.Create(
      drafter,
      null,
      draft.Id.Value).Value);
    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void SetRolloverVetoOverride_ShouldReturnSuccess_WhenRolloverVetoOverrideIsNotSet()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    // Act
    var result = drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(
      drafter,
      null,
      draft.Id.Value).Value);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetRolloverVetoOverride_ShouldReturnFailure_WhenRolloverVetoOverrideIsAlreadySet()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(
          drafter,
          null,
          draft.Id.Value).Value);

    // Act
    var result = drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(
      drafter,
      null,
      draft.Id.Value).Value);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void AddDraftStats_ShouldAddDraftStatsToList()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;

    // Act
    drafter.AddDraftStats(draft);

    // Assert
    drafter.DraftStats.Should().ContainSingle();
  }
}
