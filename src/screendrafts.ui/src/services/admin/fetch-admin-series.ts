import { env } from "@/lib/env";
import { auth } from "@/auth";
import { SeriesResponse } from "@/lib/dto";

const apiBase = env.apiUrl;

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export type SeriesListItem = SeriesResponse & { isDeleted?: boolean };

export interface CreateSeriesRequest {
  name: string;
  description?: string | null;
  kind: number;
  canonicalPolicy: number;
  continuityScope: number;
  continuityDateRule: number;
  allowedDraftTypes: number;   // bitmask integer
  defaultDraftType: number | null;
}

export type UpdateSeriesRequest = CreateSeriesRequest;

export async function listAllSeries(): Promise<SeriesListItem[]> {
  const session = await auth();
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