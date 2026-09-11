# FTUE & Onboarding v1 — Research, Gap Analysis, 10 Initiatives

**Date:** 2026-08-21 · **Status:** AWAITING RULINGS · **Trigger:** F&F cohort feedback "do not quite understand what to do" (none familiar with merge games). Echoes the earlier cohort's top item "lack of tutorial."

Research base: three parallel studies — (A) FTUE teardowns of the four narrative-merge comps (Merge Mansion, Gossip Harbor, Love & Pies, Travel Town), (B) genre-wide teaching conventions (Merge Dragons, EverMerge, Merge County, Seaside Escape), (C) measured FTUE/onboarding best practices. Full citations in the research appendix at bottom.

---

## 1. The headline numbers worth planning against

| Metric | Value | Source quality |
|---|---|---|
| Installs lost within 2 min of first launch | ~20% | measured (deltaDNA) |
| First session >9 min → D1 | 31% vs 20% for shorter | measured, correlational |
| Casual/puzzle D1 benchmark | ~30% = genre-competitive | measured (AppsFlyer/GameAnalytics) |
| Love & Pies D1 at launch | 60% (story meta alone added ~10pts in tests) | published (Trailmix, Develop 2023) |
| Players who don't read tutorial text | 80–90% | directional |
| Tutorial text budget | ≤8 words on screen (George Fan rule) | strong practice consensus |
| FTUE funnel step losing >20% | treat as a defect | practice consensus |
| Idle-nudge convention | first pulse ~4–8s, escalate | genre convention (Candy Crush) |

## 2. What the best comps actually do (compressed)

- **The board itself is the tutorial** in all four comps: pre-seeded cobwebbed/boxed/sand-covered/locked tiles force the first merges in constrained positions with zero text.
- **Merge Mansion is the reference for chain visibility**: tap any item → info button → full chain view with undiscovered tiers masked as "?" AND the name of the generator that produces it. This is their answer to "where do I get this task item?"
- **Travel Town / Love & Pies**: the moment a board item satisfies an order it is highlighted light green; orders are pinned, always visible, with item pictures.
- **Merge Mansion tasks have a "Show" button** that navigates to the task's location.
- **Teaching is pointer + ≤8 words, in character, at the moment of need** — repeated for every new feature. Never walls of text.
- **Travel Town's new-player safety net**: the first two energy-outs grant 100 free energy.
- **Story-first works** (3 of 4 open on story; Trailmix measured +10pts D1 from the narrative meta) — but the emotional hook is compact and the first interaction comes fast.
- **Documented anti-patterns**: unexplained core-loop structure (Merge Dragons camp confusion), persistent forced arrows, repeated unskippable tutorials, story firehose before play, low tier-legibility art.

## 3. AQ30 gap analysis

What we already have (and it's real): L1 first-merge choreography (seeded pair, pulse guide, auto-payoff), generator-tap and proceed-arrow one-shots, 12 Stephen-ruled contextual hint chips, mergeable-pair glow, requirement ticks on matching board items (Travel Town parity), lead cards with requirement chips + "You have: N", per-popup ?-help, family ladder view behind long-press → SHOW FAMILY.

The structural gap: **our teaching is reactive commentary; the genre's is directive instruction.** Specifically:

1. **The loop is taught once, one-third of it.** L1 choreographs a single pre-seeded merge. The full loop — tap generator, get items, merge up, watch the tick, proceed — is never walked. A merge-newcomer exits L1 having merged once and is then free in a 13-lead episode.
2. **Chain visibility is hidden behind our least discoverable gesture.** The family view answers exactly the F&F question ("what do I do?" = "make this item — here is the ladder — this generator starts it") but it's behind long-press → popup → SHOW FAMILY, and long-press itself is only taught after ten merges.
3. **No requirement → source connection.** A lead chip shows Broadcast Microphone Rig; nothing says the Field Kit generator starts that family.
4. **No stall detection.** If a confused player sits still, nothing escalates. The pair-glow is passive.
5. **First interaction is ~57s of VO away.** Beautiful, but the measured world says 20% of installs die inside 2 minutes.
6. **No retrievable how-to-play.** ?-helps are per-popup; there is no core-loop reference.
7. **No funnel visibility.** We cannot see where confusion begins.

---

## 4. The 10 initiatives

Priority = impact on the F&F symptom ÷ effort. Effort: S (<½ day), M (½–2 days), L (2+ days).

### I1 — Guided Case Loop: extend the choreography through the first full loop · **P1 · M**
After the L1 payoff closes, run one directed sequence teaching the REAL loop on L2: arrow/hand on the generator ("Tap the kit for gear.", energy visibly ticks down) → items drop → pointer pairs the first merge → merge to the required tier → the tick lands ("That's what the lead needs.") → pointer to the card's Proceed. Soft-guide style (dim + pulse, input free) exactly like the existing choreography — the infrastructure (FTUEFirstMergeChoreographyMB, stage flags, pulse/dim code) already exists to extend. All copy ≤8 words, Ally's voice.
*Benchmarks: Merge Mansion pointer-per-feature; Gossip Harbor's forced second order. This is the single highest-impact fix for "don't know what to do."*

### I2 — One-tap chain view with "MADE BY" · **P1 · M**
Promote the family ladder out of hiding: tapping any item shows a compact info affordance (or the existing TileInfoPopup gains prominence) with the full ladder — amber current tier, silhouettes + "?" for undiscovered (we already render this!) — plus a new **MADE BY: [generator icon + name]** row. Same on requirement-chip popups.
*Benchmark: Merge Mansion's info panel is the genre's reference implementation; Love & Pies' Item Path viewer. Our SHOW FAMILY popup is 80% built — this is surfacing, not building.*

### I3 — Requirement chip "SHOW ME" trail · **P1 · S/M**
Tap a lead card's requirement chip → family ladder opens AND the producing generator pulses on the board (reuse the hint-pulse rig). One tap answers "what is this and where does it come from."
*Benchmark: Merge Mansion task "Show" button navigating to the task location.*

### I4 — Stall-detection escalating nudge · **P1 · M**
Board idle (no tap/merge) ~7s AND an action is available → escalate: pair-glow strengthens → after ~7 more seconds an arrow points at the pair (or the generator if no pair exists) → after ~20s a directive chip ("Drag one onto its twin."). Resets on any action; stops firing after N successful unaided actions (trust the player). Suppressed during dialogue/FTUE.
*Benchmark: Candy Crush idle-hint convention (~4–8s); adaptive-help escalation practice. Direct antidote to the tester frozen at the board.*

### I5 — First-session energy safety net · **P2 · S**
First two energy-outs ever: auto-grant +100 with a toast ("The case doesn't sleep. Neither do we. +100 energy."). Flag in PlayerPrefs, counts persisted.
*Benchmark: Travel Town, documented. Removes the worst possible FTUE event (hard stop mid-learning) at zero real economy cost. Monetization unaffected post-net.*

### I6 — First-interaction pacing: skippable, visible intro · **P2 · S/M + ruling**
Keep the story-first opening (Trailmix data supports it) but get the player's hands on the board faster. Recommended: (a) an explicit SKIP INTRO pill on N1–N3 (tap-anywhere currently only skips VO per node, invisibly — twice per node); (b) start the seeded pair pulsing DURING N3 so play begins the instant the intro ends; (c) optional content ruling: could N1–N3 tighten toward ~35–40s?
*Data: 20% install loss in 2 min; core action ≤90s guidance. VO is our identity — skip must exist but stay quiet.*

### I7 — Two-voice hint system: directive tier + flavor tier · **P2 · S**
Keep the 12 noir chips exactly as ruled (they're flavor + soul). Add a small DIRECTIVE tier used only by I1/I4: ≤8 words, imperative, anchored with a pointer ("Drag one onto its twin." / "No pairs left. Tap the kit."). Directive tier retires permanently once the player demonstrates each action.
*Basis: 80–90% don't read; 8-word rule; hierarchy convention hand = do this now, glow = suggestion.*

### I8 — FTUE funnel instrumentation · **P1 · S**
Fire `ftue_step` analytics events: intro_start, intro_skipped, first_merge, payoff_done, L2_gen_tap, L2_first_free_merge, L2_proceed, first_energy_out, session1_end. Firebase already wired. Review after every cohort: any step dropping >20% of the previous = defect.
*Basis: funnel practice consensus. One evening of work; makes every other initiative measurable. Should ship in b7 regardless of all other rulings.*

### I9 — "Case File 101" replayable help · **P2 · S/M**
A four-card how-to-play behind the board's ?: (1) Tap the kit, it gives you gear (2) Match two identical items, drag together (3) Green tick = a lead wants it (4) Fill the card, tap PROCEED. Illustrated with real sprites, ≤8 words per card, AQTheme popup rig.
*Basis: Apple guidance — skipped/missed info must remain retrievable. Also the answer for the tester who dismissed a hint chip too fast.*

### I10 — Board-as-teacher seeding · **P3 · S now, L later**
Light version now: after the FTUE payoff, seed 4–6 mixed T1/T2 items so the first free minutes always contain visible merges (and keep ~20–25% free space on restore, Travel Town's rule). Full version post-launch: "sealed evidence bags" — pre-placed locked tiles that open when a matching item merges beside them (the cobweb/sand mechanic all four comps use as their zero-text tutor). The full version is a real mechanic with save-schema and merge-path implications — not before launch.

---

## Build state (2026-08-26)

**The entire b7 wave is implemented.** It was never "not built"; it is not cut.

| Item | Component | State |
|---|---|---|
| I1 | `Assets/Scripts/UI/Board/FTUE/GuidedCaseLoopMB.cs` | Built, 605 lines, playtested 2026-08-22 |
| I3 | `Assets/App/UI/Leads/RequirementSlotView.cs` + `Assets/Scripts/UI/Common/RequirementHoldPopupBridge.cs` | Built |
| I4 | `Assets/Scripts/UI/Hints/StallNudgeMB.cs` | Built |
| I5 | `Assets/Scripts/App/Services/FtueEnergyNet.cs` | Built |
| I8 | `AQ.App.Analytics.GameAnalytics.LogFtueEvent` | Built; **completed 2026-08-26**, see below |

**I8 funnel, fourteen steps:** `l1_intro_start` · `l1_intro_done` · `l1_first_merge` · `l1_payoff_done` · `l1_ceded` · `gl_start` · `gl_seeded` · `gl_gen_shown` · **`gl_gen_tapped`** · `gl_first_free_merge` · `gl_lead_ready` · `gl_done` · **`first_energy_out`** · **`session1_end`**.

Bold are new on 2026-08-26. `gl_gen_shown` to `gl_gen_tapped` is the pointer-worked signal; `gl_gen_shown` alone could not distinguish "we pointed" from "they understood". `session1_end` gives the funnel a denominator.

**`intro_skipped` is NOT instrumented and cannot be**, because no intro-skip affordance exists yet. It arrives with I6 in the b8 wave. Recorded rather than silently dropped.

**Remaining before b7 cuts:** play-verification, which is Stephen's under the 2026-08-26 tester ruling, plus round-7 and splash.

---

## 5. Recommended packaging

- **b7 wave (before wider F&F):** I8 (instrument first), I1, I4, I3, I5 — the directive spine plus the safety net.
- **b8 wave:** I2, I6, I7, I9.
- **Post-launch:** I10 full version.
- Success criterion for the waves: next cohort answers "what do you do in this game?" correctly without prompting; funnel shows no step dropping >20%.

## Rulings needed from Stephen
1. ~~I1/I4/I7 directive copy tier: approve the split voice and rule the lines?~~ **RESOLVED 2026-08-21 by the Gerald copy sheet and implemented. Re-opened in error on 2026-08-26 because this document was never updated. See COPY SHEET STATUS above.**
2. I6: is a quiet SKIP INTRO pill acceptable on the N1–N3 story open? And is a ~35–40s tightening of N1–N3 on the table, or is v1.1 VO locked?
3. I5: confirm the +100 double safety net against Schedule B (it is additive early-game energy; drop tables untouched).
4. I3/I2: MADE BY row reveals which generator produces a family before the family is discovered — spoiler-safe? (Recommend: show generator only if the generator itself is owned/known.)
5. Priority order sign-off for the b7 wave.

---

## Research appendix (sources)

**Comps FTUE:** Merge Mansion wiki (Starting Out / Game Mechanics / Tasks), Udonis Merge Mansion monetization, Level Winner guides, Gossip Harbor wiki (Day 1-5, Player Guide), PocketGamer.biz Gossip Harbor live-ops, Love & Pies wiki (Day 1), Trailmix Helpshift FAQs 7/9/282/283, Naavik Love & Pies deep dive, Mobilegamer.biz Trailmix retention interview, Level Winner + AppGamer Travel Town, PocketGamer.biz Travel Town deconstruction.
**Genre conventions:** Merge Dragons wiki (Merge Chains, Merging), GDC 2019 Merge Dragons pillars, EverMerge guides (Touch Tap Play, Level Winner), Merge County guides + Play Store reviews, Seaside Escape reviews, Bernstein Merge Dragons critical play, BlueStacks/Lucid Puzzle Merge Mansion references.
**Best practices:** Apple games-onboarding guidance, NN/g reading studies, GameAnalytics FTUE guide + 2025 benchmarks, AppsFlyer Q3 2022 retention benchmarks, deltaDNA first-session study (GameDeveloper.com), Udonis tutorial design, Playio onboarding, Antidote Ankama FTUE case, Candy Crush hint conventions.

---

## COPY SHEET STATUS

⚠ **CORRECTION 2026-08-26, later the same day. The four lines below are NOT in the build and were not applied.**

**The shipped copy is the Gerald copy sheet, Stephen-ruled 2026-08-21**, which was implemented in `GuidedCaseLoopMB`, `StallNudgeMB` and `FtueEnergyNet` and playtested on the 22nd. **This spec document was never updated when that sheet was ruled**, so its draft quotes below survived as apparent open items, the nag list inherited them as "four draft copy lines blocking the b7 cut", and they were put to Stephen a second time on the 26th. They had not blocked anything since 21 August.

### What is actually live

| Beat | Live copy | Ruled |
|---|---|---|
| I1 generator | **Tap the kit. Every item helps.** | 2026-08-21 |
| I1 merge | **A matching pair. Drag them together.** | 2026-08-21 |
| I1 tick lands | *deliberately silent.* `EnterQuiet` clears the banner: loop demonstrated, stop talking | 2026-08-21 |
| I1 proceed | **Lead's gone green. Tap it and proceed.** | 2026-08-21 |
| I4 stall, pair present | **Look for pairs. Drag them together.** | 2026-08-21 |
| I4 stall, no pair | **Nothing doing? Tap the kit again.** | 2026-08-21 |
| I5 energy net | **On me. +100 energy.** | 2026-08-21 |

### The 2026-08-26 alternatives, recorded and NOT applied

Ruled against this document's stale draft quotes rather than against the built behaviour. Held pending Stephen's call on whether they supersede the shipped sheet. Applying line 2 would be a design change, not a wording change: it puts a banner on the tick where 08-21 deliberately goes quiet.

| # | Line | Beat | Words |
|---|---|---|---|
| 1 | **Tap the kit for gear.** | I1. Arrow on the Field Kit, energy ticks down, audio items drop. First instruction in the game. | 5 |
| 2 | **That's what the lead needs.** | I1. Merge completes at the required tier, requirement tick lands on the card. | 5 |
| 3 | **Drag one onto its twin.** | I4. Board idle ~20s, valid pair present, arrow already on the pair. | 5 |
| 4 | **No pairs left. Tap the kit.** | I4/I7. Board idle, NO valid pair, arrow moves to the Field Kit. | 6 |

**Fiction correction, which stands regardless of which sheet wins.** The drafts said "Tap for evidence" and "It makes evidence". The Field Kit produces the **Audio Investigation** family (Earbuds in Case, Studio Headphones, Recorder & Headphones, Broadcast Microphone Rig, Audio Mixing Console, Forensic Audio Workstation). Under the 2026-07-15 family ruling, **"Audio Investigation finds the story. Forensic Tools makes it evidence."** Calling the Field Kit's output evidence gives it the Forensic Tools family's job. No directive-tier line may describe a generator as producing evidence.

**Recorded.** Lines 3 and 4 use two nouns for one concept, "twin" and "pair". Stephen ruled line 4 as "No pairs left" with that cost visible and accepted. If testers report confusion about what makes a matching pair, line 4 is the cheap place to unify.

