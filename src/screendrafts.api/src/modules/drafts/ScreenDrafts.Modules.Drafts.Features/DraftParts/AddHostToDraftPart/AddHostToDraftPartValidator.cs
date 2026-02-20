using ScreenDrafts.Modules.Drafts.Domain.Hosts;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddHostToDraftPart;

internal sealed class AddHostToDraftPartValidator : AbstractValidator<AddHostToDraftPartCommand>
{
  public AddHostToDraftPartValidator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty().WithMessage("Draft part ID is required.");
    RuleFor(x => x.HostPublicId)
      .NotEmpty().WithMessage("Host public ID is required.");
    RuleFor(x => x.HostRole)
      .Must(role => HostRole.List.Any(r => r.Value == role.Value))
      .WithMessage("Invalid host role. Valid values are: " + string.Join(", ", SmartEnum<HostRole>.List));
  }
}
