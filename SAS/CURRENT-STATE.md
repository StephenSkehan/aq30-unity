# CURRENT STATE — single entry point for the SAS pack
*As of 2026-07-17 · branch `claude/ally-quinn-bible-phase-2-p8e642` (strictly ahead of main; landing = Stephen's call). Update this file whenever a ruling lands or an integration flips. If a document below disagrees with this page, this page wins on STATUS; the named master wins on CONTENT.*

## Authority hierarchy (content)
1. `canon-character-roster.md` — character truth (names/roles/appearance)
2. `audio-investigation-requirement-map.md`, `ep1-economy-rebalance.md`, `evidence-locker-casecash-sink.md`, `generator-drop-tables.csv` — systems/economy rulings (live SOs in `Assets/` are ground truth)
3. `item-icon-regeneration-kit.md` — board-art scope/prompts (supersedes art-kit counts)
4. `stage-backgrounds-ep1-kit.md`, `portrait-prompts-canon-cast.md` — environment/portrait art
5. `ally-quinn-bible.md` — narrative canon + season arc (its SYSTEMS chapter defers to #2)
6. `episode-1-golden-path.md` v2 — the exact playable build (QA follows this)

## Live build state (playable right now)
- **Episode 1 fully playable end to end** on Schedule B: 330 T1eq, 1,410 CC + 40 energy + 5 ingots; walls L6/L8/L10/L11; regen 150s; energy cap 100; ladder 10/20/40/80 ingots; ads +20×5/day.
- **Families:** forensic (lab, starting gen), rusty_anchor + gated families (gen_junk, granted L1), food (diner, granted L5). Fingerprint RETIRED. Drop tables = Stephen's 2026-07-17 tuning (spawn-low/merge-up).
- **Evidence Locker LIVE:** 8 free + CC slots 9–12 (200/400/800/1600); locker items satisfy leads (confirm gate on proceed).
- **Board fits all form factors** (BoardFitMB dynamic cells; QA Form Factor Sweep = regression check, 8/8 clean).
- **All 11 stage backgrounds + rivermouth board master live** on the flat illustration language (reconciliation CLOSED).
- Dot portraits live (interim keyed); Ally portraits live; Gerald live (interim keyed).

## Signed but NOT integrated
- **Audio Investigation opening-family swap** — map signed off; 6 item icons imported (unwired); **blocked on 10 gen_audio_rig chain sprites**. When it lands: L1/2/3/5/12 re-flavor, L4 lab grant, starting-gen swap, golden-path v3 in the same commit.

## Known blockers / outstanding deliveries (Stephen's art stream)
| Item | Blocks |
|---|---|
| 10 gen_audio_rig chain sprites | entire audio integration |
| Del transparent-native portrait regen | Del's dialogue presence (L9/L10 beats) |
| Gerald transparent-native regen | replaces interim keyed set (cosmetic) |
| Press family art (Arthur, T1–T10) | gen_junk Ep2 drops |
| audio_investigation_t01 regen (earbuds need pocket case) | 48px readability (cosmetic) |
| ui_top_menu.png (hamburger) | replaces drawn placeholder (cosmetic) |
| VO recordings | audience-level testing |

## Verification debt (from 2026-07-17 external audit — not yet run)
- Evidence Locker crash-boundary tests (kill/relaunch inside store/retrieve/mixed-consumption/slot-purchase windows — board and locker are separate save files; duplicate/loss windows theoretically exist).
- Full Schedule B editor/device pass with real quantity>1 content (L7 easiest) + L11 climax board-pressure playtest (fallback: Schedule A's L11 shape if testers stall).
- Lead-topology economy modeling (L6/L7/L8 are parallel — route order changes pinch timing; current model is aggregate only).

## Crowd-test readiness (two milestones, per 2026-07-17 audit)
- **Usability prototype (board comprehension/navigation): READY** after the verification debt above clears.
- **Audience prototype (judging the podcast proposition): NOT READY** — needs audio opening family live, VO, FTUE first-merge choreography, Del portrait. Testing appeal before these exist risks judging a proposition players never saw.

## Archived documents (do not start work from these)
`Archive/handover-console-ui-pass.md`, `Archive/b4-editor-verification-checklist.md` — consumed 2026-07-13/16; both contain obsolete scene paths and values (banners inside).

## Canonical numbers quick-reference
Episode: 330 T1eq · 1,410 CC · 40 energy · 5 ingots. Energy: cap 100, regen 150s/pt, FTUE 100 + 500 CC + 150 ingots. Drop rates: lab 135/12/4/2/1 +1 sub-gen · diner 135/14/6/3/2/1 +1 · junk/family 140/15/8/4/2/1 +1. Locker: 8 free + 200/400/800/1600 CC. Expected taps ≈225 (family T1eq ÷ per-tap yields 1.44 F/A · 1.74 R · 1.70 D, ×1.10 overhead, −20 L5 grant).
