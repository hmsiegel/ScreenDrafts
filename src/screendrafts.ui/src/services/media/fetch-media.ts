import { auth } from "@/auth";
import { env } from "@/lib/env";
import { MediaPagedResult } from "@/lib/media-dto";

const apiBase = env.apiUrl;

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

export async function fetchMedia(params: {
  page?: number;
  pageSize?: number;
  search?: string;
  mediaType?: string;
  sort?: string;
} = {}): Promise<MediaPagedResult> {
  const url = new URL(`${apiBase}/media`);
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      url.searchParams.set(key, String(value));
    }
  });

  const response = await fetch(url.toString(), {
    method: "GET",
    headers: await authHeaders(),
    credentials: "include",
    next: { revalidate: 0 },
  });

  if (!response.ok) {
    const body = await response.text();
    throw new Error(
      `Request failed with status ${response.status}: ${response.statusText} - ${body}`
    );
  }

  return response.json() as Promise<MediaPagedResult>;
}
