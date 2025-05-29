namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class CreateDraft(ISender sender) : Endpoint<CreateDraftRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
        .WithName(nameof(CreateDraft))
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(Presentation.Permissions.CreateDraft);
  }

  public override async Task HandleAsync(CreateDraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new CreateDraftCommand(
      req.Title,
      DraftType.FromName(req.DraftType),
      req.TotalPicks,
      req.TotalDrafters,
      req.TotalDrafterTeams,
      req.TotalHosts,
      EpisodeType.FromName(req.EpisodeType),
      DraftStatus.FromName(req.DraftStatus));

    var draftId = await _sender.Send(command, ct);

    await SendOkAsync(draftId.Value, ct);
  }

}

public sealed record CreateDraftRequest(
  string Title,
  string DraftType,
  int TotalPicks,
  int TotalDrafters,
  int TotalDrafterTeams,
  int TotalHosts,
  string EpisodeType,
  string DraftStatus);

internal sealed class CreateDraftSummary : Summary<CreateDraft>
{
  public CreateDraftSummary()
  {
    Summary = "Create a new draft";
    Description = "Creates a new draft with the specified parameters.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the created draft.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to create a draft.");
  }
}
