// lib/parse-api-error.ts

/**
 * Every fetch wrapper in this app (fetch-my-drafts.ts, fetch-admin-drafts.ts,
 * etc.) throws Error objects shaped like:
 *   `${functionName} failed (${status}): ${responseBodyText}`
 * where the body is an RFC7807 problem-details JSON blob from the API
 * (see ResultExtensions.cs / ApiResults.Problem). Raw, that's a wall of
 * JSON with a traceId — not something to show a user. This pulls out the
 * human-readable `detail` (falling back to `title`) and discards the rest.
 *
 * Falls back to a generic message if the error isn't in this shape at all
 * (network failure, non-JSON body, etc.) rather than ever showing raw
 * JSON or a stack trace to the user.
 */
export function parseApiErrorMessage(
  err: unknown,
  fallback = "Something went wrong. Please try again."
): string {
  if (!(err instanceof Error)) return fallback;

  const jsonStart = err.message.indexOf("{");
  if (jsonStart === -1) return err.message || fallback;

  try {
    const problem = JSON.parse(err.message.slice(jsonStart)) as {
      detail?: string;
      title?: string;
    };
    return problem.detail || problem.title || fallback;
  } catch {
    return fallback;
  }
}