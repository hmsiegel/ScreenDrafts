// src/services/drafts/fetch-categories.ts
import { auth } from "@/auth";
import { env } from "@/lib/env";

interface CategoryOption {
  publicId: string;
  name: string;
}

export async function listCategories(): Promise<CategoryOption[]> {
  const session = await auth();
  const headers: HeadersInit = {};
  if (session?.accessToken) headers["Authorization"] = `Bearer ${session.accessToken}`;
  try {
    const response = await fetch(`${env.apiUrl}/categories`, {
      headers,
      next: { revalidate: 3600 },
    });
    if (!response.ok) return [];
    const data = await response.json();
    return (data.items ?? []).filter((c: CategoryOption & { isDeleted?: boolean }) => !c.isDeleted);
  } catch {
    return [];
  }
}