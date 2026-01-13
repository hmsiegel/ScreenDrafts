using ScreenDrafts.Common.Features.Http;
using ScreenDrafts.Common.Features.Http.Responses;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, CreatedResponse>
{
  public override void Configure()
  {
    Post(DraftRoutes.Base);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
        .WithName(DraftsOpenApi.Names.Drafts_CreateDraft)
        .Produces<CreatedResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Features.Permissions.DraftCreate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new Command
    {
      DraftType = req.DraftType,
      Title = req.Title,
      SeriesId = req.SeriesId,
      TotalPicks = req.TotalPicks,
      TotalDrafters = req.TotalDrafters,
      TotalDrafterTeams = req.TotalDrafterTeams,
      TotalHosts = req.TotalHosts,
      AutoCreateFirstPart = req.AutoCreateFirstPart
    };

    var result = await Sender.Send(command, ct);

    await this.MapCreatedResultsAsync(
      result.Map(id => new CreatedResponse(id)),
      created => DraftLocations.ById(created.PublicId),
      ct);
  }
}
