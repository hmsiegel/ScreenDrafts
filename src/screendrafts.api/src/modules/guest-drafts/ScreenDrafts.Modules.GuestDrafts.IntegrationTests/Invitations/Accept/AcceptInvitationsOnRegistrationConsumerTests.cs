using MediatR;
using ScreenDrafts.Modules.Users.IntegrationEvents;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations.Accept;

public sealed class AcceptInvitationsOnRegistrationConsumerTests
{
  [Fact]
  public async Task Handle_ShouldSendACommandMappedFromTheRegistrationEventAsync()
  {
    var sender = new RecordingSender();
    var consumer = new AcceptInvitationsOnRegistrationIntegrationEventConsumer(sender);
    var userId = Guid.NewGuid();
    var integrationEvent = new UserRegisteredIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      userId: userId,
      email: "Mary@Example.com",
      firstName: "Mary",
      lastName: "Smith",
      middleName: null
    );

    await consumer.Handle(integrationEvent, TestContext.Current.CancellationToken);

    var command = sender
      .Sent.Should()
      .ContainSingle()
      .Subject.Should()
      .BeOfType<AcceptPendingInvitationsCommand>()
      .Subject;
    command.UserId.Should().Be(userId);
    command.Email.Should().Be("Mary@Example.com");
    command.FirstName.Should().Be("Mary");
    command.LastName.Should().Be("Smith");
  }

  [Fact]
  public async Task Handle_ShouldThrow_WhenEventIsNullAsync()
  {
    var consumer = new AcceptInvitationsOnRegistrationIntegrationEventConsumer(
      new RecordingSender()
    );

    var act = async () => await consumer.Handle(null!, TestContext.Current.CancellationToken);

    await act.Should().ThrowAsync<ArgumentNullException>();
  }

  private sealed class RecordingSender : ISender
  {
    private readonly List<object> _sent = [];

    public IReadOnlyList<object> Sent => _sent;

    public Task<TResponse> Send<TResponse>(
      IRequest<TResponse> request,
      CancellationToken cancellationToken = default
    )
    {
      _sent.Add(request);
      return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
      where TRequest : IRequest
    {
      _sent.Add(request);
      return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
      _sent.Add(request);
      return Task.FromResult<object?>(null);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
      IStreamRequest<TResponse> request,
      CancellationToken cancellationToken = default
    ) => throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(
      object request,
      CancellationToken cancellationToken = default
    ) => throw new NotSupportedException();
  }
}
