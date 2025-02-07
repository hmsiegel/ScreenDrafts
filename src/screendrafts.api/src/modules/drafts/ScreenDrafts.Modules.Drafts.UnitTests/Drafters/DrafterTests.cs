namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters;

public class DrafterTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange

    // Act
    var result = DrafterFactory.CreateDrafterWithUserId();

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be(result.Value.Name);
    result.Value.UserId.Should().Be(result.Value.UserId);
  }

  [Fact]
  public void AddVeto_ShouldAddVetoToList()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var drafter = DrafterFactory.CreateDrafterWithUserId().Value;
    var pick = Pick.Create(Faker.Random.Number(1, 7), movie, drafter).Value;

    // Act
    drafter.AddVeto(pick);

    // Assert
    drafter.Vetoes.Should().ContainSingle();
  }

  [Fact]
  public void AddVetoOverride_ShouldAddVetoOverrideToList()
  {
    // Arrange
    var movie = MovieFactory.CreateMovie().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var pick = Pick.Create(Faker.Random.Number(1, 7), movie, drafter).Value;
    var veto = Veto.Create(pick);

    // Act
    drafter.AddVetoOverride(veto);

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
    var result = drafter.SetRolloverVeto(RolloverVeto.Create(drafter, draft.Id.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetRolloverVeto_ShouldReturnFailure_WhenRolloverVetoIsAlreadySet()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    drafter.SetRolloverVeto(RolloverVeto.Create(drafter, draft.Id.Value));

    // Act
    var result = drafter.SetRolloverVeto(RolloverVeto.Create(drafter, draft.Id.Value));

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
    var result = drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(drafter, draft.Id.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetRolloverVetoOverride_ShouldReturnFailure_WhenRolloverVetoOverrideIsAlreadySet()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(drafter, draft.Id.Value));

    // Act
    var result = drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(drafter, draft.Id.Value));

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

  [Fact]
  public void SetDrafterName_ShouldSetNameCorrectly()
  {
    // Arrange
    var drafter = DrafterFactory.CreateDrafter();
    var firstName = Faker.Person.FirstName;
    var lastName = Faker.Person.LastName;
    var middleName = Faker.Person.FirstName;

    // Act
    drafter.SetDrafterName(firstName, lastName, middleName);

    // Assert
    drafter.Name.Should().Be($"{firstName} {middleName} {lastName}");
  }
}
