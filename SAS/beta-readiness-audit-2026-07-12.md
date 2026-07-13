# Beta-Readiness Audit — AQ30 "The Listener"

*2026-07-12 · static audit of `main`+bible branch against the UI/UX plan, the Ep1 golden path, and the market-research beta bar. **Beta (external/TestFlight): Aug 8 — 27 days. Content lock: Aug 1 — 20 days. Submit: Aug 20 — 39 days.***

## Headline

**The project is in good shape for the beta window.** Episode 1 is content-complete as engine data; the money/energy/save/analytics stack is real and carefully built; Ep1's art (portraits, item icons, fonts) is imported. **The beta is not gated by any large unbuilt system** — it's gated by a short, concrete list: one hard store-submission blocker, one high-value UI gap (the board checkmark), and a batch of verification that can only be done in-editor because this checkout stores scenes/prefabs/UI-art as **git-LFS stubs**.

**Read this caveat first:** this audit is *static* (code + data + asset presence). Scene files, prefabs (`HUD.prefab`, `LeadCard.prefab`, board), and final `Art/UI` PNGs are LFS pointer stubs here, so anything that lives in scene/prefab *wiring* is marked PARTIAL/UNKNOWN even where the code signals are strong. Several PARTIALs are probably DONE in the actual editor — but confirming that **is itself a task** (only you can, in-editor), and it's where a hidden blocker could lurk.

---

## ✅ Done — not a concern for the beta

- **Episode 1 content & flow (100%).** All 12 leads authored and registered in `LeadsDatabase`; requirements/rewards/gates/spawns match the golden path **exactly** (economy sums to 1,365 CC + 40 energy + 5 ingots). The L9 PUBLIC/PROTECTED branch is wired via CaseGraph flags (`aq.e1.truth_public/protected`) with correct L10 and L12 variant nodes; cold-case loop (Storage Unit 44 → Second Set → Last Orders → loops) and the non-proceedable Ep2 teaser are complete. Dialogue text spot-checks verbatim against the VO lock, including the earned single `angry` uses (Ally L8, Dot L11).
- **Economy & IAP.** `PurchaseService` (Unity IAP, 4 consumables + ingot-priced energy ladder + 1 rewarded placement) matches the Part-4 beta bar. Energy config is exact: cap 100, regen 90 s/point, FTUE grant 100. Energy-out popup + rewarded-ad funnel (Google Mobile Ads, +20/5-per-day, consent-gated) wired.
- **Analytics & crash.** Firebase Analytics + Crashlytics installed and wired at real call sites (ftue_step, card_state/submit, spawn/merge, energy gain/spend, iap_*, ad_*, lead_* funnel). D1/D7 come free.
- **Save/Load — better than the tech-debt note claimed.** Atomic tmp→prev→live writes, `.prev` corruption fallback, **premium currency persisted immediately** (a crash can't eat a purchase), pause/quit saves, offline-regen recompute, clock-rollback clamps. Robust; the residual debt is that it's unproven under fault injection, not that it's absent.
- **Ep1 art & fonts.** All four Ep1 portrait casts (Ally, Gerald, Dot, Del) with full 7-emotion sets imported and GUID-wired into dialogue; all four Wave-1 item families (33 icons) + 3 generators + currency icons imported; TMP display+body font pair generated and swept. *(This closes two items the golden path still listed "pending": Dot/Del portraits and item art.)*

---

## 🔴 True beta blockers — must clear before Aug 8

| # | Blocker | Why it blocks | Owner | Effort |
|---|---|---|---|---|
| B1 | **Missing `PrivacyInfo.xcprivacy` privacy manifest** | Apple requires it for TestFlight/App Store processing when SDKs use required-reason APIs — Firebase *and* Google Mobile Ads both do. Will likely **reject the upload**. This is the single hardest blocker. | Claude (author manifest) | S |
| B2 | **Privacy policy is a draft dated "Effective 1 Sept 2026"** | App Store Connect needs a live, current-dated policy URL at submission; 1 Sept is *after* the Aug 8 beta. | Stephen (host/date) + Claude (finalize text) | S |
| B3 | **Board requirement-match checkmark (UI item 10)** — MISSING | The plan calls it "Gossip Harbor's real secret / the most important single UX device." The live data already exists (`LeadRequirementChecker` tracks per-item counts) but **nothing renders a tick on matching board tiles**. Highest-leverage UX gap; without it the board↔story connection is invisible. | Claude | M |
| B4 | **In-editor verification of LFS-stub wiring** | Board container/scrim, HUD capsules + "+" buttons, lead-card layout, portrait-fix, resolution screen — all PARTIAL/UNKNOWN here because they're scene/prefab-only. Could hide a real blocker. Must be eyeballed in a running editor before you trust the beta. | Stephen (editor pass) — I can give a checklist | M |

---

## 🟡 Should-fix for beta quality (not hard blockers)

Grouped; most are small. The UI ones assume B4 confirms the scene baseline.

**Lead cards & board feel** — reward-preview row is explicitly disabled (`LeadsBarView.cs:97`) and requirement chips show tick-only, **no owned/needed counts** (item 23); no card snap-paging / next-card peek (25); no drop-target cell hover (3, MISSING); no max-tier item glow (12, MISSING); merge "juice" is a uniform pop, not squash-stretch + ring burst (13); no board→card delivery flight (14, currency-fly-to-HUD *is* done).

**Popups & buttons** — no standard reusable button component or pressed-state/44 pt enforcement (29); popups appear instantly, no scale-in/out or standard close-X (30); toast panel uses a flat sprite, not the AQTheme skin (16).

**Art (soft)** — `ui_stamp_public` / `ui_stamp_protected` art MISSING (the PUBLIC/PROTECTED-truth resolution beat — text can be runtime-overlaid as a fallback, so soft); `bg_rivermouth_night` scene background MISSING (caseboard backdrop stands in, board covers 60% — soft); settings **gear icon** MISSING (currently a "MENU" text button).

**Instrumentation** — add an explicit `story_complete`/`episode_complete` event (only `resolution_continue` proxies it today) so the beta funnel's completion KPI is unambiguous. Trivial.

---

## 🟢 Explicitly post-beta / post-Ep1 (do NOT spend beta time on)

- Import the already-approved **Mo + Arthur** portrait sheets from OneDrive; then generate the ~15 "proposed" portraits (Helen, Priya, Cortez, Vega, Rosa, antagonists) — all later-episode.
- **Press** item family (Ep2 P0) and **WITNESS SHIELDED / TRUTH IN DAYLIGHT** stamps (Ep2 branch).
- Ingot pack tiers 4–5 (400/1500) — only 3 of 5 exist; fine for beta.
- `google-services.json` for **Android** Firebase — only needed if Android is in the beta (iOS TestFlight doesn't need it).
- **Cleanup:** item art is duplicated under two naming schemes (`food_gifts` vs `stakeout_fuel`, `Icons/Items` vs `Icons/MergeChains`) — consolidate before it causes a wrong-sprite bug.
- **VO-LOCK pickup:** `Resolve_E1_Trail` (L10) merged two nodes and added branch-variant lines the lock's cue sheet doesn't record — reconcile the master or log the pickup. Lock hygiene, not gameplay.
- `LeadData.BranchOutcomes` is empty on all leads — correct (the branch runs on dialogue flags); only revisit if a future system reads that field.

---

## Recommended sequencing (27 days)

**This week — unblock & de-risk:**
1. **B4 first** — you run the in-editor pass (I'll hand you a targeted checklist from the PARTIAL list) so we learn what's *actually* left vs. LFS-stub noise. Everything below is scoped by what that finds.
2. **B1** — I author `PrivacyInfo.xcprivacy` (Firebase + GMA required-reason declarations). Cheap, unblocks TestFlight.
3. **B3** — I build the board requirement-checkmark on the existing `LeadRequirementChecker` data. Highest-leverage UX.
4. **B2** — finalize + date the privacy policy; you host it.

**Then — beta-quality polish (the amber batch), in priority order:** card owned/needed counts + reward-preview → PUBLIC/PROTECTED stamp (art or programmatic) → popup scale animation + standard button → snap-paging → gear icon + remaining small FX. The `story_complete` event slots in whenever I'm in the analytics code.

**Parallel, your side:** Ep1 VO recording (manifest already delivered — the schedule-critical path); Rivermouth background generation if you want it over the stand-in; and the B4 editor pass gates my UI work, so that's the first ask.

**Bottom line:** no heroics needed for Aug 8. The content and systems are there. Clear B1–B4, work the amber batch as time allows, and the differentiator — the writing and the loop — is already in the build.
