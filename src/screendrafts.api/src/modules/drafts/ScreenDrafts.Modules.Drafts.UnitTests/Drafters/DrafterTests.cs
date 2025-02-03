namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters;

public class DrafterTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var name = Faker.Person.FullName;
    var userId = Guid.NewGuid();

    // Act
    var result = Drafter.Create(name, userId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be(name);
    result.Value.UserId.Should().Be(userId);
  }

  [Fact]
  public void AddVeto_ShouldAddVetoToList()
  {
    // Arrange
    var movie = Movie.Create(new MovieTitle(Faker.Lorem.Word()), MovieId.CreateUnique()).Value;
    var drafter = Drafter.Create(Faker.Person.FullName).Value;
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
    var movie = Movie.Create(new MovieTitle(Faker.Lorem.Word()), MovieId.CreateUnique()).Value;
    var drafter = Drafter.Create(Faker.Person.FullName).Value;
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
    var drafter = Drafter.Create(Faker.Person.FullName).Value;
    var draft = Draft.Create(
      Title.Create(Faker.Lorem.Word()),
      DraftType.Standard,
      7,
      2,
      2,
      DraftStatus.Created);

    // Act
    var result = drafter.SetRolloverVeto(RolloverVeto.Create(drafter, draft.Value.Id.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetRolloverVeto_ShouldReturnFailure_WhenRolloverVetoIsAlreadySet()
  {
    // Arrange
    var drafter = Drafter.Create(Faker.Person.FullName).Value;
    var draft = Draft.Create(
      Title.Create(Faker.Lorem.Word()),
      DraftType.Standard,
      7,
      2,
      2,
      DraftStatus.Created);
    drafter.SetRolloverVeto(RolloverVeto.Create(drafter, draft.Value.Id.Value));

    // Act
    var result = drafter.SetRolloverVeto(RolloverVeto.Create(drafter, draft.Value.Id.Value));

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void SetRolloverVetoOverride_ShouldReturnSuccess_WhenRolloverVetoOverrideIsNotSet()
  {
    // Arrange
    var drafter = Drafter.Create(Faker.Person.FullName).Value;
    var draft = Draft.Create(
      Title.Create(Faker.Lorem.Word()),
      DraftType.Standard,
      7,
      2,
      2,
      DraftStatus.Created);
    // Act
    var result = drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(drafter, draft.Value.Id.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetRolloverVetoOverride_ShouldReturnFailure_WhenRolloverVetoOverrideIsAlreadySet()
  {
    // Arrange
    var drafter = Drafter.Create(Faker.Person.FullName).Value;
    var draft = Draft.Create(
      Title.Create(Faker.Lorem.Word()),
      DraftType.Standard,
      7,
      2,
      2,
      DraftStatus.Created);
    drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(drafter, draft.Value.Id.Value));

    // Act
    var result = drafter.SetRolloverVetoOverride(RolloverVetoOverride.Create(drafter, draft.Value.Id.Value));

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void AddDraftStats_ShouldAddDraftStatsToList()
  {
    // Arrange
    var drafter = Drafter.Create(Faker.Person.FullName).Value;
    var draft = Draft.Create(
      Title.Create(Faker.Lorem.Word()),
      DraftType.Standard,
      7,
      2,
      2,
      DraftStatus.Created);

    // Act
    drafter.AddDraftStats(draft.Value);

    // Assert
    drafter.DraftStats.Should().ContainSingle();
  }

  [Fact]
  public void SetDrafterName_ShouldSetNameCorrectly()
  {
    // Arrange
    var drafter = Drafter.Create(Faker.Person.FullName).Value;
    var firstName = Faker.Person.FirstName;
    var lastName = Faker.Person.LastName;
    var middleName = Faker.Person.FirstName;

    // Act
    drafter.SetDrafterName(firstName, lastName, middleName);

    // Assert
    drafter.Name.Should().Be($"{firstName} {middleName} {lastName}");
  }
}
