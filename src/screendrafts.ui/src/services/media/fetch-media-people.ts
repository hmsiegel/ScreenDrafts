import { PagedResultOfMediaPersonListItemResponse } from "@/lib/dto";
import { env } from "@/lib/env";

const apiBase = env.apiUrl;

const EMPTY_RESULT: PagedResultOfMediaPersonListItemResponse = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 48,
  totalPages: 0,
};

/** One page of the cast & crew list. `role` is actor, director, writer or producer; omit for all. */
export async function fetchMediaPeople(
  params: {
    page?: number;
    pageSize?: number;
    search?: string;
    role?: string;
    sort?: string;
  } = {}
): Promise<PagedResultOfMediaPersonListItemResponse> {
  const url = new URL(`${apiBase}/media/people/browse`);
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      url.searchParams.set(key, String(value));
    }
  });

  try {
    const response = await fetch(url.toString(), { method: "GET", next: { revalidate: 0 } });

    if (!response.ok) {
      console.error(`[fetchMediaPeople] ${response.status} ${response.statusText} — ${url}`);
      return EMPTY_RESULT;
    }

    const body = (await response.json()) as { result: PagedResultOfMediaPersonListItemResponse };
    return body.result ?? EMPTY_RESULT;
  } catch (err) {
    console.error("[fetchMediaPeople] Network error:", err);
    return EMPTY_RESULT;
  }
}
