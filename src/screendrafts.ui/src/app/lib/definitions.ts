import exp from "constants";

export type User = {
   id: string;
   firstname: string;
   lastname: string;
   email: string;
   avatar: string;
   password: string;
   roles: Role[];
}

export type Role = {
   name: string;
}

export type Permission = {
   code: string;
}

export type UserRole = {
   user: User;
   role: Role;
}

export type RolePermission = {
   role: Role;
   permission: Permission;
}

export type Draft = {
   id: string;
   title: string;
   episode: number;
   type: DraftType;
   expandedDraft?: ExpandedDraftType;
   commissioners: Commissioner[];
   drafters: Drafter[];
   draft_dates: Date[];
   picks: DraftPick[];
}

export type DraftType = {
   type: 'regular' | 'expanded';
   numberOfDrafters: number;
   numberOfPicks: number;
}

export type Commissioner = {
   primary_id: number;
   id: string;
   first_name: string;
   middle: string;
   last_name: string;
   full_name: string;
   image_url: string;
}

export type Drafter = {
   primary_id: number;
   id: string;
   first_name: string;
   middle: string;
   last_name: string;
   full_name: string;
   image_url: string;
}

export type ExpandedDraftType = {
   type: 'super' | 'mini-super' | 'mega' | 'mini-mega';
}

export type DraftPick = {
   drafter: Drafter;
   draftPosition: number;
   movie: Movie;
   status: Status;
}

export type Status = {
   status: 'selected' | 'vetoed';
   draftPickVeto?: DraftPickVeto;
}

export type DraftPickVeto = {
   drafter: Drafter;
   vetoed: boolean;
   vetoOverride?:  VetoOverride;
}

export type VetoOverride = {
   drafter: Drafter;
}

export type Movie = {
   id: string;
   title: string;
   year: number;
   poster: string;
   genres: Genre[];
   cast: Cast[];
   crew: Crew[];
}

export type Genre = {
   name: string;
}

export type Cast = {
   name: string;
   character: string;
}

export type Crew = {
   name: string;
   job: string;
}