using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Edit;
using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;

namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Edit;

internal sealed class Validator : AbstractValidator<EditSeriesCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Series))
      .WithMessage("Series ID must be provided.");

    RuleFor(x => x.Name)
      .MaximumLength(100).WithMessage("Series name must not exceed 100 characters.")
      .When(x => x.Name is not null);

    RuleFor(x => x.Kind)
      .MustBeSmartEnumValue<EditSeriesCommand, SeriesKind>();

    RuleFor(x => x.CanonicalPolicy)
      .MustBeSmartEnumValue<EditSeriesCommand, CanonicalPolicy>();

    RuleFor(x => x.ContinuityScope)
      .MustBeSmartEnumValue<EditSeriesCommand, ContinuityScope>();

    RuleFor(x => x.ContinuityDateRule)
      .MustBeSmartEnumValue<EditSeriesCommand, ContinuityDateRule>();

    RuleFor(x => (DraftTypeMask)x.AllowedDraftTypes)
      .Must(m => m != DraftTypeMask.None)
      .WithMessage("At least one allowed draft type must be specified.")
      .Must(m => m.IsValidKnownMask())
      .WithMessage("Allowed draft types contain invalid values.");

    RuleFor(x => x.DefaultDraftType)
      .MustBeSmartEnumValueWhenPresent<EditSeriesCommand, DraftType>();

    RuleFor(x => x)
      .Must(x =>
      {
        if (!x.DefaultDraftType.HasValue)
        {
          return true;
        }
        var mask = (DraftTypeMask)x.AllowedDraftTypes;
        return DraftType.TryFromValue(x.DefaultDraftType.Value, out var draftType)
          && mask.Allows(draftType);
      })
      .WithMessage("Default draft type must be one of the allowed draft types.");
  }
}

