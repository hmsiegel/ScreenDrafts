import { env } from "@/lib/env";

const apiBase = env.apiUrl;

// Local types — will move to dto.ts after NSwag regeneration.
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

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export async function fetchAdminUsers(
  accessToken: string | undefined,
  search?: string,
  page = 1,
  pageSize = 25
): Promise<PagedResult<AdminUserItem>> {
  const empty: PagedResult<AdminUserItem> = {
    items: [], totalCount: 0, page, pageSize,
    totalPages: 0, hasPreviousPage: false, hasNextPage: false,
  };
  try {
    const url = new URL(`${apiBase}/admin/users`);
    if (search) url.searchParams.set("search", search);
    url.searchParams.set("page", String(page));
    url.searchParams.set("pageSize", String(pageSize));
    const response = await fetch(url.toString(), {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) {
      console.error(`[fetchAdminUsers] ${response.status}`);
      return empty;
    }
    return response.json() as Promise<PagedResult<AdminUserItem>>;
  } catch (err) {
    console.error("[fetchAdminUsers] error:", err);
    return empty;
  }
}

export async function fetchAdminRoles(
  accessToken: string | undefined
): Promise<AdminRoleItem[]> {
  try {
    const response = await fetch(`${apiBase}/admin/roles`, {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) {
      console.error(`[fetchAdminRoles] ${response.status}`);
      return [];
    }
    const data = await response.json() as { roles: string[] };
    return data.roles.map(name => ({ name }));
  } catch (err) {
    console.error("[fetchAdminRoles] error:", err);
    return [];
  }
}

export async function fetchRolePermissions(
  accessToken: string | undefined,
  roleName: string
): Promise<string[]> {
  try {
    const response = await fetch(
      `${apiBase}/admin/roles/${encodeURIComponent(roleName)}/permissions`,
      {
        headers: authHeaders(accessToken),
        cache: "no-store",
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