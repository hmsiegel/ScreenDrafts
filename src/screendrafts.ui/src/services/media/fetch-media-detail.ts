import { auth } from "@/auth";
import { env } from "@/lib/env";
import { MediaDetail } from "@/lib/media-dto";

const apiBase = env.apiUrl;

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

export async function fetchMediaDetail(publicId: string): Promise<MediaDetail> {
  const url = `${apiBase}/media/${publicId}`;

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

  return response.json() as Promise<MediaDetail>;
}
