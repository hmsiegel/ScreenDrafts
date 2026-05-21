import { auth } from "@/auth";
import { env } from "@/lib/env";
import {
  PagedResultOfMediaListItemResponse,
} from "@/lib/dto";

const apiBase = env.apiUrl;

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

const EMPTY_RESULT: PagedResultOfMediaListItemResponse = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 50,
  totalPages: 0,
};

export async function fetchMedia(params: {
  page?: number;
  pageSize?: number;
  search?: string;
  mediaType?: number;   // integer: 0=Movie 1=TvShow 2=TvEpisode 3=VideoGame 4=MusicVideo
  year?: string;
  sort?: string;
} = {}): Promise<PagedResultOfMediaListItemResponse> {
  const url = new URL(`${apiBase}/media`);
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      url.searchParams.set(key, String(value));
    }
  });

  try {
    const response = await fetch(url.toString(), {
      method: "GET",
      headers: await authHeaders(),
      next: { revalidate: 0 },
    });

    if (!response.ok) {
      console.error(`[fetchMedia] ${response.status} ${response.statusText} — ${url}`);
      return EMPTY_RESULT;
    }

    // ListMediaResponse wraps the paged result: { result: { items, totalCount, ... } }
    const body = await response.json() as { result: PagedResultOfMediaListItemResponse };
    return body.result ?? EMPTY_RESULT;
  } catch (err) {
    console.error("[fetchMedia] Network error:", err);
    return EMPTY_RESULT;
  }
}