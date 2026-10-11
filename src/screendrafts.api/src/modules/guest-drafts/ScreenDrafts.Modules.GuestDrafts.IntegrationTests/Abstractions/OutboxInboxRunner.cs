using System.Reflection;

using MassTransit;

using Quartz;

using ScreenDrafts.Common.Application.Data;
using ScreenDrafts.Common.Application.EventBus.Dispatchers;
using ScreenDrafts.Common.Application.Messaging.Dispatchers;
using ScreenDrafts.Modules.GuestDrafts.Infrastructure.Inbox;
using ScreenDrafts.Modules.GuestDrafts.Infrastructure.Outbox;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>
/// Drives the module's real <c>ProcessOutboxJob</c> / <c>ProcessInboxJob</c> and the real
/// MassTransit <c>IntegrationEventConsumer</c> without a Quartz scheduler or a broker. The
/// jobs only read <c>CancellationToken</c> from the Quartz context, and the consumer only
/// reads <c>Message</c> from the MassTransit context, so those are proxied.
/// </summary>
public static class OutboxInboxRunner
{
  private const int BatchSize = 100;

  public static async Task RunOutboxAsync(IServiceProvider services, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(services);

    using var scope = services.CreateScope();
    var sp = scope.ServiceProvider;

    var job = new ProcessOutboxJob(
      sp.GetRequiredService<IDbConnectionFactory>(),
      sp.GetRequiredService<IServiceScopeFactory>(),
      sp.GetRequiredService<IDateTimeProvider>(),
      Options.Create(new OutboxOptions { BatchSize = BatchSize }),
      NullLogger<ProcessOutboxJob>.Instance,
      sp.GetRequiredService<IGuestDraftsDomainEventDispatcher>()
    );

    await job.Execute(JobContext(cancellationToken));
  }

  public static async Task RunInboxAsync(IServiceProvider services, CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(services);

    using var scope = services.CreateScope();
    var sp = scope.ServiceProvider;

    var job = new ProcessInboxJob(
      sp.GetRequiredService<IDbConnectionFactory>(),
      sp.GetRequiredService<IServiceScopeFactory>(),
      sp.GetRequiredService<IDateTimeProvider>(),
      Options.Create(new InboxOptions { BatchSize = BatchSize }),
      NullLogger<ProcessInboxJob>.Instance,
      sp.GetRequiredService<IGuestDraftsIntegrationEventDispatcher>()
    );

    await job.Execute(JobContext(cancellationToken));
  }

  /// <summary>Writes the event into <c>guest_drafts.inbox_messages</c> through the real consumer.</summary>
  public static async Task ReceiveAsync<TEvent>(
    IServiceProvider services,
    TEvent integrationEvent,
    CancellationToken cancellationToken
  )
    where TEvent : IntegrationEvent
  {
    ArgumentNullException.ThrowIfNull(services);

    using var scope = services.CreateScope();
    var consumer = new IntegrationEventConsumer<TEvent>(
      scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>()
    );

    var proxy = DispatchProxy.Create<ConsumeContext<TEvent>, ConsumeContextProxy<TEvent>>();
    ((ConsumeContextProxy<TEvent>)(object)proxy).Message = integrationEvent;

    await consumer.Consume(proxy);
  }

  private static IJobExecutionContext JobContext(CancellationToken cancellationToken)
  {
    var proxy = DispatchProxy.Create<IJobExecutionContext, JobContextProxy>();
    ((JobContextProxy)(object)proxy).Token = cancellationToken;
    return proxy;
  }
}

/// <summary>Stands in for <c>IJobExecutionContext</c>: only <c>CancellationToken</c> is read.</summary>
public class JobContextProxy : DispatchProxy
{
  public CancellationToken Token { get; set; }

  protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
    targetMethod?.Name == "get_CancellationToken" ? Token : ProxyDefaults.For(targetMethod);
}

/// <summary>Stands in for <c>ConsumeContext&lt;T&gt;</c>: only <c>Message</c> is read.</summary>
public class ConsumeContextProxy<TMessage> : DispatchProxy
  where TMessage : class
{
  public TMessage? Message { get; set; }

  protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
    targetMethod?.Name == "get_Message" ? Message : ProxyDefaults.For(targetMethod);
}

internal static class ProxyDefaults
{
  public static object? For(MethodInfo? method)
  {
    var type = method?.ReturnType;

    return type is { IsValueType: true } && type != typeof(void) ? Activator.CreateInstance(type) : null;
  }
}
