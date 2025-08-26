namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class UpdateReleaseDate(ISender sender) : Endpoint<ReleaseDateRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Put("/drafts/{draftId:guid}/release-date");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithDescription("Update the release date of a draft")
      .WithName(nameof(UpdateReleaseDate));
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(ReleaseDateRequest req, CancellationToken ct)
  {
    var command = new UpdateReleaseDateCommand(req.DraftId, req.releaseDate);
    var result = await _sender.Send(command, ct);

    if (result.IsFailure)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await Send.NoContentAsync(ct);
    }
  }
}

public sealed record ReleaseDateRequest(
  DateOnly releaseDate,
  [FromRoute(Name = "draftId")] Guid DraftId);

internal sealed class UpdateReleaseDateSummary : Summary<UpdateReleaseDate>
{
  public UpdateReleaseDateSummary()
  {
    Summary = "Update the release date of a draft";
    Description = "Update the release date of a draft. This endpoint allows you to update the release date of a specific draft.";
    Response(StatusCodes.Status204NoContent, "Release date updated successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
