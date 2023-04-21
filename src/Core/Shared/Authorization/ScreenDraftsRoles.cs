namespace ScreenDrafts.Shared.Authorization;

public static class ScreenDraftsRoles
{
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);
    public const string Commissioner = nameof(Commissioner);
    public const string Drafter = nameof(Drafter);

    public static IReadOnlyList<string> DefaultRoles { get; } = new ReadOnlyCollection<string>(new[]
    {
        Admin,
        Basic,
        Commissioner,
        Drafter
    });

    public static bool IsDefault(string roleName) => DefaultRoles.Any(r => r == roleName);
}