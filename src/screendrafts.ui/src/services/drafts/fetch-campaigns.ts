import { auth } from "@/auth";
import { env } from "@/lib/env";

interface CampaignOption {
  publicId: string;
  name: string;
  isDeleted: boolean;
}

interface CampaignCollectionResponse {
  items: CampaignOption[];
}

export async function listCampaigns(): Promise<CampaignOption[]> {
  const session = await auth();
  const headers: HeadersInit = {};
  if (session?.accessToken) {
    headers["Authorization"] = `Bearer ${session.accessToken}`;
  }

  try {
    const response = await fetch(`${env.apiUrl}/campaigns`, {
      method: "GET",
      headers,
      next: { revalidate: 3600 }, // campaigns change rarely
    });

    if (!response.ok) return [];

    const data = await response.json() as CampaignCollectionResponse;
    return (data.items ?? [])
      .filter((c) => !c.isDeleted)
      .sort((a, b) => a.name.localeCompare(b.name));
  } catch {
    return [];
  }
}