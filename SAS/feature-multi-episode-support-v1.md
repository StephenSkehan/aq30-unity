# Feature Spec — Multi-Episode Support (audit + costed design)

**Status:** PHASE 1 AUDIT — costed design, awaiting Stephen's ruling. **No build has started.**
**Date:** 2026-08-27
**Constraint:** first release ships AT LEAST FOUR episodes. Episode count is fixed; the 1 October date is not.
**Verified against:** working tree of 2026-08-27. Every claim about current behaviour carries a file:line reference checked today.

---

## TL;DR — the bill

**The minimum that ships four episodes is 7–10 days of systems work**, on top of writing the
content itself. It is smaller than feared: `LeadsRepository` already swaps databases at runtime
(`Assets/App/Leads/LeadsRepository.cs:94-117`), the save aggregate already records an episode id
(`BoardSaveSystem.cs:558`), all episodes can share the one build scene, and the item economy is
shared across episodes, so no per-episode board or scene work exists at all. The work is: an
episode catalog, an episode-keyed save schema (0.9.0 → 1.0.0), folding story flags into the save
aggregate, a minimal selector, the transition path, and retiring roughly a dozen Episode-1
literals hardwired into cross-cutting systems.

The two genuinely hard parts, named as hard: **PlayerPrefs cannot enumerate keys** (so flag
migration must probe known names from content, and per-episode flag reset is impossible while
flags stay in prefs), and **save migration for existing 0.9.0 players** (mechanical but must be
crash-boundary tested, and the catalog's Ep1 id must match what saves in the wild recorded).

One defect was fixed in this pass, as authorised: episode-complete analytics no longer report a
hardcoded id for a retired episode (see §9).

---

## 1. What actually happens today if the episode changes

The write-down, with the lines that prove it. Suppose a second episode existed and the scene's
orchestrator were pointed at it:

1. **The save file is shared and un-partitioned.** One fixed filename `board_state.json`
   (`Assets/Scripts/UI/Board/Save/BoardSaveSystem.cs:38`, path build `:96-99`), hardcoded a second
   time in `ClearSave` (`:255-258`). Episode 2 loads Episode 1's file.
2. **The saved episode id is written but never read.** `BuildCaseFlowDTO` records
   `state.Episode.Value` (`:558`); `ApplyCaseFlow` (`:563-576`) reads **only** `stepIndex` and
   silently replays `CompleteCurrentStep()` up to it. An Ep01 save loaded under an Ep02
   orchestrator advances Ep02's step machine by Ep01's count, with no mismatch detection.
3. **Episode 1's board tiles restore onto Episode 2's board.** `ApplyCells` (`:414-463`) applies
   whatever cells the file holds; cells carry no episode key (`CellDTO :195-202`).
4. **Episode 1's lead states are silently destroyed.** `ApplyLeads` (`:642-658`) hands saved
   states to `LeadsRepository.ApplySavedStates`, which drops any leadId not found in the bound
   database with a bare `continue` (`Assets/App/Leads/LeadsRepository.cs:231-232`). Under Ep02's
   database, every `Lead_E1_*` state vanishes — and the **next debounced save overwrites the file**,
   so Episode 1's progress is gone permanently, not just hidden.
5. **Episode completion is never durably recorded anywhere.** The only caseflow implementation is
   `InMemoryCaseFlowService` (`Packages/com.aq.sharedkernel/Runtime/CaseFlow/InMemoryCaseFlowService.cs:9-32`):
   one episode, one index, no history. Completion detection is a fire-once bool in a
   DontDestroyOnLoad object matching one hardcoded flag string (`CaseResolutionService.cs:12`,
   `e1.ep01.complete`, authored in `Assets/Content/TheListener/Leads/Lead_E1_Close.asset:36`).
   Nothing durable ever says "episode N is done", so "see which episodes are complete" has no
   data to read.
6. **Story flags share one flat namespace with no mechanism behind it.** `GameFlags` keys are
   `flag_<name>` with no episode scope (`Assets/App/GameFlags.cs:21-23`). No collision exists
   today — the parked Ghost Student content prefixes its flags `gs.*`
   (`Assets/Content/GhostStudent/Leads/Lead_GS_Phase1Pod.asset:49`, `gs.phase1.complete`)
   against The Listener's `e1.*` — but that is author discipline, not a mechanism, and it is the
   only protection four episodes of content would have. Per-episode reset is impossible
   regardless (see §5).
7. **Episode identity is serialized in the scene file.** `Assets/Scenes/Main Merge.unity:192-201`
   holds `episodeId: e1_the_listener` and the real step list
   (`FTUE_Entitlements / Board_Active / Lead_Ready / Resolution`), overriding the code default
   `"Ep01"` (`CaseFlowOrchestratorMB.cs:18-20`). One scene = one episode, and the scene currently
   names the demoted episode. Note the two id namespaces: the scene says `e1_the_listener`, tests
   and the Addressables label say `Ep01`.

## 2. Full breakage inventory, ranked

**Severity A — blocks shipping a second episode at all**

| # | System | Fact | Reference |
|---|---|---|---|
| A1 | Save | One un-partitioned file; loading under a different episode destroys the previous episode's board+lead state on next save | §1.1–1.4 |
| A2 | Caseflow | No persisted episode-completion record; selector has nothing to read | §1.5 |
| A3 | Episode identity | Lives in the scene file; no registry of episodes, databases, or step lists | §1.7 |
| A4 | Resolution | Completion trigger is the Ep1 literal `e1.ep01.complete` — Episode 2's completion would simply never be detected | `CaseResolutionService.cs:12` |
| A5 | Flags | Flat un-scoped namespace; only author discipline (the `e1.`/`gs.` prefix convention) prevents collision across four episodes | §1.6 |

**Severity B — ships wrong behaviour or wrong data**

| # | System | Fact | Reference |
|---|---|---|---|
| B1 | Analytics | 12 of 13 `GameAnalytics` events are episode-blind (`ftue_step :13` … `dossier_fact_unlocked :118`); all of `LeadAnalytics` too (`Assets/App/Leads/LeadAnalytics.cs:10-47`). `IAnalytics.SetUserProperty` exists (`Assets/App/Analytics/IAnalytics.cs:14`) and is never called anywhere — the cheapest fix in this audit | `GameAnalytics.cs:71-77` is the sole episode-aware event |
| B2 | Shop | Mo's Back Room unlock is the Ep1 literal `aq.lead.e1_pod1.seen` | `Assets/Scripts/UI/Shop/MoShopService.cs:41` |
| B3 | Dossiers | Gate is the Ep1 literal `aq.lead.e1_close.seen` (`Assets/Scripts/UI/Dossiers/DossierService.cs:22`); content is hardcoded C# (`DossierCatalog.cs:41-52`); state is a flat prefs blob `aq.dossiers.state` (`DossierService.cs:21`). Same shape again in `LocationService` (`Assets/Scripts/UI/EvidenceBoard/LocationService.cs:14`) |
| B4 | Hints | Three separate `aq.lead.e1_tip.seen` literals | `Assets/Scripts/UI/Hints/HintService.cs:349, 539, 702` |
| B5 | UI | Resolution screen title hardcodes "The Listener" — the code comment itself records that stale-title defect shipping once before | `Assets/App/UI/CaseResolutionScreenMB.cs:89-93` |
| B6 | Scene flow | "Play Again" reloads the current scene by name — the natural episode-transition seam, currently a same-episode restart | `CaseResolutionScreenMB.cs:169` |

**Severity C — fine for v1, note and move on**

| # | System | Fact |
|---|---|---|
| C1 | Wallet / energy | Correctly global. In-memory only (`Packages/com.aq.sharedkernel/Runtime/Economy/WalletService.cs:12-17`); durability entirely via the aggregate (`BoardSaveSystem.cs:522-549`). No change needed |
| C2 | FTUE | All `aq.ftue.*` prefs keys are once-per-install by design (`FTUEEntitlements.cs:20`, `StudioSplashMB.cs:22`, etc.) and must **not** re-fire on episode 2 — leaving them global in PlayerPrefs is correct, not debt. The L1 choreography hardcodes Ep1 ids (`FTUEFirstMergeChoreographyMB.cs:32, 37-39`) but only runs on a fresh install, which is always Episode 1 |
| C3 | Locker / Stash / Specials | Global stores inside the aggregate (fold pattern; export at `BoardSaveSystem.cs:306-308`, import `:374-376`, hash `:713-715`). Keeping them global across episodes is the zero-work option and matches the fiction (Ally's kit travels with her) — ruling R3 below |
| C4 | Addressables | Configured for episodic delivery and vestigial: the `Ep01` group holds one sample asset (`Assets/AddressableAssetsData/AssetGroups/Ep01.asset:19`) while real Ep1 content is 17 hard GUID refs from `Assets/App/Leads/LeadsDatabase.asset:15-32`; the only loader (`Assets/App/Content/AddressableGraphLoader.cs:14, 26-27`) is wired into nothing. Four episodes of leads + dialogue are kilobytes — direct references scale fine for v1; episodic *download* is a post-launch concern |
| C5 | Build settings | Effectively one enabled scene (`ProjectSettings/EditorBuildSettings.asset:11-13`, Main Merge enabled, SampleScene disabled). All four episodes run in that scene, so **no scene or build-settings work is in the bill** |
| C6 | EpisodeId type | A bare string wrapper (`CaseFlowTypes.cs:9-14`): no equality, no comparison anywhere in the repo, unwrapped to `string` at every boundary (`CaseEvents.cs:7` re-declares it as a string field). Buys call-site clarity, protects nothing. Phase 2 should add `IEquatable` while touching it, not build on it |
| C7 | Cross-episode teasers | `SpawnLeadIds` resolve only in the *current* database (`Assets/App/Leads/LeadOutcomeMB.cs:97-104`, warn-and-skip). Ep1's tail (`cold_case_a`, `ep2_teaser`) lives inside Ep1's database, so per-episode databases keep working — but an Ep1 lead can never spawn a lead *into* Ep2. Episode unlock must be the catalog's job (completion-driven), never a cross-database spawn |

## 3. Save design (Q2) — one aggregate, episode-keyed sections

Three options were considered:

- **(a) One aggregate with an episode-keyed section — RECOMMENDED.**
- (b) One file per episode — **violates robustness rule 1.** Episode completion is a transaction
  whose halves are "mark complete" and "grant reward to wallet"; with per-episode files those
  halves live in different files and a crash separates them. Rejected.
- (c) Global file + per-episode files — same boundary problem at every episode transition, plus
  double the atomic-write machinery. Rejected.

**Design (a):** `SaveDTO` (currently `BoardSaveSystem.cs:235-251`) splits into:

- **Global, top-level (unchanged shape):** `energy`, `wallet`, `locker`, `overflow`, `specials` —
  everything that exchanges value with the wallet stays in the one atomic file, honouring rule 1.
- **New top-level:** `currentEpisodeId` (which episode the player is in) and
  `episodes: List<EpisodeSectionDTO>`.
- **Per-episode section:** `{ episodeId, complete, rows, cols, cells, caseFlow(stepIndex), leads }`
  — exactly the fields §1 identified as per-episode. `complete` is the durable record A2 is
  missing, stamped in the same atomic write as the completion reward grant (rule 5: it is set by
  the same flag-carrying lead activation that grants the reward, one snapshot).

Save writes the current episode's section plus all dormant sections verbatim; load applies only
the `currentEpisodeId` section through the existing `ApplyCells`/`ApplyLeads`/`ApplyCaseFlow`
path. `ApplyCaseFlow` finally starts reading the episode id it has been writing since 0.6.
`SnapshotHash` (`:660-719`) gains the current episode id and per-episode completion bits.
JsonUtility handles `List<T>` of `[Serializable]` classes — no serializer change.

**Schema cost:** bump to **1.0.0**. The `SchemaAtLeast` gate machinery (`:633-640`) and the
JsonUtility auto-instantiation hazard pattern (documented at `:602-631`) already exist to copy.

## 4. Migration for existing 0.9.0 players (Q3)

On load of a `schemaVersion < 1.0.0` file (the existing null-import / schema-gate fold-in
pattern, CLAUDE.md rule 1):

1. Read the flat 0.9.0 DTO exactly as today.
2. Wrap its `rows/cols/cells/caseFlow/leads` into one `EpisodeSectionDTO` keyed by
   `dto.caseFlow.episodeId` — **which in every real save is `e1_the_listener`**, because that is
   what the scene serialized (`Main Merge.unity:192`). Empty/missing id falls back to the
   catalog's first episode.
3. Set `currentEpisodeId` to the same value; `complete = false` (no 0.9.0 player has finished an
   episode that did not exist).
4. Globals pass through untouched. The next `TrySave` writes 1.0.0; `.prev.json` retains the last
   0.9.0 file as the crash fallback, exactly as the locker/stash/specials folds did.

⚠ **The id-mapping trap, named:** the catalog's Episode-1 id must either *be* `e1_the_listener`
or the migration must map it, or every migrated player's progress lands in a section no catalog
entry claims. Since Ep1's story is being re-ruled this week (C8 "The Same House Twice"), the
catalog needs an `aliases`/legacy-id field on the Ep1 entry. One line of design now saves a
data-loss bug later.

Nothing is lost, nothing moves files: same filename, same atomic writer, same fallback chain.

## 5. Flags (Q4) — fold into the aggregate; the prefs store cannot get there

**PlayerPrefs cannot enumerate keys** (`GameFlags.cs:14-17` documents it; the lazy legacy
migration exists *because* of it). Consequences: a prefix scheme (`flag_ep02.*`) can scope new
writes but can never *reset* an episode's flags, never audit them, and grandfathers every
existing Ep1 flag unprefixed forever.

There is also a live crash seam the current split creates: activating a lead writes its state
into the aggregate but its `NarrativeFlags` into PlayerPrefs (`LeadOutcomeMB.cs:84-89` →
`GameFlags.Set :25-31`) — two stores, one transaction, and `e1.ep01.complete` (the completion
half of a reward grant) is one of those flags. That is rule 1's exact bug class, currently
outside the aggregate.

**Recommended: fold `GameFlags` into the aggregate** using the established pattern —
`GameFlags` keeps its public API, mutates memory only, exposes `ExportState`/`ImportState`/
`StateHash`, stored as per-episode string-sets plus a global set inside `SaveDTO`. Flag *set*
timing moves from immediate-prefs to the same debounced snapshot as the lead state that caused
it — strictly better, because the two halves can no longer separate.

**Migration without enumeration** — the honest answer to the sharp edge: you cannot enumerate
the *keys*, but you can enumerate the *domain of names*, because every flag that matters is
declared in content: `LeadData.requiresFlag`/`forbidsFlag`/`NarrativeFlags`, `CaseGraph.Node.setsFlag`
(written at `Assets/App/UI/Dialogue/DialogueRunner.cs:501`), the `aq.lead.<id>.seen` convention
(`CaseFlowLeadBridgeMB.cs:317`), plus a short static list of system flags (hint one-shots,
`aq.hint.<id>` per `HintService.cs:24`). On the null-import path, probe each known name against
all three legacy prefixes with `PlayerPrefs.GetInt`, exactly as `Has()` does today (`:36-46`),
and fold hits into the Ep1 section. A flag no content ever reads cannot matter; a flag content
reads is by construction in the probe list. Legacy keys are deleted after the first successful
save (the `DeleteLegacyKeys` pattern, `SpecialItemsService.cs:178-184`).

**Descope option (ruling R2):** prefix-only, ~0.5 day, if 2 days cannot be spent — accepting no
per-episode reset, the standing crash seam, and permanent Ep1 grandfathering. Not recommended.

## 6. Episode selection and progression (Q5) — the cheapest acceptable thing

- **`EpisodeCatalog` ScriptableObject** (new, one asset): ordered entries of
  `{ episodeId, legacyIdAliases[], title, LeadsDatabase, steps[], completionFlag, shopUnlockFlag, dossierGateFlag }`.
  This replaces the scene-serialized `episodeId`/`steps` (A3), the resolution trigger literal
  (A4), the shop literal (B2), the dossier gate literal (B3) and the screen title (B5) with data.
  `CaseFlowOrchestratorMB` and `LeadsRepository` read the current entry at boot.
- **Selector:** one modal list (the `DossierIndexPopup` / popup pattern is the template): each
  row shows title + state — **Complete ✓ / In progress (resume) / Locked**. Unlock rule: previous
  episode's `complete` bit. No season map, no art beyond the existing popup kit.
- **Transition:** on selection → `BoardSaveSystem.SaveNow()` (`:70-82`) → set `currentEpisodeId`
  in the aggregate → reload the scene (the existing pattern at `CaseResolutionScreenMB.cs:169`).
  The normal boot restore path then does everything: `TryLoad` applies the new episode's section,
  `ReplaceFromDatabase` + `ApplySavedStates` ordering already works (`LeadsRepository.cs:97-117`,
  `:207-264`), and the statics (locker/stash/specials) are re-imported by the same `TryLoad`,
  which defuses the static-bleed hazard an in-place swap would have (statics survive scene loads;
  the restore overwrite is what makes the reload safe). **No new lifecycle code path.**
- **Fresh episode start:** a section that doesn't exist yet = the same "no save" path a fresh
  install takes today (default board seed, database initial states). Zero new code.
- **No replay in v1** (ruling R6): replaying a completed episode would re-grant every lead's
  rewards (`LeadOutcomeMB.cs:55-82`) — a currency printer. Completed episodes show ✓ and stay
  closed until a replay design exists that severs rewards.

## 7. ★ The minimum viable version (Q6)

**Yes — four episodes ship with four `LeadsDatabase` assets, one catalog, an episode-keyed save
section, folded flags, and a selector.** No new scenes, no addressables work, no per-episode
board configs (all episodes share the canonical 3-generator / 7-family economy — assumption
recorded as ruling R4), no dossier rework (new episodes append catalog entries gated on their
own flags), no FTUE changes (once-per-install is correct). The general-purpose episode framework
this job feared is not needed; the episode *is* mostly data already, and the missing 20% is the
save partition and the identity registry.

What it genuinely needs beyond the four databases, proven above: the save partition (A1, A2 —
without it episode 2 *destroys* episode 1), the catalog (A3, A4 — without it episode 2's
completion is undetectable), and the flag fold or at minimum a prefix (A5).

## 8. The bill (Q7)

Solo developer days, systems work only (content authoring not included):

| # | Work | Days |
|---|---|---|
| M1 | `EpisodeCatalog` SO + orchestrator/repository/resolution read from it; retire scene-serialized identity | 1.0 |
| M2 | Save schema 1.0.0: episode-keyed sections, `currentEpisodeId`, `complete` bit, 0.9.0 migration + id alias, `ClearSave`/`SnapshotHash` updates | 1.5 |
| M3 | Crash-boundary suite for M2 (mandatory, rule 2 — see §10) | 0.5 |
| M4 | `GameFlags` fold: memory-backed store, per-episode scoping, probe-based prefs migration, legacy deletion | 1.5 |
| M5 | Crash-boundary suite for M4 (mandatory) | 0.5 |
| M6 | Episode selector modal + locked/resume/complete states + unlock rule | 1.5 |
| M7 | Transition path: SaveNow → pointer → reload; completion stamps `complete` + unlocks next | 1.0 |
| M8 | De-hardcode sweep: B2 shop, B3 dossier gate, B4 hints, B5 title → catalog data | 0.75 |
| M9 | Analytics: `SetUserProperty("episode_id", …)` at Begin + episode id on lead events (B1) | 0.25 |
| | **Must-have total** | **8.5** (range **7–10**) |

Range honesty: 7 if M4 descopes to prefix-only (R2) and M8 shrinks; 10 if migration testing or
selector UI polish bites. Every item lands inside existing patterns — nothing here is research.

**Would be nice (not in the bill):**

| Work | Days | Note |
|---|---|---|
| Addressables episodic groups + loader wiring (C4) | 2–3 | Post-launch download/size concern, not a shipping blocker |
| Dossier/Location state into the aggregate | 1 | Flagged in `SAS/feature-dossiers-v1.md` already ("eventual save-aggregate consolidation pass"); rides along whenever, independent |
| Episode replay with reward-severed leads | 1.5–2 | Requires design ruling first (R6) |
| `EpisodeId` gets `IEquatable`/`==` (C6) | 0.25 | Do opportunistically inside M1 |
| Per-episode analytics on all 13 events as params (beyond user property) | 0.5 | User property covers slicing for launch |

## 9. Fixed in this pass (authorised exception)

`CaseResolutionService` no longer hardcodes the retired episode id `"e1_the_listener"` for
`episode_complete` analytics and `CaseResolvedEvent`. The id now comes from the running caseflow
service (`CaseResolutionService.ResolveEpisodeId()`, falling back to an `"unknown"` sentinel when
no caseflow is up), so it tracks whatever `Begin()` started and stays correct through every
future rename. EditMode coverage: `Assets/Tests/EditMode/CaseResolutionEpisodeIdTests.cs`
(6 tests: id tracks Begin, never the retired literal, sentinel fallback, event carries the live
id, fires exactly once, non-completion leads don't fire). Note the trigger flag
`e1.ep01.complete` is **still** an Ep1 literal by design — retiring it is M1/M8 work and would
have prejudged the catalog.

## 10. Test suites phase 2 must ship (rule 2 obligation)

Templates: `LockerCrashBoundaryTests`, `StashAggregateBoundaryTests`, `SpecialsAggregateBoundaryTests`.

1. **`EpisodeSaveAggregateBoundaryTests`** (M3): mutations memory-only; export/import round-trip
   of multi-section saves; `StateHash` changes-and-returns across episode switch; 0.9.0 file
   migrates into an aliased Ep1 section with globals intact; null-import resets statics; QA reset
   clears all sections; **the §1.4 regression: loading episode B never drops episode A's section**.
2. **`GameFlagsAggregateBoundaryTests`** (M5): memory-only set; round-trip; hash; probe migration
   finds legacy `flag_`/`nar_flag_`/`dlg_flag_` values for every content-declared name; legacy
   keys deleted after first save; null-import resets; QA reset; **episode scoping: same flag name
   in two episodes does not collide**.
3. **`EpisodeCatalogValidationTests`** (M1): unique ids, alias table covers `e1_the_listener` and
   `Ep01`, every entry has a database + non-empty steps + unique completion flag.
4. **`EpisodeTransitionTests`** (M7): switch preserves the dormant episode's section byte-for-byte;
   wallet/locker/stash unchanged across the switch; completion stamps `complete` and unlocks the
   next entry; a fresh section boots the no-save path.
5. Shipped today: `CaseResolutionEpisodeIdTests` (§9). Already existing and relevant:
   `LeadsBarCounterTests` (rebind case), `LeadFlagGateTests`, `GameFlagsUnificationTests` (M4
   must keep these green through the fold).

## 11. Rulings needed

- **R1 — approve the MVP scope and 1.0.0 schema** (§3, §7). This is the gate on phase 2.
- **R2 — flags:** fold into aggregate (recommended, 2d, fixes a live crash seam) vs prefix-only
  descope (0.5d, no reset, seam stands). §5.
- **R3 — locker/stash/specials stay global across episodes** (recommended: zero work, matches
  the fiction that Ally's kit travels). Alternative per-episode partitioning would add ~1.5d and
  a design question about paid locker slots.
- **R4 — assumption to confirm: all four episodes share the one item economy** (3 generators /
  7 families). If any episode needs unique families/generators, per-episode board config enters
  the bill at roughly +1–2d per divergent episode.
- **R5 — Episode 1's canonical id.** Saves in the wild say `e1_the_listener`; the scene says the
  same; tests and Addressables say `Ep01`; the episode itself is being re-ruled to C8. Pick the
  forward id now; the alias table (§4) absorbs the past either way.
- **R6 — no replay of completed episodes in v1** (recommended; replay re-grants lead rewards and
  prints currency until a severed-rewards design exists).
