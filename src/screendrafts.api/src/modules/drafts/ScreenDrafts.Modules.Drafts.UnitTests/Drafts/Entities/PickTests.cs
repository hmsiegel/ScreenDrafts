namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts.Entities;

public class PickTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var position = 1;
    var movie = MovieFactory.CreateMovie().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    var playOrder = 1;

    // Act
    var result = Pick.Create(position, movie, drafter, null, draft, playOrder);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Position.Should().Be(position);
    result.Value.Movie.Should().Be(movie);
    result.Value.Drafter.Should().Be(drafter);
    result.Value.Draft.Should().Be(draft);
    result.Value.PlayOrder.Should().Be(playOrder);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPositionIsLessThanOne()
  {
    // Arrange
    var position = 0;
    var movie = MovieFactory.CreateMovie().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    var playOrder = 1;

    // Act
    var result = Pick.Create(position, movie, drafter, null, draft, playOrder);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenMovieIsNull()
  {
    // Arrange
    var position = 1;
    Movie? movie = null;
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    var playOrder = 1;

    // Act
    var result = Pick.Create(position, movie!, drafter, null, draft, playOrder);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDrafterIsNull()
  {
    // Arrange
    var position = 1;
    var movie = MovieFactory.CreateMovie().Value;
    Drafter? drafter = null;
    var draft = DraftFactory.CreateStandardDraft().Value;
    var playOrder = 1;

    // Act
    var result = Pick.Create(position, movie, drafter!, null, draft, playOrder);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDraftIsNull()
  {
    // Arrange
    var position = 1;
    var movie = MovieFactory.CreateMovie().Value;
    var drafter = DrafterFactory.CreateDrafter();
    Draft? draft = null;
    var playOrder = 1;

    // Act
    var result = Pick.Create(position, movie, drafter, null, draft!, playOrder);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPlayOrderIsLessThanOne()
  {
    // Arrange
    var position = 1;
    var movie = MovieFactory.CreateMovie().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var draft = DraftFactory.CreateStandardDraft().Value;
    var playOrder = 0;

    // Act
    var result = Pick.Create(position, movie, drafter, null, draft, playOrder);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void AddVeto_ShouldReturnSuccess_WhenValidVetoIsProvided()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    var veto = VetoFactory.CreateVeto().Value;

    // Act
    var result = pick.VetoPick(veto);

    // Assert
    result.IsSuccess.Should().BeTrue();
    pick.Veto.Should().Be(veto);
  }

  [Fact]
  public void AddVeto_ShouldReturnFailure_WhenInvalidVetoIsProvided()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    Veto? veto = null;

    // Act
    var result = pick.VetoPick(veto!);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void AddVeto_ShouldReturnFailure_WhenPickAlreadyHasVeto()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    var veto = VetoFactory.CreateVeto().Value;
    pick.VetoPick(veto);
    // Act
    var result = pick.VetoPick(veto);
    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
