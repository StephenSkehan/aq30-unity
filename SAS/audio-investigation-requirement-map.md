# Audio Investigation — Ep1 Requirement Map (SIGNED OFF)
*v1.1 · 2026-07-17 · integration design for the opening-family swap (art spec: item-icon-regeneration-kit v1.3 Part 4). **Stephen ruled 2026-07-17: L4 lab grant · L5 pure audio · L12 bookend YES.** Art status 2026-07-17: 6 item icons imported + SOs created (421d394, not scene-wired); **blocked only on the 10 gen_audio_rig chain sprites**. gen_audio_rig drop table = the lab's TUNED curve (135/12/4/2/1 + sub-gen 1).*

Shorthand: A=audio_investigation · F=forensic_tools · R=rusty_anchor · D=food_gifts.
T1-equivalents: T2=2 · T3=4 · T4=8 · T5=16 · T6=32.

## 1 · Requirement re-flavor (L1/L2/L3/L5 + L12 bookend)

| Lead | Current | Proposed | T1eq | Narrative beat |
|---|---|---|---|---|
| L1 The Tip Line | 1× F-T2 Evidence Bag | **1× A-T2 Studio Headphones** | 2 → 2 | Ally puts the cans on to hear Dot's voicemail — the Listener listens in minute one |
| L2 The Forty Seconds | 1× F-T3 Full Forensic Case | **1× A-T3 Recorder & Headphones** | 4 → 4 | scrubbing the 40-second recording on the field kit |
| L3 Three Years of Goodnights | 1× F-T4 + 1× F-T2 | **1× A-T4 Broadcast Mic Rig + 1× A-T2** | 10 → 10 | the goodnight-voicemail archive goes on the show |
| L5 Case Alert: Dot Ellis | 1× F-T4 + 1× F-T3 | **1× A-T4 Broadcast Mic Rig + 1× A-T3** | 12 → 12 | going on air — pairs with BG-1B's red tally lamp |
| L12 Goodnight, Harbour | 1× F-T2 (ceremonial) | **1× A-T2 Studio Headphones** | 2 → 2 | the episode closes at the mic the way it opened — headphones on for Dot's last voicemail |

L4/L6–L11 unchanged. Requirement `label` strings update with the GUID swaps.

## 2 · Generator provisioning changes

| Slot | Current | Proposed |
|---|---|---|
| Starting generator | gen_investigation_lab | **gen_audio_rig** (defaultGeneratorFamily swap in Main Merge) |
| L1 grant | gen_junk | unchanged |
| **L4 grant (NEW)** | — | **gen_investigation_lab** via `generatorRewardTypeId` on Lead_E1_Bridge |
| L5 grant | corner_diner | unchanged |

Why L4: `LeadData.generatorRewardTypeId` is a single string, so L5 can't carry both diner and lab. First forensic ask after the swap is L6 (spawned by L5), so a lab granted at L4 has runway while the player works L5's audio asks. Narrative: Gerald confirms it's Dot at L4 — the case gets real, the forensic kit arrives. gen_audio_rig's drop table must mirror the lab's tap→tier curve so a tap costs the same wherever it lands.

## 3 · Economy check

- Family totals: forensic 68 → 38 T1eq · audio 0 → 30 · rusty 56 · food 20. **Episode total unchanged: 144 T1eq.** Rewards untouched (1,365 CC + 40 energy + 5 ingots). Pinch model holds — same tap count, same energy.
- **Watch #1:** L6's F-T5 (16 T1eq) is now the first big ask on a lab that's only existed since L4 — previously the lab ran from minute one. ~18 focused taps between L4 and L6. Phase-2 pinch is by design; if it plays grindy, fallback = grant the lab at L2 (weaker beat).
- **Watch #2:** board carries **4 generators** by L5 (audio, junk, lab, diner) vs 3 today — one more tile of permanent board pressure through Phase 2.
- Audio caps T6, Ep1 asks max A-T4 — same headroom pattern as the other families.

## 4 · Decisions (RULED 2026-07-17)

1. **Lab grant at L4** — CONFIRMED.
2. **L5 pure audio** (A-T4 + A-T3) — CONFIRMED.
3. **L12 bookend F-T2 → A-T2** — CONFIRMED (folded into the table above). Forensic's last ceremonial cameo removed; forensic now appears only via the lab's own asks (L6/L9/L10).

## 5 · Integration checklist (post-sign-off, post-art)

1. 6× ItemDefinitionSO (`audio_investigation_t1–t6`) + item PNGs (kit Part 4 filenames).
2. `GeneratorType_AudioRig.asset` + 10 chain sprites; drop table = lab curve, audio only.
3. Scene: register SOs in itemDefinitions[]/generatorTypes[]; defaultGeneratorFamily → gen_audio_rig.
4. Requirement GUID + label swaps on L1/L2/L3/L5/L12; `generatorRewardTypeId: gen_investigation_lab` on Lead_E1_Bridge.
5. Golden-path doc v2 (table + provisioning + shorthand); Lead Audit; fresh QA-reset headless run.
6. Cleanup rider: delete fingerprint SOs/PNGs/MergeIconCatalog entries (retirement leftovers). press_t04 telephone+message-pad amendment already LOCKED in art-kit-items-and-ui.md.
