# b7 Device Pass Checklist (Stephen's TestFlight run)

**Status:** ready, 2026-08-22. b7 = b6 + ~42 commits: audit fixes, crash-consistency hardening (save schema 0.7.0 → 0.9.0), review fixes, full onboarding wave. The risky surfaces are money, save migration, and input, in that order.

## A. Fresh-install FTUE (the headline)
- [ ] Delete app → install b7 → full first run: logo → film → L1 intro → seeded pair pulses WITH ghost-drag demo → merge → payoff.
- [ ] Guided loop on The Volume-Up: Gerald banner "Tap the kit. Every item helps." → generator pulses → pair drops → "A matching pair. Drag them together." + ghost demo → after first free merge the banner goes quiet → lead Ready → "Lead's gone green. Tap it and proceed."
- [ ] Kill the app DURING the L1 intro → relaunch → intro replays from N1 (stage flag no longer burned early).
- [ ] Stand idle 8 / 16 / 30 seconds: pulse → ghost demo → one toast, then quiet. Never during dialogue.
- [ ] Requirement chips: tap one → family ladder opens + source generator pulses after close.
- [ ] Source pulse: only generators for UNSATISFIED requirements pulse; generator in Stash → Stash button pulses; satisfied requirement → its source stops.
- [ ] Settings > Help > CASE FILE 101 opens, reads right, closes.
- [ ] Run energy to zero twice: each time +100 "On me." toast and the tap goes through; third time the normal popup.

## B. b6-save migration (second install pass, or second device)
- [ ] Install b7 OVER b6 with a real mid-game save: board, locker contents, Stash contents, Case Kit counts, CaseCash/ingots, lead states all survive.
- [ ] Force-quit immediately after first launch → relaunch → everything still there (legacy stores deleted, aggregate owns all).
- [ ] Tutorial correctly does NOT run on the migrated save.

## C. Money (sandbox)
- [ ] Buy Starter Pack at full energy → force-quit → relaunch: the +300 energy SURVIVES (the b6 bug).
- [ ] Ingot ladder refill → force-quit fast → relaunch: never charged-without-delivery.
- [ ] Rewarded ad watched to the end → close → force-quit before anything else → relaunch: reward present (or re-granted).
- [ ] Restore Purchases does nothing weird.

## D. Input layer (historically fragile — finger feel)
- [ ] Dialogue: ONE tap per action, VO never cut by its own advance. Back button still works.
- [ ] Splash skip: works with a tap, works with a finger held from before launch.
- [ ] Rest one finger on the screen, tap with another: taps still land everywhere.
- [ ] EPISODE CLOSED screen (QA-drive if needed): corner buttons dead beneath it.
- [ ] Hint chip close-X over the evidence board / locker: closes ONLY the chip.
- [ ] Drag/click alternation on the board still feels right (the ancient fragility).

## E. Reset + misc
- [ ] Triple-tap Reset Game WITH ingots in the wallet → confirms → truly fresh (the b6-era resurrection bug).
- [ ] Stash button icon shows the actual top item (coffee looks like coffee); no STASH label.
- [ ] Dossier / dossier index / profile modal: rounded corners.
- [ ] On-mic dialogue title reads "Ally - Podcasting Echoes of Havenbay"; in-person scenes still "Ally Quinn".
- [ ] Generator info popup says "Salvage Stores".
- [ ] Firebase DebugView: ftue_funnel steps arrive in order during the fresh run.

## Submission reminder (NOT for b7)
`AQ_DEBUG_TAB` stays for tester builds; strip it only for the App Store submission build.
