# Handover → local console session (UI/UX #2 pass)

*Paste the fenced block below as the first message to a fresh Claude Code console session opened in the local `aq30-unity` project. Written 2026-07-12 by the cloud (web) session that did the bible/episodes/audit work.*

```
You are picking up AQ30 — "Ally Quinn: True Crime Merge," a Unity 6 (6000.3.14f1)
narrative merge game for iOS. A cloud Claude Code session did the writing, canon,
and a build-readiness audit; you're the LOCAL session taking over the engine/art
work, because you can see the real scenes/prefabs and reach OneDrive — the cloud
session could not (scenes were git-LFS stubs there; no local filesystem).

FIRST, SYNC:
- git fetch; the working branch is `claude/ally-quinn-bible-phase-2-p8e642`.
  Pull it. PR #3 (open) carries the current docs + fixes (Ep2 v7, beta-readiness
  audit, PrivacyInfo.xcprivacy, the Ep1 generator fix, the art kit). For the UI
  engineering you may continue on this branch or branch fresh from it — your call.
- Read these first, they ARE your brief:
  - SAS/beta-readiness-audit-2026-07-12.md  (the done/left/beta-blocking map)
  - SAS/ui-ux-improvement-plan.md            (the V1-MUST checklist, items 1–30)
  - SAS/handover-console-ui-pass.md           (this file — the task list + gotchas)

CONTEXT — where the project is (Aug 8 external beta, ~27 days out):
- Episode 1 "The Listener" is CONTENT-COMPLETE as engine data and plays clean
  end-to-end. A hard progression blocker was just fixed (L5 now grants the Corner
  Diner generator via generatorRewardTypeId: corner_diner, so L7/L9/L11 food leads
  are satisfiable). Bible v2.0 merged; Episode 2 is structure-locked, awaiting a
  human table read (not your job).
- Systems are real and solid: IAP (PurchaseService), energy (100 cap / 90s regen /
  FTUE 100), rewarded ads (AdService, +20/5-day), Firebase Analytics + Crashlytics,
  a crash-safe save system (BoardSaveSystem). Ep1 art (4 portrait casts, 4 item
  families, 3 generators, TMP fonts) is imported.
- Noir styling IS applied and live (AQTheme tokens, Staatliches/Nunito fonts).

YOUR JOB: the UI/UX V1-MUST pass (audit section #2). Verified live via a play-test,
so these are FACTS, not guesses:
- The HUD is oversized (~1/3 of the screen). The target is Gossip-Harbor-slim.
  RULING (already made): the top bar is COMPONENT-BUILT, not a background image.
  DELETE the `HUDImage` placeholder (Canvas_Board>GameRoot>SafeAreaRoot>HUDImage,
  sprite HUD3_Transparent_0, ~320px) and rebuild a compact bar from existing
  sprites: ui_meter_pill_9s (pills), ui_top_avatar_frame (portrait), ui_top_energy/
  soft/premium (icons), plus a settings GEAR (currently a "MENU" text button set by
  RestyleHudNoir.cs).
- The board cells are large and sparse; densify toward a readable 7×9 with ~80%
  cell fill and a two-tone checkerboard tint (tokens BoardCellA/BoardCellB exist in
  AQTheme).
- Board requirement CHECKMARK is MISSING: when a lead needs an item and a matching
  item is on the board, that board TILE must show a green tick. The live data
  already exists (LeadRequirementChecker tracks per-item counts); nothing renders
  it on tiles. This is the single highest-value UX gap ("Gossip Harbor's secret").
- The HUD "+" buttons don't do anything — wire energy+ → EnergyOutPopup.Show(),
  ingot+ → the ingot store (EnergyOutPopup doubles as it).
- Popups DON'T share one skin: TileInfoPopup and EnergyOutPopup use the good navy
  AQTheme.StylePanel; the SETTINGS popup is flat grey with a plain red X — bring it
  onto StylePanel + a standard header/close.
- Lead cards: requirement chips are icon+tick only (NO owned/needed counts like
  "1/2"), and the reward-preview row is explicitly disabled (LeadsBarView.cs:~97).
  Add counts + a reward-preview row.

PRIORITY ORDER (biggest visible win first):
1. Board requirement-checkmark on tiles (drive off LeadRequirementChecker). PURE
   CODE — highest leverage.
2. HUD shrink: delete HUDImage, build the compact component bar + gear icon.
3. Board density pass (cell tint/padding/size toward the GH reference).
4. Import the Rivermouth background + add a TUNABLE dark scrim between bg and board
   (a serialized [0..1] opacity; the art is intentionally dark, so tune the scrim,
   don't lighten the art). PNG is at OneDrive: Images/Production 2026 Images/
   Backgrounds/bg_rivermouth_night.png — copy to Assets/Art/UI/Backgrounds/, set
   Texture Type = Sprite (2D and UI), assign to the scene Background image
   (Canvas_Board>GameRoot>PillarBackground or Background) replacing the purple
   stand-in.
5. Wire the HUD "+" buttons.
6. Unify the Settings popup onto AQTheme.StylePanel.
7. Lead-card owned/needed counts + reward-preview row.
THEN the amber batch (lower priority): drop-target cell hover, max-tier item glow,
merge squash-stretch + ring burst, board→card delivery flight, card snap-paging,
popup scale-in/out, and one explicit `story_complete`/`episode_complete` analytics
event (only `resolution_continue` proxies it today).

KEY FILES (verified locations):
- Theme/tokens: Assets/App/UI/AQTheme.cs
- Lead cards: Assets/App/Leads/LeadCardPresenter.cs, RequirementSlotView.cs,
  Assets/UI/.../LeadsBarView.cs (reward preview disabled ~:97; nested canvas ~:29)
- Board: Assets/.../MergeBoardController.cs (rows=9 cols=7), BoardTileView.cs
  (energy badge ~:133), BoardFxPlayer.cs (spawn/merge FX), GeneratorTileAnimator.cs
- Checkmark data source: LeadRequirementChecker.cs (per-item live counts)
- HUD: Assets/Resources/App/UI/Prefabs/HUD.prefab; restyler RestyleHudNoir.cs
  (MENU button ~:37, Ally portrait ~:21); EnergyHudTMP/SoftCurrencyHudTMP/PremiumHudTMP
- Popups: EnergyOutPopup.cs (uses AQTheme.StylePanel/Round), TileInfoPopup.cs,
  GeneratorInfoPopup.cs, ToastService.cs; find the Settings popup script and align it
- Rewards FX (done): FlightFX.cs (currency fly-to-HUD)
- Scenes: Assets/Scenes/Case/Case_Board_Portrait.unity (main), Main Merge.unity,
  Ep01_ColdOpen.unity (FTUE cold-open scene)
- Editor menus: AQ/Setup/Restyle HUD (Noir) | Restyle Board | Restyle Lead Card |
  Apply Theme Fonts; AQ/Dev/Capture Game View Screenshot; AQ/Board/Clear Saved Board State
  (NOTE: the noir restyle is applied via these AQ/Setup commands — if a scene looks
  unstyled after an edit, re-run them.)

STILL-OPEN BEATS (parallel, mostly Stephen's — don't block on them):
- B2 (beta blocker, Stephen): the privacy policy is dated "Effective 1 Sep 2026" —
  needs a live current date + hosted URL for App Store Connect. B1 (PrivacyInfo
  .xcprivacy) is DONE and auto-installs via IOSPostBuild.cs.
- Ep1 VO recording (Stephen): manifest + actor-sides ready at SAS/episode-1-vo-
  recording-manifest.md; drop clips into each dialogue node's voiceClip slot.
- Art generation (Stephen): Rivermouth bg done; Press family (art kit v1.2 Batch F)
  and Rosa neutral portrait have paste-ready prompts; Mo + Arthur approved portraits
  await OneDrive→Assets import.
- FTUE cold-open (~45s first-merge choreography) in Ep01_ColdOpen.unity is still a
  pending wiring task (was never authored) — nice-to-have, not a beta blocker.

GOTCHAS:
- Do NOT push to main. Commit focused, one logical change each. Large binaries via
  Git LFS (this repo had a 2.65 GiB LFS corruption event — keep Library/Temp/build
  out of git).
- Verify UI changes by actually playing (AQ/Board/Clear Saved Board State → Play),
  not just compiling. Capture before/after via AQ/Dev/Capture Game View Screenshot.
- The reference target for sizing/density is Gossip Harbor (compact HUD, dense board,
  state overlays on every actionable item).
- CLAUDE.md at repo root has the architecture rules (assembly boundaries, event bus,
  service locators, the "archived domain merge system is intentionally unused" note).

Start by reading the audit + this file, confirm the branch, then do #1 (the board
checkmark). Report a short plan before large refactors.
```
