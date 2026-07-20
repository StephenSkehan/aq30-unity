# Episode 1 — The Listener · Golden Path Walkthrough
*v3 · 2026-07-18 (v2 2026-07-17, v1 2026-07-11) · for editor QA (`AQ > QA Reset + Play`) and device passes. Run BOTH branches (two full runs, or save-scum the L9 choice). **This document describes the exact playable build.***

```text
LIVE BUILD AS OF: 2026-07-18 · commit 46b71a6 (branch claude/ally-quinn-bible-phase-2-p8e642)
Audio integration:    ★ LIVE — Field Kit (gen_field_kit, 6 tiers) is the starting generator,
                      producing audio_investigation only (tuned curve 135/12/4/2/1 + sub-gen 1).
                      Investigation Lab granted at L4. (10-tier gen_audio_rig retired pre-integration.)
Fingerprint family:   RETIRED (2026-07-15)
Economy:              Schedule B (330 T1eq · 1,410 CC) · regen 150s · Evidence Locker live
Known art/wiring gaps: Ally portraits LIVE (green-screen regens) · Dot LIVE (interim keyed) ·
                      Del NOT live (green-screen regen pending) · VO fields empty ·
                      FTUE first-merge choreography pending · hamburger menu = drawn placeholder ·
                      audio t01 earbuds regen pending (audience-test gate)
```

Item shorthand: A=audio_investigation · F=forensic_tools · R=rusty_anchor · D=food_gifts. Tiers are display tiers (T1 = base).

| # | Lead | Unlocks when | Requirements | Rewards | Verify |
|---|---|---|---|---|---|
| 1 | The Tip Line | game start (Available) | 1× A-T2 Studio Headphones | 20 CC · **grants gen_junk generator via overflow** · sets rusty_anchor flag | Dot's voicemail plays FIRST, then SFX caption node; dialogue = 5 nodes; spawns leads 2+3 |
| 2 | The Forty Seconds | spawned by 1 | 1× A-T3 Recorder & Headphones | 20 CC | "the house does when they cross it" — no gender before L7 |
| 3 | Three Years of Goodnights | spawned by 1 | 1× A-T4 Broadcast Mic Rig + 1× A-T2 | 50 CC | fish-hook message beat (worried portrait) |
| 4 | The Volume-Up | leads 2 AND 3 done | 1× R-T4 Beer Bottle *(needs gen_junk from lead 1 + rusty flag)* | 50 CC · **grants Investigation Lab via overflow (2026-07-18 — forensic source for L6/L9/L10)** | Gerald portraits (neutral + worried); Mo's line quotes "Dorothy Ellis" |
| 5 | Case Alert: Dot Ellis | lead 4 | 1× A-T4 + 1× A-T3 | 200 CC + 20 energy · **grants corner_diner generator via overflow** (2026-07-12 fix — food source for L7/L9/L11) | spawns 6+7+8; progress label ticks |
| 6 | The Cold Kettle | spawned by 5 | 2× F-T5 Complete Kit + 1× F-T4 + 2× F-T2 | 95 CC | the coin beat; Gerald's "Don't put that on the show" |
| 7 | The Man Who Came at Noon | spawned by 5 | 2× D-T4 Burger + 3× D-T3 Coffee & Donut | 50 CC | dead lamp + clean plate planted |
| 8 | The Quiet Boats | spawned by 5 | 3× R-T5 Wine Glass Red + 1× R-T4 Beer Bottle | 95 CC | Ally's ANGRY portrait (first use) on final node |
| 9 | Off the Record — **THE BRANCH** | leads 7 AND 8 | 1× F-T4 + 2× D-T2 | 50 CC | **Del Cruz speaks (no portrait yet — name only, expected). CHOICE UI: two buttons. Verify BOTH paths converge; flag set (public/protected)** |
| 10 | Where Dot Went | leads 6 AND 9 | 2× F-T5 + 1× F-T4 | 95 CC | **branch variant node**: public run shows "the queue paid its way"; protected shows "Del's quiet trace" — one only |
| 11 | The Hill Cottage | lead 10 | 2× R-T6 Champagne Flute + 2× R-T5 + 2× D-T4 | 185 CC + **2 ingots** | Dot speaks in person (angry beat = her one use); apology; van glimpse |
| 12 | Goodnight, Harbour | lead 11 | 1× A-T2 Studio Headphones (ceremonial bookend) | 500 CC + 20 energy + **3 ingots** | **branch variant node 2A/2B** (Harbourline named vs withheld); Dot's voicemail; final line; STINGER node; resolution screen fires; cold_case_a + Ep2 teaser appear |

## Post-episode checks
- Cold Case chain: complete Storage Unit 44 → The Second Set spawns → Last Orders → cycles back. Save/reload mid-chain keeps the active cold case (repeatable-lead fix).
- Teaser card "Episode 2: The Ferryman" present, non-proceedable.
- Progress label ends 12/12 (cold cases don't increment it).
- Evidence board: e1 cards cluster PHASE 1 / PHASE 2; cold cases + teaser absent from board.
- Energy: full FTUE tank should carry roughly through Phase 1; pinch expected mid-Phase-2 at the Schedule B walls (L6/L8/L10/L11); regen is 150s/point since 2026-07-17 (dev -50 button to force-test the popup funnel).
- **Evidence Locker (2026-07-17):** long-press an item → STORE moves it off-board (8 free slots; slots 9–12 cost 200/400/800/1600 CC — the CaseCash sink). Locker items still satisfy leads; a Proceed that would draw from the locker shows a "USE LOCKER ITEMS?" confirm. LOCKER button bottom-left, above the overflow pocket. QA drivers under AQ/Dev/QA Locker - *.
- **Form factors (2026-07-17):** BoardFitMB shrinks cell size at runtime so all 9 rows + the corner buttons fit every aspect (was: bottom row clipped on 16:9/4:3). Regression check: AQ/Dev/QA Form Factor Sweep in play mode — 8 clean audit lines = pass.

## Generator provisioning (added 2026-07-12)

Four generators source Ep1's four live requirement families (audio, forensic, rusty, food), granted progressively via the overflow bucket:
- **Field Kit** (`gen_field_kit`, starting generator, 6 tiers) → audio_investigation ONLY — L1/L2/L3/L5/L12. Semantics: the Field Kit CARRIES; audio items LISTEN; forensic PROVES.
- **Investigation Lab** (granted **L4**, 2026-07-18) → forensic_tools ONLY (fingerprint retired 2026-07-15) — L6/L9/L10.
- **gen_junk** (granted L1; `aq.loc.rusty_anchor.active` set at L1 unlocks its bar drops) → rusty_anchor (R) — L4, L8, L11. *(gen_junk's garage/press/helens drops stay locked behind their Ep2+ character flags — correctly inert in Ep1.)*
- **Corner Diner** (granted **L5**) → food_gifts (D) — L7, L9, L11. **This grant was missing** until 2026-07-12: no lead provisioned the food generator, so L7 "The Man Who Came at Noon" (burger + coffee) was **unsatisfiable — a hard Ep1 progression blocker**. Fixed by setting `generatorRewardTypeId: corner_diner` on `Lead_E1_Pod1` (L5), the Phase-2 opener, before any food lead spawns.

## Total arc economy
≈330 T1-equivalents across 12 leads (**Schedule B rebalance 2026-07-17** — walls at L6/L8/L10/L11, regen 90→150s in the same change; model + rationale in SAS/ep1-economy-rebalance.md) · rewards: 1,410 CaseCash + 40 energy + 5 ingots (L10 50→95 band fix with the Evidence Locker sink, 2026-07-17). Fingerprint retired 2026-07-15; the audio swap **went LIVE 2026-07-18 (46b71a6)** — the table above is the shipped state. Family split: audio 30 · forensic 92 · rusty 160 · food 48 T1eq.

## Known-pending (not bugs)
**Dot portraits are LIVE** (interim keyed set imported 2026-07-14; transparent-native regen supersedes eventually). **Del portrait is NOT live** — her nodes show name + line only until the green-screen regen is delivered (the chroma pipeline that shipped Ally's regens makes navy-on-navy keyable — deliver Del on green). VO fields empty until recording lands. **FTUE first-merge choreography is LIVE (2026-07-21):** on a fresh boot, `Resolve_E1_Tip` nodes 1–3 play up front, two audio T1s are pre-seeded, and the first merge auto-proceeds L1 with nodes 4–5 as the payoff (no card tap). The dialogue asset is unchanged — a manual card-tap proceed (fallback paths) still plays all 5 nodes in sequence.
