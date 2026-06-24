namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateDraftRequest, CreatedResponse>
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
    Policies(DraftsAuth.Permissions.DraftCreate);
  }

  public override async Task HandleAsync(CreateDraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new CreateDraftCommand
    {
      Title = req.Title,
      DraftType = req.DraftType,
      SeriesId = req.SeriesId,
      Parts =
      [
        .. req.Parts.Select(p => new CreateDraftPartInput
        {
          PartIndex = p.PartIndex,
          MinimumPosition = p.MinimumPosition,
          MaximumPosition = p.MaximumPosition,
          Community = p.Community is null
            ? null
            : new CommunityInput
            {
              MaxCommunityPicks = p.Community.MaxCommunityPicks,
              MaxCommunityVetoes = p.Community.MaxCommunityVetoes,
              FilmRules =
              [
                .. p.Community.FilmRules.Select(r => new CommunityFilmRuleInput
                {
                  RuleKind = r.RuleKind,
                  TargetSlot = r.TargetSlot,
                  TmdbId = r.TmdbId,
                }),
              ],
            },
          Positions =
          [
            .. p.Positions.Select(pos => new DraftPositionInput
            {
              Name = pos.Name,
              Picks = pos.Picks,
              HasBonusVeto = pos.HasBonusVeto,
              HasBonusVetoOverride = pos.HasBonusVetoOverride,
            }),
          ],
        }),
      ],
      Hosts =
      [
        .. req.Hosts.Select(h => new CreateDraftHostInput
        {
          HostPublicId = h.HostPublicId,
          HostRole = h.HostRole,
        }),
      ],
      DrafterIds = req.DrafterIds,
      TeamIds = req.TeamIds,
      CategoryIds = req.CategoryIds,
      CampaignId = req.CampaignId,
    };
    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => DraftLocations.ById(created.PublicId),
      ct
    );
  }
}
