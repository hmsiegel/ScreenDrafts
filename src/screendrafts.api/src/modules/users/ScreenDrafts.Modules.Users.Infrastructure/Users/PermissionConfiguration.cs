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
          // Users
          Permission.GetUser,
          Permission.ModifyUser,

          // Roles & Permissions
          Permission.GetRoles,
          Permission.ModifyRoles,
          Permission.GetPermissions,
          Permission.ModifyPermissions,

          // Drafts
          Permission.GetDrafts,
          Permission.ListDrafts,
          Permission.CreateDraft,
          Permission.ModifyDraft,
          Permission.DeleteDraft,
          Permission.SearchDrafts,
          Permission.ReadPatreonDrafts,
          Permission.SearchPatreonDrafts,

          // Draft Parts
          Permission.ReadDraftPart,
          Permission.ListDraftParts,
          Permission.CreateDraftPart,
          Permission.UpdateDraftPart,
          Permission.SetDraftPartStatus,

          // Draft Boards
          Permission.ReadDraftBoard,
          Permission.ListDraftBoards,
          Permission.CreateDraftBoard,
          Permission.UpdateDraftBoard,
          Permission.DeleteDraftBoard,

          // Draft Pools
          Permission.CreateDraftPool,
          Permission.UpdateDraftPool,
          Permission.ReadDraftPool,

          // Picks
          Permission.AddPicks,
          Permission.CreatePick,
          Permission.UpdatePick,
          Permission.UndoPick,
          Permission.VetoPicks,
          Permission.VetoOverride,
          Permission.CommissionerOverride,

          // People
          Permission.CreatePeople,
          Permission.GetPeople,
          Permission.ListPeople,
          Permission.UpdatePerson,
          Permission.SearchPeople,

          // Drafters
          Permission.AddDrafters,
          Permission.CreateDrafters,
          Permission.RemoveDrafters,
          Permission.ModifyDrafters,
          Permission.GetDrafters,
          Permission.ListDrafters,

          // Hosts
          Permission.AddHosts,
          Permission.CreateHosts,
          Permission.RemoveHosts,
          Permission.ModifyHosts,
          Permission.GetHosts,
          Permission.ListHosts,

          // Drafter Teams
          Permission.ReadDrafterTeam,
          Permission.ListDrafterTeams,
          Permission.CreateDrafterTeam,
          Permission.UpdateDrafterTeam,
          Permission.ManageDrafterTeamMembers,

          // Game Boards
          Permission.CreateGameBoard,
          Permission.ModifyGameBoard,

          // Movies
          Permission.CreateMovies,
          Permission.GetMovies,
          Permission.SearchMovies,
          Permission.SearchActors,
          Permission.SearchCrew,
          Permission.SearchGenres,
          Permission.SearchStudios,

          // Series
          Permission.ReadSeries,
          Permission.ListSeries,
          Permission.CreateSeries,
          Permission.UpdateSeries,
          Permission.DeleteSeries,

          // Categories
          Permission.CreateCategories,
          Permission.UpdateCategories,
          Permission.GetCategories,
          Permission.SearchCategories,
          Permission.DeleteCategories,

          // Campaigns
          Permission.CreateCampaign,
          Permission.UpdateCampaign,
          Permission.ReadCampaign,
          Permission.ListCampaigns,
          Permission.DeleteCampaign,
          Permission.RestoreCampaign,

          // Candidate Lists
          Permission.ReadCandidateList,
          Permission.ListCandidateLists,
          Permission.CreateCandidateList,
          Permission.UpdateCandidateList,
          Permission.DeleteCandidateList,

          // Speed Sub-Drafts
          Permission.ReadSubDraft,
          Permission.CreateSubDraft,
          Permission.UpdateSubDraft);

    builder.HasMany<Role>()
      .WithMany()
      .UsingEntity(jb =>
      {
        jb.ToTable(Tables.RolePermissions);

        jb.HasData(
         // -------------------------
         // Guest
         // -------------------------
         CreateRolePermission(Role.Guest, Permission.GetUser),
         CreateRolePermission(Role.Guest, Permission.ModifyUser),
         CreateRolePermission(Role.Guest, Permission.ListDrafts),
         CreateRolePermission(Role.Guest, Permission.ListDraftParts),
         CreateRolePermission(Role.Guest, Permission.SearchDrafts),
         CreateRolePermission(Role.Guest, Permission.SearchMovies),
         CreateRolePermission(Role.Guest, Permission.SearchActors),
         CreateRolePermission(Role.Guest, Permission.SearchCrew),
         CreateRolePermission(Role.Guest, Permission.SearchGenres),
         CreateRolePermission(Role.Guest, Permission.SearchStudios),
         CreateRolePermission(Role.Guest, Permission.SearchPeople),
         CreateRolePermission(Role.Guest, Permission.SearchCategories),
         CreateRolePermission(Role.Guest, Permission.GetDrafters),
         CreateRolePermission(Role.Guest, Permission.ListDrafters),
         CreateRolePermission(Role.Guest, Permission.GetHosts),
         CreateRolePermission(Role.Guest, Permission.ListHosts),
         CreateRolePermission(Role.Guest, Permission.ReadDrafterTeam),
         CreateRolePermission(Role.Guest, Permission.ListDrafterTeams),
         CreateRolePermission(Role.Guest, Permission.ReadSubDraft),

         // -------------------------
         // Host
         // -------------------------
         CreateRolePermission(Role.Host, Permission.GetUser),
         CreateRolePermission(Role.Host, Permission.ModifyUser),
         CreateRolePermission(Role.Host, Permission.GetDrafts),
         CreateRolePermission(Role.Host, Permission.ListDrafts),
         CreateRolePermission(Role.Host, Permission.SearchDrafts),
         CreateRolePermission(Role.Host, Permission.ReadDraftPart),
         CreateRolePermission(Role.Host, Permission.ListDraftParts),
         CreateRolePermission(Role.Host, Permission.ReadDraftBoard),
         CreateRolePermission(Role.Host, Permission.ReadDraftPool),
         CreateRolePermission(Role.Host, Permission.CreateDraftPool),
         CreateRolePermission(Role.Host, Permission.UpdateDraftPool),
         CreateRolePermission(Role.Host, Permission.CreateCandidateList),
         CreateRolePermission(Role.Host, Permission.UpdateCandidateList),
         CreateRolePermission(Role.Host, Permission.ReadCandidateList),
         CreateRolePermission(Role.Host, Permission.CommissionerOverride),
         CreateRolePermission(Role.Host, Permission.GetDrafters),
         CreateRolePermission(Role.Host, Permission.ListDrafters),
         CreateRolePermission(Role.Host, Permission.GetHosts),
         CreateRolePermission(Role.Host, Permission.ListHosts),
         CreateRolePermission(Role.Host, Permission.GetPeople),
         CreateRolePermission(Role.Host, Permission.ListPeople),
         CreateRolePermission(Role.Host, Permission.SearchPeople),
         CreateRolePermission(Role.Host, Permission.SearchMovies),
         CreateRolePermission(Role.Host, Permission.SearchActors),
         CreateRolePermission(Role.Host, Permission.SearchCrew),
         CreateRolePermission(Role.Host, Permission.SearchGenres),
         CreateRolePermission(Role.Host, Permission.SearchStudios),
         CreateRolePermission(Role.Host, Permission.SearchCategories),
         CreateRolePermission(Role.Host, Permission.ReadSeries),
         CreateRolePermission(Role.Host, Permission.ListSeries),
         CreateRolePermission(Role.Host, Permission.ReadCampaign),
         CreateRolePermission(Role.Host, Permission.ListCampaigns),
         CreateRolePermission(Role.Host, Permission.ReadSubDraft),
         CreateRolePermission(Role.Host, Permission.CreateSubDraft),
         CreateRolePermission(Role.Host, Permission.UpdateSubDraft),

         // -------------------------
         // Drafter
         // -------------------------
         CreateRolePermission(Role.Drafter, Permission.GetUser),
         CreateRolePermission(Role.Drafter, Permission.ModifyUser),
         CreateRolePermission(Role.Drafter, Permission.GetDrafts),
         CreateRolePermission(Role.Drafter, Permission.ListDrafts),
         CreateRolePermission(Role.Drafter, Permission.SearchDrafts),
         CreateRolePermission(Role.Drafter, Permission.ReadDraftPart),
         CreateRolePermission(Role.Drafter, Permission.ListDraftParts),
         CreateRolePermission(Role.Drafter, Permission.ReadDraftBoard),
         CreateRolePermission(Role.Drafter, Permission.ReadDraftPool),
         CreateRolePermission(Role.Drafter, Permission.CreateDraftBoard),
         CreateRolePermission(Role.Drafter, Permission.DeleteDraftBoard),
         CreateRolePermission(Role.Drafter, Permission.UpdateDraftBoard),
         CreateRolePermission(Role.Drafter, Permission.CreateCandidateList),
         CreateRolePermission(Role.Drafter, Permission.UpdateCandidateList),
         CreateRolePermission(Role.Drafter, Permission.ReadCandidateList),
         CreateRolePermission(Role.Drafter, Permission.AddPicks),
         CreateRolePermission(Role.Drafter, Permission.VetoPicks),
         CreateRolePermission(Role.Drafter, Permission.VetoOverride),
         CreateRolePermission(Role.Drafter, Permission.GetDrafters),
         CreateRolePermission(Role.Drafter, Permission.ListDrafters),
         CreateRolePermission(Role.Drafter, Permission.GetHosts),
         CreateRolePermission(Role.Drafter, Permission.GetPeople),
         CreateRolePermission(Role.Drafter, Permission.SearchPeople),
         CreateRolePermission(Role.Drafter, Permission.SearchMovies),
         CreateRolePermission(Role.Drafter, Permission.SearchActors),
         CreateRolePermission(Role.Drafter, Permission.SearchCrew),
         CreateRolePermission(Role.Drafter, Permission.SearchGenres),
         CreateRolePermission(Role.Drafter, Permission.SearchStudios),
         CreateRolePermission(Role.Drafter, Permission.SearchCategories),
         CreateRolePermission(Role.Drafter, Permission.ReadSeries),
         CreateRolePermission(Role.Drafter, Permission.ListSeries),
         CreateRolePermission(Role.Drafter, Permission.ReadSubDraft),

         // -------------------------
         // Administrator
         // -------------------------
         CreateRolePermission(Role.Administrator, Permission.GetUser),
         CreateRolePermission(Role.Administrator, Permission.ModifyUser),

         CreateRolePermission(Role.Administrator, Permission.GetDrafts),
         CreateRolePermission(Role.Administrator, Permission.ListDrafts),
         CreateRolePermission(Role.Administrator, Permission.CreateDraft),
         CreateRolePermission(Role.Administrator, Permission.ModifyDraft),
         CreateRolePermission(Role.Administrator, Permission.DeleteDraft),
         CreateRolePermission(Role.Administrator, Permission.SearchDrafts),
         CreateRolePermission(Role.Administrator, Permission.ReadPatreonDrafts),

         CreateRolePermission(Role.Administrator, Permission.ReadDraftPart),
         CreateRolePermission(Role.Administrator, Permission.ListDraftParts),
         CreateRolePermission(Role.Administrator, Permission.CreateDraftPart),
         CreateRolePermission(Role.Administrator, Permission.UpdateDraftPart),
         CreateRolePermission(Role.Administrator, Permission.SetDraftPartStatus),

         CreateRolePermission(Role.Administrator, Permission.ReadDraftBoard),
         CreateRolePermission(Role.Administrator, Permission.ListDraftBoards),
         CreateRolePermission(Role.Administrator, Permission.CreateDraftBoard),
         CreateRolePermission(Role.Administrator, Permission.UpdateDraftBoard),
         CreateRolePermission(Role.Administrator, Permission.DeleteDraftBoard),

         CreateRolePermission(Role.Administrator, Permission.CreateDraftPool),
         CreateRolePermission(Role.Administrator, Permission.UpdateDraftPool),
         CreateRolePermission(Role.Administrator, Permission.ReadDraftPool),

         CreateRolePermission(Role.Administrator, Permission.AddPicks),
         CreateRolePermission(Role.Administrator, Permission.CreatePick),
         CreateRolePermission(Role.Administrator, Permission.UpdatePick),
         CreateRolePermission(Role.Administrator, Permission.UndoPick),
         CreateRolePermission(Role.Administrator, Permission.VetoPicks),
         CreateRolePermission(Role.Administrator, Permission.VetoOverride),
         CreateRolePermission(Role.Administrator, Permission.CommissionerOverride),

         CreateRolePermission(Role.Administrator, Permission.CreatePeople),
         CreateRolePermission(Role.Administrator, Permission.GetPeople),
         CreateRolePermission(Role.Administrator, Permission.ListPeople),
         CreateRolePermission(Role.Administrator, Permission.UpdatePerson),
         CreateRolePermission(Role.Administrator, Permission.SearchPeople),

         CreateRolePermission(Role.Administrator, Permission.AddDrafters),
         CreateRolePermission(Role.Administrator, Permission.CreateDrafters),
         CreateRolePermission(Role.Administrator, Permission.RemoveDrafters),
         CreateRolePermission(Role.Administrator, Permission.ModifyDrafters),
         CreateRolePermission(Role.Administrator, Permission.GetDrafters),
         CreateRolePermission(Role.Administrator, Permission.ListDrafters),

         CreateRolePermission(Role.Administrator, Permission.AddHosts),
         CreateRolePermission(Role.Administrator, Permission.CreateHosts),
         CreateRolePermission(Role.Administrator, Permission.RemoveHosts),
         CreateRolePermission(Role.Administrator, Permission.ModifyHosts),
         CreateRolePermission(Role.Administrator, Permission.GetHosts),
         CreateRolePermission(Role.Administrator, Permission.ListHosts),

         CreateRolePermission(Role.Administrator, Permission.ReadDrafterTeam),
         CreateRolePermission(Role.Administrator, Permission.ListDrafterTeams),
         CreateRolePermission(Role.Administrator, Permission.CreateDrafterTeam),
         CreateRolePermission(Role.Administrator, Permission.UpdateDrafterTeam),
         CreateRolePermission(Role.Administrator, Permission.ManageDrafterTeamMembers),

         CreateRolePermission(Role.Administrator, Permission.CreateGameBoard),
         CreateRolePermission(Role.Administrator, Permission.ModifyGameBoard),

         CreateRolePermission(Role.Administrator, Permission.CreateMovies),
         CreateRolePermission(Role.Administrator, Permission.GetMovies),
         CreateRolePermission(Role.Administrator, Permission.SearchMovies),
         CreateRolePermission(Role.Administrator, Permission.SearchActors),
         CreateRolePermission(Role.Administrator, Permission.SearchCrew),
         CreateRolePermission(Role.Administrator, Permission.SearchGenres),
         CreateRolePermission(Role.Administrator, Permission.SearchStudios),

         CreateRolePermission(Role.Administrator, Permission.ReadSeries),
         CreateRolePermission(Role.Administrator, Permission.ListSeries),
         CreateRolePermission(Role.Administrator, Permission.CreateSeries),
         CreateRolePermission(Role.Administrator, Permission.UpdateSeries),
         CreateRolePermission(Role.Administrator, Permission.DeleteSeries),

         CreateRolePermission(Role.Administrator, Permission.CreateCategories),
         CreateRolePermission(Role.Administrator, Permission.UpdateCategories),
         CreateRolePermission(Role.Administrator, Permission.GetCategories),
         CreateRolePermission(Role.Administrator, Permission.SearchCategories),
         CreateRolePermission(Role.Administrator, Permission.DeleteCategories),

         CreateRolePermission(Role.Administrator, Permission.CreateCampaign),
         CreateRolePermission(Role.Administrator, Permission.UpdateCampaign),
         CreateRolePermission(Role.Administrator, Permission.ReadCampaign),
         CreateRolePermission(Role.Administrator, Permission.ListCampaigns),
         CreateRolePermission(Role.Administrator, Permission.DeleteCampaign),
         CreateRolePermission(Role.Administrator, Permission.RestoreCampaign),

         CreateRolePermission(Role.Administrator, Permission.ReadCandidateList),
         CreateRolePermission(Role.Administrator, Permission.ListCandidateLists),
         CreateRolePermission(Role.Administrator, Permission.CreateCandidateList),
         CreateRolePermission(Role.Administrator, Permission.UpdateCandidateList),
         CreateRolePermission(Role.Administrator, Permission.DeleteCandidateList),

         CreateRolePermission(Role.Administrator, Permission.ReadSubDraft),
         CreateRolePermission(Role.Administrator, Permission.CreateSubDraft),
         CreateRolePermission(Role.Administrator, Permission.UpdateSubDraft),

         // -------------------------
         // Super Administrator
         // -------------------------
         CreateRolePermission(Role.SuperAdministrator, Permission.GetUser),
         CreateRolePermission(Role.SuperAdministrator, Permission.ModifyUser),

         CreateRolePermission(Role.SuperAdministrator, Permission.GetRoles),
         CreateRolePermission(Role.SuperAdministrator, Permission.ModifyRoles),
         CreateRolePermission(Role.SuperAdministrator, Permission.GetPermissions),
         CreateRolePermission(Role.SuperAdministrator, Permission.ModifyPermissions),

         CreateRolePermission(Role.SuperAdministrator, Permission.GetDrafts),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListDrafts),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateDraft),
         CreateRolePermission(Role.SuperAdministrator, Permission.ModifyDraft),
         CreateRolePermission(Role.SuperAdministrator, Permission.DeleteDraft),
         CreateRolePermission(Role.SuperAdministrator, Permission.SearchDrafts),
         CreateRolePermission(Role.SuperAdministrator, Permission.ReadPatreonDrafts),

         CreateRolePermission(Role.SuperAdministrator, Permission.ReadDraftPart),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListDraftParts),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateDraftPart),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateDraftPart),
         CreateRolePermission(Role.SuperAdministrator, Permission.SetDraftPartStatus),

         CreateRolePermission(Role.SuperAdministrator, Permission.ReadDraftBoard),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListDraftBoards),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateDraftBoard),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateDraftBoard),
         CreateRolePermission(Role.SuperAdministrator, Permission.DeleteDraftBoard),

         CreateRolePermission(Role.SuperAdministrator, Permission.CreateDraftPool),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateDraftPool),
         CreateRolePermission(Role.SuperAdministrator, Permission.ReadDraftPool),

         CreateRolePermission(Role.SuperAdministrator, Permission.AddPicks),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreatePick),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdatePick),
         CreateRolePermission(Role.SuperAdministrator, Permission.UndoPick),
         CreateRolePermission(Role.SuperAdministrator, Permission.VetoPicks),
         CreateRolePermission(Role.SuperAdministrator, Permission.VetoOverride),
         CreateRolePermission(Role.SuperAdministrator, Permission.CommissionerOverride),

         CreateRolePermission(Role.SuperAdministrator, Permission.CreatePeople),
         CreateRolePermission(Role.SuperAdministrator, Permission.GetPeople),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListPeople),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdatePerson),
         CreateRolePermission(Role.SuperAdministrator, Permission.SearchPeople),

         CreateRolePermission(Role.SuperAdministrator, Permission.AddDrafters),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateDrafters),
         CreateRolePermission(Role.SuperAdministrator, Permission.RemoveDrafters),
         CreateRolePermission(Role.SuperAdministrator, Permission.ModifyDrafters),
         CreateRolePermission(Role.SuperAdministrator, Permission.GetDrafters),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListDrafters),

         CreateRolePermission(Role.SuperAdministrator, Permission.AddHosts),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateHosts),
         CreateRolePermission(Role.SuperAdministrator, Permission.RemoveHosts),
         CreateRolePermission(Role.SuperAdministrator, Permission.ModifyHosts),
         CreateRolePermission(Role.SuperAdministrator, Permission.GetHosts),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListHosts),

         CreateRolePermission(Role.SuperAdministrator, Permission.ReadDrafterTeam),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListDrafterTeams),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateDrafterTeam),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateDrafterTeam),
         CreateRolePermission(Role.SuperAdministrator, Permission.ManageDrafterTeamMembers),

         CreateRolePermission(Role.SuperAdministrator, Permission.CreateGameBoard),
         CreateRolePermission(Role.SuperAdministrator, Permission.ModifyGameBoard),

         CreateRolePermission(Role.SuperAdministrator, Permission.CreateMovies),
         CreateRolePermission(Role.SuperAdministrator, Permission.GetMovies),
         CreateRolePermission(Role.SuperAdministrator, Permission.SearchMovies),
         CreateRolePermission(Role.SuperAdministrator, Permission.SearchActors),
         CreateRolePermission(Role.SuperAdministrator, Permission.SearchCrew),
         CreateRolePermission(Role.SuperAdministrator, Permission.SearchGenres),
         CreateRolePermission(Role.SuperAdministrator, Permission.SearchStudios),

         CreateRolePermission(Role.SuperAdministrator, Permission.ReadSeries),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListSeries),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateSeries),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateSeries),
         CreateRolePermission(Role.SuperAdministrator, Permission.DeleteSeries),

         CreateRolePermission(Role.SuperAdministrator, Permission.CreateCategories),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateCategories),
         CreateRolePermission(Role.SuperAdministrator, Permission.GetCategories),
         CreateRolePermission(Role.SuperAdministrator, Permission.SearchCategories),
         CreateRolePermission(Role.SuperAdministrator, Permission.DeleteCategories),

         CreateRolePermission(Role.SuperAdministrator, Permission.CreateCampaign),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateCampaign),
         CreateRolePermission(Role.SuperAdministrator, Permission.ReadCampaign),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListCampaigns),
         CreateRolePermission(Role.SuperAdministrator, Permission.DeleteCampaign),
         CreateRolePermission(Role.SuperAdministrator, Permission.RestoreCampaign),

         CreateRolePermission(Role.SuperAdministrator, Permission.ReadCandidateList),
         CreateRolePermission(Role.SuperAdministrator, Permission.ListCandidateLists),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateCandidateList),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateCandidateList),
         CreateRolePermission(Role.SuperAdministrator, Permission.DeleteCandidateList),

         CreateRolePermission(Role.SuperAdministrator, Permission.ReadSubDraft),
         CreateRolePermission(Role.SuperAdministrator, Permission.CreateSubDraft),
         CreateRolePermission(Role.SuperAdministrator, Permission.UpdateSubDraft),

         // -------------------------
         // Patreon
         // -------------------------
         CreateRolePermission(Role.Patreon, Permission.SearchPatreonDrafts));
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

