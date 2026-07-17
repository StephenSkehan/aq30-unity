# Ep1 Economy Rebalance — Requirement Schedules A/B (RULED: SCHEDULE B)
*v1.1 · 2026-07-17 · companion: `ep1-economy-scenarios.csv` (full model grid). Written against the signed-off audio requirement map (audio-investigation-requirement-map.md v1.1); tier-for-tier identical if applied pre-audio-swap (A→F).*

**Stephen ruled 2026-07-17: Schedule B · regen 150s · ~2-day Ep1 accepted. IMPLEMENTED same day** — 6 lead assets + EnergyConfig updated. **L10-B amended at implementation:** `4× F-T4 + 2× F-T3` exceeded the `LeadRequirement.quantity` [Range(1,3)] cap, replaced with **`2× F-T5 + 1× F-T4`** (same 40 T1eq, no code change). Quantity>1 satisfaction path code-verified (checker greedy-allocates `cnt >= needed`, consumption loops per quantity, chip badge reads NeededCount) — first live exercise is this content; include a quantity lead in the next editor QA pass.

## Goal
Ep1 currently costs ~100 taps against a 100-energy FTUE tank + 40/hr regen + 100/day ad energy → a free player clears it in one sitting with ≈0 EnergyOutPopup encounters. Add real pacing and monetization surface to Phase 2 **without touching the Act-1 hook (L1–L5) or walling the story climax**.

## ⚠ Structural finding (read first)
The scenario model shows **requirement mass is a ~2-day lever, not a 4-day one.** Daily free energy is dominated by the per-session tank refill (100/session via regen-to-cap between sessions), so even Schedule B (330 T1eq, 2.3× current) completes in ~2 days for a light 1-session player. Regen speed (90→180s) barely moves days-to-complete — it only changes how painful waiting at a wall feels mid-session. Getting to a genre-typical 4–5 day episode would need ~700+ T1eq (quantity stretching that strains board space) **or** tank-side surgery (smaller cap / slower session refill), which touches FTUE. Recommendation below accepts ~2 days for Ep1 and treats deeper metering as a season-scale decision.

## Schedules (L1–L5 and L12 unchanged in both)

| Lead | Current (post-audio) | T1eq | **Schedule A** | T1eq | **Schedule B (recommended)** | T1eq |
|---|---|---|---|---|---|---|
| L1 Tip Line | 1× A-T2 | 2 | — | 2 | — | 2 |
| L2 Forty Seconds | 1× A-T3 | 4 | — | 4 | — | 4 |
| L3 Goodnights | 1× A-T4 + 1× A-T2 | 10 | — | 10 | — | 10 |
| L4 Volume-Up | 1× R-T4 | 8 | — | 8 | — | 8 |
| L5 Case Alert | 1× A-T4 + 1× A-T3 | 12 | — | 12 | — | 12 |
| **L6 Cold Kettle · wall 1** | 1× F-T5 + 1× F-T2 | 18 | 2× F-T5 + 2× F-T2 | 36 | 2× F-T5 + 1× F-T4 + 2× F-T2 | 44 |
| L7 Man at Noon | 1× D-T4 + 1× D-T2 | 10 | 2× D-T4 + 2× D-T2 | 20 | 2× D-T4 + 3× D-T3 | 28 |
| **L8 Quiet Boats · wall 2** | 1× R-T5 | 16 | 2× R-T5 + 1× R-T4 | 40 | 3× R-T5 + 1× R-T4 | 56 |
| L9 Off the Record *(branch — stays light)* | 1× F-T4 + 1× D-T2 | 10 | 1× F-T4 + 2× D-T2 | 12 | 1× F-T4 + 2× D-T2 | 12 |
| **L10 Where Dot Went · wall 3** | 1× F-T4 + 1× F-T3 | 12 | 3× F-T4 + 1× F-T3 | 28 | 4× F-T4 + 2× F-T3 | 40 |
| **L11 Hill Cottage · climax wall** | 1× R-T6 + 1× D-T4 | 40 | 2× R-T6 + 1× R-T4 + 2× D-T4 | 88 | 2× R-T6 + 2× R-T5 + 2× D-T4 | 112 |
| L12 Goodnight, Harbour | 1× A-T2 | 2 | — | 2 | — | 2 |
| **Episode total** | | **144** | | **262** | | **330** |

## Model method (documented 2026-07-17 post-audit — the grid is reproducible from this)
- **Per-tap T1eq yields** (drop-table expectation, sub-generator entries included in the denominator at 0 T1eq): lab/audio 223/155 = **1.4387** · junk-per-family 298/171 = **1.7427** · diner 275/162 = **1.6975**. *(An external recheck derived ~1.336 for the lab and ~233 total taps — that treats sub-gen rolls differently; THIS is the canonical method: expected taps = Σ(family T1eq ÷ family yield) × 1.10 merge-alignment overhead, then −20 taps for the L5 energy grant. Schedule B: (30+92)/1.4387 + 160/1.7427 + 48/1.6975 = 204.9 × 1.10 ≈ 225.)*
- **Assumptions:** sessions start at the full 100 cap (gaps ≥ regen-to-full); ads = +20 energy each at the archetype's ads/day; sub-generator drops consume a tap and yield no requirement progress; the 1.10 overhead covers merge-composition misalignment + end-of-episode leftovers; locker use assumed neutral to board efficiency.
- **Known limitation — lead topology is NOT modeled:** L6/L7/L8 are parallel (all spawn from L5), so "wall 1/wall 2" ordering and popup timing vary by route (rusty-first vs forensic-first players see different congestion). The grid is aggregate-mass only; route modeling (lowest-tier-first / story-order / single-family-focus) is future work if crowd-test data shows route-dependent stalls.

## Model outcomes (from the CSV; new drop tables, audio rig = new lab curve, 10% merge-alignment overhead, L5 +20 credited)

| Schedule | Expected taps | Light (1 sess/day) | Engaged (2 sess + 2 ads) | Popups (light) |
|---|---|---|---|---|
| Current 144 | ~100 | 0.7 days | 0.3 days | 0 |
| A 262 | ~180 | 1.4–1.5 days | ~0.6 days | 1 |
| **B 330** | **~225** | **1.8–1.9 days** | ~0.7 days | 1–2 |

## Recommendation
1. **Adopt Schedule B.** Walls at L6/L8/L10/L11 land between story beats; branch lead L9 and both podcast bookends stay frictionless. **On L11, stated honestly: the CLIMAX is deliberately the episode's largest earned build (112 T1eq); the FINALE (L12) is frictionless.** "Don't wall the climax" in the goal statement means don't wall the *resolution payoff* — the build-up to the cottage is meant to be earned. Crowd-test fallback threshold: if testers routinely need 3+ sessions inside L11 alone, exhaust energy twice within it, or abandon after L10, drop to Schedule A's L11 shape (2× R-T6 + 1× R-T4 + 2× D-T4 = 88).
2. **Regen 90 → 150s/point.** Doesn't change days-to-complete; makes waiting out a wall slow enough that the popup's ad/refill offer is genuinely tempting. (180s starts to feel punitive against a 100 tank.)
3. **Accept ~2-day light-player completion for Ep1** and buy long-tail pacing elsewhere: cold-case chain is the elder game, Ep2 cadence is the real content meter. If 4–5 day metering is wanted later, that's a tank-cap/segment decision for a future sprint, not more quantity stacking.

## Guardrails / notes
- **Board space:** L11-B (two T6 + two T5 rusty builds concurrent) is the heaviest single ask — with 4 generators resident it's feasible on the current grid but should be play-tested first; fallback is L11-A's shape.
- Quantity>1 requirement chips (owned/needed badge, LeadRequirementChecker.GetLiveCount) are already built but have **never been exercised by real content** — this schedule is their first real test.
- Rewards unchanged; CC/ingot totals untouched. Golden-path doc gets a v2 on adoption.
- Implementation on sign-off = quantity/tier edits on 6 lead assets + EnergyConfig.RegenSecondsPerPoint + Lead Audit + a fresh golden-path QA run.

## Decisions needed
1. Schedule A or **B** (or amended values)?
2. Regen 90 → **150s**: yes/no?
3. Accept ~2-day Ep1 (defer deeper metering to season scale): yes/no?
