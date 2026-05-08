"""
Fill missing Spotify URLs in screen_drafts_episodes.csv
Strategy:
1. Parse Libsyn RSS feed for Spotify URLs
2. Fetch individual Libsyn episode pages for Spotify links
3. Scrape Spotify show page for episode links
4. Fuzzy-match titles to assign URLs
"""

import csv
import re
import time
import urllib.parse
from difflib import SequenceMatcher
import xml.etree.ElementTree as ET

try:
    import requests
except ImportError:
    print("requests not available, using urllib")
    import urllib.request as requests_fallback
    requests = None

INPUT_FILE = r"C:\Repos\ScreenDrafts\src\screendrafts.api\data\screen_drafts_episodes.csv"
OUTPUT_FILE = r"C:\Repos\ScreenDrafts\src\screendrafts.api\data\spotlights.csv"

LIBSYN_RSS = "https://screendrafts.libsyn.com/rss"
SPOTIFY_SHOW_ID = "5qXizYahLTEgZSS7Cos9jB"

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
}


def fetch_url(url, timeout=30):
    """Fetch a URL and return text content."""
    if requests:
        try:
            r = requests.get(url, headers=HEADERS, timeout=timeout)
            r.raise_for_status()
            return r.text
        except Exception as e:
            print(f"  ERROR fetching {url}: {e}")
            return None
    else:
        try:
            req = urllib_request.Request(url, headers=HEADERS)
            with urllib_request.urlopen(req, timeout=timeout) as resp:
                return resp.read().decode("utf-8", errors="replace")
        except Exception as e:
            print(f"  ERROR fetching {url}: {e}")
            return None


def clean_spotify_url(url):
    """Strip query params from Spotify URL."""
    if not url:
        return url
    parsed = urllib.parse.urlparse(url)
    clean = urllib.parse.urlunparse(parsed._replace(query="", fragment=""))
    return clean.strip()


def extract_spotify_episode_urls(text):
    """Extract all Spotify episode URLs from text."""
    pattern = r'https://open\.spotify\.com/episode/([A-Za-z0-9]+)'
    matches = re.findall(pattern, text)
    return [f"https://open.spotify.com/episode/{m}" for m in matches]


def similarity(a, b):
    """Return similarity ratio between two strings."""
    a_clean = re.sub(r'[^a-z0-9 ]', '', a.lower().strip())
    b_clean = re.sub(r'[^a-z0-9 ]', '', b.lower().strip())
    return SequenceMatcher(None, a_clean, b_clean).ratio()


def best_match(title, candidates, threshold=0.75):
    """Find best matching title from candidates dict {title: url}."""
    best_score = 0
    best_url = None
    best_title = None
    for cand_title, url in candidates.items():
        score = similarity(title, cand_title)
        if score > best_score:
            best_score = score
            best_url = url
            best_title = cand_title
    if best_score >= threshold:
        return best_url, best_score, best_title
    return None, best_score, best_title


def parse_rss_feed(xml_text):
    """
    Parse RSS feed and extract episode titles + any Spotify URLs.
    Returns list of dicts: {title, spotify_url, link, guid}
    """
    episodes = []
    try:
        root = ET.fromstring(xml_text)
    except ET.ParseError as e:
        print(f"  XML parse error: {e}")
        return episodes

    # Print namespace info from first few items for debugging
    channel = root.find("channel")
    if channel is None:
        print("  No channel found in RSS")
        return episodes

    items = channel.findall("item")
    print(f"  Found {len(items)} items in RSS feed")

    if items:
        first = items[0]
        print("  First item tags:", [child.tag for child in first])
        print("  First item namespaces in tag names:", set(
            child.tag.split('}')[0].lstrip('{') for child in first if '{' in child.tag
        ))

    for item in items:
        ep = {}

        # Title
        title_el = item.find("title")
        ep["title"] = title_el.text.strip() if title_el is not None and title_el.text else ""

        # Link
        link_el = item.find("link")
        ep["link"] = link_el.text.strip() if link_el is not None and link_el.text else ""

        # GUID
        guid_el = item.find("guid")
        ep["guid"] = guid_el.text.strip() if guid_el is not None and guid_el.text else ""

        # Look for Spotify URL in all child elements
        spotify_url = None
        full_text = ET.tostring(item, encoding="unicode")

        # Check for open.spotify.com/episode URLs
        matches = extract_spotify_episode_urls(full_text)
        if matches:
            spotify_url = matches[0]

        ep["spotify_url"] = spotify_url
        episodes.append(ep)

    return episodes


def scrape_libsyn_page(url):
    """Fetch a Libsyn episode page and look for Spotify episode URLs."""
    text = fetch_url(url)
    if not text:
        return None
    matches = extract_spotify_episode_urls(text)
    if matches:
        return matches[0]
    return None


def scrape_spotify_show_page():
    """
    Scrape the Spotify show page for episode URLs.
    Returns dict {title: spotify_url}
    """
    print("\n--- Step 3: Scraping Spotify show page ---")
    episodes = {}

    url = f"https://open.spotify.com/show/{SPOTIFY_SHOW_ID}"
    text = fetch_url(url)
    if not text:
        print("  Failed to fetch Spotify show page")
        return episodes

    # Look for episode links
    matches = extract_spotify_episode_urls(text)
    print(f"  Found {len(matches)} episode URLs on show page")

    # Also try to extract titles near episode URLs
    # Spotify renders via JS so page source may not have much
    # Try the embed endpoint
    embed_url = f"https://open.spotify.com/embed/show/{SPOTIFY_SHOW_ID}"
    embed_text = fetch_url(embed_url)
    if embed_text:
        embed_matches = extract_spotify_episode_urls(embed_text)
        print(f"  Found {len(embed_matches)} episode URLs on embed page")
        matches = list(set(matches + embed_matches))

    return matches


def read_csv():
    """Read the input CSV and return (header, rows)."""
    with open(INPUT_FILE, encoding='utf-8', errors='replace') as f:
        reader = csv.reader(f)
        rows = list(reader)
    header = rows[0]
    data = rows[1:]
    return header, data


def write_csv(header, rows):
    """Write output CSV."""
    with open(OUTPUT_FILE, 'w', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        writer.writerow(header)
        writer.writerows(rows)
    print(f"\nWrote {len(rows)} rows to {OUTPUT_FILE}")


def main():
    print("=== Screen Drafts Spotify URL Filler ===\n")

    # Read input
    header, rows = read_csv()
    print(f"Loaded {len(rows)} rows. Header: {header}")

    # Ensure 5 columns in every row
    for row in rows:
        while len(row) < 5:
            row.append("")

    # Identify rows needing URLs
    missing = [(i, row) for i, row in enumerate(rows) if not row[4].strip()]
    print(f"Rows missing spotify_url: {len(missing)}")
    print(f"Rows already have spotify_url: {len(rows) - len(missing)}")

    # Clean existing URLs (strip ?si= params)
    for row in rows:
        if row[4].strip():
            row[4] = clean_spotify_url(row[4])

    # ---- Step 1: Fetch and parse Libsyn RSS ----
    print("\n--- Step 1: Fetching Libsyn RSS feed ---")
    rss_text = fetch_url(LIBSYN_RSS)

    rss_episodes = []
    if rss_text:
        print(f"  RSS fetched, {len(rss_text)} chars")
        rss_episodes = parse_rss_feed(rss_text)
        print(f"  Parsed {len(rss_episodes)} episodes from RSS")
        rss_with_spotify = [e for e in rss_episodes if e.get("spotify_url")]
        print(f"  Episodes with Spotify URL in RSS: {len(rss_with_spotify)}")
        if rss_with_spotify:
            print("  Sample RSS Spotify URLs:")
            for ep in rss_with_spotify[:3]:
                print(f"    {ep['title']!r} -> {ep['spotify_url']}")
    else:
        print("  Failed to fetch RSS feed")

    # Build title->url map from RSS
    rss_title_map = {}  # title -> spotify_url
    rss_link_map = {}   # title -> libsyn_link (for step 2 fallback)

    for ep in rss_episodes:
        t = ep["title"]
        if ep.get("spotify_url"):
            rss_title_map[t] = ep["spotify_url"]
        # Store link for fallback
        link = ep.get("link") or ep.get("guid", "")
        if link and "libsyn.com" in link:
            rss_link_map[t] = link

    print(f"\nRSS title->spotify map has {len(rss_title_map)} entries")
    print(f"RSS title->link map has {len(rss_link_map)} entries")

    # ---- Step 1b: Match RSS Spotify URLs to CSV rows ----
    print("\n--- Step 1b: Matching RSS Spotify URLs to CSV titles ---")
    matched_step1 = 0
    still_missing = []

    for i, row in missing:
        csv_title = row[2]
        url, score, matched_title = best_match(csv_title, rss_title_map, threshold=0.75)
        if url:
            rows[i][4] = url
            print(f"  [RSS] Ep {row[1]}: {csv_title!r} -> {matched_title!r} (score={score:.2f}) -> {url}")
            matched_step1 += 1
        else:
            still_missing.append((i, row))

    print(f"\nMatched via RSS direct: {matched_step1}")
    print(f"Still missing after RSS: {len(still_missing)}")

    # ---- Step 2: Fetch Libsyn episode pages for remaining ----
    print(f"\n--- Step 2: Fetching Libsyn episode pages for {len(still_missing)} remaining ---")

    # Build a map from RSS title -> libsyn page URL for episodes WITHOUT spotify URLs in RSS
    # We need to fetch those pages
    libsyn_page_spotify = {}  # libsyn_title -> spotify_url

    # Process all RSS episodes that don't have Spotify URLs yet
    episodes_needing_pages = [ep for ep in rss_episodes if not ep.get("spotify_url")]
    print(f"  RSS episodes without Spotify URL: {len(episodes_needing_pages)}")
    print(f"  Will fetch pages for these to find Spotify links")

    # To avoid fetching hundreds of pages, first try fuzzy matching still_missing to rss_link_map
    # and only fetch pages for those that match
    pages_to_fetch = {}  # rss_title -> link
    for ep in episodes_needing_pages:
        link = ep.get("link") or ep.get("guid", "")
        if link and ("libsyn.com" in link or "screendrafts" in link):
            pages_to_fetch[ep["title"]] = link

    print(f"  RSS episodes with fetchable pages (no Spotify URL): {len(pages_to_fetch)}")

    # Fetch pages in batches with rate limiting
    batch_size = 50
    fetched_count = 0
    for rss_title, page_url in list(pages_to_fetch.items()):
        if fetched_count >= 400:  # Safety limit
            break
        if fetched_count % 20 == 0:
            print(f"  Fetching page {fetched_count+1}/{min(len(pages_to_fetch), 400)}: {rss_title[:50]}")
        spotify_url = scrape_libsyn_page(page_url)
        if spotify_url:
            libsyn_page_spotify[rss_title] = spotify_url
        fetched_count += 1
        time.sleep(0.3)  # Be polite to the server

    print(f"\n  Found Spotify URLs on {len(libsyn_page_spotify)} Libsyn pages")

    # Now match these to still_missing rows
    matched_step2 = 0
    still_missing2 = []

    for i, row in still_missing:
        csv_title = row[2]
        url, score, matched_title = best_match(csv_title, libsyn_page_spotify, threshold=0.75)
        if url:
            rows[i][4] = url
            print(f"  [Libsyn page] Ep {row[1]}: {csv_title!r} -> {matched_title!r} (score={score:.2f}) -> {url}")
            matched_step2 += 1
        else:
            still_missing2.append((i, row))

    print(f"\nMatched via Libsyn pages: {matched_step2}")
    print(f"Still missing after Libsyn pages: {len(still_missing2)}")

    # ---- Step 3: Spotify show page scraping ----
    spotify_show_urls = scrape_spotify_show_page()
    print(f"  Total unique Spotify episode URLs from show page: {len(spotify_show_urls)}")

    # ---- Step 4: Try Spotify RSS/API endpoints ----
    print("\n--- Step 4: Trying additional Spotify endpoints ---")

    # Try Spotify RSS feed (some shows have one)
    spotify_rss_url = f"https://anchor.fm/s/{SPOTIFY_SHOW_ID}/podcast/rss"
    print(f"  Trying Anchor/Spotify RSS: {spotify_rss_url}")
    anchor_text = fetch_url(spotify_rss_url)
    if anchor_text and "episode" in anchor_text.lower():
        print(f"  Got Anchor RSS, {len(anchor_text)} chars")
        anchor_episodes = parse_rss_feed(anchor_text)
        anchor_map = {}
        for ep in anchor_episodes:
            if ep.get("spotify_url"):
                anchor_map[ep["title"]] = ep["spotify_url"]
        print(f"  Anchor RSS episodes with Spotify URL: {len(anchor_map)}")

        matched_step4 = 0
        still_missing3 = []
        for i, row in still_missing2:
            csv_title = row[2]
            url, score, matched_title = best_match(csv_title, anchor_map, threshold=0.75)
            if url:
                rows[i][4] = url
                print(f"  [Anchor] Ep {row[1]}: {csv_title!r} -> {matched_title!r} ({score:.2f})")
                matched_step4 += 1
            else:
                still_missing3.append((i, row))
        print(f"  Matched via Anchor RSS: {matched_step4}")
        still_missing2 = still_missing3
    else:
        print("  No useful Anchor RSS found")

    # ---- Final summary ----
    print("\n=== FINAL SUMMARY ===")
    final_filled = sum(1 for row in rows if row[4].strip())
    final_empty = sum(1 for row in rows if not row[4].strip())
    print(f"Total rows: {len(rows)}")
    print(f"Rows WITH spotify_url: {final_filled}")
    print(f"Rows WITHOUT spotify_url: {final_empty}")

    if still_missing2:
        print(f"\nStill missing ({len(still_missing2)} rows):")
        for i, row in still_missing2[:20]:
            print(f"  Ep {row[1]}: {row[2]!r}")
        if len(still_missing2) > 20:
            print(f"  ... and {len(still_missing2) - 20} more")

    # Write output
    write_csv(header, rows)

    # Show 5 sample filled rows
    print("\n=== 5 Sample Filled Rows ===")
    newly_filled = [(i, row) for i, row in enumerate(rows) if row[4].strip()]
    for i, row in newly_filled[:5]:
        print(f"  Ep {row[1]}: {row[2]!r} -> {row[4]}")

    print("\nDone!")


if __name__ == "__main__":
    main()
