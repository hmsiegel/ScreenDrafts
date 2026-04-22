namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class CreatePredictionContestantTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreatePredictionContestant_WithValidPerson_ShouldSucceedAsync()
  {
    // Arrange
    var people = new PeopleFactory(Sender, Faker);
    var personPublicId = await people.CreateAndSavePersonAsync();

    var command = new CreatePredictionContestantCommand { PersonPublicId = personPublicId };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
    result.Value.Should().StartWith("pc_");
  }

  [Fact]
  public async Task CreatePredictionContestant_ShouldPersistToDatabase_WhenSuccessfulAsync()
  {
    // Arrange
    var people = new PeopleFactory(Sender, Faker);
    var personPublicId = await people.CreateAndSavePersonAsync();

    var command = new CreatePredictionContestantCommand { PersonPublicId = personPublicId };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var exists = await DbContext.PredictionContestants
      .AnyAsync(c => c.PublicId == result.Value, TestContext.Current.CancellationToken);
    exists.Should().BeTrue();
  }

  [Fact]
  public async Task CreatePredictionContestant_WithNonExistentPerson_ShouldFailAsync()
  {
    // Arrange
    var command = new CreatePredictionContestantCommand { PersonPublicId = "pe_nonexistent123" };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreatePredictionContestant_WhenContestantAlreadyExistsForPerson_ShouldFailAsync()
  {
    // Arrange
    var people = new PeopleFactory(Sender, Faker);
    var personPublicId = await people.CreateAndSavePersonAsync();

    var command = new CreatePredictionContestantCommand { PersonPublicId = personPublicId };

    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act — attempt to create again for the same person
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.ContestantAlreadyExists");
  }

  [Fact]
  public async Task CreateMultiplePredictionContestants_ForDifferentPeople_ShouldAllSucceedAsync()
  {
    // Arrange
    var people = new PeopleFactory(Sender, Faker);
    var personPublicId1 = await people.CreateAndSavePersonAsync();
    var personPublicId2 = await people.CreateAndSavePersonAsync();

    // Act
    var result1 = await Sender.Send(new CreatePredictionContestantCommand { PersonPublicId = personPublicId1 }, TestContext.Current.CancellationToken);
    var result2 = await Sender.Send(new CreatePredictionContestantCommand { PersonPublicId = personPublicId2 }, TestContext.Current.CancellationToken);

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result2.IsSuccess.Should().BeTrue();
    result1.Value.Should().NotBe(result2.Value);
  }
}
