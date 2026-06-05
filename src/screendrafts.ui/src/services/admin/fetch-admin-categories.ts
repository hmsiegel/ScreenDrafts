import { env } from "@/lib/env";
import { CategoryResponse } from "@/lib/dto";
import { auth } from "@/auth";

const apiBase = env.apiUrl;

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export type CategoryListItem = CategoryResponse;

export interface CreateCategoryRequest {
  name: string;
  description: string;
}

export interface UpdateCategoryRequest {
  name?: string;
  description?: string;
}

export async function listAllCategories(): Promise<CategoryListItem[]> {
  const session = await auth();
  const res = await fetch(`${apiBase}/categories`, {
    headers: authHeaders(session?.accessToken),
    cache: "no-store",
  });
  if (!res.ok) return [];
  const data = await res.json();
  return data.items ?? data ?? [];
}

export async function createCategory(
  data: CreateCategoryRequest,
  accessToken: string | undefined,
): Promise<{ publicId: string }> {
  const res = await fetch(`${apiBase}/categories`, {
    method: "POST",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw await res.json();
  return res.json();
}

export async function updateCategory(
  publicId: string,
  data: UpdateCategoryRequest,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/categories/${publicId}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify(data),
  });
  if (!res.ok) throw await res.json();
}

export async function retireCategory(
  publicId: string,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/categories/${publicId}`, {
    method: "DELETE",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify({}),
  });
  if (!res.ok) throw await res.json();
}

export async function restoreCategory(
  publicId: string,
  accessToken: string | undefined,
): Promise<void> {
  const res = await fetch(`${apiBase}/categories/${publicId}/restore`, {
    method: "POST",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify({}),
  });
  if (!res.ok) throw await res.json();
}
