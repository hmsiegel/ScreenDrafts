// src/services/admin/fetch-attendances.ts

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

function authHeaders(accessToken: string) {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  };
}

export interface AttendanceItem {
  publicId: string;
  personPublicId: string;
  personName: string;
  status: number;
  statusName: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export async function fetchAttendances(
  accessToken: string,
  draftPartId: string,
): Promise<AttendanceItem[]> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/attendances`,
    { headers: { Authorization: `Bearer ${accessToken}` }, cache: 'no-store' },
  );
  if (!res.ok) throw new Error(`fetchAttendances failed: ${res.status}`);
  const data = await res.json();
  return data.items ?? [];
}

export async function confirmAttendance(
  accessToken: string,
  draftPartId: string,
  personPublicId: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/attendances/${personPublicId}/confirm`,
    { method: 'PUT', headers: authHeaders(accessToken), body: JSON.stringify({}) },
  );
  if (!res.ok) throw new Error(`confirmAttendance failed: ${res.status}`);
}

export async function withdrawAttendance(
  accessToken: string,
  draftPartId: string,
  personPublicId: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/attendances/${personPublicId}/withdraw`,
    { method: 'PUT', headers: authHeaders(accessToken), body: JSON.stringify({}) },
  );
  if (!res.ok) throw new Error(`withdrawAttendance failed: ${res.status}`);
}

export async function reinstateAttendance(
  accessToken: string,
  draftPartId: string,
  personPublicId: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/attendances/${personPublicId}/reinstate`,
    { method: 'PUT', headers: authHeaders(accessToken), body: JSON.stringify({}) },
  );
  if (!res.ok) throw new Error(`reinstateAttendance failed: ${res.status}`);
}