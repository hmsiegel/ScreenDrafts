namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class CreateDraftCommandHandler(
  IDraftRepository draftRepository,
  ISeriesRepository seriesRepository,
  IHostRepository hostRepository,
  ICategoryRepository categoryRepository,
  ICampaignRepository campaignRepository,
  ParticipantResolver participantResolver,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<CreateDraftCommand, string>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly IHostRepository _hostRepository = hostRepository;
  private readonly ICategoryRepository _categoryRepository = categoryRepository;
  private readonly ICampaignRepository _campaignRepository = campaignRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    CreateDraftCommand request,
    CancellationToken cancellationToken
  )
  {
    // ── Phase 1: resolve all reference data ───────────────────────────────────

    var series = await _seriesRepository.GetByPublicIdAsync(request.SeriesId, cancellationToken);

    if (series is null)
    {
      return Result.Failure<string>(SeriesErrors.SeriesNotFound(request.SeriesId));
    }

    var hosts = new List<(Host Host, int Role)>();
    foreach (var h in request.Hosts)
    {
      var host = await _hostRepository.GetByPublicIdAsync(h.HostPublicId, cancellationToken);

      if (host is null)
      {
        return Result.Failure<string>(HostErrors.NotFound(h.HostPublicId));
      }

      hosts.Add((host, h.HostRole));
    }

    var drafterParticipants = new List<Participant>();
    foreach (var id in request.DrafterIds)
    {
      var resolved = await _participantResolver.ResolveAsync(
        id,
        ParticipantKind.Drafter,
        cancellationToken
      );

      if (!resolved.IsFailure)
      {
        var validation = resolved.Value.Validate();

        if (validation.IsFailure)
        {
          return Result.Failure<string>(validation.Errors);
        }

        drafterParticipants.Add(resolved.Value);
      }
      else
      {
        return Result.Failure<string>(resolved.Errors);
      }
    }

    var teamParticipants = new List<Participant>();
    foreach (var id in request.TeamIds)
    {
      var resolved = await _participantResolver.ResolveAsync(
        id,
        ParticipantKind.Team,
        cancellationToken
      );

      if (resolved.IsFailure)
      {
        return Result.Failure<string>(resolved.Errors);
      }

      var validation = resolved.Value.Validate();

      if (validation.IsFailure)
      {
        return Result.Failure<string>(validation.Errors);
      }

      teamParticipants.Add(resolved.Value);
    }

    IReadOnlyList<Category> categories = [];
    if (request.CategoryIds.Count > 0)
    {
      categories = await _categoryRepository.GetByPublicIdsAsync(
        request.CategoryIds,
        cancellationToken
      );

      if (categories.Count != request.CategoryIds.Count)
      {
        return Result.Failure<string>(DraftErrors.OneOrMoreCategoriesNotFound);
      }
    }

    Campaign? campaign = null;
    if (request.CampaignId is not null)
    {
      campaign = await _campaignRepository.GetByPublicIdAsync(
        request.CampaignId,
        cancellationToken
      );

      if (campaign is null)
      {
        return Result.Failure<string>(CampaignErrors.NotFound(request.CampaignId));
      }
    }

    // Resolve community participant once if any part needs it
    Participant? communityParticipant = null;

    if (request.Parts.Any(p => p.Community is not null))
    {
      var communityResult = await _participantResolver.ResolveAsync(
        null,
        ParticipantKind.Community,
        cancellationToken
      );

      if (communityResult.IsFailure)
      {
        return Result.Failure<string>(communityResult.Errors);
      }

      communityParticipant = communityResult.Value;
    }

    // ── Phase 2: build the Draft aggregate ───────────────────────────────────

    var draftPublicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Draft);
    var createResult = Draft.Create(
      title: new Title(request.Title),
      publicId: draftPublicId,
      draftType: DraftType.FromValue(request.DraftType),
      series: series
    );

    if (createResult.IsFailure)
    {
      return Result.Failure<string>(createResult.Error!);
    }

    var draft = createResult.Value;

    if (categories.Count > 0)
    {
      draft.ReplaceCategories(categories);
    }

    if (campaign is not null)
    {
      draft.SetCampaign(campaign);
    }

    // ── Phase 3: add parts and configure them entirely in-memory ─────────────
    // DraftPart instances created by Draft.AddPart live inside draft._parts.
    // We reach them via draft.Parts after each AddPart call — no repository
    // round-trip needed, and no intermediate SaveChangesAsync that would
    // conflict with the xmin concurrency token on the not-yet-persisted Draft.

    var parts =
      request.Parts.Count > 0
        ? request.Parts
        :
        [
          new CreateDraftPartInput
          {
            PartIndex = 1,
            MinimumPosition = 1,
            MaximumPosition = 7,
          },
        ];

    foreach (var partInput in parts)
    {
      var partPublicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPart);

      var addResult = draft.AddPart(
        partInput.PartIndex,
        partInput.MinimumPosition,
        partInput.MaximumPosition,
        partPublicId
      );

      if (addResult.IsFailure)
      {
        return Result.Failure<string>(addResult.Error!);
      }

      // Reach the newly-created DraftPart directly from the aggregate
      var draftPart = draft.Parts.Single(p => p.PublicId == partPublicId);

      foreach (var (host, role) in hosts)
      {
        var result = role == 0 ? draftPart.SetPrimaryHost(host) : draftPart.AddCoHost(host);

        if (result.IsFailure)
        {
          return Result.Failure<string>(result.Errors);
        }
      }

      foreach (var participant in drafterParticipants)
      {
        var result = draftPart.AddParticipant(participant);

        if (result.IsFailure)
        {
          return Result.Failure<string>(result.Errors);
        }
      }

      foreach (var participant in teamParticipants)
      {
        var result = draftPart.AddParticipant(participant);

        if (result.IsFailure)
        {
          return Result.Failure<string>(result.Errors);
        }
      }

      if (partInput.Community is { } community)
      {
        var addCommunity = draftPart.AddParticipant(
          communityParticipant ?? throw new ScreenDraftsException()
        );

        if (addCommunity.IsFailure)
        {
          return Result.Failure<string>(addCommunity.Errors);
        }

        var limitsResult = draftPart.SetCommunityLimits(
          community.MaxCommunityPicks,
          community.MaxCommunityVetoes
        );

        if (limitsResult.IsFailure)
        {
          return Result.Failure<string>(limitsResult.Errors);
        }

        foreach (var rule in community.FilmRules)
        {
          var rulePublicId = _publicIdGenerator.GeneratePublicId(
            PublicIdPrefixes.CommunityFilmRule
          );
          var ruleKind = CommunityFilmRuleKind.FromValue(rule.RuleKind);

          var ruleResult = draftPart.AddCommunityFilmRule(
            publicId: rulePublicId,
            ruleKind: ruleKind,
            targetSlot: rule.TargetSlot
          );

          if (ruleResult.IsFailure)
          {
            return Result.Failure<string>(ruleResult.Errors);
          }

          if (rule.TmdbId.HasValue)
          {
            var assignResult = draftPart.AssignFilmToCommunityFilmRule(
              rulePublicId,
              rule.TmdbId.Value
            );

            if (assignResult.IsFailure)
            {
              return Result.Failure<string>(assignResult.Errors);
            }
          }
        }
      }

      // Positions — set after participants so GameBoard.AssignDraftPositions
      // sees the correct TotalDrafters + TotalDrafterTeams count.
      if (partInput.Positions.Count > 0)
      {
        if (draftPart.GameBoard is null)
        {
          return Result.Failure<string>(DraftPartErrors.GameBoardNotFound);
        }

        var gameBoard = draftPart.GameBoard;
        gameBoard.ClearPositions();

        var draftPositions = new List<DraftPosition>();
        foreach (var pos in partInput.Positions)
        {
          var posPublicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPosition);
          var posResult = DraftPosition.Create(
            gameBoard,
            pos.Name,
            pos.Picks,
            posPublicId,
            pos.HasBonusVeto,
            pos.HasBonusVetoOverride
          );
          if (posResult.IsFailure)
            return Result.Failure<string>(posResult.Errors);
          draftPositions.Add(posResult.Value);
        }

        var assignResult = gameBoard.AssignDraftPositions(draftPositions);

        if (assignResult.IsFailure)
        {
          return Result.Failure<string>(assignResult.Errors);
        }

        var communityPosResult = draftPart.EnsureCommunityPositions(() =>
          _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPosition)
        );

        if (communityPosResult.IsFailure)
        {
          return Result.Failure<string>(communityPosResult.Errors);
        }
      }
    }

    // Single Add — the UnitOfWorkBehavior calls SaveChangesAsync after the
    // handler returns. No mid-handler saves means xmin is never read on an
    // unpersisted row, so no DbUpdateConcurrencyException.
    _draftRepository.Add(draft);

    return draftPublicId;
  }
}
