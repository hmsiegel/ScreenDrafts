import { auth } from "@/auth";
import { env } from "@/lib/env";

const apiBase = env.apiUrl;

// Local types — will move to dto.ts after NSwag regeneration when backend adds these endpoints.
export interface AdminUserItem {
  publicId: string;
  displayName: string;
  email: string;
  roles: string[];
}

export interface AdminRoleItem {
  name: string;
  description?: string;
}

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

export async function fetchAdminUsers(search?: string): Promise<AdminUserItem[]> {
  try {
    const url = new URL(`${apiBase}/admin/users`);
    if (search) url.searchParams.set("search", search);
    const response = await fetch(url.toString(), {
      headers: await authHeaders(),
      next: { revalidate: 0 },
    });
    if (!response.ok) {
      console.error(`[fetchAdminUsers] ${response.status}`);
      return [];
    }
    return response.json() as Promise<AdminUserItem[]>;
  } catch (err) {
    console.error("[fetchAdminUsers] error:", err);
    return [];
  }
}

export async function fetchAdminRoles(): Promise<AdminRoleItem[]> {
  try {
    const response = await fetch(`${apiBase}/admin/roles`, {
      headers: await authHeaders(),
      next: { revalidate: 0 },
    });
    if (!response.ok) {
      console.error(`[fetchAdminRoles] ${response.status}`);
      return [];
    }
    return response.json() as Promise<AdminRoleItem[]>;
  } catch (err) {
    console.error("[fetchAdminRoles] error:", err);
    return [];
  }
}

export async function fetchRolePermissions(roleName: string): Promise<string[]> {
  try {
    const response = await fetch(
      `${apiBase}/admin/roles/${encodeURIComponent(roleName)}/permissions`,
      {
        headers: await authHeaders(),
        next: { revalidate: 0 },
      }
    );
    if (!response.ok) {
      console.error(`[fetchRolePermissions] ${response.status}`);
      return [];
    }
    const data = await response.json() as { permissions?: string[] };
    return data.permissions ?? [];
  } catch (err) {
    console.error("[fetchRolePermissions] error:", err);
    return [];
  }
}
