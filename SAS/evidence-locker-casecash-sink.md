# Evidence Locker — the CaseCash Sink (RULED + IMPLEMENTED 2026-07-17)

**Stephen ruled same day: adopt · prices per sheet · L10 50→95 CC · built immediately (not Sprint 8).** Shipped: `EvidenceLockerService` (AQ.App) + `LockerScreen` (Assembly-CSharp, HUD button above overflow bucket) + TileInfoPopup STORE button + QA-reset/ClearSave riders + `locker_slot_purchased` analytics + 5 QA menu drivers (AQ/Dev/QA Locker - *). Play-verified headlessly: store → panel → retrieve → buy 200 ✓ / 400 ✓ / 800-insufficient refused ✓ → locker_state.json persists purchasedSlots. Note: editor QA menu lives in `Assets/Scripts/UI/Board/Editor/` (Assembly-CSharp-Editor) — `Assets/Editor` (AQ.Editor asmdef) cannot see board types.

*v1 · 2026-07-17 · gives soft currency its first spend. Canon basis: economy v0.2 sheet's "purchasable inventory slots — CaseCash slots 9–12: 200/400/800/1600; ingot slots 13+" (deferred-post-v1 list, proposed pulled forward). Companion decisions: ep1-economy-rebalance.md (Schedule B made board space the pressure point this sink relieves).*

## Problem (historical — as it stood at proposal time, 2026-07-17 morning)
CaseCash was faucet-only: nothing spent it; the HUD counter was pure score. Meanwhile Schedule B raised concurrent build pressure (4 resident generators, 112-T1eq climax ask) — board space became the thing players actually run out of. *(Current faucet: 1,410 CC — includes the L10 band fix ruled with this feature. The cash "+" was later removed outright, 215f2b3.)*

**Scope honesty (post-audit note 2026-07-17):** the locker is a FINITE progression sink — 3,000 CC total across four slots, then it stops consuming. It solves "CC has no value," not long-term soft-currency inflation; season-scale sinks (cosmetics, board utilities) remain future work.

## Current-state reality (matters for scope)
`OverflowBucketService` is an **uncapped, invisible FILO stack** (auto-drips to board; no slots, no UI, own `overflow_state.json`). The sheet's slot pricing presumes a visible 8-slot inventory. So this feature *introduces* capacity + a pocket UI; it does not extend an existing one. Scope is honest-feature-work, not a toggle: ~2 sessions.

## Design — "Evidence Locker"
Off-board storage for board items, skinned as Ally's evidence locker (noir flavor: where a podcaster stashes what she can't pin yet).

- **Store:** new "Store" button in the existing long-press `TileInfoPopup` (deliberately avoids the fragile drag/click input path — no drag-to-locker in v1). Generators cannot be stored (board identity + abuse guard).
- **Retrieve:** locker panel (HUD button or board-edge tab, AQTheme.StylePanel) shows slots; tap a stored item → placed via the existing overflow placement path (`PlaceFromOverflow` pattern — first free cell, refuses when board full).
- **Capacity:** 8 slots free. Slots 9–12 purchasable with CaseCash **200 / 400 / 800 / 1600** (sheet-priced). Slots 13+ (ingots) deferred with the rest of the post-v1 list.
- **Locked-slot row** renders greyed with price tag → tap → confirm → `IWallet.TrySpend` soft → slot unlocks permanently.

## Faucet fit
Ep1 nets **1,410 CC** (with the L10 band fix) → slots 9–10 (600 CC) are comfortably in-episode purchases; 11–12 (2,400 CC) are cold-case-era goals. The sink never gates progression — it sells convenience, so free players lose nothing but tidiness. **No CC→energy path** (canon: ladder/ads only) — locker is orthogonal to the ingot funnel.

## Architecture
- `EvidenceLockerService` (static, mirrors OverflowBucketService): `List<OverflowTileData>` storage + `UnlockedSlots` int; own `locker_state.json`; `LockerChanged` event.
- `LockerPanelMB` (auto-installing, pattern of EvidenceBoardScreen/popups): slot grid, item icons via `MergeBoardController.SpriteForItem`, buy flow, AQTheme styling.
- `TileInfoPopup`: + Store button (hidden for generators / when locker full).
- Wallet: existing `TrySpend` covers it; log `locker_slot_purchased` analytics event (wallet bridge already logs spends).
- **QA Reset MUST add `locker_state.json` to its delete list** — same class of bug as the 2026-07-12 overflow accumulation incident (QA reset file list is synced with ClearSave; extend both).
- Lead Audit untouched; board save untouched (locker is its own file, restore-order independent).

## Guardrails
- Do not touch drag/drop code (known-fragile; store/retrieve rides popups + existing placement paths only).
- Retrieval into a full board must refuse with a toast, not overflow-queue (keeps the two systems distinct).
- ~~Storing a lead-needed item will drop its tick from the board~~ **SUPERSEDED (Stephen-ruled 2026-07-17): locker items COUNT toward lead satisfaction.** LeadRequirementChecker merges locker counts (board + locker); consumption pulls board first, then locker; when a Ready card's proceed would draw from the locker, a ConfirmPopup ("Use locker items?") gates the activation. Locker entries carry itemId (recorded at store time from the ItemDefinitionSO).
- Cold-case repeatable CC grants are the long-tail faucet; if slot 12 sells too fast/slow, retune prices not capacity.

## Decision record (all RULED 2026-07-17, same day)
1. ✅ Adopted as the v1 CC sink (pulled forward from post-v1) — built immediately, not Sprint 8.
2. ✅ Slot prices 200/400/800/1600 CC per sheet.
3. ✅ L10 reward 50 → 95 CC (episode total 1,365 → **1,410**).
4. ✅ v2 same day: locker items COUNT toward lead satisfaction, confirm popup gates locker draw-down on Proceed.

## Implemented behaviour (shipped defb63c + 11c733e)
`EvidenceLockerService` (locker_state.json, entries carry itemId) · `LockerScreen` panel + HUD button · TileInfoPopup STORE button · QAReset/ClearSave riders · `locker_slot_purchased` analytics · checker merges locker counts · consumption board-first-then-locker · FlightFX holds past dialogue. Play-verified: store → panel → retrieve → buy ×2 → insufficient-refusal → ordinary-relaunch persistence.

## Outstanding verification (from the 2026-07-17 external audit — NOT yet run)
Board and locker persist in SEPARATE files; store/retrieve/mixed-consumption cross both. A crash between the two writes could duplicate (store: locker saved before board autosave) or lose (retrieve: locker entry removed before board autosave) an item. **Kill/relaunch tests needed at each boundary:** after board removal · after locker insertion · during retrieval · during mixed board+locker lead consumption · during slot purchase/wallet deduction. Candidate hardening: force a board TrySave immediately after every locker transaction.

## Future tuning
Slot prices retune-able in place; if the locker proves too strong a satisfaction buffer (players pre-stash wall requirements), revisit whether locker items should satisfy leads at full weight; the confirm popup gains a "don't ask again" only after the pinch model is re-checked.
