namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class UpdateReleaseDate(ISender sender) : Endpoint<ReleaseDateRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Put("/drafts/{draftId:guid}/release-date");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(ReleaseDateRequest req, CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var command = new UpdateReleaseDateCommand(draftId, req.releaseDate);
    var result = await _sender.Send(command, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendNoContentAsync(ct);
    }
  }
}

public sealed record ReleaseDateRequest(DateOnly releaseDate);
