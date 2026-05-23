import { auth } from "@/auth";
import { env } from "@/lib/env";
import { GetUserResponse } from "@/lib/dto";

const apiBase = env.apiUrl;

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

export async function fetchProfile(): Promise<GetUserResponse | null> {
  try {
    const response = await fetch(`${apiBase}/users/profile`, {
      headers: await authHeaders(),
      next: { revalidate: 0 },
    });
    if (!response.ok) {
      console.error(`[fetchProfile] ${response.status} ${response.statusText}`);
      return null;
    }
    return response.json() as Promise<GetUserResponse>;
  } catch (err) {
    console.error("[fetchProfile] error:", err);
    return null;
  }
}
