import { getServerSession } from "next-auth";
import { authOptions } from "../api/auth/[...nextauth]/route";
import { DraftResponse } from "./dto";
import { env } from "./env";

const apiBase = env.apiUrl;

export async function getLatestDrafts(): Promise<DraftResponse[]> {
   const url = `${apiBase}/drafts/latest`;

   const session = await getServerSession(authOptions);
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   const response = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
      next: { revalidate: 0 }, // Disable caching
   });

   if (!response.ok) {
      const body = await response.text();
      throw new Error(
         `Request failed with status ${response.status}: ${response.statusText} - ${body}`
      );
   }

   return response.json() as Promise<DraftResponse[]>;
}

export async function getUpcomingDrafts(): Promise<DraftResponse[]> {
   const url = `${apiBase}/drafts/upcoming`;

   const session = await getServerSession(authOptions);
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   const response = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
      next: { revalidate: 0 }, // Disable caching
   });

   if (!response.ok) {
      const body = await response.text();
      throw new Error(
         `Request failed with status ${response.status}: ${response.statusText} - ${body}`
      );
   }

   return response.json() as Promise<DraftResponse[]>;
}

export async function listDrafts(): Promise<DraftResponse[]> {
   const url = `${apiBase}/drafts`;

   const session = await getServerSession(authOptions);
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   const response = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
      next: { revalidate: 0 }, // Disable caching
   });

   if (!response.ok) {
      const body = await response.text();
      throw new Error(
         `Request failed with status ${response.status}: ${response.statusText} - ${body}`
      );
   }

   //return response.json() as Promise<DraftResponse[]>;
   const data = await response.json();
   console.log("Fetched drafts:", data);
   return data.value;
}