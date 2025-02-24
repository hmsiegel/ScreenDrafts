namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafter;

public sealed record DrafterResponse(Guid Id, string Name, Guid? UserId = null)
{
  public DrafterResponse()
      : this(Guid.Empty, string.Empty)
  {
  }
}
