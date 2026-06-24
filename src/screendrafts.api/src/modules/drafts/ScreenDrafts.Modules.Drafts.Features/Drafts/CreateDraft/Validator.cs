namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class Validator : AbstractValidator<CreateDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.DraftType).MustBeSmartEnumValue<CreateDraftCommand, DraftType>();
    RuleFor(x => x.SeriesId)
      .NotEmpty()
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Series))
      .WithMessage("SeriesId must be a valid public ID.");

    RuleForEach(x => x.CategoryIds)
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Category))
      .WithMessage("Each CategoryId must be a valid public ID.");

    When(
      x => x.CampaignId is not null,
      () =>
      {
        RuleFor(x => x.CampaignId!)
          .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Campaign))
          .WithMessage("CampaignId must be a valid public ID.");
      }
    );

    RuleForEach(x => x.DrafterIds)
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Drafter))
      .WithMessage("Each DrafterId must be a valid public ID.");

    RuleForEach(x => x.TeamIds)
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DrafterTeam))
      .WithMessage("Each TeamId must be a valid public ID.");

    RuleForEach(x => x.Hosts).SetValidator(new HostInputValidator());
    RuleForEach(x => x.Parts).SetValidator(new PartInputValidator());
  }
}

internal sealed class HostInputValidator : AbstractValidator<CreateDraftHostInput>
{
  public HostInputValidator()
  {
    RuleFor(x => x.HostPublicId)
      .NotEmpty()
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Host))
      .WithMessage("HostPublicId must be a valid public ID.");

    RuleFor(x => x.HostRole).MustBeSmartEnumValue<CreateDraftHostInput, HostRole>();
  }
}

internal sealed class PartInputValidator : AbstractValidator<CreateDraftPartInput>
{
  public PartInputValidator()
  {
    RuleFor(x => x.PartIndex).GreaterThan(0);
    RuleFor(x => x.MinimumPosition).GreaterThan(0);
    RuleFor(x => x.MaximumPosition)
      .GreaterThan(0)
      .GreaterThanOrEqualTo(x => x.MinimumPosition)
      .WithMessage("MaximumPosition must be greater than or equal to MinimumPosition.");

    When(
      x => x.Community is not null,
      () =>
      {
        RuleFor(x => x.Community!.MaxCommunityPicks).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Community!.MaxCommunityVetoes).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Community!.FilmRules).SetValidator(new FilmRuleInputValidator());
      }
    );

    RuleForEach(x => x.Positions).SetValidator(new PositionInputValidator());
  }
}

internal sealed class FilmRuleInputValidator : AbstractValidator<CommunityFilmRuleInput>
{
  public FilmRuleInputValidator()
  {
    RuleFor(x => x.RuleKind).MustBeSmartEnumValue<CommunityFilmRuleInput, CommunityFilmRuleKind>();

    When(
      x => CommunityFilmRuleKind.FromValue(x.RuleKind) == CommunityFilmRuleKind.BoostersPick,
      () =>
      {
        RuleFor(x => x.TargetSlot)
          .NotNull()
          .GreaterThan(0)
          .WithMessage(
            "TargetSlot is required and must be greater than zero for BoostersPick rules."
          );
      }
    );
  }
}

internal sealed class PositionInputValidator : AbstractValidator<DraftPositionInput>
{
  public PositionInputValidator()
  {
    RuleFor(x => x.Name).NotEmpty();
    RuleFor(x => x.Picks).NotEmpty().WithMessage("Each position must have at least one pick slot.");
    RuleForEach(x => x.Picks).GreaterThan(0);
  }
}
