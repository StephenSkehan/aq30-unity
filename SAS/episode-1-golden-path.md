# Episode 1 — The Listener · Golden Path Walkthrough
*v1 · 2026-07-11 · for editor QA (`AQ > QA Reset + Play`) and device passes. Run BOTH branches (two full runs, or save-scum the L9 choice).*

Item shorthand: F=forensic_tools · P=fingerprint_evidence · R=rusty_anchor · D=food_gifts. Tiers are display tiers (T1 = base).

| # | Lead | Unlocks when | Requirements | Rewards | Verify |
|---|---|---|---|---|---|
| 1 | The Tip Line | game start (Available) | 1× F-T2 Evidence Bag | 20 CC · **grants gen_junk generator via overflow** · sets rusty_anchor flag | Dot's voicemail plays FIRST, then SFX caption node; dialogue = 5 nodes; spawns leads 2+3 |
| 2 | The Forty Seconds | spawned by 1 | 1× F-T3 Full Forensic Case | 20 CC | "the house does when they cross it" — no gender before L7 |
| 3 | Three Years of Goodnights | spawned by 1 | 1× F-T4 UV Light + 1× P-T2 Lifted Print Tape | 50 CC | fish-hook message beat (worried portrait) |
| 4 | The Volume-Up | leads 2 AND 3 done | 1× R-T4 Beer Bottle *(needs gen_junk from lead 1 + rusty flag)* | 50 CC | Gerald portraits (neutral + worried); Mo's line quotes "Dorothy Ellis" |
| 5 | Case Alert: Dot Ellis | lead 4 | 1× F-T4 + 1× F-T3 | 200 CC + 20 energy · **grants corner_diner generator via overflow** (2026-07-12 fix — food source for L7/L9/L11) | spawns 6+7+8; progress label ticks |
| 6 | The Cold Kettle | spawned by 5 | 2× F-T5 Complete Kit + 1× F-T4 + 2× F-T2 | 95 CC | the coin beat; Gerald's "Don't put that on the show" |
| 7 | The Man Who Came at Noon | spawned by 5 | 2× D-T4 Burger + 3× D-T3 Coffee & Donut | 50 CC | dead lamp + clean plate planted |
| 8 | The Quiet Boats | spawned by 5 | 3× R-T5 Wine Glass Red + 1× R-T4 Beer Bottle | 95 CC | Ally's ANGRY portrait (first use) on final node |
| 9 | Off the Record — **THE BRANCH** | leads 7 AND 8 | 1× F-T4 + 2× D-T2 | 50 CC | **Del Cruz speaks (no portrait yet — name only, expected). CHOICE UI: two buttons. Verify BOTH paths converge; flag set (public/protected)** |
| 10 | Where Dot Went | leads 6 AND 9 | 2× F-T5 + 1× F-T4 | 50 CC | **branch variant node**: public run shows "the queue paid its way"; protected shows "Del's quiet trace" — one only |
| 11 | The Hill Cottage | lead 10 | 2× R-T6 Champagne Flute + 2× R-T5 + 2× D-T4 | 185 CC + **2 ingots** | Dot speaks in person (angry beat = her one use); apology; van glimpse |
| 12 | Goodnight, Harbour | lead 11 | 1× F-T2 Evidence Bag (ceremonial) | 500 CC + 20 energy + **3 ingots** | **branch variant node 2A/2B** (Harbourline named vs withheld); Dot's voicemail; final line; STINGER node; resolution screen fires; cold_case_a + Ep2 teaser appear |

## Post-episode checks
- Cold Case chain: complete Storage Unit 44 → The Second Set spawns → Last Orders → cycles back. Save/reload mid-chain keeps the active cold case (repeatable-lead fix).
- Teaser card "Episode 2: The Ferryman" present, non-proceedable.
- Progress label ends 12/12 (cold cases don't increment it).
- Evidence board: e1 cards cluster PHASE 1 / PHASE 2; cold cases + teaser absent from board.
- Energy: full FTUE tank should carry roughly through Phase 1; pinch expected mid-Phase-2 (dev -50 button to force-test the popup funnel).

## Generator provisioning (added 2026-07-12)

Three generators source Ep1's four requirement families, granted progressively via the overflow bucket:
- **Investigation Lab** (starting generator) → forensic_tools + fingerprint_evidence (F, P) — every F/P lead.
- **gen_junk** (granted L1; `aq.loc.rusty_anchor.active` set at L1 unlocks its bar drops) → rusty_anchor (R) — L4, L8, L11. *(gen_junk's garage/press/helens drops stay locked behind their Ep2+ character flags — correctly inert in Ep1.)*
- **Corner Diner** (granted **L5**) → food_gifts (D) — L7, L9, L11. **This grant was missing** until 2026-07-12: no lead provisioned the food generator, so L7 "The Man Who Came at Noon" (burger + coffee) was **unsatisfiable — a hard Ep1 progression blocker**. Fixed by setting `generatorRewardTypeId: corner_diner` on `Lead_E1_Pod1` (L5), the Phase-2 opener, before any food lead spawns.

## Total arc economy
≈330 T1-equivalents across 12 leads (**Schedule B rebalance 2026-07-17** — walls at L6/L8/L10/L11, regen 90→150s in the same change; model + rationale in SAS/ep1-economy-rebalance.md) · rewards unchanged: 1,365 CaseCash + 40 energy + 5 ingots. Fingerprint (P) asks were remapped to forensic 2026-07-15 (family retired); the audio swap (L1/2/3/5/12 → audio_investigation) is signed off and pending art — table shorthand updates with that integration.

## Known-pending (not bugs)
Dot/Del portraits absent until generated (their dialogue nodes show name + line, no portrait — by design until import). VO fields empty until recording lands. FTUE first-merge choreography (rig merge at ~45s) is a CaseFlow wiring task in the UI phase, not in this data.
