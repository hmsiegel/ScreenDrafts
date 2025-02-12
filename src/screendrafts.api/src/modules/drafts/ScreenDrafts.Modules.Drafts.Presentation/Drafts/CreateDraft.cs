﻿namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class CreateDraft(ISender sender) : Endpoint<DraftRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    Policies(Presentation.Permissions.CreateDraft);
  }

  public override async Task HandleAsync(DraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new CreateDraftCommand(
      req.Title,
      DraftType.FromName(req.DraftType),
      req.TotalPicks,
      req.TotalDrafters,
      req.TotalHosts,
      DraftStatus.FromName(req.DraftStatus));

    var draftId = await _sender.Send(command, ct);

    await SendOkAsync(draftId.Value, ct);
  }

}

  public sealed record DraftRequest(
    string Title,
    string DraftType,
    int TotalPicks,
    int TotalDrafters,
    int TotalHosts,
    string DraftStatus);
