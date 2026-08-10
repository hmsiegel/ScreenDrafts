"""
legend_rotten_movies.py

Finds which ScreenDrafts "Legend" (drafter with 20+ main-feed draft
appearances) has landed the most Rotten Tomatoes "Rotten" movies on
regular main-feed drafts.

Definitions used (per Harlan, 2026-08-07):
  - Legend           = drafter with >= 20 distinct DRAFTS (not draft
                        parts -- a multi-part draft counts once) that
                        are main-feed-eligible.
  - Regular drafts    = DraftType IN (Standard, MiniMega, Mega, Super)
                        i.e. excludes MiniSuper and SpeedDraft (Patreon-only).
  - Main feed         = Series.CanonicalPolicy == Always (0)
                        OR (CanonicalPolicy == OnMainFeed (2)
                            AND the draft part has a MainFeed release row)
  - Played (counted)  = LANDED picks only:
                        not commissioner-overridden, AND
                        (not vetoed OR vetoed-but-overridden)
  - Team drafts        = when two+ drafters play as a DrafterTeam
                        (e.g. Clay + Ryan on "The Spielberg Produced Mega
                        Draft"), the team's appearance AND every one of its
                        landed picks are attributed to EACH member
                        individually (dual-counted, not split/shared).
  - Rotten            = OMDb's Rotten Tomatoes rating < --rotten-threshold
                        (default 60, RT's own Fresh/Rotten cutoff)

Movies with no IMDb id, or no Rotten Tomatoes rating on OMDb, are
reported separately rather than silently dropped or silently counted
as Fresh.

Usage (PowerShell):

    $env:OMDB_API_KEY = "your-key-here"
    python legend_rotten_movies.py `
        --db-host localhost --db-port 5432 --db-name screendrafts `
        --db-user postgres --db-password postgres

Requires: psycopg2-binary, requests
    pip install psycopg2-binary requests --break-system-packages
"""

from __future__ import annotations

import argparse
import csv
import json
import os
import re
import sys
import time
from collections import defaultdict
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

import psycopg2
import psycopg2.extras
import requests

# --- Domain constants (confirmed against the codebase) ---------------------

DRAFT_TYPE_STANDARD = 0
DRAFT_TYPE_MINI_MEGA = 1
DRAFT_TYPE_MEGA = 2
DRAFT_TYPE_SUPER = 3
DRAFT_TYPE_MINI_SUPER = 4   # Patreon-only, excluded
DRAFT_TYPE_SPEED_DRAFT = 5  # Patreon-only, excluded
REGULAR_DRAFT_TYPES = (
    DRAFT_TYPE_STANDARD,
    DRAFT_TYPE_MINI_MEGA,
    DRAFT_TYPE_MEGA,
    DRAFT_TYPE_SUPER,
)

CANONICAL_POLICY_ALWAYS = 0
CANONICAL_POLICY_NEVER = 1
CANONICAL_POLICY_ON_MAIN_FEED = 2

RELEASE_CHANNEL_MAIN_FEED = 0

PARTICIPANT_KIND_DRAFTER = 0
PARTICIPANT_KIND_DRAFTER_TEAM = 1

LEGEND_APPEARANCE_THRESHOLD = 20

# --- SQL ---------------------------------------------------------------

ELIGIBLE_DRAFT_PARTS_CTE = """
eligible_draft_parts AS (
    SELECT dp.id AS draft_part_id, d.id AS draft_id
    FROM drafts.draft_parts dp
    JOIN drafts.drafts d ON d.id = dp.draft_id
    JOIN drafts.series s ON s.id = d.series_id
    WHERE d.draft_type = ANY(%(regular_draft_types)s)
      AND d.is_deleted = false
      AND (
        s.canonical_policy = %(policy_always)s
        OR (
          s.canonical_policy = %(policy_on_main_feed)s
          AND EXISTS (
            SELECT 1 FROM drafts.draft_releases dr
            WHERE dr.part_id = dp.id
              AND dr.release_channel = %(main_feed_channel)s
          )
        )
      )
)
"""

LEGENDS_SQL = f"""
WITH {ELIGIBLE_DRAFT_PARTS_CTE},
-- Expand both solo-drafter and team participation to individual drafter_ids.
-- A team's appearance in a draft counts as an appearance for every member.
participant_appearances AS (
    SELECT dpp.participant_id_value AS drafter_id, edp.draft_id
    FROM drafts.draft_part_participants dpp
    JOIN eligible_draft_parts edp ON edp.draft_part_id = dpp.draft_part_id
    WHERE dpp.participant_kind_value = %(drafter_kind)s

    UNION ALL

    SELECT dtd.drafter_id, edp.draft_id
    FROM drafts.draft_part_participants dpp
    JOIN eligible_draft_parts edp ON edp.draft_part_id = dpp.draft_part_id
    JOIN drafts.drafter_team_drafter dtd ON dtd.drafter_team_id = dpp.participant_id_value
    WHERE dpp.participant_kind_value = %(drafter_team_kind)s
),
legend_counts AS (
    SELECT
        drafter_id,
        COUNT(DISTINCT draft_id) AS appearance_count
    FROM participant_appearances
    GROUP BY drafter_id
    HAVING COUNT(DISTINCT draft_id) >= %(legend_threshold)s
)
SELECT
    lc.drafter_id,
    lc.appearance_count,
    dr.public_id AS drafter_public_id,
    COALESCE(pe.display_name, pe.first_name || ' ' || pe.last_name) AS display_name
FROM legend_counts lc
JOIN drafts.drafters dr ON dr.id = lc.drafter_id
JOIN drafts.people pe ON pe.id = dr.person_id
ORDER BY lc.appearance_count DESC;
"""

LANDED_PICKS_SQL = f"""
WITH {ELIGIBLE_DRAFT_PARTS_CTE},
landed_picks_raw AS (
    SELECT
        pk.id AS pick_id,
        pk.played_by_participant_id_value AS participant_id,
        pk.played_by_participant_kind_value AS participant_kind,
        pk.movie_id
    FROM drafts.picks pk
    JOIN eligible_draft_parts edp ON edp.draft_part_id = pk.draft_part_id
    WHERE pk.played_by_participant_kind_value IN (%(drafter_kind)s, %(drafter_team_kind)s)
      AND NOT EXISTS (
        SELECT 1 FROM drafts.commissioner_overrides co
        WHERE co.pick_id = pk.id
      )
      AND (
        NOT EXISTS (SELECT 1 FROM drafts.vetoes v WHERE v.target_pick_id = pk.id)
        OR EXISTS (
          SELECT 1 FROM drafts.vetoes v
          WHERE v.target_pick_id = pk.id AND v.is_overridden = true
        )
      )
),
-- A pick played by a team counts once per team member (dual-attributed),
-- per Harlan: "their picks should count for both of them."
landed_picks AS (
    SELECT pick_id, participant_id AS drafter_id, movie_id
    FROM landed_picks_raw
    WHERE participant_kind = %(drafter_kind)s

    UNION ALL

    SELECT lpr.pick_id, dtd.drafter_id, lpr.movie_id
    FROM landed_picks_raw lpr
    JOIN drafts.drafter_team_drafter dtd ON dtd.drafter_team_id = lpr.participant_id
    WHERE lpr.participant_kind = %(drafter_team_kind)s
)
SELECT
    lp.drafter_id,
    lp.pick_id,
    m.id AS movie_id,
    m.movie_title,
    m.year,
    m.imdb_id,
    m.media_type
FROM landed_picks lp
JOIN drafts.movies m ON m.id = lp.movie_id
WHERE lp.drafter_id = ANY(%(legend_ids)s::uuid[]);
"""

# --- OMDb -----------------------------------------------------------------

OMDB_BASE_URL = "http://www.omdbapi.com/"


@dataclass
class OmdbResult:
    found: bool
    rt_percent: Optional[int] = None  # None if OMDb has no RT rating
    title: Optional[str] = None
    error: Optional[str] = None


class OmdbClient:
    def __init__(self, api_key: str, cache_path: Path, delay_seconds: float = 0.25):
        self.api_key = api_key
        self.cache_path = cache_path
        self.delay_seconds = delay_seconds
        self.cache: dict[str, dict] = {}
        if cache_path.exists():
            self.cache = json.loads(cache_path.read_text(encoding="utf-8"))

    def _save_cache(self) -> None:
        self.cache_path.write_text(json.dumps(self.cache, indent=2), encoding="utf-8")

    def get_rotten_tomatoes(self, imdb_id: str) -> OmdbResult:
        if imdb_id in self.cache:
            cached = self.cache[imdb_id]
            return OmdbResult(**cached)

        result = self._fetch(imdb_id)
        self.cache[imdb_id] = result.__dict__
        self._save_cache()
        time.sleep(self.delay_seconds)
        return result

    _debug_samples_shown = 0
    _DEBUG_SAMPLE_LIMIT = 3

    def _fetch(self, imdb_id: str) -> OmdbResult:
        try:
            resp = requests.get(
                OMDB_BASE_URL,
                params={"i": imdb_id, "apikey": self.api_key},
                timeout=10,
            )
            status = resp.status_code
            data = resp.json()
        except requests.RequestException as exc:
            self._debug(f"[network error] imdb_id={imdb_id} exc={exc}")
            return OmdbResult(found=False, error=str(exc))
        except ValueError as exc:
            self._debug(f"[bad json] imdb_id={imdb_id} status={status} body={resp.text[:200]!r}")
            return OmdbResult(found=False, error=f"non-JSON response: {exc}")

        if data.get("Response") != "True":
            err = data.get("Error", "unknown OMDb error")
            self._debug(f"[omdb error] imdb_id={imdb_id} status={status} error={err!r} raw={data}")
            return OmdbResult(found=False, error=err)

        rt_percent = None
        found_rt_source = False
        for rating in data.get("Ratings", []):
            if rating.get("Source") == "Rotten Tomatoes":
                found_rt_source = True
                raw = rating.get("Value", "").rstrip("%")
                if raw.isdigit():
                    rt_percent = int(raw)
                break

        if not found_rt_source:
            self._debug(f"[no RT source] imdb_id={imdb_id} title={data.get('Title')!r} "
                        f"type={data.get('Type')!r} ratings={data.get('Ratings')}")

        return OmdbResult(found=True, rt_percent=rt_percent, title=data.get("Title"))

    def _debug(self, message: str) -> None:
        if OmdbClient._debug_samples_shown < OmdbClient._DEBUG_SAMPLE_LIMIT:
            print(f"  DEBUG: {message}")
            OmdbClient._debug_samples_shown += 1


# --- tv-api.com fallback (opt-in) ------------------------------------------
#
# tv-api.com (formerly imdb-api.com) has a dedicated Ratings endpoint:
#   GET https://tv-api.com/en/API/Ratings/{apiKey}/{imdbID}
# aggregating IMDb, Metacritic, Rotten Tomatoes, TheMovieDb and TV.com scores.
# It's a paid/registered community service, NOT an official RT data feed --
# same category as OMDb -- so whether it actually has post-2018 titles OMDb
# is missing is unverified. The exact response field names are also
# unconfirmed (docs don't show a sample payload), so this prints the raw
# JSON for the first few lookups; check that output before trusting the
# aggregate numbers.

TV_API_BASE_URL = "https://tv-api.com/en/API/Ratings"


@dataclass
class TvApiResult:
    found: bool
    rt_percent: Optional[int] = None
    error: Optional[str] = None


class TvApiClient:
    _debug_samples_shown = 0
    _DEBUG_SAMPLE_LIMIT = 3

    def __init__(self, api_key: str, cache_path: Path, delay_seconds: float = 0.3):
        self.api_key = api_key
        self.cache_path = cache_path
        self.delay_seconds = delay_seconds
        self.cache: dict[str, dict] = {}
        if cache_path.exists():
            self.cache = json.loads(cache_path.read_text(encoding="utf-8"))

    def _save_cache(self) -> None:
        self.cache_path.write_text(json.dumps(self.cache, indent=2), encoding="utf-8")

    def lookup(self, imdb_id: str) -> TvApiResult:
        if imdb_id in self.cache:
            return TvApiResult(**self.cache[imdb_id])

        result = self._fetch(imdb_id)
        self.cache[imdb_id] = result.__dict__
        self._save_cache()
        time.sleep(self.delay_seconds)
        return result

    def _fetch(self, imdb_id: str) -> TvApiResult:
        url = f"{TV_API_BASE_URL}/{self.api_key}/{imdb_id}"
        try:
            resp = requests.get(url, timeout=10)
            data = resp.json()
        except (requests.RequestException, ValueError) as exc:
            return TvApiResult(found=False, error=str(exc))

        self._debug(f"imdb_id={imdb_id} status={resp.status_code} raw={data}")

        # Confirmed live response uses camelCase keys (rottenTomatoes,
        # errorMessage), not the PascalCase of the underlying C# RatingData
        # model -- ASP.NET's default JSON serializer camelCases property names.
        error_message = data.get("errorMessage")
        if error_message:
            return TvApiResult(found=False, error=error_message)

        raw_value = data.get("rottenTomatoes")
        if raw_value:
            digits = re.sub(r"[^\d]", "", str(raw_value))
            if digits:
                return TvApiResult(found=True, rt_percent=int(digits))

        return TvApiResult(found=False, error="rottenTomatoes field empty (title not rated on RT, or not found)")

    def _debug(self, message: str) -> None:
        if TvApiClient._debug_samples_shown < TvApiClient._DEBUG_SAMPLE_LIMIT:
            print(f"  TV-API DEBUG: {message}")
            TvApiClient._debug_samples_shown += 1


# --- Rotten Tomatoes fallback scraper (opt-in) -----------------------------
#
# OMDb stopped syncing Rotten Tomatoes data around 2017-2018 after RT ended
# third-party data licensing; any RT rating still present in an OMDb response
# is a stale pre-2018 leftover. Titles released or re-scored since then
# (e.g. Tar, 2022) will never get an RT value from OMDb no matter how many
# times you ask.
#
# RT has no public API. This fallback fetches the RT movie page directly and
# reads the Tomatometer score out of the server-rendered <score-board
# tomatometerscore="91" ...> element. This is scraping, and is against RT's
# Terms of Service -- it's meant for a small, one-off, personal lookup
# (only the handful of titles OMDb couldn't classify), not a bulk crawl.
# RT's bot protection may rate-limit or block you; back off if that happens
# rather than retrying aggressively.

import unicodedata

RT_SCORE_ATTR_RE = re.compile(r'tomatometerscore="(\d{1,3})"')
RT_TITLE_ATTR_RE = re.compile(r'data-qa="score-panel-movie-title"[^>]*>([^<]+)<')


def rt_slug(title: str) -> str:
    normalized = unicodedata.normalize("NFKD", title)
    ascii_only = normalized.encode("ascii", "ignore").decode("ascii")
    slug = re.sub(r"[^a-z0-9]+", "_", ascii_only.lower()).strip("_")
    return slug


@dataclass
class RtScrapeResult:
    found: bool
    rt_percent: Optional[int] = None
    matched_url: Optional[str] = None
    status: str = "not_found"  # not_found | blocked | error | found


class RtScraper:
    _HEADERS = {
        "User-Agent": (
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
            "(KHTML, like Gecko) Chrome/124.0 Safari/537.36"
        )
    }

    def __init__(self, cache_path: Path, delay_seconds: float = 1.5, max_consecutive_blocks: int = 5):
        self.cache_path = cache_path
        self.delay_seconds = delay_seconds
        self.max_consecutive_blocks = max_consecutive_blocks
        self._consecutive_blocks = 0
        self._disabled = False
        self.cache: dict[str, dict] = {}
        if cache_path.exists():
            self.cache = json.loads(cache_path.read_text(encoding="utf-8"))

    def _save_cache(self) -> None:
        self.cache_path.write_text(json.dumps(self.cache, indent=2), encoding="utf-8")

    def lookup(self, imdb_id: str, title: str, year: Optional[str]) -> RtScrapeResult:
        if imdb_id in self.cache:
            return RtScrapeResult(**self.cache[imdb_id])

        if self._disabled:
            result = RtScrapeResult(found=False, status="skipped_after_blocks")
        else:
            result = self._fetch(title, year)
            time.sleep(self.delay_seconds)

        self.cache[imdb_id] = result.__dict__
        self._save_cache()
        return result

    def _fetch(self, title: str, year: Optional[str]) -> RtScrapeResult:
        slug = rt_slug(title)
        candidates = [f"/m/{slug}"]
        if year:
            candidates.insert(0, f"/m/{slug}_{year}")

        for path in candidates:
            url = f"https://www.rottentomatoes.com{path}"
            try:
                resp = requests.get(url, headers=self._HEADERS, timeout=10)
            except requests.RequestException:
                continue

            if resp.status_code in (403, 429):
                self._consecutive_blocks += 1
                if self._consecutive_blocks >= self.max_consecutive_blocks:
                    self._disabled = True
                    print("  RT scraper: too many blocked responses in a row, "
                          "disabling further RT lookups for this run.")
                return RtScrapeResult(found=False, status="blocked")

            if resp.status_code != 200:
                continue

            self._consecutive_blocks = 0
            match = RT_SCORE_ATTR_RE.search(resp.text)
            if match:
                return RtScrapeResult(
                    found=True, rt_percent=int(match.group(1)),
                    matched_url=url, status="found",
                )

        return RtScrapeResult(found=False, status="not_found")


# --- Main -------------------------------------------------------------


@dataclass
class LegendStats:
    drafter_id: str
    display_name: str
    appearance_count: int
    landed_pick_count: int = 0
    rotten_instances: int = 0
    rotten_titles: set[str] = field(default_factory=set)
    fresh_instances: int = 0
    no_rt_data_instances: int = 0
    no_imdb_id_instances: int = 0


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--db-host", default=os.environ.get("PGHOST", "localhost"))
    parser.add_argument("--db-port", default=os.environ.get("PGPORT", "5432"))
    parser.add_argument("--db-name", default=os.environ.get("PGDATABASE", "screendrafts"))
    parser.add_argument("--db-user", default=os.environ.get("PGUSER", "postgres"))
    parser.add_argument("--db-password", default=os.environ.get("PGPASSWORD", "postgres"))
    parser.add_argument("--omdb-key", default=os.environ.get("OMDB_API_KEY"))
    parser.add_argument("--rotten-threshold", type=int, default=60,
                         help="RT %% below which a movie is 'Rotten' (default 60)")
    parser.add_argument("--cache-file", default="omdb_cache.json")
    parser.add_argument("--output-csv", default="legend_rotten_report.csv")
    parser.add_argument("--rt-fallback", action="store_true",
                         help="For titles OMDb has no RT rating for, scrape "
                              "rottentomatoes.com directly. See the module "
                              "docstring/comments for what this does and its caveats.")
    parser.add_argument("--rt-cache-file", default="rt_scrape_cache.json")
    parser.add_argument("--tv-api-key", default=os.environ.get("TV_API_KEY"),
                         help="tv-api.com API key. If set, used as a second-tier "
                              "source (after OMDb, before/instead of --rt-fallback) "
                              "for titles OMDb has no RT rating for.")
    parser.add_argument("--tv-api-cache-file", default="tv_api_cache.json")
    args = parser.parse_args()

    if not args.omdb_key:
        sys.exit("Missing OMDb API key. Pass --omdb-key or set OMDB_API_KEY.")

    conn = psycopg2.connect(
        host=args.db_host,
        port=args.db_port,
        dbname=args.db_name,
        user=args.db_user,
        password=args.db_password,
    )

    sql_params = {
        "regular_draft_types": list(REGULAR_DRAFT_TYPES),
        "policy_always": CANONICAL_POLICY_ALWAYS,
        "policy_on_main_feed": CANONICAL_POLICY_ON_MAIN_FEED,
        "main_feed_channel": RELEASE_CHANNEL_MAIN_FEED,
        "drafter_kind": PARTICIPANT_KIND_DRAFTER,
        "drafter_team_kind": PARTICIPANT_KIND_DRAFTER_TEAM,
        "legend_threshold": LEGEND_APPEARANCE_THRESHOLD,
    }

    with conn.cursor(cursor_factory=psycopg2.extras.RealDictCursor) as cur:
        print("Finding Legends (>= 20 main-feed drafts, Standard/MiniMega/Mega/Super)...")
        cur.execute(LEGENDS_SQL, sql_params)
        legend_rows = cur.fetchall()

        if not legend_rows:
            print("No drafters currently meet the Legend threshold. Nothing to report.")
            return

        legends: dict[str, LegendStats] = {}
        for row in legend_rows:
            legends[str(row["drafter_id"])] = LegendStats(
                drafter_id=str(row["drafter_id"]),
                display_name=row["display_name"],
                appearance_count=row["appearance_count"],
            )

        print(f"Found {len(legends)} Legend(s):")
        for stats in legends.values():
            print(f"  - {stats.display_name}: {stats.appearance_count} main-feed drafts")

        print("\nPulling landed picks for these Legends on eligible drafts...")
        cur.execute(
            LANDED_PICKS_SQL,
            {**sql_params, "legend_ids": list(legends.keys())},
        )
        pick_rows = cur.fetchall()

    conn.close()
    print(f"Retrieved {len(pick_rows)} landed picks.")

    omdb = OmdbClient(args.omdb_key, Path(args.cache_file))

    unique_imdb_ids = sorted({r["imdb_id"] for r in pick_rows if r["imdb_id"]})
    print(f"\nQuerying OMDb for {len(unique_imdb_ids)} unique IMDb titles "
          f"(cached results in {args.cache_file} are reused)...")

    rt_lookup: dict[str, OmdbResult] = {}
    for i, imdb_id in enumerate(unique_imdb_ids, start=1):
        rt_lookup[imdb_id] = omdb.get_rotten_tomatoes(imdb_id)
        if i % 50 == 0 or i == len(unique_imdb_ids):
            print(f"  ...{i}/{len(unique_imdb_ids)}")

    tv_api = None
    if args.tv_api_key:
        print("\ntv-api.com fallback enabled: will print the raw JSON for the "
              "first few lookups so we can confirm field names and coverage.")
        tv_api = TvApiClient(args.tv_api_key, Path(args.tv_api_cache_file))
    else:
        print("\ntv-api.com fallback NOT enabled (no --tv-api-key / TV_API_KEY set).")

    rt_scraper = None
    if args.rt_fallback:
        print("RT scraper fallback enabled: scraping rottentomatoes.com directly for "
              "titles OMDb had no RT rating for. This is best-effort and may "
              "get rate-limited by RT's bot protection.")
        rt_scraper = RtScraper(Path(args.rt_cache_file))
    else:
        print("RT scraper fallback NOT enabled (pass --rt-fallback to turn it on).")

    if tv_api is None and rt_scraper is None:
        print("WARNING: no fallback source configured -- titles OMDb has no RT "
              "rating for will be reported as NO_RT_DATA with nothing filling the gap.")

    csv_rows = []
    for row in pick_rows:
        drafter_id = str(row["drafter_id"])
        stats = legends[drafter_id]
        stats.landed_pick_count += 1

        imdb_id = row["imdb_id"]
        title = row["movie_title"]
        year = row["year"]

        if not imdb_id:
            stats.no_imdb_id_instances += 1
            csv_rows.append([stats.display_name, title, year, imdb_id, "NO_IMDB_ID", ""])
            continue

        rt = rt_lookup[imdb_id]
        if not rt.found or rt.rt_percent is None:
            rt_percent = None
            source = "OMDB"

            if rt_percent is None and tv_api is not None:
                tv_result = tv_api.lookup(imdb_id)
                if tv_result.found and tv_result.rt_percent is not None:
                    rt_percent = tv_result.rt_percent
                    source = "TV_API"

            if rt_percent is None and rt_scraper is not None:
                scrape = rt_scraper.lookup(imdb_id, title, year)
                if scrape.found and scrape.rt_percent is not None:
                    rt_percent = scrape.rt_percent
                    source = "RT_SCRAPE"

            if rt_percent is None:
                stats.no_rt_data_instances += 1
                csv_rows.append([stats.display_name, title, year, imdb_id, "NO_RT_DATA", rt.error or ""])
                continue

            if rt_percent < args.rotten_threshold:
                stats.rotten_instances += 1
                stats.rotten_titles.add(f"{title} ({year})")
                csv_rows.append([stats.display_name, title, year, imdb_id, f"ROTTEN ({source})", rt_percent])
            else:
                stats.fresh_instances += 1
                csv_rows.append([stats.display_name, title, year, imdb_id, f"FRESH ({source})", rt_percent])
            continue

        if rt.rt_percent < args.rotten_threshold:
            stats.rotten_instances += 1
            stats.rotten_titles.add(f"{title} ({year})")
            csv_rows.append([stats.display_name, title, year, imdb_id, "ROTTEN", rt.rt_percent])
        else:
            stats.fresh_instances += 1
            csv_rows.append([stats.display_name, title, year, imdb_id, "FRESH", rt.rt_percent])

    # --- Report ---

    with open(args.output_csv, "w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(["drafter", "title", "year", "imdb_id", "classification", "rt_percent_or_error"])
        writer.writerows(csv_rows)

    ranked = sorted(legends.values(), key=lambda s: s.rotten_instances, reverse=True)

    print("\n" + "=" * 72)
    print(f"ROTTEN LANDED PICKS BY LEGEND (RT < {args.rotten_threshold}%, main-feed, landed only)")
    print("=" * 72)
    for stats in ranked:
        print(
            f"{stats.display_name:30s} "
            f"rotten={stats.rotten_instances:<4d} "
            f"(distinct titles={len(stats.rotten_titles):<4d}) "
            f"fresh={stats.fresh_instances:<4d} "
            f"no_rt_data={stats.no_rt_data_instances:<4d} "
            f"no_imdb={stats.no_imdb_id_instances:<4d} "
            f"total_landed={stats.landed_pick_count}"
        )

    winner = ranked[0]
    print("\n" + "-" * 72)
    print(f"WINNER (by rotten-pick instances): {winner.display_name} "
          f"with {winner.rotten_instances} Rotten landed picks "
          f"({len(winner.rotten_titles)} distinct titles)")

    by_distinct = sorted(legends.values(), key=lambda s: len(s.rotten_titles), reverse=True)[0]
    if by_distinct.drafter_id != winner.drafter_id:
        print(f"WINNER (by distinct Rotten titles): {by_distinct.display_name} "
              f"with {len(by_distinct.rotten_titles)} distinct Rotten titles")

    print(f"\nFull per-pick detail written to {args.output_csv}")


if __name__ == "__main__":
    main()