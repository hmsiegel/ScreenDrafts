namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Commands;

public class DraftParticipantTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task AddDrafterToDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var drafter = await CreateDrafterAsync();

    // Act
    var result = await Sender.Send(new AddDrafterToDraftCommand(draft, drafter));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(drafter);

    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.Drafters.Should().Contain(d => d.Id == drafter);
  }

  [Fact]
  public async Task RemoveDrafterFromDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var drafter = await CreateDrafterAsync();
    await Sender.Send(new AddDrafterToDraftCommand(draft, drafter));

    // Act
    var result = await Sender.Send(new RemoveDrafterFromDraftCommand(draft, drafter));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(drafter);

    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.Drafters.Should().NotContain(d => d.Id == drafter);
  }

  [Fact]
  public async Task AddHostToDraft_AsPrimary_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var host = await CreateHostAsync();

    // Act
    var result = await Sender.Send(new AddHostToDraftCommand(
      draft,
      host,
      HostRole.Primary.Name));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(host);

    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.PrimaryHost.Should().NotBeNull();
    updatedDraft.Value.PrimaryHost!.Id.Should().Be(host);
    updatedDraft.Value.CoHosts.Should().BeEmpty();
  }

  [Fact]
  public async Task AddHostToDraft_AsCoHost_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var primary = await CreateHostAsync();
    var coHost = await CreateHostAsync();

    // Act
    await Sender.Send(new AddHostToDraftCommand(
      draft,
      primary,
      HostRole.Primary.Name));

    var result = await Sender.Send(new AddHostToDraftCommand(
      draft,
      coHost,
      HostRole.CoHost.Name));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(coHost);
    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.PrimaryHost!.Id.Should().Be(primary);
    updatedDraft.Value.CoHosts.Should().Contain(h => h.Id == coHost);
  }

  [Fact]
  public async Task RemoveCoHostFromDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draft = await CreateDraftAsync();
    var primary = await CreateHostAsync();
    await Sender.Send(new AddHostToDraftCommand(draft, primary, HostRole.Primary.Name));
    var cohost = await CreateHostAsync();
    await Sender.Send(new AddHostToDraftCommand(draft, cohost, HostRole.CoHost.Name));

    // Act
    var result = await Sender.Send(new RemoveHostFromDraftCommand(draft, cohost));

    // Assert
    result.IsSuccess.Should().BeTrue();

    var updatedDraft = await Sender.Send(new GetDraftQuery(draft));
    updatedDraft.Value.CoHosts.Should().NotContain(h => h.Id == cohost);
    updatedDraft.Value.PrimaryHost!.Id.Should().Be(primary);
  }

  private async Task<Guid> CreateDraftAsync()
  {
    var command = new CreateDraftCommand(
      "Test Draft",
      DraftType.Standard,
      7,
      2,
      0,
      2,
      DraftStatus.Created);

    var result = await Sender.Send(command);
    return result.Value;
  }

  private async Task<Guid> CreateDrafterAsync()
  {
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateDrafterCommand(personId);
    var result = await Sender.Send(command);
    return result.Value;
  }

  private async Task<Guid> CreateHostAsync()
  {
    var personFactory = new PeopleFactory(Sender, Faker);
    var personId = await personFactory.CreateAndSavePersonAsync();
    var command = new CreateHostCommand(personId);
    var result = await Sender.Send(command);
    return result.Value;
  }
}
