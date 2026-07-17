# Evidence Locker — the CaseCash Sink (RULED + IMPLEMENTED 2026-07-17)

**Stephen ruled same day: adopt · prices per sheet · L10 50→95 CC · built immediately (not Sprint 8).** Shipped: `EvidenceLockerService` (AQ.App) + `LockerScreen` (Assembly-CSharp, HUD button above overflow bucket) + TileInfoPopup STORE button + QA-reset/ClearSave riders + `locker_slot_purchased` analytics + 5 QA menu drivers (AQ/Dev/QA Locker - *). Play-verified headlessly: store → panel → retrieve → buy 200 ✓ / 400 ✓ / 800-insufficient refused ✓ → locker_state.json persists purchasedSlots. Note: editor QA menu lives in `Assets/Scripts/UI/Board/Editor/` (Assembly-CSharp-Editor) — `Assets/Editor` (AQ.Editor asmdef) cannot see board types.

*v1 · 2026-07-17 · gives soft currency its first spend. Canon basis: economy v0.2 sheet's "purchasable inventory slots — CaseCash slots 9–12: 200/400/800/1600; ingot slots 13+" (deferred-post-v1 list, proposed pulled forward). Companion decisions: ep1-economy-rebalance.md (Schedule B made board space the pressure point this sink relieves).*

## Problem
CaseCash is faucet-only: Ep1 grants 1,365 CC (already banded Easy 20 / Standard 50 / Hard 95 / VH 185 + milestone 200 / finale 500 — the sheet's "flat 100" collision note is stale) and nothing spends it. The HUD counter is pure score; the cash "+" is greyed. Meanwhile Schedule B raised concurrent build pressure (4 resident generators, 112-T1eq climax ask) — board space is now the thing players actually run out of.

## Current-state reality (matters for scope)
`OverflowBucketService` is an **uncapped, invisible FILO stack** (auto-drips to board; no slots, no UI, own `overflow_state.json`). The sheet's slot pricing presumes a visible 8-slot inventory. So this feature *introduces* capacity + a pocket UI; it does not extend an existing one. Scope is honest-feature-work, not a toggle: ~2 sessions.

## Design — "Evidence Locker"
Off-board storage for board items, skinned as Ally's evidence locker (noir flavor: where a podcaster stashes what she can't pin yet).

- **Store:** new "Store" button in the existing long-press `TileInfoPopup` (deliberately avoids the fragile drag/click input path — no drag-to-locker in v1). Generators cannot be stored (board identity + abuse guard).
- **Retrieve:** locker panel (HUD button or board-edge tab, AQTheme.StylePanel) shows slots; tap a stored item → placed via the existing overflow placement path (`PlaceFromOverflow` pattern — first free cell, refuses when board full).
- **Capacity:** 8 slots free. Slots 9–12 purchasable with CaseCash **200 / 400 / 800 / 1600** (sheet-priced). Slots 13+ (ingots) deferred with the rest of the post-v1 list.
- **Locked-slot row** renders greyed with price tag → tap → confirm → `IWallet.TrySpend` soft → slot unlocks permanently.

## Faucet fit
Ep1 nets 1,365 CC → slots 9–10 (600 CC) are comfortably in-episode purchases; 11–12 (2,400 CC) are cold-case-era goals. The sink never gates progression — it sells convenience, so free players lose nothing but tidiness. **No CC→energy path** (canon: ladder/ads only) — locker is orthogonal to the ingot funnel.

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
- Storing a lead-needed item will drop its tick from the board — LeadRequirementChecker counts board only; acceptable v1 (locker panel shows the tick-equivalent later if it confuses).
- Cold-case repeatable CC grants are the long-tail faucet; if slot 12 sells too fast/slow, retune prices not capacity.

## Optional faucet amendment (flag while we're here)
L10 "Where Dot Went" pays Standard 50 CC but is now a 40-T1eq Hard wall under Schedule B → propose **50 → 95 CC** (episode total 1,365 → 1,410). Cosmetic to the pinch model; keeps band logic honest.

## Decisions needed
1. Adopt Evidence Locker as the v1 CC sink (pull forward from post-v1)? Scope ≈ 2 sessions.
2. Slot prices 200/400/800/1600 CC per sheet — confirm or retune.
3. L10 reward 50 → 95 CC: yes/no?
4. Sprint slot: proposed **Sprint 8** alongside its existing economy-tuning remit.

## Implementation checklist (post-sign-off)
1. `EvidenceLockerService` + save + tests-in-editor QA menu (`AQ/Dev/QA Store First Item`, `QA Open Locker`).
2. `LockerPanelMB` + HUD entry point + buy flow.
3. TileInfoPopup Store button.
4. QA Reset + ClearSave file-list extension (`locker_state.json`).
5. Analytics event; golden-path doc note; play-verify store→retrieve→purchase→kill/relaunch persistence.
