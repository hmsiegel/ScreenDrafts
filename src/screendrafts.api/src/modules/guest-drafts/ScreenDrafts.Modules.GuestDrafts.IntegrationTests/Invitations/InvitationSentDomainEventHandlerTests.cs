namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations;

public sealed class InvitationSentDomainEventHandlerTests
{
  private const string BaseUrl = "https://screendrafts.test/register";

  private readonly CapturingEventBus _eventBus = new();
  private readonly InMemoryDraftRepository _drafts = new();
  private readonly FakeUsersApi _users = new();
  private readonly MutableDateTimeProvider _clock = new(InvitationTestData.Now);

  private readonly Guid _inviterId = Guid.NewGuid();

  private InvitationSentDomainEventHandler Sut(string registerBase = BaseUrl) =>
    new(
      _eventBus,
      _clock,
      _drafts,
      _users,
      Options.Create(new InvitationOptions { RegisterBaseUrl = registerBase })
    );

  private static InvitationSentDomainEvent Event(
    string draftPublicId,
    Guid inviterId,
    string email = "guest@example.com",
    string name = "Guest"
  ) => new(Guid.NewGuid(), draftPublicId, inviterId, email, name);

  private Draft SeedDraft(string title = "Friday Night Draft")
  {
    var draft = InvitationTestData.CreateDraft(_inviterId, title);
    _drafts.Seed(draft);
    return draft;
  }

  private GuestDraftInvitationSentIntegrationEvent Published() =>
    _eventBus
      .CapturedEvents.Should()
      .ContainSingle()
      .Subject.Should()
      .BeOfType<GuestDraftInvitationSentIntegrationEvent>()
      .Subject;

  private Task HandleAsync(InvitationSentDomainEvent domainEvent, string registerBase = BaseUrl) =>
    Sut(registerBase).Handle(domainEvent, TestContext.Current.CancellationToken);

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Handle_ShouldThrowInvalidOperation_WhenRegisterBaseUrlIsBlankAsync(string registerBase)
  {
    var draft = SeedDraft();

    var act = () => HandleAsync(Event(draft.PublicId, _inviterId), registerBase);

    await act.Should().ThrowAsync<InvalidOperationException>();
    _eventBus.CapturedEvents.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldPublishNothing_WhenTheDraftNoLongerExistsAsync()
  {
    await HandleAsync(Event("gd_doesnotexist0000", _inviterId));

    _eventBus.CapturedEvents.Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldFallBackToAScreenDraftsMember_WhenTheInviterIsMissingAsync()
  {
    var draft = SeedDraft();

    await HandleAsync(Event(draft.PublicId, Guid.NewGuid()));

    Published().InvitedByName.Should().Be("a Screen Drafts member");
  }

  [Fact]
  public async Task Handle_ShouldUseTheInvitersFullName_WhenTheInviterExistsAsync()
  {
    var draft = SeedDraft();
    _users.RegisterUser(_inviterId, InvitationTestData.NewUserPublicId(), "Olivia", "Owner");

    await HandleAsync(Event(draft.PublicId, _inviterId));

    Published().InvitedByName.Should().Be("Olivia Owner");
  }

  [Fact]
  public async Task Handle_ShouldPublishTheDraftTitleRecipientAndTimestampAsync()
  {
    var draft = SeedDraft("The Big One");
    _users.RegisterUser(_inviterId, InvitationTestData.NewUserPublicId(), "Olivia", "Owner");

    await HandleAsync(Event(draft.PublicId, _inviterId, "bob@example.com", "Bob Smith"));

    var published = Published();
    published.DraftTitle.Should().Be("The Big One");
    published.RecipientEmail.Should().Be("bob@example.com");
    published.RecipientName.Should().Be("Bob Smith");
    published.OccurredOnUtc.Should().Be(InvitationTestData.Now);
    published.Id.Should().NotBe(Guid.Empty);
  }

  [Theory]
  [InlineData("Mary Jane", "mary@example.com", "?name=Mary%20Jane&email=mary%40example.com")]
  [InlineData("A+B", "a+b@example.com", "?name=A%2BB&email=a%2Bb%40example.com")]
  [InlineData("Tom & Jerry", "t&j@example.com", "?name=Tom%20%26%20Jerry&email=t%26j%40example.com")]
  [InlineData("José Ñandú", "jose@example.com", "?name=Jos%C3%A9%20%C3%91and%C3%BA&email=jose%40example.com")]
  public async Task Handle_ShouldBuildTheRegisterLinkWithEscapedNameAndEmailAsync(
    string name,
    string email,
    string expectedQuery
  )
  {
    var draft = SeedDraft();

    await HandleAsync(Event(draft.PublicId, _inviterId, email, name));

    Published().RegisterLink.Should().Be(BaseUrl + expectedQuery);
  }
}
