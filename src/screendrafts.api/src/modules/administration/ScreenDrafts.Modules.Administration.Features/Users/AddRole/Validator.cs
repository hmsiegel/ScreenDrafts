namespace ScreenDrafts.Modules.Administration.Features.Users.AddRole;

internal sealed class Validator : AbstractValidator<AddRoleRequest>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
