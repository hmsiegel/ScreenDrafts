namespace ScreenDrafts.Common.Features.Abstractions.Extensions;

public static class ValidatorExtensions
{
  public static bool BeValidGuid(this Guid id)
  {
    return id != Guid.Empty;
  }
}
