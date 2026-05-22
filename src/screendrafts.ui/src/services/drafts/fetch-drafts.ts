import { auth } from "@/auth";
import { DraftPartPredictionResponse, GetDraftResponse, GetTriviaResultsResponse, ListDraftsHostResponse, ListDraftsResponse, PagedResultOfListDraftsResponse, PredictionStandingsResponse } from "@/lib/dto";
import { env } from "@/lib/env";
import { PagedResult, toPagedDraftResult } from "@/types/paged-result";

const apiBase = env.apiUrl;

// ── Fallbacks ──────────────────────────────────────────────────────────────

const FALLBACK_HOSTS: ListDraftsHostResponse[] = [
  { displayName: "Clay Keller", hostPublicId: '', role: { name: "Primary", value: 0 } },
  { displayName: "Ryan Marker", hostPublicId: '', role: { name: "Co-Host", value: 1 } }
];

const FALLBACK_DRAFTS_LIST: ListDraftsResponse[] = [
  {
    draftPartPublicId: '',
    draftPublicId: '',
    draftStatus: {
      value: 4,
      name: "Completed"
    },
    draftType: 2,
    hasCommunityParticipant: false,
    hosts: [
      { displayName: "Clay Keller", hostPublicId: '', role: { name: "Primary", value: 0 } }
    ],
    label: "2025 Mega Draft",
    partStatus: {
      name: "Completed",
      value: 4
    },
    releases: [
      {
        episodeNumber: 368,
        releaseChannel: {
          name: "Main Feed",
          value: 0
        },
        releaseDate: new Date("Mar 9, 2026")
      }
    ],
    totalPicks: 20,
    participants: [
      { displayName: "Ryan Marker", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      { displayName: "Drea Clark", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      { displayName: "Katie Walsh", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      { displayName: "Daniel Waters", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
    ],
  },
  {
    draftPartPublicId: '',
    draftPublicId: '',
    draftStatus: {
      value: 4,
      name: "Completed"
    },
    draftType: 1,
    hasCommunityParticipant: false,
    hosts: [
      { displayName: "Ryan Marker", hostPublicId: '', role: { name: "Primary", value: 0 } },
      { displayName: "Adam B. Vary", hostPublicId: '', role: { name: "Co-Host", value: 1 } },
      { displayName: "Jim Laczkowski", hostPublicId: '', role: { name: "Co-Host", value: 1 } }
    ],
    label: "Holly Hunter mini-Mega",
    partStatus: {
      name: "Completed",
      value: 4
    },
    releases: [
      {
        episodeNumber: 369,
        releaseChannel: {
          name: "Main Feed",
          value: 0
        },
        releaseDate: new Date("Mar 16, 2026")
      }
    ],
    totalPicks: 9,
    participants: [
      { displayName: "Clay Keller", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      { displayName: "Phil Iscove", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
    ],
  },
  {
    draftPartPublicId: '',
    draftPublicId: '',
    draftStatus: {
      value: 4,
      name: "Completed"
    },
    draftType: 0,
    hasCommunityParticipant: false,
    hosts: FALLBACK_HOSTS,
    label: "George A. Romero",
    partStatus: {
      name: "Completed",
      value: 4
    },
    releases: [
      {
        episodeNumber: 370,
        releaseChannel: {
          name: "Main Feed",
          value: 0
        },
        releaseDate: new Date("Mar 23, 2026")
      }
    ],
    totalPicks: 7,
    participants: [
      { displayName: "Daniel Kraus", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      { displayName: "Greg Nicotero", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
    ],
  },
  {
    draftPartPublicId: '',
    draftPublicId: '',
    draftStatus: {
      value: 4,
      name: "Completed"
    },
    draftType: 0,
    hasCommunityParticipant: false,
    hosts: FALLBACK_HOSTS,
    label: "Nuns",
    partStatus: {
      name: "Completed",
      value: 4
    },
    releases: [
      {
        episodeNumber: 371,
        releaseChannel: {
          name: "Main Feed",
          value: 0
        },
        releaseDate: new Date("Mar 31, 2026")
      }
    ],
    totalPicks: 7,
    participants: [
      { displayName: "Rosalie Kicks", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      { displayName: "Ryan Silberstein", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
    ],
  },
  {
    draftPartPublicId: '',
    draftPublicId: '',
    draftStatus: {
      value: 4,
      name: "Completed"
    },
    draftType: 0,
    hasCommunityParticipant: false,
    hosts: FALLBACK_HOSTS,
    label: "Diane Keaton",
    partStatus: {
      name: "Completed",
      value: 4
    },
    releases: [
      {
        episodeNumber: 372,
        releaseChannel: {
          name: "Main Feed",
          value: 0
        },
        releaseDate: new Date("Apr 6, 2026")
      }
    ],
    totalPicks: 7,
    participants: [
      { displayName: "Bryan Cogman", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      { displayName: "Oriana Nudo", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
    ],
  },
  {
    draftPartPublicId: '',
    draftPublicId: '',
    draftStatus: {
      value: 3,
      name: "Paused"
    },
    draftType: 1,
    hasCommunityParticipant: false,
    hosts: [
      { displayName: "Clay Keller", hostPublicId: '', role: { name: "Primary", value: 0 } },
      { displayName: "Alonso Duralde", hostPublicId: '', role: { name: "Co-Host", value: 1 } },
    ],
    label: "Robert Altman Super Draft Part II",
    partStatus: {
      name: "Completed",
      value: 4
    },
    releases: [
      {
        episodeNumber: 354,
        releaseChannel: {
          name: "Main Feed",
          value: 0
        },
        releaseDate: new Date("Apr 14, 2026")
      }
    ],
    totalPicks: 7,
    participants: [
      { displayName: "Ryan Marker", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      { displayName: "Walter Chaw", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
    ],
  },
];

const FALLBACK_DRAFT_RESPONSE: GetDraftResponse = {
  publicId: '',
  seriesName: "Regular",
  seriesPublicId: '',
  title: "2025 Mega Draft",
  campaignName: '',
  campaignPublicId: '',
  description: "2025 Mega Draft is the 368th episode of Screen Drafts. Co-commissioner Ryan Marker was joined by Guest G.M.s Daniel Waters, Drea Clark and Katie Walsh to rank the 20 best movies of 2025.",
  draftStatus: {
    name: "Completed",
    value: 4
  },
  draftType: {
    name: "Mega",
    value: 2
  },
  parts: [
    {
      publicId: '',
      primaryHost: {
        displayName: "Clay Keller",
        hostPublicId: '',
      },
      coHosts: [

      ],
      draftType: {
        name: "Mega",
        value: 2
      },
      partIndex: 1,
      releases: [
        {
          releaseChannel: {
            name: "Main Feed",
            value: 0
          },
          releaseDate: new Date("Mar 9, 2026"),
        },
      ],
      status: {
        name: "Completed",
        value: 3
      },
      picks: [
        { moviePublicId: '', movieTitle: "Wake Up Dead Man: A Knives Out Mystery", actedByPublicId: '', playOrder: 1, position: 20, playedByParticipantIdValue: "Daniel Waters", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "The Baltimorons", actedByPublicId: '', playOrder: 2, position: 19, playedByParticipantIdValue: "Katie Walsh", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Splitsville", actedByPublicId: '', playOrder: 4, position: 18, playedByParticipantIdValue: "Katie Walsh", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "The Ballad of Wallis Island", actedByPublicId: '', playOrder: 3, position: 17, playedByParticipantIdValue: "Drea Clark", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Lurker", actedByPublicId: '', playOrder: 4, position: 16, playedByParticipantIdValue: "Daniel Waters", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "The Testament of Ann Lee", actedByPublicId: '', playOrder: 5, position: 15, playedByParticipantIdValue: "Katie Walsh", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Superman", actedByPublicId: '', playOrder: 6, position: 14, playedByParticipantIdValue: "Ryan Marker", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Sorry, Baby", actedByPublicId: '', playOrder: 7, position: 13, playedByParticipantIdValue: "Drea Clark", playedByParticipantKindValue: { name: "Drafter", value: 0 }, veto: { issuedByParticipantId: "Katie Walsh", } },
        { moviePublicId: '', movieTitle: "If I Had Legs I'd Kick You", actedByPublicId: '', playOrder: 8, position: 13, playedByParticipantIdValue: "Drea Clark", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Twinless", actedByPublicId: '', playOrder: 9, position: 12, playedByParticipantIdValue: "Daniel Waters", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "No Other Choice", actedByPublicId: '', playOrder: 10, position: 11, playedByParticipantIdValue: "Katie Walsh", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Cover-Up", actedByPublicId: '', playOrder: 11, position: 10, playedByParticipantIdValue: "Ryan Marker", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Hamnet", actedByPublicId: '', playOrder: 12, position: 9, playedByParticipantIdValue: "Drea Clark", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Sirat", actedByPublicId: '', playOrder: 13, position: 8, playedByParticipantIdValue: "Daniel Waters", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Sorry, Baby", actedByPublicId: '', playOrder: 14, position: 7, playedByParticipantIdValue: "Katie Walsh", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Blue Moon", actedByPublicId: '', playOrder: 15, position: 6, playedByParticipantIdValue: "Ryan Marker", playedByParticipantKindValue: { name: "Drafter", value: 0 }, veto: { issuedByParticipantId: "Katie Walsh", isOverriden: true, override: { issuedByParticipantId: "Daniel Waters" } } },
        { moviePublicId: '', movieTitle: "Sentimental Value", actedByPublicId: '', playOrder: 16, position: 5, playedByParticipantIdValue: "Drea Clark", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Train Dreams", actedByPublicId: '', playOrder: 17, position: 4, playedByParticipantIdValue: "Drea Clark", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "Bugonia", actedByPublicId: '', playOrder: 18, position: 3, playedByParticipantIdValue: "Daniel Waters", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "The Secret Agent", actedByPublicId: '', playOrder: 19, position: 2, playedByParticipantIdValue: "Katie Walsh", playedByParticipantKindValue: { name: "Drafter", value: 0 }, veto: { issuedByParticipantId: "Drea Clark" } },
        { moviePublicId: '', movieTitle: "Sinners", actedByPublicId: '', playOrder: 20, position: 2, playedByParticipantIdValue: "Katie Walsh", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
        { moviePublicId: '', movieTitle: "One Battle After Another", actedByPublicId: '', playOrder: 21, position: 1, playedByParticipantIdValue: "Ryan Marker", playedByParticipantKindValue: { name: "Drafter", value: 0 } },
      ],
      participants: [
        { displayName: "Ryan Marker", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
        { displayName: "Drea Clark", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
        { displayName: "Katie Walsh", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
        { displayName: "Daniel Waters", participantIdValue: '', participantKindValue: { name: "Drafter", value: 0 } },
      ]
    }
  ],
  nextDraftPublicId: '',
  nextDraftTitle: "Holly Hunter mini-Mega",
  previousDraftPublicId: '',
  previousDraftTitle: "1980 mini-Mega"
}

const FALLBACK_PAGED_DRAFTS: PagedResultOfListDraftsResponse = {
  items: FALLBACK_DRAFTS_LIST,
  page: 1,
  pageSize: 5,
  totalCount: 6,
}

// ── Fetchers ──────────────────────────────────────────────────────────────

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

export async function listDrafts(params: {
  fromDate?: string;
  toDate?: string;
  minDrafters?: number;
  maxDrafters?: number;
  minPicks?: number;
  maxPicks?: number;
  draftType?: number;
  sort?: string;
  dir?: "asc" | "desc";
  q?: string;
  campaignPublicId?: string;
  page?: number;
  pageSize?: number;
} = {}): Promise<PagedResult<ListDraftsResponse>> {
  try {
    const url = new URL(`${apiBase}/drafts`);

    const paramMap: Record<string, string> = {
      campaignPublicId: "campaignPublicId",
      sort: "sortBy",
      dir: "dir",
    }


    Object.entries(params).forEach(([key, value]) => {
      if (value === undefined || value === null || value === "") return;
      const backendKey = paramMap[key] ?? key;
      url.searchParams.set(backendKey, String(value));
    });

    const response = await fetch(url, {
      method: "GET",
      headers: await authHeaders(),
      credentials: "include",
      next: { revalidate: 0 },
    });

    if (!response.ok) return toPagedDraftResult(FALLBACK_PAGED_DRAFTS);
    const data = await response.json();
    return toPagedDraftResult(data as PagedResultOfListDraftsResponse) ?? toPagedDraftResult(FALLBACK_PAGED_DRAFTS);
  } catch {
    return toPagedDraftResult(FALLBACK_PAGED_DRAFTS);
  }
}

export async function getDraftDetails(id: string): Promise<GetDraftResponse> {
  try {
    const url = `${apiBase}/drafts/${id}`;

    const response = await fetch(url, {
      method: "GET",
      headers: await authHeaders(),
      credentials: "include",
      next: { revalidate: 0 },
    });

    if (!response.ok) return FALLBACK_DRAFT_RESPONSE;
    return response.json() as Promise<GetDraftResponse> ?? FALLBACK_DRAFT_RESPONSE;
  } catch {
    return FALLBACK_DRAFT_RESPONSE;
  }
}

export async function getDraftPartTriviaResults(
  draftPartPublicId: string
): Promise<GetTriviaResultsResponse> {
  const empty: GetTriviaResultsResponse = { draftPartId: "", results: [] };
  try {
    const response = await fetch(
      `${apiBase}/draft-parts/${draftPartPublicId}/trivia-results`,
      { next: { revalidate: 0 } }
    );
    if (!response.ok) return empty;
    return (await response.json()) as GetTriviaResultsResponse;
  } catch {
    return empty;
  }
}

export async function getDraftPartPredictions(
  draftPartPublicId: string
): Promise<DraftPartPredictionResponse[]> {
  try {
    const response = await fetch(
      `${apiBase}/draft-parts/${draftPartPublicId}/predictions`,
      { next: { revalidate: 0 } }
    );
    if (!response.ok) return [];
    return (await response.json()) as DraftPartPredictionResponse[];
  } catch {
    return [];
  }
}

export async function getPredictionStandings(
  seasonPublicId: string,
  asOfDraftPartId: string
): Promise<PredictionStandingsResponse | null> {
  try {
    const url = `${apiBase}/prediction-seasons/${seasonPublicId}/standings?asOfDraftPartId=${encodeURIComponent(asOfDraftPartId)}`;
    const response = await fetch(
      url,
      { next: { revalidate: 0 } }
    );
    if (!response.ok) return null;
    return (await response.json()) as PredictionStandingsResponse;
  } catch {
    return null;
  }
}