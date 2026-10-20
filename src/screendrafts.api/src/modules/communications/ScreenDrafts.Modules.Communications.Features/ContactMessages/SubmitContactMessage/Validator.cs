namespace ScreenDrafts.Modules.Communications.Features.ContactMessages.SubmitContactMessage;

internal sealed class Validator : AbstractValidator<SubmitContactMessageCommand>
{
  // Turnstile tokens are at most 2048 characters.
  private const int MaxTurnstileTokenLength = 2048;
  private const int MinMessageLength = 10;

  public Validator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name is required.")
      .MaximumLength(ContactMessage.MaxNameLength)
      .WithMessage($"Name must be {ContactMessage.MaxNameLength} characters or fewer.");

    RuleFor(x => x.Email)
      .NotEmpty()
      .WithMessage("Email is required.")
      .MaximumLength(ContactMessage.MaxEmailLength)
      .WithMessage($"Email must be {ContactMessage.MaxEmailLength} characters or fewer.")
      .EmailAddress()
      .WithMessage("Email is not a valid address.");

    RuleFor(x => x.Category)
      .NotEmpty()
      .WithMessage("Category is required.")
      .Must(BeAKnownCategory)
      .WithMessage("Category is not valid.");

    RuleFor(x => x.Message)
      .NotEmpty()
      .WithMessage("Message is required.")
      .MinimumLength(MinMessageLength)
      .WithMessage($"Message must be at least {MinMessageLength} characters.")
      .MaximumLength(ContactMessage.MaxMessageLength)
      .WithMessage($"Message must be {ContactMessage.MaxMessageLength} characters or fewer.");

    RuleFor(x => x.TurnstileToken)
      .NotEmpty()
      .WithMessage("Verification is required.")
      .MaximumLength(MaxTurnstileTokenLength)
      .WithMessage("Verification token is invalid.");
  }

  private static bool BeAKnownCategory(string? category) =>
    Enum.TryParse<ContactCategory>(category, ignoreCase: true, out var parsed)
    && Enum.IsDefined(parsed);
}
