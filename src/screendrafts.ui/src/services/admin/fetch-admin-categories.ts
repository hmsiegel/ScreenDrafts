import { env } from "@/lib/env";
import { CategoryResponse } from "@/lib/dto";
import { auth } from "@/auth";

const apiBase = env.apiUrl;

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export type CategoryListItem = CategoryResponse;

export async function listAllCategories(includeDeleted = false): Promise<CategoryListItem[]> {
  const session = await auth();
  const headers = authHeaders(session?.accessToken);

  const url = new URL(`${apiBase}/categories`);
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
