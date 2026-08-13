# Feature Spec — Subscribers (progression spine) v1

**Status:** SPEC / rulings resolved 2026-08-13, build gated on L7+L11 playtests
**Date:** 2026-08-13
**Scope:** Scenario B (ladder + rewards + cold-case pacing), built so Scenario C (capability progression) layers on later without restructuring.

*(Supersedes the working draft `feature-listeners-v1.md`. Renamed per Stephen's ruling: the metric is subscribers, not listeners.)*

---

## 1. What problem this solves

Not the stall problem. If a player is walled on energy at L8 they are not merging, so any board-earned progression is silent exactly when you wanted it talking. This cannot fix that and should not try.

**The tail problem.** Ep1 is a ~2-day episode (Schedule B: 330 T1eq, ~233 production taps, ~213 net energy). The three cold cases chain A → B → C off the case close and can be burned in one sitting. After that there is nothing until Ep2, which is 6–8 weeks out.

Subscribers is a **progression spine that outlives the episode**: a number earned from board activity, with a reward ladder that paces the cold-case content out instead of dumping it.

### The arithmetic that justifies it

All figures below computed, not estimated (see §4 for the model and its assumptions).

- Earn rate under a realistic T2–T5 requirement mix: **8.05 subscribers per T1eq**
- One full 100-energy tank ≈ 160 T1eq produced (drop tables yield ~1.6 T1eq/tap) ≈ **1,288 subscribers per session**
- Ep1 completion ≈ **8,400 subscribers** across ~2.3 tanks of energy

Cold cases gate at 11,000 / 14,100 / 17,100, i.e. roughly **two sessions apart**.

**Net effect: a ~2.3-session episode becomes a ~12-session lifecycle.** The ladder does not create tail content, it paces the tail content we already have. Every cold case added later slots into the ladder for free.

> ⚠ An earlier draft of this spec published an Ep1 projection of ~10,350 that assumed *every* T1eq is merged all the way to T5 — the most generous possible case. Under a realistic mix the true figure is ~8,400, and the original thresholds put the first cold case 5.7 sessions past the episode close instead of 2. Thresholds below are corrected.

---

## 2. Fiction (RESOLVED)

**Subscribers**, not listeners. Ally has had casual listeners for three years — Dot among them. Episode 1, **"The Listener"**, is the case that converts them into subscribers. The count therefore **starts at 0** and is literally true rather than a fudge: it measures the audience this case committed, not her lifetime reach.

The framing also tightens the Tip Line causality. Subscribers are the people who committed to the show, so they are the ones who call the line. More subscribers means more tips means cold cases surface. The Tip Line is already a canon object-character with a working blink rig, so the loop closes on itself.

No "Level 7" anywhere. Raw count plus a named tier.

**Scale check (assumption, not a ruling):** ~10,000 subscribers off a case that goes citywide is plausible for breakout true crime, and big numbers climb better than small ones. If it reads as inflated at playtest, divide every earn rate and threshold by the same constant — the curve shape is unaffected.

---

## 3. Existing hooks (verified 2026-08-13)

| Hook | Location | Use |
|---|---|---|
| `MergeBoardController.OnItemCreated(family, tier)` | `Assets/Scripts/UI/Board/MergeBoardController.cs:79` | **DO NOT USE for earning.** Fires from six call sites: merge `:358`, generator spawn `:442`, overflow placement `:519`, special upgrade `:1122`, special split `:1139`, post-restore re-fire `:1261`. Counting off it awards subscribers for loading your save. |
| *(new)* `OnTilesMerged(family, resultTier)` | add at `MergeBoardController.cs:358` | The merge site is already its own call site, so this is a one-line addition rather than a filtering exercise. |
| `LeadsRuntimeBus.OnLeadStateChanged(LeadData)` | `Assets/App/Leads/LeadsRuntimeBus.cs:13` | Lead resolution earn source. |
| `ItemDiscoveryService` | `Assets/Scripts/UI/Common/ItemDiscoveryService.cs` | First-time discovery earn source (already dedupes via `aq.discovered.items`). |
| `SpecialItemsService.Grant(SpecialId, amount)` | `Assets/App/Specials/SpecialItemsService.cs:73` | Case Kit rewards. |
| `SpecialItemsService.GrantCassette(path)` | `:96` | **Not used in v1** — see §5. |
| `IAnalytics.LogEvent(name, IDictionary<string,object>)` | `Assets/App/Analytics/IAnalytics.cs:11` | `subscriber_milestone`. |
| `BoardSaveSystem.WalletRestoreCompleted` | `Assets/Scripts/UI/Board/Save/BoardSaveSystem.cs:55` | Boot-order gate for reconciliation (same hook the FTUE choreography waits on). |

Cold cases are `Lead_ColdCase_A/B/C.asset`, chained via `SpawnLeadIds` (A → B → C), 40 CC each, `RequiredLeadIds: []`. They are spawned by `Lead_E1_Close` ("Goodnight, Harbour"), whose `SpawnLeadIds` are `cold_case_a` + `ep2_teaser`.

> ⚠ **Folder naming trap.** The cold cases live under `Assets/Content/GhostStudent/Leads/`, but they are **Episode 1 tail content**, not Ghost Student content. Episode 1 is **"The Listener"** (`Assets/Content/TheListener/`, 13 `Lead_E1_*` leads). "The Ghost Student" is the superseded original Case 1: its `Resolve_GS_*` dialogue assets still sit at the root of `Assets/Content/GhostStudent/`, but no GS leads remain in `LeadsDatabase`. Do not infer case membership from the folder name.

---

## 4. Earn rates

Subscribers per **merge**, scaled by the tier produced:

| Result tier | Subscribers |
|---|---|
| T2 | 5 |
| T3 | 12 |
| T4 | 30 |
| T5 | 75 |
| T6 | 180 |
| T7 | 400 |
| T8+ | 800 |

The curve is ~2.4× per tier against a 2× cost curve, so deep merging pays better per T1eq (5.5/T1eq at T3, 13.9/T1eq at T5). Deliberate: deep merges are the skill expression and the genre rewards them. It back-loads income toward late episode, which is fine.

Other sources:

| Source | Subscribers |
|---|---|
| Lead resolved (standard) | 250 |
| Episode close lead | 1,000 |
| First-time item discovery | 50 |
| Cold case resolved | 500 |

### Model (computed — do not re-estimate these by hand)

Merging one item of tier N consumes `2^(N-1)` T1eq and requires `2^(N-k)` merges producing each tier k. Yield per T1eq therefore rises with merge depth:

| Built to | T1eq | Subscribers | Per T1eq |
|---|---|---|---|
| T2 | 2 | 5 | 2.50 |
| T3 | 4 | 22 | 5.50 |
| T4 | 8 | 74 | 9.25 |
| T5 | 16 | 223 | 13.94 |
| T6 | 32 | 626 | 19.56 |

**The blended rate depends entirely on the requirement tier mix, and that is the number that matters.**

| Mix | Rate | Ep1 total |
|---|---|---|
| All T5 (over-generous, do not use) | 13.94 | 10,349 |
| **Realistic T2–T5 (0.15/0.30/0.35/0.20) — the planning number** | **8.05** | **8,406** |
| Shallow T2–T4 | 6.06 | 7,751 |

**Ep1 projection at the planning rate:** 2,657 (merges across 330 T1eq) + 3,750 (11 leads + close) + 2,000 (~40 discoveries) = **8,406**, landing the player at tier 4 as the episode closes.

**Session yield:** 100 energy ≈ 100 taps ≈ 160 T1eq (drop tables yield E≈1.44 lab / 1.70 diner / 1.74 junk) ≈ **1,288 subscribers**.

Re-run the model whenever the earn table, the drop tables or Schedule B changes. The tier mix is the sensitive input: the all-T5 and shallow cases differ by 2.3× in rate.

---

## 5. Blast radius — the constraint that picks the rewards

Schedule B is tuned but **unvalidated**: the L7 and L11 playtests that confirm the walls have not been run.

### Correction to the first draft

The draft budgeted rewards against CaseCash displacement and came out too conservative. The metric that actually matters is **tap displacement** — does the reward reduce the taps Schedule B assumes? CaseCash displacement is a weak secondary concern because **Mo's Back Room restocks daily and is effectively an unbounded sink**, so CC never runs out of places to go. A free locker slot dents the locker sink by 200 of 3,000 CC (7%) and changes zero taps.

Rewards ranked by **tap** displacement:

| Radius | Rewards | In v1? |
|---|---|---|
| **Zero taps** | Cold case unlock, tier title, Ally line, Search Warrant (pure discovery), free locker slot (storage only) | ✅ |
| **Tap-neutral** | Box Knife (split is the exact reverse of a 2:1 merge, by design) | ✅ |
| **Removes taps** | Carbon Copy, Skeleton Key, Evidence Tag | ❌ v1 — these are what C re-models |
| **Direct income** | Energy, CaseCash, ingots | ❌ v1 |

Everything in the v1 ladder is tap-neutral, so **Schedule B is untouched** and the ladder can be meatier than the draft allowed.

### Cassettes are out (R4, resolved by inspection)

Only one cassette clip exists: `Assets/Resources/App/Audio/Cassettes/cassette_dot_goodnight.mp3`, already double-sourced from Dot's dossier completion (`DossierCatalog.cs:147`) and the first Mo's purchase (`MoShopService.cs:98`), deduped by `GrantCassette`. The other four dossiers carry `completionCassette = null` marked reserved. Two ladder cassettes would need two clips that do not exist, and VO is the bottleneck. Cassettes become the natural ladder reward once more tapes are recorded — the grant path already works.

---

## 6. The ladder (RESOLVED — compressed, every tier tangible)

Eight tiers: four during the episode, four in the tail. No narrative-only tiers.

**Tier names are canon Havenbay districts** (world bible §Districts), tracking *Echoes of Havenbay* spreading through the city Ally is investigating. The title is the bare district name; the milestone toast carries the meaning.

| # | Subscribers | Title | Toast line (game-facing) | Reward | Tap impact |
|---|---|---|---|---|---|
| 1 | 600 | Harbor Ward | "The Anchor regulars are subscribing. Mo says she told them." | 1× Search Warrant | none |
| 2 | 1,800 | Rivermouth | "Rivermouth is listening. Dot's neighbours, mostly." | Free locker slot 9 | none |
| 3 | 3,300 | Stonebridge | "Under the viaduct, someone is playing your show out loud." | 1× Box Knife | neutral |
| 4 | 8,400 | Civic Row | "Civic Row subscribed today. The press scrum noticed." | 1× Search Warrant | none |
| — | *~Ep1 close lands here* | | | | |
| 5 | 11,000 | The University | "Student radio picked you up. You are on a playlist now." | **Cold Case A** + 1× Box Knife | neutral |
| 6 | 14,100 | Highcliff Heights | "Highcliff Heights is listening. Think about what that means." | **Cold Case B** + 1× Search Warrant | none |
| 7 | 17,100 | Kestrel Point | "All the way out to the lighthouse. The signal carries." | **Cold Case C** + free locker slot 10 | none |
| 8 | 21,000 | Echoes of Havenbay | "Echoes of Havenbay. The whole city, now." | Title + Ep2 teaser hook | none |

Three beats this ordering buys:

- **Rivermouth at tier 2** is Dot's own district (world bible: retired Chandler Road school cleaner, Rivermouth). Ep1 is her story, so the show reaching her neighbours early is the right early beat.
- **Civic Row at Ep1 close** is City Hall, the courts and the press scrums. The case going public *is* the episode's ending.
- **Highcliff Heights at Cold Case B** is the moneyed overlook and Voss Group HQ. The people Ally investigates are now listening. That is the noir turn, and it lands in the tail where the player needs a reason to keep going.

Copy check: no em dashes in any game-facing string above, per the hard rule.

CC-equivalent: 480 in-episode, 580 in the tail. Under the tap-displacement framing both are fine, and the tail is starved of rewards anyway so value there fills a vacuum.

Tier titles and Ally lines are **text only in v1** (R3, resolved). VO is the bottleneck and these are the lowest-priority lines in the game. Voicing them later changes nothing structural.

---

## 7. UX

**Milestone toast.** Fires on tier crossing, in play. Does the emotional work at zero HUD cost.

**Profile modal row.** Ally's profile modal already carries portrait, name and the CASE FILE button. Add a subscriber count + current title row there. Fiction-perfect: it is her show's audience, on her screen.

**No HUD pill in v1.** Three pills plus avatar plus settings is already contested, and a fourth is scene surgery through the Rebuild HUD tool (edit mode, then save, per the play-mode-rebuild lesson). Promote later if it earns its place at playtest.

**Post-close target visibility is mandatory.** Once the episode closes, subscribers are the only thing progressing and the grind needs a visible goal. The profile row and the leads surface must both show the next threshold explicitly, e.g. `Cold Case A opens at 13,000 subscribers`. Without this the tail is 3 days of unmotivated merging. This is not polish, it is the difference between the feature working and not.

---

## 8. Architecture

New, all in `AQ.App` except the toast and QA menu:

```
Assets/App/Progression/SubscriberService.cs      // static: prefs, Award(), tier derivation, capability query
Assets/App/Progression/SubscriberLadder.cs       // code-defined tier table (matches DossierCatalog pattern)
Assets/App/Progression/SubscriberRewards.cs      // typed reward enum -> existing grant calls
Assets/App/Progression/SubscriberEarnHooks.cs    // subscribes to merge/lead/discovery events
Assets/Scripts/UI/Progression/SubscriberMilestoneToast.cs
Assets/Scripts/UI/Board/Editor/QASubscribers.cs  // set count, reset, force tier, grant all
```

Modified:
- `MergeBoardController.cs:358` — add `OnTilesMerged` event (one line)
- `LeadData` — additive `RequiredSubscribers` int field (schema-additive, same precedent as `specialRewardId`)
- Ally profile modal — subscriber row
- Leads surface — next-threshold line post-close

**Persistence:** `aq.subs.total` (int), `aq.subs.granted` (pipe-joined tier indices), matching the `aq.specials.cassettes` and `aq.discovered.items` conventions.

### Crash boundary

The locker lesson applies: a milestone crossed and the app killed before save must not double-grant or lose the reward.

**Design: grants are derived, not event-driven.** Persist the total and a granted-set. On boot (after `WalletRestoreCompleted`), reconcile — for every ladder tier whose threshold ≤ current total and whose index is not in the granted set, grant it and record the index. Self-healing, idempotent, correct across any crash point. Crossing several tiers in one award is the same code path as recovering from a crash, so it gets tested twice over.

---

## 9. Forward-compatibility with Scenario C

Two rules, no speculative seams:

1. **The ladder is data.** Reward entries are a typed enum plus payload, so C's reward types are new enum cases and handlers, not a restructure.
2. **Capabilities are queried through `SubscriberService`, never read from the ladder table.** In B the only consumer is the cold-case gate. In C every consumer uses the same call:
   ```csharp
   SubscriberService.CapabilityBonus(Capability.EnergyCap)   // 0 in v1
   SubscriberService.HasUnlocked(Capability.PressFamily)     // false in v1
   ```

That is the entire forward-compat story. Deliberately **not** building unused seams — that is speculative generality and this codebase does not need more of it.

### What C will actually need to change

| C feature | Touchpoint | Difficulty |
|---|---|---|
| Energy cap raises | Wherever the regen cap is read from `EnergyConfig` | The one genuine code change. Drags a full Schedule B re-model with it. |
| Generator drop improvements | Generator drop roll consults a tier multiplier | Moderate; drop tables already round-trip via `SAS/generator-drop-tables.csv`. |
| Family unlocks by tier | Drop-table gating flags | **Cheap — the seam already exists.** The press family is already flag-gated; the subscriber gate substitutes for the flag. |
| Tap-removing specials in the ladder | `SubscriberRewards` enum | Trivial once Schedule B is re-modelled. |
| Board expansion | — | **Declined.** Grid geometry is portrait-fit with `BoardFit` reserving space above the corner buttons, and every drop table and requirement count is tuned to the current board size. High blast radius, low differentiation. |

---

## 10. Tests

`Assets/Tests/EditMode/SubscriberLadderTests.cs`, namespace `AQ.Tests.EditMode` (run_tests needs the **full** name; the short name silently matches zero):

- thresholds strictly monotonic
- award accumulates; tier index derives correctly at, below, and above each boundary
- crossing multiple tiers in one award grants every crossed tier exactly once
- boot reconciliation grants missed tiers exactly once after a simulated mid-grant kill
- repeated boot grants nothing further (idempotence)
- prefs round-trip including empty and malformed granted-set strings
- cold-case gate returns false below threshold, true at and above
- `OnTilesMerged` does not fire on the post-restore re-fire path (regression guard on the `OnItemCreated` trap)

---

## 11. Sequencing (RESOLVED)

**Build after the L7 and L11 Schedule B playtests, before submission.**

Rationale: until the playtests confirm whether the walls read as pacing or paywall, we cannot judge whether the tail needs propping or how long the ladder should be. But D1/D7 baselines get measured at launch and there is only one first read of those numbers, so shipping it in v1 is worth the effort if the playtests support it.

Effort: **~2 days** focused — service, ladder, hooks, toast, profile row, cold-case gate, QA menu, tests.

Cut position: **above** preview video, **below** anything touching monetization or compliance. Scoped so it can be dropped without debris — nothing else in the build depends on it.

---

## 12. Rulings log

| ID | Ruling | Resolution |
|---|---|---|
| R1 | Metric and starting count | **Subscribers, starting at 0** (Stephen). Casual listeners of three years convert to subscribers on this case, so 0 is canon-true and the Tip Line causality tightens. |
| R2 | Reward density | **Compress to fewer, meatier tiers.** In-episode ladder cut 6 → 4, every tier tangible, no narrative-only tiers. |
| R3 | Titles and Ally lines | **Text only in v1.** VO is the bottleneck; voicing later is structurally free. |
| R4 | Cassettes in the ladder | **Out.** Only `cassette_dot_goodnight` exists and it is already double-sourced. Revisit when more tapes are recorded. |
| R5 | Cold-case gate at launch | **Gate all three (A, B, C).** Full tail design; accepts that a player finishing Ep1 sees fewer immediately-available leads than today. |
