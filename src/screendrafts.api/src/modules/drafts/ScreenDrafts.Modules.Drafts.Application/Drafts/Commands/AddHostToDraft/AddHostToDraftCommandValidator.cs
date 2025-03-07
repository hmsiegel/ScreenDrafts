namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddHostToDraft;

internal sealed class AddHostToDraftCommandValidator : AbstractValidator<AddHostToDraftCommand>
{
  public AddHostToDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
    RuleFor(x => x.HostId).NotEmpty();
  }
}
