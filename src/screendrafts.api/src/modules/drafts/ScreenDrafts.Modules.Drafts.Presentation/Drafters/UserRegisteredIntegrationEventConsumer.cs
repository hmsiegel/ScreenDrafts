namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

public sealed class UserRegisteredIntegrationEventConsumer(ISender sender)
  : IConsumer<UserRegisteredIntegrationEvent>
{
  private readonly ISender _sender = sender;

  public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var drafterName = $"{context.Message.FirstName} {context.Message.MiddleName} {context.Message.LastName}";

    var command = new CreateDrafterCommand(drafterName, context.Message.UserId);

    var result = await _sender.Send(
      command,
      context.CancellationToken);

    if (result.IsFailure)
    {
      throw new ScreenDraftsException(nameof(CreateDrafterCommand), result.Error);
    }
  }
}
