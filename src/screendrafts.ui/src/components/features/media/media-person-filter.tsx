// components/features/media/media-person-filter.tsx
"use client";

import { useEffect, useId, useRef, useState } from "react";
import type { KeyboardEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { env } from "@/lib/env";
// New after NSwag regen (GET /media/people).
import type { SearchMediaPeopleResponse } from "@/lib/dto";

const ROLE_OPTIONS = [
  { value: "any", label: "ANY ROLE" },
  { value: "actor", label: "ACTOR" },
  { value: "director", label: "DIRECTOR" },
  { value: "writer", label: "WRITER" },
  { value: "producer", label: "PRODUCER" },
];

const MIN_QUERY_LENGTH = 2;

interface PersonOption {
  /** "t:{tmdbId}" or "i:{imdbId}" — what GET /media expects as personKey. */
  personKey: string;
  name: string;
  titleCount: number;
}

export interface MediaPersonFilterProps {
  mediaType: string;
  sort: string;
  q?: string;
  year?: string;
  /** Selected person's key ("t:…" or "i:…"), as it appears in the URL. */
  personKey?: string;
  /** Display name for the selected person; the API ignores it. */
  personName?: string;
  role?: string;
}

export default function MediaPersonFilter({
  mediaType,
  sort,
  q,
  year,
  personKey,
  personName,
  role: roleProp,
}: MediaPersonFilterProps) {
  const router = useRouter();
  const listboxId = useId();
  const wrapperRef = useRef<HTMLDivElement>(null);

  // Role lives in local state until a person is chosen; only then does it reach the URL.
  const [role, setRole] = useState(roleProp ?? "any");
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<PersonOption[]>([]);
  const [searching, setSearching] = useState(false);
  const [failed, setFailed] = useState(false);
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);

  const hasPerson = Boolean(personKey && personName);

  // Back/forward navigation can change the role in the URL.
  useEffect(() => {
    if (roleProp) setRole(roleProp);
  }, [roleProp]);

  // Debounced search, re-run when the role changes so results match the role.
  useEffect(() => {
    const term = query.trim();
    if (term.length < MIN_QUERY_LENGTH) {
      setResults([]);
      setSearching(false);
      setFailed(false);
      return;
    }

    const controller = new AbortController();
    const handle = setTimeout(async () => {
      setSearching(true);
      setFailed(false);
      try {
        const url = new URL(`${env.apiUrl}/media/people`);
        url.searchParams.set("query", term);
        url.searchParams.set("role", role);
        url.searchParams.set("limit", "10");

        const res = await fetch(url.toString(), { cache: "no-store", signal: controller.signal });
        if (!res.ok) {
          console.error("[MediaPersonFilter] /media/people returned", res.status);
          setResults([]);
          setFailed(true);
          setOpen(true);
          return;
        }

        const data = (await res.json()) as SearchMediaPeopleResponse;
        setResults(
          (data.items ?? []).flatMap((p) =>
            p.personKey && p.name
              ? [{ personKey: p.personKey, name: p.name, titleCount: p.titleCount ?? 0 }]
              : []
          )
        );
        setActive(-1);
        setOpen(true);
      } catch (err) {
        if (!(err instanceof DOMException && err.name === "AbortError")) {
          console.error("[MediaPersonFilter]", err);
          setResults([]);
          setFailed(true);
          setOpen(true);
        }
      } finally {
        if (!controller.signal.aborted) setSearching(false);
      }
    }, 300);

    return () => {
      clearTimeout(handle);
      controller.abort();
    };
  }, [query, role]);

  // Close the list when the user clicks elsewhere.
  useEffect(() => {
    if (!open) return;
    function onPointerDown(e: MouseEvent) {
      if (!wrapperRef.current?.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", onPointerDown);
    return () => document.removeEventListener("mousedown", onPointerDown);
  }, [open]);

  function navigate(next?: { personKey: string; name: string; role: string }) {
    const qs = new URLSearchParams({ sort, page: "1" });
    if (mediaType) qs.set("mediaType", mediaType);
    if (q) qs.set("q", q);
    if (year) qs.set("year", year);
    if (next) {
      qs.set("personKey", next.personKey);
      qs.set("personName", next.name);
      qs.set("role", next.role);
    }
    router.push(`?${qs.toString()}`);
  }

  function choose(person: PersonOption) {
    setQuery("");
    setResults([]);
    setOpen(false);
    navigate({ personKey: person.personKey, name: person.name, role });
  }

  function handleRoleChange(next: string) {
    setRole(next);
    if (personKey && personName) {
      navigate({ personKey, name: personName, role: next });
    }
  }

  function handleKeyDown(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Escape") {
      setOpen(false);
      return;
    }
    if (results.length === 0) return;

    if (e.key === "ArrowDown") {
      e.preventDefault();
      setOpen(true);
      setActive((i) => (i + 1) % results.length);
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setOpen(true);
      setActive((i) => (i <= 0 ? results.length - 1 : i - 1));
    } else if (e.key === "Enter" && open && active >= 0) {
      e.preventDefault();
      choose(results[active]);
    }
  }

  const showEmpty =
    open &&
    !searching &&
    !failed &&
    query.trim().length >= MIN_QUERY_LENGTH &&
    results.length === 0;

  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:gap-4">
      <span className="font-mono text-[10px] tracking-widest text-sd-blue shrink-0">
        CAST &amp; CREW
      </span>

      <select
        value={role}
        onChange={(e) => handleRoleChange(e.target.value)}
        aria-label="Credit role"
        className="border border-sd-ink/30 bg-white px-3 py-1.5 font-mono text-[11px] text-sd-ink focus:outline-hidden focus:border-sd-ink"
      >
        {ROLE_OPTIONS.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>

      {hasPerson ? (
        <div className="flex items-center gap-2 self-start border border-sd-ink bg-sd-paper px-3 py-1.5">
          <span className="font-oswald text-[14px] leading-tight text-sd-ink">{personName}</span>
          {personKey && (
            <Link
              // "t:31644" -> "31644", "i:nm0000229" -> "nm0000229"
              href={`/cast-crew/${personKey.slice(2)}`}
              className="font-mono text-[10px] tracking-widest text-sd-blue transition-colors hover:text-sd-ink"
            >
              PROFILE
            </Link>
          )}
          <button
            type="button"
            onClick={() => navigate()}
            aria-label={`Clear ${personName} filter`}
            className="font-mono text-[14px] leading-none text-sd-ink/50 transition-colors hover:text-sd-red"
          >
            ×
          </button>
        </div>
      ) : (
        <div ref={wrapperRef} className="relative w-full sm:w-64">
          <input
            type="text"
            role="combobox"
            aria-expanded={open}
            aria-controls={listboxId}
            aria-autocomplete="list"
            aria-activedescendant={active >= 0 ? `${listboxId}-${active}` : undefined}
            value={query}
            onChange={(e) => {
              setQuery(e.target.value);
              setOpen(true);
            }}
            onFocus={() => results.length > 0 && setOpen(true)}
            onKeyDown={handleKeyDown}
            placeholder="Find a person…"
            autoComplete="off"
            className="w-full border border-sd-ink/30 px-3 py-1.5 font-mono text-[11px] text-sd-ink placeholder:text-sd-ink/40 focus:outline-hidden focus:border-sd-ink"
          />

          {searching && (
            <span className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 font-mono text-[10px] text-sd-ink/40 animate-pulse">
              …
            </span>
          )}

          {open && results.length > 0 && (
            <ul
              id={listboxId}
              role="listbox"
              className="absolute z-30 mt-1 max-h-72 w-full overflow-y-auto border-2 border-sd-ink bg-white"
            >
              {results.map((person, i) => (
                <li
                  key={person.personKey}
                  id={`${listboxId}-${i}`}
                  role="option"
                  aria-selected={i === active}
                  onMouseDown={(e) => {
                    e.preventDefault();
                    choose(person);
                  }}
                  onMouseEnter={() => setActive(i)}
                  className={`flex cursor-pointer items-baseline justify-between gap-3 px-3 py-2 ${
                    i === active ? "bg-sd-paper" : ""
                  }`}
                >
                  <span className="min-w-0 truncate font-oswald text-[15px] leading-tight text-sd-ink">
                    {person.name}
                  </span>
                  <span className="shrink-0 font-mono text-[10px] text-sd-ink/50">
                    {person.titleCount} {person.titleCount === 1 ? "TITLE" : "TITLES"}
                  </span>
                </li>
              ))}
            </ul>
          )}

          {open && failed && (
            <div className="absolute z-30 mt-1 w-full border-2 border-sd-red bg-white px-3 py-3 font-mono text-[11px] text-sd-red">
              Search failed. Try again.
            </div>
          )}

          {showEmpty && (
            <div className="absolute z-30 mt-1 w-full border-2 border-sd-ink bg-white px-3 py-3 font-mono text-[11px] text-sd-ink/50">
              No matching people in the Vault.
            </div>
          )}
        </div>
      )}
    </div>
  );
}
