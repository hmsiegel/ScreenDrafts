namespace ScreenDrafts.Modules.Users.Domain.Users;
public sealed class Role
{
  public static readonly Role SuperAdministrator = new("SuperAdministrator");

  public static readonly Role Administrator = new("Administrator");

  public static readonly Role Guest = new("Guest");

  public static readonly Role Host = new("Host");

  public static readonly Role Drafter = new("Drafter");

  private Role(string name) => Name = name;

  private Role() { }

  public string Name { get; private set; } = default!;
}
