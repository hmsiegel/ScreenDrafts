namespace ScreenDrafts.Modules.Administration.Features.Users.AddRole;

internal sealed record AddRoleCommand : ICommand<bool>
{
    public required string Name { get; init; }
}
