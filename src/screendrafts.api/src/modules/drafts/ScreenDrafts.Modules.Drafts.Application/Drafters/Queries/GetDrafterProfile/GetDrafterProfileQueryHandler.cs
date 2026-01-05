namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafterProfile;

internal sealed class GetDrafterProfileQueryHandler(IDbConnectionFactory dbConnectionFactory, IUsersApi usersApi)
  : IQueryHandler<GetDrafterProfileQuery, DrafterProfileResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result<DrafterProfileResponse>> Handle(
    GetDrafterProfileQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string personSql = $"""
      select
        p.id as {nameof(PersonResponse.Id)},
        p.user_id as {nameof(PersonResponse.UserId)},
        p.display_name as {nameof(PersonResponse.DisplayName)}
      from drafts.people p
      join drafts.drafters d on d.person_id = p.id
      where d.id = @DrafterId;
      """;


    var person = await connection.QuerySingleOrDefaultAsync<PersonResponse>(
      personSql,
      new { request.DrafterId });

    if (person is null)
    {
      return Result.Failure<DrafterProfileResponse>(DrafterErrors.NotFound(request.DrafterId));
    }

    const string sql = $"""
      -- 1 )Total drafts, first draft, most recent draft
      select 
      	count(distinct dd.draft_id) as total_drafts,
      	min(drd.release_date) as first_draft_date,
      	max(drd.release_date ) as most_recent_draft_date
      from drafts.drafts_drafters dd
      join drafts.draft_release_date drd on dd.draft_id = drd.draft_id 
      where dd.drafter_id = @DrafterId;

      -- 2) First Draft
      select 
        d.id as draft_id,
        d.title as draft_title,
        array_agg(distinct drd.release_date order by drd.release_date) as dates
      from drafts.drafts d 
      join drafts.drafts_drafters dd on dd.draft_id = d.id
      join drafts.draft_release_date drd on drd.draft_id = d.id
      where dd.drafter_id = @DrafterId
      group  by d.id , d.title, drd.release_date
      order by drd.release_date 
      limit 1;

      -- 3) Most Recent Draft
      select 
        d.id as draft_id,
        d.title as draft_title,
        array_agg(distinct drd.release_date order by drd.release_date) as dates
      from drafts.drafts d 
      join drafts.drafts_drafters dd on dd.draft_id = d.id
      join drafts.draft_release_date drd on drd.draft_id = d.id
      where dd.drafter_id = @DrafterId
      group  by d.id , d.title, drd.release_date
      order by drd.release_date desc
      limit 1;

      -- 4) Vetoes and veto overrides used, times overrode
      select
      	sum(stats.vetoes_used) as total_vetoes_used,
      	sum(stats.veto_overrides_used) as total_veto_overrides_used,
      	sum(stats.commissioner_overrides ) as total_commissioner_overrides
      from drafts.drafter_draft_stats stats
      where stats.drafter_id = @DrafterId;

      -- 5) times vetoed
      select 
      	count(*) as times_vetoed
      from drafts.vetoes v 
      join drafts.picks p on v.pick_id = p.id 
      left join drafts.veto_overrides vo on vo.veto_id = v.id
      where p.drafter_id = @DrafterId and vo.id is null;

      -- 6) times their veto was overridden
      select count(*) as times_veto_overrides_against
      from drafts.veto_overrides vo 
      join drafts.vetoes v on vo.veto_id = v.id 
      where v.drafter_id  = @DrafterId;

      -- 7) total films drafted
      select count(*) as total_films_drafted
      from drafts.picks p 
      left join drafts.vetoes v on v.pick_id = p.id 
      left join drafts.veto_overrides vo on vo.veto_id = v.id 
      left join drafts.commissioner_overrides co on co.pick_id = p.id
      where p.drafter_id = @DrafterId
      	and co.id is null
      	and (v.id is null or vo.id is not null);

      -- 8) rollover veto and rollover veto overrides
      select 
        stats.rollover_veto = 1 as has_rollover_veto,
        stats.rollover_veto_override = 1 as has_rollover_veto_overrides
      from drafts.drafter_draft_stats stats
      join drafts.draft_release_date drd on drd.draft_id = stats.draft_id 
      where stats.drafter_id = @DrafterId
      order by drd.release_date desc 
      limit 1;

      -- 9) draft history
      select
      	d.id as draft_id,
      	d.title as draft_title,
      	array_agg(distinct drd.release_date order by drd.release_date) as release_dates,
      	p.id as pick_id,
      	p."position",
      	p.play_order,
      	m.id as movie_id,
      	m.movie_title,co.id,
      	v.id is not null as was_vetoed,
      	v.drafter_id as vetoed_by_id,
      	vp.display_name as vetoed_by_name,
      	vo.id is not null as was_veto_overridden,
      	vo.drafter_id as veto_override_by_id,
      	vop.display_name as veto_override_by_name,
      	co.id is not null as was_commissioner_overridden
      from drafts.picks p 
      join drafts.drafts d on d.id = p.draft_id 
      left join drafts.draft_release_date drd on drd.draft_id = d.id 
      left join drafts.movies m on m.id = p.movie_id 
      left join drafts.vetoes v on v.pick_id = p.id 
      left join drafts.drafters dr on v.drafter_id = dr.id
      left join drafts.people vp on dr.person_id = vp.id
      left join drafts.veto_overrides vo on vo.veto_id = v.id 
      left join drafts.drafters drr on drr.id = vo.drafter_id
      left join drafts.people vop on vop.id = drr.person_id
      left join drafts.commissioner_overrides co on co.pick_id = p.id 
      where p.drafter_id = @DrafterId
      group by d.id, d.title, p.id, m.id, m.movie_title, v.id, v.drafter_id, vp.display_name,
      	vo.id, vo.drafter_id, vop.display_name, co.id 
      order by min(drd.release_date) asc, p.play_order;

      -- 10) draft history (vetoes used)
      select
      	d.id as draft_id,
      	d.title as draft_title,
      	array_agg(distinct drd.release_date order by drd.release_date) as release_date,
      	v.id as veto_id,
      	p.id as target_pick_id,
      	p."position",
      	p.play_order,
      	m.id as movie_id,
      	m.movie_title as target_movie_title,
        pv.id as target_drafter_id,
      	pv.display_name as target_drafter_name,
      	vo.id is not null as was_veto_overridden,
        pvo.id as veto_override_id,
      	pvo.display_name as override_name
      from drafts.vetoes v 
      join drafts.picks p on p.id = v.pick_id 
      join drafts.drafts d on d.id = p.draft_id 
      left join drafts.draft_release_date drd on drd.draft_id = d.id 
      left join drafts.movies m on m.id = p.movie_id 
      left join drafts.drafters dr on dr.id = p.drafter_id 
      left join drafts.people pv on pv.id = dr.person_id 
      left join drafts.veto_overrides vo on vo.veto_id = v.id 
      left join drafts.drafters drr on drr.id  = vo.drafter_id 
      left join drafts.people pvo on pvo.id = drr.person_id 
      where v.drafter_id = @DrafterId
      group by d.id, d.title, v.id, p.id, m.id, m.movie_title, pv.id, pv.display_name,
        vo.id, pvo.id, pvo.display_name
      order by min(drd.release_date) asc, p.play_order;
      """;

    var grid = await connection.QueryMultipleAsync(
      sql,
      new { request.DrafterId });

    var totals = await grid.ReadSingleAsync();
    var firstDraft = await grid.ReadSingleAsync();
    var mostRecentDraft = await grid.ReadSingleAsync();
    var blessingCounts = await grid.ReadSingleAsync();
    var timesVetoed = await grid.ReadSingleAsync();
    var timesVetoedOverrides = await grid.ReadSingleAsync();
    var totalFilmsDrafted = await grid.ReadSingleAsync();
    var rollover = await grid.ReadSingleAsync();
    var pickRows = (await grid.ReadAsync()).ToList();
    var vetoRows = (await grid.ReadAsync()).ToList();

    var firstDraftResponse = firstDraft is not null
      ? new DraftBrief(
          firstDraft.draft_id,
          firstDraft.draft_title,
          ((DateTime[])firstDraft.dates)
            .Select(DateOnly.FromDateTime)
            .ToList()
        )
      : null;

    var mostRecentDraftResponse = totals.most_recent_draft_date is not null
      ? new DraftBrief(
          mostRecentDraft.draft_id,
          mostRecentDraft.draft_title,
          ((DateTime[])mostRecentDraft.dates)
            .Select(DateOnly.FromDateTime)
            .ToList()
        )
      : null;

    var history = pickRows
      .GroupBy(r => new { r.draft_id, r.draft_title, r.release_dates })
      .Select(g =>
      {
        var dates = ((DateTime[])g.Key.release_dates)
          .Select(d => DateOnly.FromDateTime(d))
          .ToList();
        var brief = new DraftBrief(
          g.Key.draft_id,
          g.Key.draft_title,
          dates
        );
        var picks = g.Select(p => new PickItem(
          p.pick_id,
          p.position,
          p.play_order,
          p.movie_id,
          p.movie_title,
          p.was_vetoed,
          p.was_veto_overridden,
          p.was_commissioner_overridden,
          p.vetoed_by_id,
          p.vetoed_by_name,
          p.veto_override_by_id,
          p.veto_override_by_name
        )).ToList();
        return new DraftHistoryItem(
          brief,
          picks
        );
      })
      .ToList();

    var vetoHistory = vetoRows
      .GroupBy(r => new { r.draft_id, r.draft_title, r.release_date })
      .SelectMany(g =>
      {
        var dates = ((DateTime[])g.Key.release_date)
          .Select(d => DateOnly.FromDateTime(d))
          .ToList();
        var brief = new DraftBrief(
          g.Key.draft_id,
          g.Key.draft_title,
          dates
        );
        return g.Select(v => new VetoHistoryItem(
          brief,
          v.veto_id,
          v.target_pick_id,
          v.position,
          v.play_order,
          v.movie_id,
          v.target_movie_title,
          v.target_drafter_id,
          v.target_drafter_name,
          v.was_veto_overridden,
          v.veto_override_id,
          v.override_name
        ));
      })
      .ToList();

    var socialHandles = new SocialHandles(
      Twitter: null,
      Instagram: null,
      Bluesky: null,
      Letterboxd: null,
      ProfilePicturePath: null
    );

    var userSocials = await _usersApi.GetUserSocialsAsync(
      person.UserId,
      cancellationToken);

    if (userSocials is not null)
    {
      socialHandles = new SocialHandles(
        Twitter: userSocials.Twitter,
        Instagram: userSocials.Instagram,
        Bluesky: userSocials.Bluesky,
        Letterboxd: userSocials.Letterboxd,
        ProfilePicturePath: userSocials.ProfilePicturePath
      );
    }

    var totalDrafts = totals.total_drafts is not null
      ? (int)totals.total_drafts
      : 0;
    var filmsDrafted = totalFilmsDrafted.total_films_drafted is not null
      ? (int)totalFilmsDrafted.total_films_drafted
      : 0;
    var vetoesUsed = blessingCounts.total_vetoes_used is not null ? (int?)blessingCounts.total_vetoes_used : 0;
    var vetoOverridesUsed = blessingCounts.total_veto_overrides_used is not null
      ? (int?)blessingCounts.total_veto_overrides_used
      : 0;
    var commissionerOverrides = blessingCounts.total_commissioner_overrides is not null
      ? (int?)blessingCounts.total_commissioner_overrides
      : 0;
    var timesVetoedAgainst = timesVetoed.times_vetoed is not null
      ? (int?)timesVetoed.times_vetoed
      : 0;
    var timesVetoOverridden = timesVetoedOverrides.times_veto_overrides_against is not null
      ? (int?)timesVetoedOverrides.times_veto_overrides_against
      : 0;
    var hasRolloverVeto = rollover.has_rollover_veto is not null && (bool)rollover.has_rollover_veto;
    var hasRolloverVetoOverride = rollover.has_rollover_veto_overrides is not null && (bool)rollover.has_rollover_veto_overrides;


    var response = new DrafterProfileResponse(
      DrafterId: request.DrafterId,
      PersonId: person.Id,
      DisplayName: person.DisplayName,
      TotalDrafts: totalDrafts,
      FirstDraft: firstDraftResponse,
      MostRecentDraft: mostRecentDraftResponse,
      FilmsDrafted: filmsDrafted,
      VetoesUsed: vetoesUsed,
      VetoOverridesUsed: vetoOverridesUsed,
      CommissionerOverrides: commissionerOverrides,
      TimesVetoed: timesVetoedAgainst,
      TimesVetoOverridesAgainst: timesVetoOverridden,
      HasRolloverVeto: hasRolloverVeto,
      HasRolloverVetoOverride: hasRolloverVetoOverride,
      SocialHandles: socialHandles,
      DraftHistory: history,
      VetoHistory: vetoHistory);

    return response;
  }
}
