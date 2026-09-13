import { env } from "@/lib/env";
import { CampaignResponse } from "@/lib/dto";
import { auth } from "@/auth";

const apiBase = env.apiUrl;

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export type CampaignListItem = CampaignResponse;

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
