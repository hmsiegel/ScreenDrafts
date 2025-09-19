# Create a new draft
* Assign drafters
* Assign hosts
* If the draft has a specific topic, assign eligible titles
* Drafters can also create a list of titles they would like to draft

# Start a draft
* ## Trivia
   * Mark down questions won by each player
   * Update trivia results
   * Assign draft positions based on trivia results and drafter preferences
* ## Game Play (standard draft)
   * Load the draft
   * Load vetoes into memory
   * Load eligible titles into memory (if applicable)
   * Load drafter's preferred titles into memory (if applicable)
   * The primary host will read the rules of Screen Drafts
   * Drafter makes a selection (either from the eligible titles or their preferred titles, or they can search for a title) and clicks "Submit Pick"
   * The pick selection is sent to the primary host to be read aloud
      *  After the pick is read aloud, a couple of things can happen:
         * Either the primary host or the secondary host can call for a commissioner review
            * If the hosts find the pick ineligible, they will select "Commissioner Override"
         * The other drafter can select "Veto" in their interface. The pick is removed from the list, but can be re-selected.
            * A veto is removed from the drafter's stats
         * The drafter that made the pick will talk about their pick.
            * The drafter that made the pick can also "Self-Veto" their pick.
         * Once the pick is finalized, the primary host or secondary host will select "Finalize Pick"
            * The pick is added to the drafter's list of picks
            * The pick is removed from the eligible titles list (if applicable)
            * The pick is removed from the drafter's preferred titles list (if applicable)
            * The draft position is updated to the next drafter
   * The draft continues until all draft positions are filled
   * The draft can be paused at any time by the primary host or secondary host
      * When the draft is paused, the draft can be resumed by either host
   * The draft can be ended at any time by the primary host or secondary host
      * When the draft is ended, the draft is marked as completed and no further picks can be made
* Game Play (expanded draft)
   * The expanded draft follows the same process as the standard draft, but with the following differences:
      * There are more draft positions
      * There are more drafters
      * There is also the veto override, which allows a drafter to override a veto. This can only be used on another drafter's pick.
         * A veto override is removed from the drafter's stats
* ## Post-Draft
   * The draft results are displayed
   * The draft results are sent to all drafters via email
   * The draft results are saved to the database
   * Rollover vetoes and overrides are applied to the drafters' stats
   * Honorifics are updated as necessary
   * The draft can be archived by the primary host or secondary host
      * When the draft is archived, it is removed from the list of active drafts and can only be accessed in the archive section


## Domain Events (inside Drafts)
### Setup & configuration

* ~~DraftCreated~~
* ~~DraftPartAdded~~
* ~~DrafterAssignedToDraft / DrafterRemovedFromDraft~~
* ~~HostAssignedToDraft / HostRemovedFromDraft (enforce one primary host)~~
* EligibleTitlesAssigned / EligibleTitlesCleared
* DrafterPreferredTitlesSet / DrafterPreferredTitleAdded / DrafterPreferredTitleRemoved
* *~~ReleaseScheduled (channel, date)~~
* *~~SeriesLinkedToDraft / SeriesUnlinkedFromDraft~~
### Start & trivia
* ~~DraftStarted~~
* ~~TriviaResultRecorded (per question/per player)~~
* ~~DraftPositionsAssignedFromTrivia (includes preference resolution)~~
### Gameplay (standard & expanded)
* PickSubmitted (tentative)
* PickReadAloud (host gate)
* CommissionerReviewRequested / CommissionerOverrideApplied (with reason)
* VetoUsed (by opposing drafter)
* SelfVetoUsed (by picking drafter)
* VetoOverrideUsed (expanded drafts only)
* PickFinalized (canonical addition to board)
* DraftPositionAdvanced
* DraftPaused / DraftResumed
* DraftEnded (no further picks allowed)
### Post-draft & lifecycle
* DraftResultsCompiled
* RolloverVetoesApplied
* HonorificsRecalculatedForDraft (scoped trigger; projections update elsewhere)
* DraftArchived
* DraftUnarchived
* ReleasePublished (when a scheduled release actually “goes live”)
* DraftMarkedNonCanonical / DraftMarkedCanonical (if policy flips)
### Core domain handlers (examples)
* On PickFinalized → remove title from eligible & preferred, persist pick, advance position.
* On VetoUsed / SelfVetoUsed / VetoOverrideUsed → decrement inventories; record stats deltas.
* On CommissionerOverrideApplied → mark pick ineligible; allow re-selection.
* On DraftEnded → lock write paths; emit results compilation.
* On ReleaseScheduled → validate future dates; check channel rules (Patreon/Main Feed).
* On HonorificsRecalculatedForDraft → update module-local read models only (no cross-module writes).
