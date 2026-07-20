# CURRENT STATE — single entry point for the SAS pack
*As of 2026-07-19 · branch **`main`** — **LANDED 2026-07-19** (fast-forward `062a137 → 65cb603`, Stephen-ruled; `claude/ally-quinn-bible-phase-2-p8e642` is history, all future work on main) · **describes commit `65cb603`** (update the hash when the state changes). Update this file whenever a ruling lands or an integration flips. If a document below disagrees with this page, this page wins on STATUS; the named master wins on CONTENT.*

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
- **Corner Diner chain is SIX-TIER (2026-07-18, df35827):** Coffee Supply Crate → Countertop Coffee Station → Coffee Cart → Diner Food Truck → Night Service Window → Corner Diner (hero). Old 10-tier ladder retired (t07–t10 SOs deleted; legacy saves clamp over-max generator tiers on restore). Drop table/economy unchanged. **Chains are not uniformly 10-tier: Field Kit 6 · Corner Diner 6 · investigation_lab 6 (2026-07-19, PPE ladder retired) · gen_junk 10.** Board-art scope: **91/91 COMPLETE (2026-07-19, commit 7dfcda4)** — the six-tier lab chain finals delivered transparent-native and imported in place (pixel-only overwrite, GUIDs/metas preserved, margin-cropped per the full-cell ruling). Ladder: **Lab Supply Cupboard** → Lab Trolley → Lab Workbench → Analysis Station → Cleanroom Airlock → Research Facility. **T1 concept swap FORMALISED 2026-07-19:** Sample Prep Tray REJECTED (read as produced items, not a source; centrifuge alternative also rejected — unrecognisable at 48px). T1 sprite renamed to `gen_investigation_lab_t01_lab_supply_cupboard.png` (GUID kept via meta rename — SO/scene refs bind by GUID, zero reference migration; the misleading filename is gone, which was the point), `investigation_lab_t01` SO displayName → "Lab Supply Cupboard" (itemId/family/tier untouched — saves reference family+tier, never names/paths). T2–T6 sprite filenames still carry old PPE slugs — cosmetic, out of the T1-swap scope. **IMPORT BUG CAUGHT + FIXED during acceptance: the 7dfcda4 pixel-only overwrite left all six spriteMode-2 metas holding sprite RECTS derived from the old art — rects exceeded the new cropped texture bounds, so the importer silently dropped every lab sprite (blank tiles; invisible until now because the lab needs the L4 grant). All six rects re-derived to the cropped full-texture bounds; live-verified rendering on the grid. LESSON (extends the 2026-07-18 slice-rect lesson): a pixel swap on a spriteMode-2 asset is NEVER pixels-only — re-derive the sheet rects whenever dimensions change.** New QA drivers: AQ/Dev/QA Grant Lab Generator (pocket) + QA Drain Pocket Once. Live acceptance run: T1+T1→T2 merge ✓ · lab tap dropped forensic_tools ✓ · both tiers restored from an existing save ✓ · 48/64px legibility ✓. Stephen eyeball: lab chain in real play.
- **Portraits: EP1 CAST COMPLETE ON FINAL ART (2026-07-19, cba33df)** — Ally, Dot, Del, Gerald all on transparent-native/chroma-keyed final sets. Del's L9/L10 dialogue presence is live (her node wiring predated the art).
- **Evidence Locker LIVE:** 8 free + CC slots 9–12 (200/400/800/1600); locker items satisfy leads (confirm gate on proceed).
- **Board fits all form factors** (BoardFitMB dynamic cells; QA Form Factor Sweep = regression check, 8/8 clean).
- **All 10 stage backgrounds plus the rivermouth board master (11 images total) live** on the flat illustration language (reconciliation CLOSED; per-asset matrix in the stage kit).

## Signed but NOT integrated
- *(none — the audio opening integration landed 2026-07-18 in `46b71a6`)*

## Known blockers / outstanding deliveries (Stephen's art stream)
| Item | Blocks |
|---|---|
| VO recordings | audience-level testing |

*(Press family DELIVERED + INTEGRATED 2026-07-20 — 10 icons live, SOs + registry wired, flag-gate QA PASS, dormant until `aq.char.arthur.active`. Hamburger icon DELIVERED + LIVE 2026-07-19. VO is the only outstanding delivery.)*

## Sprint 7 compliance — CODE COMPLETE (verified 2026-07-19)
Audit found Sprint 7 was already implemented (2026-07-09-era session), contrary to later planning notes that called it "unstarted":
- **UMP consent**: `ConsentService` gathers at boot, editor bypass, dev-build EEA debug geography; `AdService` initializes ONLY after ConsentResolved.
- **Settings Privacy tab**: Restore Purchases · Privacy Policy link (indigochimpstudios.com/privacy) · Manage Consent (EEA-only, UMP privacy-options form).
- **ATT**: `NSUserTrackingUsageDescription` injected by IOSPostBuild; the ATT prompt itself is presented by UMP's IDFA message flow.
- **Reset semantics**: full player reset = cold-install (consent wiped, re-gathered on boot — GDPR-correct, documented in GameResetMB).

**Device/console checklist remaining (Stephen, next Mac build):**
1. AdMob console: confirm the UMP consent + IDFA explainer messages are PUBLISHED for the app.
2. Dev build: paste the device hash (printed in Xcode console on first run) into ConsentService's ConsentDebugSettings, verify the EEA form + ATT prompt sequence.
3. Verify Restore Purchases on device (sandbox) + `episode_complete` in Firebase DebugView (emission is code-proven via CaseResolvedEvent firing).

## Verification debt (from 2026-07-17/18 external audits)
- ~~Evidence Locker crash-boundary~~ **RESOLVED 2026-07-19: locker state folded into the board's atomic save aggregate (schema 0.7.0).** `EvidenceLockerService` is runtime-state-only; `BoardSaveSystem` persists it inside `board_state.json` (one atomic tmp→prev→live write via new `AQ.App.Persistence.AtomicSaveFile`, which exposes fault-injection checkpoints). A crash can no longer separate a locker transaction from its board/wallet half — both roll back together to the last consistent save. `locker_state.json` is now a legacy file: migrated on first load of a pre-0.7.0 (or absent) save, deleted after the first successful aggregate save. 13 EditMode fault-injection tests (`LockerCrashBoundaryTests`) crash at every write checkpoint across store / retrieve / mixed-consumption / slot-purchase transitions and assert the recovered aggregate is exactly-old or exactly-new — never a dup, never a loss. Migration gate is schema-version-based because JsonUtility auto-instantiates absent DTO fields (a null check would wipe migrating lockers). **Bonus find (live smoke): a PRE-EXISTING save-wipe bug — Unity fires OnApplicationPause(true) on the first play frame when the editor is unfocused, BEFORE Start()/restore, and its TrySave clobbered the on-disk aggregate with boot-empty wallet/leads/locker (locker previously escaped via its own file; wallet/leads did not). Fixed: TrySave now refuses until WalletRestored — never persist before restore. Verified live: store → debounced aggregate save → quit-save → unfocused re-entry (no clobber) → retrieve restored the item to the board.**
- Full Schedule B editor/device pass with real quantity>1 content (L7 easiest) + L11 climax board-pressure playtest (fallback: Schedule A's L11 shape if testers stall).
- Lead-topology economy modeling (L6/L7/L8 are parallel — route order changes pinch timing; current model is aggregate only).

## Crowd-test readiness (two milestones, per 2026-07-17 audit)
- **Usability prototype (board comprehension/navigation): QA GATE PENDING** — flips to READY only when the verification debt above passes.
- **Audience prototype (judging the podcast proposition): NOT READY** — audio opening ✅ and Del ✅ are now live — remaining: VO and FTUE first-merge choreography only (audio t01 RULED FINAL 2026-07-19 — the spec'd pocket case read as cufflinks at low res; bare recognisable earbuds are canon). Testing appeal before these exist risks judging a proposition players never saw.

## Archived documents (do not start work from these)
`Archive/handover-console-ui-pass.md`, `Archive/b4-editor-verification-checklist.md` — consumed 2026-07-13/16; both contain obsolete scene paths and values (banners inside).

## Canonical numbers quick-reference
Episode: 330 T1eq · 1,410 CC · 40 energy · 5 ingots. Energy: cap 100, regen 150s/pt, FTUE 100 + 500 CC + 150 ingots. Drop rates: lab 135/12/4/2/1 +1 sub-gen · diner 135/14/6/3/2/1 +1 · junk/family 140/15/8/4/2/1 +1. Locker: 8 free + 200/400/800/1600 CC. Production taps ≈**233** · net energy demand ≈**213** (family T1eq ÷ per-tap yields **1.3355 F/A** · 1.7427 R · 1.6975 D, ×1.10 overhead; the L5 +20 reduces energy demand, not taps — corrected 2026-07-18 after external audit caught a lab-yield arithmetic error).
