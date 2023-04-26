namespace ScreenDrafts.Infrastructure.Persistence.Configuration;

internal static class SchemaNames
{
    // TODO: figure out how to capitalize these only for Oracle
    public static string Auditing = nameof(Auditing); // "AUDITING";
    public static string Catalog = nameof(Catalog); // "CATALOG";
    public static string Identity = nameof(Identity); // "IDENTITY";
    public static string MultiTenancy = nameof(MultiTenancy); // "MULTITENANCY";
    public static string Drafts = nameof(Drafts);
    public static string Movies = nameof(Movies);
    public static string SelectedMovies = nameof(SelectedMovies);
    public static string Hosts = nameof(Hosts);
    public static string Drafters = nameof(Drafters);
}

internal static class ValueObjectNames
{
    public static string DraftId = nameof(DraftId);
    public static string SelectedMovieId = nameof(SelectedMovieId);
    public static string MovieId = nameof(MovieId);
    public static string DrafterId = nameof(DrafterId);
    public static string DraftPosition = nameof(DraftPosition);
    public static string HostId = nameof(HostId);
}

internal static class DatabaseConstants
{
    public const string Id = nameof(Id);
}

internal static class TableNames
{
    public static string DraftDrafterIds = nameof(DraftDrafterIds);
    public static string DraftHostIds = nameof(DraftHostIds);
    public static string MoviesDraftsSelectedInIds = nameof(MoviesDraftsSelectedInIds);
    public static string MoviesDraftsVetoedInIds = nameof(MoviesDraftsVetoedInIds);
    public static string DrafterParticipatedDrafts = nameof(DrafterParticipatedDrafts);
    public static string DrafterMovieList = nameof(DrafterMovieList);
}

internal static class ColumnNames
{
    public static string DraftsSelectedInId = nameof(DraftsSelectedInId);
    public static string DraftsVetoedInId = nameof(DraftsVetoedInId);
    public static string DrafterWhoPlayedVeto = nameof(DrafterWhoPlayedVeto);
    public static string DrafterWhoPlayedVetoOverride = nameof(DrafterWhoPlayedVetoOverride);
}