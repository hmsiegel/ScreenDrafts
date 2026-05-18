import { auth } from "@/auth";
import {
  GetParticipantProfileResponse,
  PagedResultOfParticipantListItem,
  ParticipantListItem,
} from "@/lib/dto";
import { env } from "@/lib/env";

const apiBase = env.apiUrl;

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

export interface ParticipantPagedResult {
  items: ParticipantListItem[];
  total: number;
  page: number;
  pageSize: number;
}

export async function listParticipants(params: {
  q?: string;
  role?: string;
  retired?: string;
  sort?: string;
  page?: number;
  pageSize?: number;
  honorific?: string;
} = {}): Promise<ParticipantPagedResult> {
  const url = new URL(`${apiBase}/participants`);
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

  const data = (await response.json()) as { results: PagedResultOfParticipantListItem };
  const paged = data.results;
  return {
    items: paged.items ?? [],
    total: paged.totalCount ?? 0,
    page: paged.page ?? 1,
    pageSize: paged.pageSize ?? 24,
  };
}

export async function getParticipantProfile(
  personPublicId: string
): Promise<GetParticipantProfileResponse> {
  const url = `${apiBase}/participants/${personPublicId}`;

  const response = await fetch(url, {
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

  return response.json() as Promise<GetParticipantProfileResponse>;
}