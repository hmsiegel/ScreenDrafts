// services/home/fetch-home-data.ts
import { formatDraftType } from '@/lib/draft-type-display';
import { parseISO } from 'date-fns/parseISO';
import type {
  LatestDraftResponse,
  ListLatestDraftsResponse,
  UpcomingDraftResponse,
  ListUpcomingDraftsResponse,
  PredictionSeasonSummaryResponse,
  SeasonContestantStandingResponse,
  GetActiveSpotlightResponse,
  GetSiteStatsResponse
} from '@/lib/dto';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

// ── Fetchers ───────────────────────────────────────────────────────────────
//
// Each section of the home page loads independently. A failed fetch logs and returns
// an empty result ([] or null) so that one section renders its empty state instead
// of the whole page failing — the page awaits all five together.

const REVALIDATE = process.env.NODE_ENV === 'development' ? 0 : 3600;

async function getJson<T>(path: string): Promise<T | null> {
  try {
    const res = await fetch(`${API_BASE}${path}`, { next: { revalidate: REVALIDATE } });
    if (!res.ok) {
      // 404 is an expected empty state for some endpoints (e.g. no active spotlight).
      if (res.status !== 404) console.error(`[home] GET ${path} failed: ${res.status} ${res.statusText}`);
      return null;
    }
    return (await res.json()) as T;
  } catch (err) {
    console.error(`[home] GET ${path} failed:`, err);
    return null;
  }
}

export async function fetchLatestDrafts(): Promise<LatestDraftResponse[]> {
  const data = await getJson<ListLatestDraftsResponse>('/drafts/latest');
  return data?.drafts ?? [];
}

export async function fetchUpcomingDrafts(): Promise<UpcomingDraftResponse[]> {
  const data = await getJson<ListUpcomingDraftsResponse>('/drafts/upcoming');
  return data?.drafts ?? [];
}

/** Null when there is no current season or the request failed. */
export async function fetchCurrentStandings(): Promise<PredictionSeasonSummaryResponse | null> {
  return getJson<PredictionSeasonSummaryResponse>('/prediction-seasons/current');
}

/** Null when no spotlight is active or the request failed. */
export async function fetchSpotlight(): Promise<GetActiveSpotlightResponse | null> {
  return getJson<GetActiveSpotlightResponse>('/spotlight');
}

/** Null when the request failed. Also used by the drafts list banner. */
export async function fetchSiteStats(): Promise<GetSiteStatsResponse | null> {
  return getJson<GetSiteStatsResponse>('/stats');
}

// ── Mappers ────────────────────────────────────────────────────────────────

export interface MappedRecentDraft {
  publicId: string;
  number: number;
  title: string;
  drafters: string;
  date: string;
}

export interface MappedUpcomingDraft {
  draftPartPublicId: string;
  date: string;
  title: string;
  type: string;
  access: 'PUBLIC' | 'PATRON';
}

export interface MappedStanding {
  rank: number;
  name: string;
  points: number;
  carryoverPoints: number;
  totalPoints: number;
  hasCrossedTarget: boolean;
}

export interface MappedStandings {
  seasonNumber: number;
  firstEpisodeNumber: number | null;
  lastEpisodeNumber: number | null;
  targetPoints: number;
  isClosed: boolean;
  entries: MappedStanding[];
}

export interface MappedSpotlight {
  draftPublicId: string;
  episodeNumber: number;
  draftType: string;
  totalParts: number;
  totalPicks: number;
  title: string;
  spotlightDescription: string;
  spotifyUrl: string | null;
  topPicks: { position: number; mediaPublicId: string; title: string }[];
}

export interface MappedStat {
  value: string;
  label: string;
}

// parseISO, not new Date(): release dates arrive as bare "yyyy-MM-dd" strings, which
// new Date() reads as UTC midnight — a day early anywhere west of UTC. Same fix as
// drafts-sidebar.tsx's formatDate.
function formatDate(raw: Date | string | undefined): string {
  if (!raw) return 'TBA';
  try {
    const date = typeof raw === 'string' ? parseISO(raw) : raw;
    if (isNaN(date.getTime())) return 'TBA';
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  } catch {
    return 'TBA';
  }
}

function formatStatNumber(n: number | undefined): string {
  if (n == null) return '_';
  return n.toLocaleString('en-US');
}

export function mapLatestDraft(draft: LatestDraftResponse): MappedRecentDraft {
  const drafterLine = draft.participants
    ?.map((p) => p.displayName)
    .filter(Boolean)
    .join(' · ') ?? '';

  return {
    publicId: draft.draftPublicId ?? '',
    number: draft.episodeNumber ?? 0,
    title: draft.title ?? '',
    drafters: drafterLine,
    date: formatDate(draft.releaseDate),
  };
}

// Mirrors ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums.PartAccessLevel.Patreon.
// Unreleased parts (no release rows yet) are treated as PUBLIC — there's no badge for that state.
const PATREON_ACCESS_LEVEL = 1;

export function mapUpcomingDraft(draft: UpcomingDraftResponse): MappedUpcomingDraft {
  const totalParts = draft.totalParts ?? 1;
  const title = totalParts > 1
    ? `${draft.title ?? ''} (Part ${draft.partNumber ?? ''})`
    : draft.title ?? '';

  return {
    draftPartPublicId: draft.draftPartPublicId ?? '',
    date: formatDate(draft.releaseDate),
    title,
    type: draft.status?.name ?? 'Draft',
    access: draft.accessLevel?.value === PATREON_ACCESS_LEVEL ? 'PATRON' : 'PUBLIC',
  };
}

export function mapStandings(response: PredictionSeasonSummaryResponse): MappedStandings {
  const entries = (response.standings ?? [])
    .map((entry: SeasonContestantStandingResponse, index: number) => ({
      rank: index + 1,
      name: entry.displayName ?? '',
      points: entry.points ?? 0,
      carryoverPoints: entry.carryoverPoints ?? 0,
      totalPoints: entry.totalPoints ?? 0,
      hasCrossedTarget: entry.hasCrossedTarget ?? false,
    }))
    .sort((a, b) => b.totalPoints - a.totalPoints)
    .map((e, i) => ({ ...e, rank: i + 1 }));

  return {
    seasonNumber: response.number ?? 0,
    firstEpisodeNumber: response.firstEpisodeNumber ?? null,
    lastEpisodeNumber: response.lastEpisodeNumber ?? null,
    targetPoints: response.targetPoints ?? 100,
    isClosed: response.isClosed ?? false,
    entries,
  };
}

export function mapSpotlight(response: GetActiveSpotlightResponse): MappedSpotlight {
  return {
    draftPublicId: response.draftPublicId ?? '',
    episodeNumber: response.episodeNumber ?? 0,
    draftType: formatDraftType(response.draftType),
    totalParts: response.totalParts ?? 1,
    totalPicks: response.totalPicks ?? 7,
    title: response.title ?? '',
    spotlightDescription: response.spotlightDescription ?? '',
    spotifyUrl: response.spotifyUrl ?? null,
    topPicks: (response.topPicks ?? []).map((p) => ({
      position: p.position ?? 0,
      mediaPublicId: p.mediaPublicId ?? '',
      title: p.mediaTitle ?? '',
    })),
  };
}

export function mapSiteStats(response: GetSiteStatsResponse): MappedStat[] {
  return [
    { value: formatStatNumber(response.episodesProduced), label: 'EPISODES PRODUCED' },
    { value: formatStatNumber(response.filmsDrafted), label: 'FILMS DRAFTED' },
    { value: formatStatNumber(response.guestGMs), label: 'GUEST G.M.s' },
    { value: formatStatNumber(response.vetoesDeployed), label: 'VETOES DEPLOYED' },
    { value: formatStatNumber(response.legends), label: 'LEGENDS' },
  ];
}