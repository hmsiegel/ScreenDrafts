namespace ScreenDrafts.Modules.Communications.Infrastructure.ContactMessages;

internal sealed class ContactMessageRepository(CommunicationsDbContext dbContext)
  : IContactMessageRepository
{
  private readonly CommunicationsDbContext _dbContext = dbContext;

  public void Add(ContactMessage contactMessage)
  {
    _dbContext.ContactMessages.Add(contactMessage);
  }
}
