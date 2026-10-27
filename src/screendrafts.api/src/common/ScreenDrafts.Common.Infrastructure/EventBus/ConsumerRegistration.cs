namespace ScreenDrafts.Common.Infrastructure.EventBus;

internal sealed record ConsumerRegistration(Type EventType, Type HandlerType);
