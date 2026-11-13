-- merge-duplicate-people.sql
--
-- The billing-order resync could not match some legacy people (tmdb_id = 0, IMDb id set)
-- to TMDb's cast (TMDb has no IMDb id for them), so it created a second person
-- (tmdb_id > 0, no IMDb id) for the same human on the same title.
--
-- This merges each such pair into the LEGACY row: the legacy person takes the TMDb id,
-- inherits the billing order, and the duplicate person is deleted.
--
-- Exploratory: runs in a transaction and always ends with ROLLBACK. Read the summaries.
-- The real change ships as the migrator fix script, never as a hand-run COMMIT.
--
-- A pair is merged only when, on at least one shared title, a legacy person and a
-- TMDb-only person have the same name (case-insensitive, trimmed), AND the pairing is
-- one-to-one across the whole database. Anything ambiguous is listed and left alone.

begin;

-- 1. Every (title, person) credit, any role, with the person's identity.
create temp table credits on commit drop as
select c.media_id, c.person_id, p.tmdb_id, p.imdb_id, lower(btrim(p.name)) as name_key
from (
  select media_id, actor_id    as person_id from movies.media_actors
  union all
  select media_id, director_id as person_id from movies.media_directors
  union all
  select media_id, writer_id   as person_id from movies.media_writers
  union all
  select media_id, producer_id as person_id from movies.media_producers
) c
join movies.people p on p.id = c.person_id;

-- 2. Candidate pairs: legacy person (l) and TMDb-only person (b), same name, same title.
create temp table candidate_pairs on commit drop as
select distinct l.person_id as legacy_id, b.person_id as tmdb_only_id, l.name_key
from credits l
join credits b
  on b.media_id = l.media_id
 and b.name_key = l.name_key
where l.tmdb_id = 0
  and l.imdb_id is not null and l.imdb_id <> ''
  and b.tmdb_id > 0
  and (b.imdb_id is null or b.imdb_id = '')
  and l.person_id <> b.person_id;

-- 3. Keep only one-to-one pairs. A legacy person matching two TMDb people (or the
--    reverse) is a homonym or a bad import and needs a human.
create temp table pairs on commit drop as
select cp.*
from candidate_pairs cp
where (select count(*) from candidate_pairs x where x.legacy_id    = cp.legacy_id)    = 1
  and (select count(*) from candidate_pairs x where x.tmdb_only_id = cp.tmdb_only_id) = 1;

-- ---- Summary 1: what will be merged and what is skipped --------------------------
select
  (select count(*) from pairs)                                         as pairs_to_merge,
  (select count(*) from candidate_pairs) - (select count(*) from pairs) as ambiguous_skipped;

-- ---- Summary 2: skipped pairs, for a manual look ---------------------------------
select cp.name_key, cp.legacy_id, cp.tmdb_only_id
from candidate_pairs cp
where not exists (select 1 from pairs p where p.legacy_id = cp.legacy_id and p.tmdb_only_id = cp.tmdb_only_id)
order by cp.name_key
limit 50;

-- 4. Actors: carry the billing order onto the legacy row where both rows exist on a title.
update movies.media_actors l_row
set billing_order = b_row.billing_order
from pairs p
join movies.media_actors b_row on b_row.actor_id = p.tmdb_only_id
where l_row.actor_id = p.legacy_id
  and l_row.media_id = b_row.media_id
  and l_row.billing_order is null
  and b_row.billing_order is not null;

-- 5. Per role: drop the duplicate's row where the legacy person already has one on the
--    same title (primary key is media + person), then repoint the rest.
delete from movies.media_actors b_row
using pairs p
where b_row.actor_id = p.tmdb_only_id
  and exists (select 1 from movies.media_actors l_row
              where l_row.media_id = b_row.media_id and l_row.actor_id = p.legacy_id);
update movies.media_actors r set actor_id = p.legacy_id
from pairs p where r.actor_id = p.tmdb_only_id;

delete from movies.media_directors b_row
using pairs p
where b_row.director_id = p.tmdb_only_id
  and exists (select 1 from movies.media_directors l_row
              where l_row.media_id = b_row.media_id and l_row.director_id = p.legacy_id);
update movies.media_directors r set director_id = p.legacy_id
from pairs p where r.director_id = p.tmdb_only_id;

delete from movies.media_writers b_row
using pairs p
where b_row.writer_id = p.tmdb_only_id
  and exists (select 1 from movies.media_writers l_row
              where l_row.media_id = b_row.media_id and l_row.writer_id = p.legacy_id);
update movies.media_writers r set writer_id = p.legacy_id
from pairs p where r.writer_id = p.tmdb_only_id;

delete from movies.media_producers b_row
using pairs p
where b_row.producer_id = p.tmdb_only_id
  and exists (select 1 from movies.media_producers l_row
              where l_row.media_id = b_row.media_id and l_row.producer_id = p.legacy_id);
update movies.media_producers r set producer_id = p.legacy_id
from pairs p where r.producer_id = p.tmdb_only_id;

-- 6. The legacy person takes the TMDb id; the duplicate person goes.
update movies.people l
set tmdb_id = b.tmdb_id
from pairs p
join movies.people b on b.id = p.tmdb_only_id
where l.id = p.legacy_id;

-- Safety check: a duplicate must have no credits left before it is deleted (the foreign
-- keys cascade, so a leftover credit would be silently lost). Abort if any remain.
do $$
declare leftover int;
begin
  select
    (select count(*) from movies.media_actors     r join pairs p on r.actor_id    = p.tmdb_only_id) +
    (select count(*) from movies.media_directors  r join pairs p on r.director_id = p.tmdb_only_id) +
    (select count(*) from movies.media_writers    r join pairs p on r.writer_id   = p.tmdb_only_id) +
    (select count(*) from movies.media_producers  r join pairs p on r.producer_id = p.tmdb_only_id)
  into leftover;
  if leftover > 0 then
    raise exception 'merge aborted: % credits still point at duplicate people', leftover;
  end if;
end $$;

delete from movies.people b using pairs p where b.id = p.tmdb_only_id;

-- ---- Summary 3: after the merge, same-name unbilled/billed pairs should be ~0 ----
select count(*) as same_name_unbilled_vs_billed_after
from movies.media_actors n
join movies.people pn on pn.id = n.actor_id
join movies.media_actors o on o.media_id = n.media_id and o.billing_order is not null
join movies.people po on po.id = o.actor_id and po.name = pn.name
where n.billing_order is null;

-- ---- Summary 4: what is left, by identity shape -----------------------------------
select (pn.tmdb_id > 0)                                  as unbilled_has_tmdb,
       (pn.imdb_id is not null and pn.imdb_id <> '')     as unbilled_has_imdb,
       (po.tmdb_id > 0)                                  as billed_has_tmdb,
       (po.imdb_id is not null and po.imdb_id <> '')     as billed_has_imdb,
       count(*)                                          as rows
from movies.media_actors n
join movies.people pn on pn.id = n.actor_id
join movies.media_actors o on o.media_id = n.media_id and o.billing_order is not null
join movies.people po on po.id = o.actor_id and po.name = pn.name
where n.billing_order is null
group by 1, 2, 3, 4
order by rows desc;

-- ---- Summary 5: a sample of what is left ------------------------------------------
select n.media_id, pn.name,
       pn.id as unbilled_person, pn.tmdb_id as u_tmdb, pn.imdb_id as u_imdb,
       po.id as billed_person,   po.tmdb_id as b_tmdb, po.imdb_id as b_imdb
from movies.media_actors n
join movies.people pn on pn.id = n.actor_id
join movies.media_actors o on o.media_id = n.media_id and o.billing_order is not null
join movies.people po on po.id = o.actor_id and po.name = pn.name
where n.billing_order is null
limit 20;

-- ---- Residual pairs: one row per (unbilled person, billed person) ------------------
-- State here is post-merge, inside this transaction.
create temp table resid on commit drop as
select n.media_id,
       pn.id as u_id, pn.name, pn.tmdb_id as u_tmdb, pn.imdb_id as u_imdb,
       po.id as b_id,           po.tmdb_id as b_tmdb, po.imdb_id as b_imdb
from movies.media_actors n
join movies.people pn on pn.id = n.actor_id
join movies.media_actors o on o.media_id = n.media_id and o.billing_order is not null
join movies.people po on po.id = o.actor_id and po.name = pn.name
where n.billing_order is null;

create temp table resid_pairs on commit drop as
select g.*,
       count(*) over (partition by u_id) as u_matches,
       count(*) over (partition by b_id) as b_matches,
       case when u_imdb ~ '^nm[0-9]+$' and b_imdb ~ '^nm[0-9]+$'
            then abs(substr(u_imdb, 3)::bigint - substr(b_imdb, 3)::bigint)
       end as imdb_distance
from (
  select u_id, b_id, name, u_tmdb, u_imdb, b_tmdb, b_imdb,
         count(distinct media_id) as shared_titles
  from resid
  group by u_id, b_id, name, u_tmdb, u_imdb, b_tmdb, b_imdb
) g;

-- ---- Summary 6: residual pairs by confidence tier ----------------------------------
--   A  one-to-one and IMDb ids within 100 of each other (IMDb duplicate profile)
--   B  one-to-one and the pair shares two or more titles
--   C  one-to-one, one shared title, ids far apart or missing
--   D  not one-to-one (a person matches several others)
select case
         when u_matches > 1 or b_matches > 1                       then 'D not 1:1'
         when imdb_distance is not null and imdb_distance <= 100   then 'A 1:1, imdb ids within 100'
         when shared_titles >= 2                                   then 'B 1:1, 2+ shared titles'
         else                                                           'C 1:1, one title, ids far apart'
       end as tier,
       count(*)           as pairs,
       sum(shared_titles) as credit_rows
from resid_pairs
group by 1
order by 1;

-- ---- Summary 7: tiers C and D, for a manual look ----------------------------------
select name, u_id, u_imdb, b_id, b_tmdb, b_imdb, shared_titles, u_matches, b_matches, imdb_distance
from resid_pairs
where u_matches > 1 or b_matches > 1
   or (coalesce(imdb_distance, 1000000) > 100 and shared_titles < 2)
order by name
limit 60;

-- ---- Summary 8: people still without a TMDb id that carry a billed credit ----------
-- (matched by IMDb id during the resync, so billed, but never given a tmdb_id)
select count(distinct a.actor_id) as billed_people_without_tmdb_id
from movies.media_actors a
join movies.people p on p.id = a.actor_id
where a.billing_order is not null
  and p.tmdb_id = 0;

-- Exploratory only. Always rolls back. The real change ships as the migrator fix script.
rollback;
