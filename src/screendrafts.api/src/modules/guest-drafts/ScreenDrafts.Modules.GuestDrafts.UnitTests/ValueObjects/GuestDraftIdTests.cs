namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.ValueObjects;

public class GuestDraftIdTests
{
  [Fact]
  public void CreateUnique_ShouldGenerateDifferentValues_OnEachCall()
  {
    // Act
    var first = GuestDraftId.CreateUnique();
    var second = GuestDraftId.CreateUnique();

    // Assert
    first.Should().NotBe(second);
  }

  [Fact]
  public void Create_ShouldSetValue()
  {
    // Arrange
    var value = Guid.NewGuid();

    // Act
    var id = GuestDraftId.Create(value);

    // Assert
    id.Value.Should().Be(value);
  }

  [Fact]
  public void FromString_ShouldRoundTrip_WithTheOriginalValue()
  {
    // Arrange
    var original = GuestDraftId.CreateUnique();

    // Act
    var roundTripped = GuestDraftId.FromString(original.Value.ToString());

    // Assert
    roundTripped.Should().Be(original);
  }

  [Fact]
  public void Empty_ShouldHaveAnEmptyGuidValue()
  {
    // Act & Assert
    GuestDraftId.Empty.Value.Should().Be(Guid.Empty);
  }

  [Fact]
  public void Equality_ShouldReturnTrue_WhenValuesMatch()
  {
    // Arrange
    var value = Guid.NewGuid();

    // Act & Assert
    GuestDraftId.Create(value).Should().Be(GuestDraftId.Create(value));
  }

  [Fact]
  public void Equality_ShouldReturnFalse_WhenValuesDiffer()
  {
    // Act & Assert
    GuestDraftId.CreateUnique().Should().NotBe(GuestDraftId.CreateUnique());
  }
}
