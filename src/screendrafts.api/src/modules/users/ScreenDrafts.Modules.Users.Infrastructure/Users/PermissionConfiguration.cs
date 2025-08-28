namespace ScreenDrafts.Modules.Users.Infrastructure.Users;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
  public void Configure(EntityTypeBuilder<Permission> builder)
  {
    builder.ToTable(Tables.Permissions);

    builder.HasKey(x => x.Code);

    builder.Property(x => x.Code)
      .HasMaxLength(100)
      .IsRequired();

    builder.HasData(
      Permission.GetUser,
      Permission.ModifyUser,
      Permission.GetDrafts,
      Permission.CreateDraft,
      Permission.SearchDrafts,
      Permission.ModifyDraft,
      Permission.AddPicks,
      Permission.VetoPicks,
      Permission.VetoOverride,
      Permission.AddDrafters,
      Permission.CreateDrafters,
      Permission.RemoveDrafters,
      Permission.ModifyDrafters,
      Permission.GetDrafters,
      Permission.GetRoles,
      Permission.ModifyRoles,
      Permission.GetPermissions,
      Permission.ModifyPermissions,
      Permission.AddHosts,
      Permission.CreateHosts,
      Permission.RemoveHosts,
      Permission.ModifyHosts,
      Permission.GetHosts,
      Permission.SearchMovies,
      Permission.SearchActors,
      Permission.SearchCrew,
      Permission.SearchGenres,
      Permission.SearchStudios,
      Permission.SearchPatreonDrafts,
      Permission.CreatePeople,
      Permission.GetPeople,
      Permission.UpdatePerson,
      Permission.SearchPeople,
      Permission.CreateMovies,
      Permission.GetMovies,
      Permission.CreateCategories,
      Permission.UpdateCategories,
      Permission.GetCategories,
      Permission.SearchCategories,
      Permission.DeleteCategories);

    builder.HasMany<Role>()
      .WithMany()
      .UsingEntity(jb =>
      {
        jb.ToTable(Tables.RolePermissions);

        jb.HasData(
          // Guest Permissions
          CreateRolePermission(Role.Guest, Permission.GetUser),
          CreateRolePermission(Role.Guest, Permission.SearchDrafts),
          CreateRolePermission(Role.Guest, Permission.ModifyUser),
          CreateRolePermission(Role.Guest, Permission.SearchMovies),
          CreateRolePermission(Role.Guest, Permission.SearchActors),
          CreateRolePermission(Role.Guest, Permission.SearchCrew),
          CreateRolePermission(Role.Guest, Permission.SearchGenres),
          CreateRolePermission(Role.Guest, Permission.SearchStudios),
          CreateRolePermission(Role.Guest, Permission.SearchPeople),
          CreateRolePermission(Role.Guest, Permission.SearchCategories),

          // Host Permissions
          CreateRolePermission(Role.Host, Permission.GetUser),
          CreateRolePermission(Role.Host, Permission.SearchDrafts),
          CreateRolePermission(Role.Host, Permission.ModifyUser),
          CreateRolePermission(Role.Host, Permission.GetDrafts),
          CreateRolePermission(Role.Host, Permission.SearchMovies),
          CreateRolePermission(Role.Host, Permission.SearchActors),
          CreateRolePermission(Role.Host, Permission.SearchCrew),
          CreateRolePermission(Role.Host, Permission.SearchGenres),
          CreateRolePermission(Role.Host, Permission.SearchStudios),
          CreateRolePermission(Role.Host, Permission.SearchPeople),
          CreateRolePermission(Role.Host, Permission.SearchCategories),

          // Drafter Permissions
          CreateRolePermission(Role.Drafter, Permission.GetUser),
          CreateRolePermission(Role.Drafter, Permission.ModifyUser),
          CreateRolePermission(Role.Drafter, Permission.SearchDrafts),
          CreateRolePermission(Role.Drafter, Permission.AddPicks),
          CreateRolePermission(Role.Drafter, Permission.VetoPicks),
          CreateRolePermission(Role.Drafter, Permission.VetoOverride),
          CreateRolePermission(Role.Drafter, Permission.SearchMovies),
          CreateRolePermission(Role.Drafter, Permission.SearchActors),
          CreateRolePermission(Role.Drafter, Permission.SearchCrew),
          CreateRolePermission(Role.Drafter, Permission.SearchGenres),
          CreateRolePermission(Role.Drafter, Permission.SearchStudios),
          CreateRolePermission(Role.Drafter, Permission.SearchPeople),
          CreateRolePermission(Role.Drafter, Permission.SearchCategories),

          // Administrator Permissions
          CreateRolePermission(Role.Administrator, Permission.GetUser),
          CreateRolePermission(Role.Administrator, Permission.ModifyUser),
          CreateRolePermission(Role.Administrator, Permission.GetDrafts),
          CreateRolePermission(Role.Administrator, Permission.CreateDraft),
          CreateRolePermission(Role.Administrator, Permission.SearchDrafts),
          CreateRolePermission(Role.Administrator, Permission.ModifyDraft),
          CreateRolePermission(Role.Administrator, Permission.AddPicks),
          CreateRolePermission(Role.Administrator, Permission.VetoPicks),
          CreateRolePermission(Role.Administrator, Permission.VetoOverride),
          CreateRolePermission(Role.Administrator, Permission.AddDrafters),
          CreateRolePermission(Role.Administrator, Permission.CreateDrafters),
          CreateRolePermission(Role.Administrator, Permission.RemoveDrafters),
          CreateRolePermission(Role.Administrator, Permission.ModifyDrafters),
          CreateRolePermission(Role.Administrator, Permission.GetDrafters),
          CreateRolePermission(Role.Administrator, Permission.AddHosts),
          CreateRolePermission(Role.Administrator, Permission.CreateHosts),
          CreateRolePermission(Role.Administrator, Permission.RemoveHosts),
          CreateRolePermission(Role.Administrator, Permission.ModifyHosts),
          CreateRolePermission(Role.Administrator, Permission.GetHosts),
          CreateRolePermission(Role.Administrator, Permission.SearchMovies),
          CreateRolePermission(Role.Administrator, Permission.SearchActors),
          CreateRolePermission(Role.Administrator, Permission.SearchCrew),
          CreateRolePermission(Role.Administrator, Permission.SearchGenres),
          CreateRolePermission(Role.Administrator, Permission.SearchStudios),
          CreateRolePermission(Role.Administrator, Permission.CreatePeople),
          CreateRolePermission(Role.Administrator, Permission.GetPeople),
          CreateRolePermission(Role.Administrator, Permission.UpdatePerson),
          CreateRolePermission(Role.Administrator, Permission.SearchPeople),
          CreateRolePermission(Role.Administrator, Permission.CreateCategories),
          CreateRolePermission(Role.Administrator, Permission.UpdateCategories),
          CreateRolePermission(Role.Administrator, Permission.GetCategories),
          CreateRolePermission(Role.Administrator, Permission.SearchCategories),
          CreateRolePermission(Role.Administrator, Permission.DeleteCategories),
          CreateRolePermission(Role.Administrator, Permission.CreateMovies),
          CreateRolePermission(Role.Administrator, Permission.GetMovies),

          // Super Administrator Permissions
          CreateRolePermission(Role.SuperAdministrator, Permission.GetUser),
          CreateRolePermission(Role.SuperAdministrator, Permission.ModifyUser),
          CreateRolePermission(Role.SuperAdministrator, Permission.GetDrafts),
          CreateRolePermission(Role.SuperAdministrator, Permission.CreateDraft),
          CreateRolePermission(Role.SuperAdministrator, Permission.SearchDrafts),
          CreateRolePermission(Role.SuperAdministrator, Permission.ModifyDraft),
          CreateRolePermission(Role.SuperAdministrator, Permission.AddPicks),
          CreateRolePermission(Role.SuperAdministrator, Permission.VetoPicks),
          CreateRolePermission(Role.SuperAdministrator, Permission.VetoOverride),
          CreateRolePermission(Role.SuperAdministrator, Permission.AddDrafters),
          CreateRolePermission(Role.SuperAdministrator, Permission.RemoveDrafters),
          CreateRolePermission(Role.SuperAdministrator, Permission.ModifyDrafters),
          CreateRolePermission(Role.SuperAdministrator, Permission.GetDrafters),
          CreateRolePermission(Role.SuperAdministrator, Permission.AddHosts),
          CreateRolePermission(Role.SuperAdministrator, Permission.RemoveHosts),
          CreateRolePermission(Role.SuperAdministrator, Permission.ModifyHosts),
          CreateRolePermission(Role.SuperAdministrator, Permission.GetHosts),
          CreateRolePermission(Role.SuperAdministrator, Permission.SearchMovies),
          CreateRolePermission(Role.SuperAdministrator, Permission.SearchActors),
          CreateRolePermission(Role.SuperAdministrator, Permission.SearchCrew),
          CreateRolePermission(Role.SuperAdministrator, Permission.SearchGenres),
          CreateRolePermission(Role.SuperAdministrator, Permission.SearchStudios),
          CreateRolePermission(Role.SuperAdministrator, Permission.GetRoles),
          CreateRolePermission(Role.SuperAdministrator, Permission.ModifyRoles),
          CreateRolePermission(Role.SuperAdministrator, Permission.GetPermissions),
          CreateRolePermission(Role.SuperAdministrator, Permission.ModifyPermissions),
          CreateRolePermission(Role.SuperAdministrator, Permission.CreatePeople),
          CreateRolePermission(Role.SuperAdministrator, Permission.GetPeople),
          CreateRolePermission(Role.SuperAdministrator, Permission.UpdatePerson),
          CreateRolePermission(Role.SuperAdministrator, Permission.SearchPeople),
          CreateRolePermission(Role.SuperAdministrator, Permission.CreateCategories),
          CreateRolePermission(Role.SuperAdministrator, Permission.UpdateCategories),
          CreateRolePermission(Role.SuperAdministrator, Permission.GetCategories),
          CreateRolePermission(Role.SuperAdministrator, Permission.SearchCategories),
          CreateRolePermission(Role.SuperAdministrator, Permission.DeleteCategories),
          CreateRolePermission(Role.SuperAdministrator, Permission.CreateMovies),
          CreateRolePermission(Role.SuperAdministrator, Permission.GetMovies),

          // Patreon Permissions
          CreateRolePermission(Role.Patreon, Permission.SearchPatreonDrafts)
          );
      });
  }

  private static object CreateRolePermission(Role role, Permission permission)
  {
    return new
    {
      RoleName = role.Name,
      PermissionCode = permission.Code
    };
  }
}

