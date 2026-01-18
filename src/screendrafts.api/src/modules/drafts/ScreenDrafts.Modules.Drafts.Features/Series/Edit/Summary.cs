namespace ScreenDrafts.Modules.Drafts.Features.Series.Edit;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Edits an existing series.";
    Description = "Edits an existing series.";
    ExampleRequest = new Request
    {
      Name = "Updated Series Name",
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.Series.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = DraftTypeMask.Standard | DraftTypeMask.MiniMega,
      DefaultDraftType = DraftType.Standard.Value
    };
    Response(StatusCodes.Status204NoContent, "Series edited successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Series not found.");
  }
}
