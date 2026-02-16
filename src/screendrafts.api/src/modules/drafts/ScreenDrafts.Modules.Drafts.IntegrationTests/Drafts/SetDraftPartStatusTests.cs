using ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class SetDraftPartStatusTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetDraftPartStatus_StartAction_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
  }

  [Fact]
  public async Task SetDraftPartStatus_CompleteAction_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    
    // Start the part first
    var startRequest = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    };
    await Sender.Send(new SetDraftPartStatusCommand { SetDraftPartStatusRequest = startRequest });

    // Complete the part
    var completeRequest = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Complete
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = completeRequest
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
  }

  [Fact]
  public async Task SetDraftPartStatus_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Arrange
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = Faker.Random.AlphaNumeric(10),
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_WithInvalidPartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 999,
      Action = DraftPartStatusAction.Start
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_WithZeroPartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 0,
      Action = DraftPartStatusAction.Start
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_CompleteWithoutStart_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Complete
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  private async Task<string> CreateDraftWithAutoPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var command = new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
      MinPosition = 1,
      MaxPosition = 7,
      AutoCreateFirstPart = true
    };

    var result = await Sender.Send(command);
    return result.Value;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var command = new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    };

    var result = await Sender.Send(command);
    return result.Value;
  }
}
