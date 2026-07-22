namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class EnsureSeasonAndLockPredictionSetsOnDraftPartStartedDomainEventHandler(
  IDraftPartRepository draftPartRepository,
  IDraftPartPredictionRulesRepository rulesRepository,
  IDraftPredictionSetRepository setRepository,
  IPredictionSeasonRepository seasonRepository,
  IPublicIdGenerator publicIdGenerator,
  IDateTimeProvider dateTimeProvider,
  IUnitOfWork unitOfWork
) : DomainEventHandler<DraftPartStartedDomainEvent>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;
  private readonly IDraftPredictionSetRepository _setRepository = setRepository;
  private readonly IPredictionSeasonRepository _seasonRepository = seasonRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public override async Task Handle(
    DraftPartStartedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    // ── Step 1: ensure a current, open season exists. Runs first, always,
    // before any set is touched. ─────────────────────────────────────────
    var currentSeason = await _seasonRepository.GetCurrentAsync(cancellationToken);

    if (currentSeason is null || currentSeason.IsClosed)
    {
      var allSeasons = await _seasonRepository.GetAllAsync(cancellationToken);
      var highestNumber = allSeasons.Count > 0 ? allSeasons.Max(s => s.Number) : 0;
      var nextNumber = highestNumber + 1;

      var numberExists = await _seasonRepository.ExistsByNumberAsync(nextNumber, cancellationToken);

      if (numberExists)
      {
        return;
      }

      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.PredictionSeason);
      var startsOn = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

      var seasonResult = PredictionSeason.Create(nextNumber, startsOn, publicId);

      if (seasonResult.IsFailure)
      {
        return;
      }

      currentSeason = seasonResult.Value;

      _seasonRepository.Add(currentSeason);
    }

    // ── Step 2: resolve the draft part. ──────────────────
    var draftPartId = DraftPartId.Create(domainEvent.DraftPartId);

    var draftPart = await _draftPartRepository.GetByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return;
    }

    var rules = await _rulesRepository.GetByDraftPartIdAsync(draftPart.Id, cancellationToken);

    if (rules is null)
    {
      return;
    }

    var sets = await _setRepository.GetByDraftPartIdAsync(draftPart.Id, cancellationToken);

    if (sets.Count == 0)
    {
      return;
    }

    // ── Step 3: reassign season, THEN lock. Never the other order. ───────
    var now = _dateTimeProvider.UtcNow;
    var snapshot = new PredictionRulesSnapshot(
      rules.PredictionMode,
      rules.RequiredCount,
      rules.TopN
    );

    foreach (var set in sets)
    {
      if (set.IsLocked)
      {
        continue;
      }

      // Reassign to whichever season is current NOW, not whichever was
      // current when this set was first saved — this is the actual fix
      // for the sync gap. A set saved weeks ago against a since-closed
      // season gets refiled under the real current season right here,
      // before it's locked in permanently.
      if (set.SeasonId != currentSeason.Id)
      {
        var reassignResult = set.ReassignSeason(currentSeason);

        if (reassignResult.IsFailure)
        {
          continue;
        }
      }

      var lockResult = set.Lock(snapshot, now);

      if (lockResult.IsFailure)
      {
        continue;
      }

      _setRepository.Update(set);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
