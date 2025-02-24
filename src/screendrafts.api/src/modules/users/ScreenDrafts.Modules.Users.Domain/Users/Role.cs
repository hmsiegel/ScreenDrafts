namespace ScreenDrafts.Modules.Users.Domain.Users;

public sealed class Role
{
  public static readonly Role SuperAdministrator = new("SuperAdministrator");

  public static readonly Role Administrator = new("Administrator");

  public static readonly Role Guest = new("Guest");

  public static readonly Role Host = new("Host");

  public static readonly Role Drafter = new("Drafter");

  public static readonly Role Patreon = new("Patreon");

  private Role(string name) => Name = name;

  private Role() { }

  public string Name { get; private set; } = default!;

  public static Role FromName(string name)
  {
    return name switch
    {
      "SuperAdministrator" => SuperAdministrator,
      "Administrator" => Administrator,
      "Guest" => Guest,
      "Host" => Host,
      "Drafter" => Drafter,
      _ => throw new ArgumentException($"The role {name} is not supported.")
    };
  }
}
