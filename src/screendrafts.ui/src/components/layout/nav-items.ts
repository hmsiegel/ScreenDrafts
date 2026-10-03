// components/layout/nav-items.ts
// Single source for nav destinations, shared by the desktop header,
// AdminDropdown, and MobileNav.

export type NavItem = {
  label: string;
  href: string;
  /** Match the path exactly instead of by prefix (e.g. "/admin" is the parent of every admin page). */
  exact?: boolean;
};

export const PRIMARY_NAV_ITEMS: NavItem[] = [
  { label: "DRAFTS", href: "/drafts" },
  { label: "DRAFTERS", href: "/drafters" },
  { label: "FILMS", href: "/media" },
  { label: "PREDICTIONS", href: "/predictions" },
];

export const GUEST_DRAFTS_NAV_ITEM: NavItem = { label: "GUEST DRAFTS", href: "/guest-drafts" };

export const ADMIN_NAV_ITEMS: NavItem[] = [
  { label: "User Management", href: "/admin", exact: true },
  { label: "Draft Management", href: "/admin/drafts" },
  { label: "Drafter Teams", href: "/admin/drafter-teams" },
  { label: "Spotlight Management", href: "/admin/spotlight" },
  { label: "Campaigns", href: "/admin/campaigns" },
  { label: "Categories", href: "/admin/categories" },
  { label: "Series", href: "/admin/series" },
];

export function isNavItemActive(pathname: string | null, item: NavItem): boolean {
  if (!pathname) return false;
  if (item.exact) return pathname === item.href;
  return pathname === item.href || pathname.startsWith(`${item.href}/`);
}