namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record TitleHonorificValidation
{
  public TitleHonorificSpec? Spec { get; private init; }
  public SDError? Error { get; private init; }

  public static TitleHonorificValidation Ok(TitleHonorificSpec spec) => new() { Spec = spec };

  public static TitleHonorificValidation Fail(SDError error) => new() { Error = error };
}
