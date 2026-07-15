import { env } from "@/lib/env";
import { CampaignResponse } from "@/lib/dto";
import { auth } from "@/auth";

const apiBase = env.apiUrl;

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export type CampaignListItem = CampaignResponse;

export interface CreateCampaignRequest {
  name: string;
  slug: string;
}

export interface UpdateCampaignRequest {
  name?: string;
  slug?: string;
}

export async function listAllCampaigns(includeDeleted = false): Promise<CampaignListItem[]> {
  const session = await auth();
  const headers = authHeaders(session?.accessToken);

  const url = new URL(`${apiBase}/campaigns`);
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

export async function createCampaign(
  data: CreateCampaignRequest,
  accessToken: string | undefined,
): Promise<{ publicId: string }> {
  const res = await fetch(`${apiBase}/campaigns`, {
    method: "POST",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw await res.json();
  return res.json();
}

export async function updateCampaign(
  publicId: string,
  data: UpdateCampaignRequest,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/campaigns/${publicId}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw await res.json();
}

export async function retireCampaign(
  publicId: string,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/campaigns/${publicId}`, {
    method: "DELETE",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify({}),
  });
  if (!res.ok) throw await res.json();
}

export async function restoreCampaign(
  publicId: string,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/campaigns/${publicId}/restore`, {
    method: "POST",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify({}),
  });
  if (!res.ok) throw await res.json();
}
