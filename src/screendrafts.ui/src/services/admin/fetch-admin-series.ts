import { env } from "@/lib/env";
import { auth } from "@/auth";
import { SeriesResponse } from "@/lib/dto";

const apiBase = env.apiUrl;

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export type SeriesListItem = SeriesResponse & { isDeleted?: boolean };

export async function listAllSeries(includeDeleted = false): Promise<SeriesListItem[]> {
  const session = await auth();
  const headers = authHeaders(session?.accessToken);

  const url = new URL(`${apiBase}/series`);
  if (includeDeleted) {
    url.searchParams.set("includeDeleted", "true");
  }

  let res = await fetch(url.toString(), { headers, cache: "no-store" });

  if (res.status === 403 && includeDeleted) {
    url.searchParams.delete("includeDeleted");
    res = await fetch(url.toString(), { headers, cache: "no-store" });
  }

  if (!res.ok) return [];
  const data = await res.json();
  return data.items ?? data ?? [];
}
