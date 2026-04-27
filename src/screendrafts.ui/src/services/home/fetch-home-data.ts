import type {
  LatestDraftResponse,
  ListLatestDraftsResponse,
  UpcomingDraftResponse,
  ListUpcomingDraftsResponse,
} from '@/lib/dto';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

// ── Fallbacks ──────────────────────────────────────────────────────────────

const FALLBACK_LATEST: LatestDraftResponse[] = [
  { episodeNumber: 307, title: '1999 mini-Mega',           participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Nick' }] },
  { episodeNumber: 306, title: 'Whit Stillman mini-Super', participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Matt Z. Seitz' }] },
  { episodeNumber: 305, title: 'Charlie Kaufman Super',    participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Griffin Newman' }] },
  { episodeNumber: 304, title: 'Holiday Horror',           participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Emily Edwards' }] },
  { episodeNumber: 303, title: 'François Truffaut Super',  participants: [{ displayName: 'Clay' }, { displayName: 'Ryan' }, { displayName: 'Bilge Ebiri' }] },
];

const FALLBACK_UPCOMING: UpcomingDraftResponse[] = [
  { title: 'A24 Mega-Draft' },
  { title: 'Patreon: Hidden Gems Vol. IV' },
  { title: 'Christopher Guest Super' },
  { title: 'Legends Invitational 2026' },
];

// ── Fetchers ───────────────────────────────────────────────────────────────

export async function fetchLatestDrafts(): Promise<LatestDraftResponse[]> {
  try {
    const res = await fetch(`${API_BASE}/drafts/latest`, {
      next: { revalidate: 3600 },
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
      next: { revalidate: 3600 },
    });
    if (!res.ok) return FALLBACK_UPCOMING;
    const data = await res.json() as ListUpcomingDraftsResponse;
    return data.drafts ?? FALLBACK_UPCOMING;
  } catch {
    return FALLBACK_UPCOMING;
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
  date: string;
  title: string;
  type: string;
  access: 'PUBLIC' | 'PATRON';
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
  return {
    date: formatDate(draft.releaseDate),
    title: draft.title ?? '',
    type: draft.status?.name ?? 'Draft',
    // TODO: wire access level when backend adds it to UpcomingDraftResponse
    access: 'PUBLIC',
  };
}
