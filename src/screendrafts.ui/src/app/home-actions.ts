// app/home-actions.ts
'use server';

import { updateTag } from 'next/cache';
import { auth } from '@/auth';
import { HOME_CACHE_TAG } from '@/services/home/fetch-home-data';

/**
 * Expires the cached home page data (recent drafts, upcoming, standings, stats) so the next
 * visit refetches. Call after an action that changes what the home page shows, such as
 * completing a draft part.
 *
 * Server actions are public POST endpoints, so this requires a signed-in user.
 */
export async function refreshHomeData(): Promise<void> {
  const session = await auth();
  if (!session?.accessToken || session.error) return;

  updateTag(HOME_CACHE_TAG);
}
