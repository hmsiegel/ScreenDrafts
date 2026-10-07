namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetRecordBookRequest, GetRecordBookResponse>
{
  public override void Configure()
  {
    Get(DraftReportingRoutes.RecordBook);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Stats)
        .WithName(ReportingOpenApi.Names.Stats_GetRecordBook)
        .Produces<GetRecordBookResponse>(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetRecordBookRequest req, CancellationToken ct)
  {
    // The Patreon/Speed toggle only takes effect for Patreon members, as in GET /stats.
    var isPatreonMember = User.HasPermission(ReportingAuth.Permissions.StatsReadPatreon);

    var query = new GetRecordBookQuery { IncludeAll = req.IncludeAll && isPatreonMember };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
