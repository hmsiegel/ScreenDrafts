﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Host : Entity<HostId>
{
  private readonly List<Draft> _hostedDrafts = [];

  private Host(
    string hostName,
    Guid? userId = null,
    HostId? id = null)
    : base(id ?? HostId.CreateUnique())
  {
    UserId = userId;
    HostName = hostName;
  }

  private Host()
  {
  }

  public int ReadableId { get; init; }

  public Guid? UserId { get; private set; }

  public string HostName { get; private set; } = default!;

  public IReadOnlyCollection<Draft> HostedDrafts => _hostedDrafts.AsReadOnly();

  public static Result<Host> Create(
    string hostName,
    Guid? userId = null,
    HostId? id = null)
  {
    if (string.IsNullOrWhiteSpace(hostName))
    {
      return Result.Failure<Host>(HostErrors.InvalidHostName);
    }
    var host = new Host(
      id: id,
      userId: userId,
      hostName: hostName);
    return host;
  }

  public void AddDraft(Draft draft)
  {
    _hostedDrafts.Add(draft);
  }

  public void RemoveDraft(Draft draft)
  {
    _hostedDrafts.Remove(draft);
  }

  public void UpdateHostName(string firstName, string lastName, string? middleName = null)
  {
    if (middleName is null)
    {
      HostName = $"{firstName} {lastName}";
      return;
    }

    HostName = $"{firstName} {middleName} {lastName}";
  }
}
