import { MediaPersonProfileResponse } from "@/lib/dto";
import { env } from "@/lib/env";

const apiBase = env.apiUrl;

/**
 * A cast or crew member's profile and Vault credits.
 * `id` is a TMDb person id ("31644") or an IMDb person id ("nm0000229").
 * Returns null when the person is not in the Vault.
 */
export async function fetchMediaPerson(id: string): Promise<MediaPersonProfileResponse | null> {
  const response = await fetch(`${apiBase}/media/people/${encodeURIComponent(id)}`, {
    method: "GET",
    next: { revalidate: 0 },
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    const body = await response.text();
    throw new Error(
      `Request failed with status ${response.status}: ${response.statusText} - ${body}`
    );
  }

  return response.json() as Promise<MediaPersonProfileResponse>;
}
