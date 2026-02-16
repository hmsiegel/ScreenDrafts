namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddHostToDraft;

internal sealed class AddHostToDraftCommandValidator : AbstractValidator<AddHostToDraftCommand>
{
  public AddHostToDraftCommandValidator()
  {
    RuleFor(x => x.HostId).NotEmpty();
    RuleFor(x => x.DraftPartId).NotNull();
  }
}
