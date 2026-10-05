// app/stats/actions.ts
"use server";

import { auth } from "@/auth";
import { postStatsQuery } from "@/services/stats/fetch-stats";
import type { StatsQueryInput, StatsQueryOutcome } from "@/services/stats/stats-types";

/**
 * Runs a custom stats query on behalf of the signed-in user. The access token stays on the server;
 * the browser only ever sees the result.
 */
export async function runStatsQuery(input: StatsQueryInput): Promise<StatsQueryOutcome> {
  const session = await auth();

  if (!session?.accessToken || session.error) {
    return { ok: false, message: "Sign in to run custom queries." };
  }

  return postStatsQuery(session.accessToken, input);
}
