import { auth } from "@/auth";
import { env } from "@/lib/env";
import { PagedResultOfMediaListItemResponse } from "@/lib/dto";

const apiBase = env.apiUrl;

const EMPTY_RESULT: PagedResultOfMediaListItemResponse = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 50,
  totalPages: 0,
};

export async function fetchMedia(
  params: {
    page?: number;
    pageSize?: number;
    search?: string;
    mediaType?: number;
    year?: string;
    sort?: string;
  } = {},
  accessToken?: string
): Promise<PagedResultOfMediaListItemResponse> {
  // If no token supplied, try to get one server-side via auth()
  let token = accessToken;
  if (!token) {
    try {
      const session = await auth();
      token = session?.accessToken;
    } catch {
      // Running client-side — no session available, proceed without token
    }
  }

  const headers: HeadersInit = token ? { Authorization: `Bearer ${token}` } : {};

  const url = new URL(`${apiBase}/media`);
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      url.searchParams.set(key, String(value));
    }
  });

  try {
    const response = await fetch(url.toString(), {
      method: "GET",
      headers,
      next: { revalidate: 0 },
    });

    if (!response.ok) {
      console.error(`[fetchMedia] ${response.status} ${response.statusText} — ${url}`);
      return EMPTY_RESULT;
    }

    const body = (await response.json()) as { result: PagedResultOfMediaListItemResponse };
    return body.result ?? EMPTY_RESULT;
  } catch (err) {
    console.error("[fetchMedia] Network error:", err);
    return EMPTY_RESULT;
  }
}