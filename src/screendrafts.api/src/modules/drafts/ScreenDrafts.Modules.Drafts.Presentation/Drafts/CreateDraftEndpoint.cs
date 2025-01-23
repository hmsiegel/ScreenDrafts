namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

public class CreateDraftEndpoint(ISender sender) : Endpoint<DraftRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    AllowAnonymous();
  }

  public override async Task HandleAsync(DraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new CreateDraftCommand(
      req.Title,
      DraftType.FromValue(req.DraftType),
      req.NumberOfDrafters,
      req.NumberOfCommissioners,
      req.NumberOfMovies);

    var draftId = await _sender.Send(command, ct);

    await SendOkAsync(draftId.Value, ct);
  }
}

