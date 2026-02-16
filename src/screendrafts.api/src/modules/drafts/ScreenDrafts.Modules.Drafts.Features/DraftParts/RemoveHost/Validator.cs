namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

internal sealed class Validator : AbstractValidator<RemoveHostDraftPartRequest>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
        .NotEmpty().WithMessage("Draft part ID must be provided.");
    RuleFor(x => x.HostId)
        .NotEmpty().WithMessage("Host ID must be provided.");
  }
}

