using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Helpers;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Helpers;

public class GuestDraftBoardTemplatesTests
{
  [Theory]
  [MemberData(nameof(FixedTypes))]
  public void IsFixed_ShouldReturnTrue_ForStandardAndMiniSuper(DraftType guestDraftType)
  {
    // Act & Assert
    GameBoardTemplates.IsFixed(guestDraftType).Should().BeTrue();
  }

  [Theory]
  [MemberData(nameof(NonFixedTypes))]
  public void IsFixed_ShouldReturnFalse_ForMiniMegaSuperAndMega(DraftType guestDraftType)
  {
    // Act & Assert
    GameBoardTemplates.IsFixed(guestDraftType).Should().BeFalse();
  }

  public static TheoryData<DraftType> FixedTypes() =>
    new() { DraftType.Standard, DraftType.MiniSuper };

  public static TheoryData<DraftType> NonFixedTypes() =>
    new() { DraftType.MiniMega, DraftType.Super, DraftType.Mega };
}
