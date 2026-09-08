using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.ValueObjects;

public class GuestDraftIdTests
{
  [Fact]
  public void CreateUnique_ShouldGenerateDifferentValues_OnEachCall()
  {
    // Act
    var first = DraftId.CreateUnique();
    var second = DraftId.CreateUnique();

    // Assert
    first.Should().NotBe(second);
  }

  [Fact]
  public void Create_ShouldSetValue()
  {
    // Arrange
    var value = Guid.NewGuid();

    // Act
    var id = DraftId.Create(value);

    // Assert
    id.Value.Should().Be(value);
  }

  [Fact]
  public void FromString_ShouldRoundTrip_WithTheOriginalValue()
  {
    // Arrange
    var original = DraftId.CreateUnique();

    // Act
    var roundTripped = DraftId.FromString(original.Value.ToString());

    // Assert
    roundTripped.Should().Be(original);
  }

  [Fact]
  public void Empty_ShouldHaveAnEmptyGuidValue()
  {
    // Act & Assert
    DraftId.Empty.Value.Should().Be(Guid.Empty);
  }

  [Fact]
  public void Equality_ShouldReturnTrue_WhenValuesMatch()
  {
    // Arrange
    var value = Guid.NewGuid();

    // Act & Assert
    DraftId.Create(value).Should().Be(DraftId.Create(value));
  }

  [Fact]
  public void Equality_ShouldReturnFalse_WhenValuesDiffer()
  {
    // Act & Assert
    DraftId.CreateUnique().Should().NotBe(DraftId.CreateUnique());
  }
}
