namespace ScreenDrafts.Modules.Drafts.Features.People.UploadAvatar;

internal sealed class Validator : AbstractValidator<UploadAvatarCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Person))
      .WithMessage("PublicId is not valid.");
    RuleFor(x => x.FileStream).NotNull().WithMessage("File stream is required.");
    RuleFor(x => x.FileName).NotEmpty().WithMessage("File name is required.");
    RuleFor(x => x.ContentType).NotEmpty().WithMessage("Content type is required.");
  }
}
