namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Update;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty().WithMessage("PublicId is required.")
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Draft))
      .WithMessage("PublicId is not valid.");

    RuleFor(x => x.Title)
      .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

    RuleFor(x => x.Description)
      .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

    RuleFor(x => x.SeriesPublicId)
      .Must(seriesPublicId => string.IsNullOrEmpty(seriesPublicId) || PublicIdGuards.IsValidWithPrefix(seriesPublicId!, PublicIdPrefixes.Series))
      .WithMessage("SeriesPublicId is not valid.");

    RuleFor(x => x.CampaignPublicId)
      .Must(campaignPublicId => string.IsNullOrEmpty(campaignPublicId) || PublicIdGuards.IsValidWithPrefix(campaignPublicId!, PublicIdPrefixes.Campaign))
      .WithMessage("CampaignPublicId is not valid.");

    RuleFor(x => x.DraftTypeValue)
      .IsInEnum().WithMessage("DraftTypeValue is not valid.");

    RuleFor(x => x.PublicCategoryIds)
      .Must(publicCategoryIds => publicCategoryIds != null && publicCategoryIds.All(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Category)))
      .WithMessage("PublicCategoryIds must be valid.");

  }
}
