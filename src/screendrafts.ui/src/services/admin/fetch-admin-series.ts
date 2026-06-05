import { env } from "@/lib/env";
import { SeriesResponse } from "@/lib/dto";
import { auth } from "@/auth";

const apiBase = env.apiUrl;

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export type SeriesListItem = SeriesResponse;

export interface CreateSeriesRequest {
  name: string;
  description?: string;
}

export interface UpdateSeriesRequest {
  name?: string;
  description?: string;
}

export async function listAllSeries(): Promise<SeriesListItem[]> {
  const session = await auth();
  // TODO: backend SearchSeries not yet implemented — stub returns [] until endpoint is available
  const res = await fetch(`${apiBase}/series`, {
    headers: authHeaders(session?.accessToken),
    cache: "no-store",
  });
  if (!res.ok) return [];
  const data = await res.json();
  return data.items ?? data ?? [];
}

export async function createSeries(
  data: CreateSeriesRequest,
  accessToken: string | undefined,
): Promise<{ publicId: string }> {
  const res = await fetch(`${apiBase}/series`, {
    method: "POST",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw await res.json();
  return res.json();
}

export async function updateSeries(
  publicId: string,
  data: UpdateSeriesRequest,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/series/${publicId}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw await res.json();
}

export async function retireSeries(
  publicId: string,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/series/${publicId}`, {
    method: "DELETE",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify({}),
  });
  if (!res.ok) throw await res.json();
}

export async function restoreSeries(
  publicId: string,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/series/${publicId}/restore`, {
    method: "POST",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify({}),
  });
  if (!res.ok) throw await res.json();
}
