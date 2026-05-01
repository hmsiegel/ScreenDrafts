import type {
  LatestDraftResponse,
  ListLatestDraftsResponse,
  UpcomingDraftResponse,
  ListUpcomingDraftsResponse,
  PredictionSeasonSummaryResponse,
  SeasonContestantStandingResponse
} from '@/lib/dto';
import exp from 'constants';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

// ── Fallbacks ──────────────────────────────────────────────────────────────

const FALLBACK_LATEST: LatestDraftResponse[] = [
  { episodeNumber: 307, title: '1999 mini-Mega', participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Nick' }] },
  { episodeNumber: 306, title: 'Whit Stillman mini-Super', participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Matt Z. Seitz' }] },
  { episodeNumber: 305, title: 'Charlie Kaufman Super', participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Griffin Newman' }] },
  { episodeNumber: 304, title: 'Holiday Horror', participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Emily Edwards' }] },
  { episodeNumber: 303, title: 'François Truffaut Super', participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Bilge Ebiri' }] },
];

const FALLBACK_UPCOMING: UpcomingDraftResponse[] = [
  { title: 'A24 Mega-Draft' },
  { title: 'Patreon: Hidden Gems Vol. IV' },
  { title: 'Christopher Guest Super' },
  { title: 'Legends Invitational 2026' },
];

const FALLBACK_STANDINGS: PredictionSeasonSummaryResponse = {
  number: 3,
  firstEpisodeNumber: 34,
  lastEpisodeNumber: 65,
  targetPoints: 100,
  isClosed: false,
  publicId: '',
  startDate: new Date(),
  endDate: undefined,
  standings: [
    { contestantPublicId: '', displayName: 'Clay', points: 91, hasCrossedTarget: false },
    { contestantPublicId: '', displayName: 'Ryan', points: 85, hasCrossedTarget: false },
  ],
}

// ── Fetchers ───────────────────────────────────────────────────────────────

export async function fetchLatestDrafts(): Promise<LatestDraftResponse[]> {
  try {
    const res = await fetch(`${API_BASE}/drafts/latest`, {
      next: { revalidate: process.env.NODE_ENV === 'development' ? 0 : 3600 },
    });
    if (!res.ok) return FALLBACK_LATEST;
    const data = await res.json() as ListLatestDraftsResponse;
    return data.drafts ?? FALLBACK_LATEST;
  } catch {
    return FALLBACK_LATEST;
  }
}

export async function fetchUpcomingDrafts(): Promise<UpcomingDraftResponse[]> {
  try {
    const res = await fetch(`${API_BASE}/drafts/upcoming`, {
      next: { revalidate: process.env.NODE_ENV === 'development' ? 0 : 3600 },
    });
    if (!res.ok) return FALLBACK_UPCOMING;
    const data = await res.json() as ListUpcomingDraftsResponse;
    return data.drafts ?? FALLBACK_UPCOMING;
  } catch (e) {
    return FALLBACK_UPCOMING;
  }
}

export async function fetchCurrentStandings(): Promise<PredictionSeasonSummaryResponse> {
  try {
    const res = await fetch(`${API_BASE}/prediction-seasons/current`, {
      next: { revalidate: process.env.NODE_ENV === 'development' ? 0 : 3600 },
    });
    if (!res.ok) return FALLBACK_STANDINGS;
    return await res.json() as PredictionSeasonSummaryResponse;
  } catch {
    return FALLBACK_STANDINGS;
  }
}

// ── Mappers ────────────────────────────────────────────────────────────────

export interface MappedRecentDraft {
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

function formatDate(raw: Date | string | undefined): string {
  if (!raw) return 'TBA';
  try {
    return new Date(raw).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  } catch {
    return 'TBA';
  }
}

export function mapLatestDraft(draft: LatestDraftResponse): MappedRecentDraft {
  const drafterLine = draft.participants
    ?.map((p) => p.displayName)
    .filter(Boolean)
    .join(' · ') ?? '';

  return {
    number: draft.episodeNumber ?? 0,
    title: draft.title ?? '',
    drafters: drafterLine,
    date: formatDate(draft.releaseDate),
  };
}

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
    // TODO: wire access level when backend adds it to UpcomingDraftResponse
    access: 'PUBLIC',
  };
}

export function mapStandings(response: PredictionSeasonSummaryResponse): MappedStandings {
  const entries = (response.standings ?? [])
    .map((entry: SeasonContestantStandingResponse, index: number) => ({
      rank: index + 1,
      name: entry.displayName ?? '',
      points: entry.points ?? 0,
      hasCrossedTarget: entry.hasCrossedTarget ?? false,
    }));

  return {
    seasonNumber: response.number ?? 0,
    firstEpisodeNumber: response.firstEpisodeNumber ?? null,
    lastEpisodeNumber: response.lastEpisodeNumber ?? null,
    targetPoints: response.targetPoints ?? 100,
    isClosed: response.isClosed ?? false,
    entries,
  };
}