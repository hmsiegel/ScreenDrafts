namespace ScreenDrafts.Modules.Drafts.Features.Participants;

internal sealed class DraftsOptions
{
  public const string SectionName = "Shared:People";

  public string[] CommissionerPersonPublicIds { get; set; } = [];
}
