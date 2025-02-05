namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafter;

public sealed record DrafterResponse(Guid Id, string Name)
{
  public DrafterResponse()
      : this(Guid.Empty, string.Empty)
  {
  }
}
