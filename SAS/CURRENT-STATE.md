# CURRENT STATE — single entry point for the SAS pack
*As of 2026-07-18 · branch `claude/ally-quinn-bible-phase-2-p8e642` · **describes commit `cba33df`** (update the hash when the state changes; strictly ahead of main; landing = Stephen's call). Update this file whenever a ruling lands or an integration flips. If a document below disagrees with this page, this page wins on STATUS; the named master wins on CONTENT.*

## Authority hierarchy (content)
1. `canon-character-roster.md` — character truth (names/roles/appearance)
2. `audio-investigation-requirement-map.md`, `ep1-economy-rebalance.md`, `evidence-locker-casecash-sink.md`, `generator-drop-tables.csv` — systems/economy rulings (live SOs in `Assets/` are ground truth)
3. `item-icon-regeneration-kit.md` — board-art scope/prompts (supersedes art-kit counts)
4. `stage-backgrounds-ep1-kit.md`, `portrait-prompts-canon-cast.md` — environment/portrait art
5. `ally-quinn-bible.md` — narrative canon + season arc (its SYSTEMS chapter defers to #2)
6. `episode-1-golden-path.md` v2 — the exact playable build (QA follows this)

## Live build state (playable right now)
- **Episode 1 fully playable end to end** on Schedule B: 330 T1eq, 1,410 CC + 40 energy + 5 ingots; walls L6/L8/L10/L11; regen 150s; energy cap 100; ladder 10/20/40/80 ingots; ads +20×5/day.
- **★ AUDIO OPENING LIVE (2026-07-18, 46b71a6):** the **Field Kit** (`gen_field_kit`, six tiers: pouch → satchel → backpack → hard case → trunk → mobile unit) is the starting generator, producing **audio_investigation only** in Ep1 on the tuned curve (135/12/4/2/1 + sub-gen 1). L1/L2/L3/L5/L12 require audio; the **Investigation Lab is granted at L4** (forensic for L6/L9/L10); gen_junk stays the L1 grant, Corner Diner the L5 grant. **Semantics:** Field Kit = how Ally CARRIES/deploys equipment · Audio Investigation = equipment that LISTENS/records/analyses · Lab/Forensic = equipment that PROVES. The Field Kit is deliberately broad enough to host additional portable families later — multi-family drops are intentional future capability, NOT active in Ep1 (would dilute the deterministic opening + need economy retune). The proposed 10-tier `gen_audio_rig` was **retired before integration** (too visually close to the audio items it produced; existed only in docs, never in code/assets).
- **Families:** audio (Field Kit, starting gen), forensic (lab, granted L4), rusty_anchor + gated families (gen_junk, granted L1), food (diner, granted L5). Fingerprint RETIRED. Drop tables = Stephen's 2026-07-17 tuning (spawn-low/merge-up).
- **Corner Diner chain is SIX-TIER (2026-07-18, df35827):** Coffee Supply Crate → Countertop Coffee Station → Coffee Cart → Diner Food Truck → Night Service Window → Corner Diner (hero). Old 10-tier ladder retired (t07–t10 SOs deleted; legacy saves clamp over-max generator tiers on restore). Drop table/economy unchanged. **Chains are not uniformly 10-tier: Field Kit 6 · Corner Diner 6 · investigation_lab 6 (2026-07-19, PPE ladder retired) · gen_junk 10.** Board-art scope recounted from repo: **91 deliverables · 85 done · 6 remaining** (the NEW six-tier lab chain — ART PENDING; six-tier mechanics live on interim sprites, ladder: Sample Prep Tray → Lab Trolley → Lab Workbench → Analysis Station → Cleanroom Airlock → Research Facility).
- **Portraits: EP1 CAST COMPLETE ON FINAL ART (2026-07-19, cba33df)** — Ally, Dot, Del, Gerald all on transparent-native/chroma-keyed final sets. Del's L9/L10 dialogue presence is live (her node wiring predated the art).
- **Evidence Locker LIVE:** 8 free + CC slots 9–12 (200/400/800/1600); locker items satisfy leads (confirm gate on proceed).
- **Board fits all form factors** (BoardFitMB dynamic cells; QA Form Factor Sweep = regression check, 8/8 clean).
- **All 10 stage backgrounds plus the rivermouth board master (11 images total) live** on the flat illustration language (reconciliation CLOSED; per-asset matrix in the stage kit).

## Signed but NOT integrated
- *(none — the audio opening integration landed 2026-07-18 in `46b71a6`)*

## Known blockers / outstanding deliveries (Stephen's art stream)
| Item | Blocks |
|---|---|
| Press family art (Arthur, T1–T10) | gen_junk Ep2 drops |
| ui_top_menu.png (hamburger) | replaces drawn placeholder (cosmetic) |
| VO recordings | audience-level testing |

## Verification debt (from 2026-07-17/18 external audits — not yet run)
- Evidence Locker crash-boundary tests (kill/relaunch inside store/retrieve/mixed-consumption/slot-purchase windows — board and locker are separate save files; duplicate/loss windows theoretically exist). **Hardening target (2026-07-18): forcing a board save after locker transactions only NARROWS the window — proper fix is a transaction journal / idempotent operation IDs, or folding locker state into the board's atomic save aggregate. Add deterministic fault-injection checkpoints for the tests.**
- Full Schedule B editor/device pass with real quantity>1 content (L7 easiest) + L11 climax board-pressure playtest (fallback: Schedule A's L11 shape if testers stall).
- Lead-topology economy modeling (L6/L7/L8 are parallel — route order changes pinch timing; current model is aggregate only).

## Crowd-test readiness (two milestones, per 2026-07-17 audit)
- **Usability prototype (board comprehension/navigation): QA GATE PENDING** — flips to READY only when the verification debt above passes.
- **Audience prototype (judging the podcast proposition): NOT READY** — audio opening ✅ and Del ✅ are now live — remaining: VO and FTUE first-merge choreography only (audio t01 RULED FINAL 2026-07-19 — the spec'd pocket case read as cufflinks at low res; bare recognisable earbuds are canon). Testing appeal before these exist risks judging a proposition players never saw.

## Archived documents (do not start work from these)
`Archive/handover-console-ui-pass.md`, `Archive/b4-editor-verification-checklist.md` — consumed 2026-07-13/16; both contain obsolete scene paths and values (banners inside).

## Canonical numbers quick-reference
Episode: 330 T1eq · 1,410 CC · 40 energy · 5 ingots. Energy: cap 100, regen 150s/pt, FTUE 100 + 500 CC + 150 ingots. Drop rates: lab 135/12/4/2/1 +1 sub-gen · diner 135/14/6/3/2/1 +1 · junk/family 140/15/8/4/2/1 +1. Locker: 8 free + 200/400/800/1600 CC. Production taps ≈**233** · net energy demand ≈**213** (family T1eq ÷ per-tap yields **1.3355 F/A** · 1.7427 R · 1.6975 D, ×1.10 overhead; the L5 +20 reduces energy demand, not taps — corrected 2026-07-18 after external audit caught a lab-yield arithmetic error).
