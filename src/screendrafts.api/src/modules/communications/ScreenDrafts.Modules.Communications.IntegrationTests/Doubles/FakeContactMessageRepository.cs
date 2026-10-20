namespace ScreenDrafts.Modules.Communications.IntegrationTests.Doubles;

internal sealed class FakeContactMessageRepository : IContactMessageRepository
{
  private readonly List<ContactMessage> _added = [];

  public IReadOnlyList<ContactMessage> Added => _added;

  public void Add(ContactMessage contactMessage) => _added.Add(contactMessage);
}
