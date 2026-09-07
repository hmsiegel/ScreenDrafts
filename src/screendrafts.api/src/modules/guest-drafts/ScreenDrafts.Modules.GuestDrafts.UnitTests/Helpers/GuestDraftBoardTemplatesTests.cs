namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Helpers;

public class GuestDraftBoardTemplatesTests
{
  [Theory]
  [MemberData(nameof(FixedTypes))]
  public void IsFixed_ShouldReturnTrue_ForStandardAndMiniSuper(GuestDraftType guestDraftType)
  {
    // Act & Assert
    GuestDraftBoardTemplates.IsFixed(guestDraftType).Should().BeTrue();
  }

  [Theory]
  [MemberData(nameof(NonFixedTypes))]
  public void IsFixed_ShouldReturnFalse_ForMiniMegaSuperAndMega(GuestDraftType guestDraftType)
  {
    // Act & Assert
    GuestDraftBoardTemplates.IsFixed(guestDraftType).Should().BeFalse();
  }

  public static TheoryData<GuestDraftType> FixedTypes() =>
    new()
    {
      GuestDraftType.Standard,
      GuestDraftType.MiniSuper,
    };

  public static TheoryData<GuestDraftType> NonFixedTypes() =>
    new()
    {
      GuestDraftType.MiniMega,
      GuestDraftType.Super,
      GuestDraftType.Mega,
    };
}
