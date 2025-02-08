namespace ScreenDrafts.Common.Infrastructure.EventBus;

public sealed record RabbitMqSettings(string Host, string UserName = "guest", string Password = "guest");
