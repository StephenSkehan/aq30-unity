# B4 — In-Editor Verification Checklist (for Stephen)

> **ARCHIVED 2026-07-17 — this checklist was completed and its outputs consumed (UI/UX passes #1–#2 + amber batch all shipped). Two corrections for anyone reading it later: the live playable scene is `Assets/Scenes/Main Merge.unity` (Build Index 0), NOT Case_Board_Portrait (a stale prototype stub); and LFS files are real content in this checkout, not pointer stubs.**

*2026-07-12 · the one thing that gates Claude's UI/UX work. Goal: tell Claude which UI items are REAL in the running game vs. still stubs, so he codes the actual gaps and doesn't rebuild what exists. This checkout stores scenes/prefabs/final UI art as git-LFS pointer stubs, so Claude literally cannot see the scene wiring — only you can.*

**Time: ~15 minutes. What Claude needs back: this list filled in (Y/N/notes) + 4 screenshots.**

---

## STEP 0 — open the game scene (1 min)
Open the scene you normally hit Play on to test Episode 1. Most likely **`Assets/Scenes/Case/Case_Board_Portrait.unity`**. If your Build Settings shows a different scene at index 0, open that instead. (Note which scene you used: __________.)

## STEP 1 — the play-through (5 min)
1. Reset the save so you start Ep1 clean: **`AQ ▸ Board ▸ Clear Saved Board State`** (and if you have an `AQ ▸ QA Reset + Play` item, use that instead — the golden path references it).
2. Press **Play**.
3. Play through **the first ~4 leads** (Tip → Forty Seconds → Three Years of Goodnights → The Volume-Up): merge to satisfy each lead's requirement, hit Proceed, watch the dialogue fire.
4. Grab **4 screenshots** (use **`AQ ▸ Dev ▸ Capture Game View Screenshot`**, or just OS-screenshot the Game view):
   - **SHOT 1 — the board + HUD at rest** (top bar and board both visible)
   - **SHOT 2 — a lead card with an unmet requirement** (so I can see the requirement chips)
   - **SHOT 3 — the board while a lead needs an item that's sitting on the board** (this is the key one — see Q7)
   - **SHOT 4 — a dialogue scene playing** (portrait + line)

## STEP 2 — answer these while playing (the observations I need)
Mark Y / N / "not sure" and add a word if useful. Each maps to a UI-audit item.

| # | Question | Y/N/notes |
|---|---|---|
| Q1 | Does the board look like a **rounded panel with a soft drop-shadow sitting on the scene**, or a bare/flat grid? | |
| Q2 | Do the board **cells have a two-tone checkerboard tint** (alternating light/dark), or are they flat / hairline-outlined? | |
| Q3 | Is there a **dark gradient/scrim behind the board** making it the focal plane (background dimmed)? | |
| Q4 | Top bar: is there a **portrait in a frame + three currency capsules** (energy, CaseCash, ingots), each with a **"+" button**? | |
| Q5 | Do those **"+" buttons actually do something** when tapped (energy "+" opens the energy-out popup; ingot "+" opens a store)? | |
| Q6 | Do the **lead cards sit cleanly**, or do they **overlap / clip over the HUD** or collide with their own portrait? | |
| Q7 | **THE KEY ONE:** when a lead needs an item and a matching item is **on the board**, does that board tile show a **green checkmark / tick badge**? (SHOT 3) | |
| Q8 | On a lead card, do the **requirement chips show owned/needed COUNTS** (like "1 / 2"), and is there a **reward-preview row** (coin/energy icons + amounts)? Or just tick-only chips with no reward preview? | |
| Q9 | Do the **popups share one look** — rounded navy panel, same header/close — across energy-out, tile-info, settings, case-resolution, evidence-board? Or are some **flat grey / mismatched**? | |
| Q10 | Does Episode 1 **play end-to-end for the first 4 leads** — merges satisfy requirements, Proceed works, dialogue fires, rewards land, next leads appear — **with no errors in the Console**? (paste any red errors) | |

## STEP 3 — the "has the noir restyle been applied?" question (2 min)
There are editor commands that *apply* the noir styling programmatically: **`AQ ▸ Setup ▸ Restyle HUD (Noir)`**, **`Restyle Board (Noir)`**, **`Restyle Lead Card Prefab (Noir)`**, **`Apply Theme Fonts To Open Scene`**.
- Q11: In your play-through, did the game **already look noir-styled** (navy/teal/amber, the Nunito/Staatliches fonts), or did it look **default/unstyled**? __________
- If it looked unstyled: run those four Restyle commands on the open scene, save, Play again, and tell me if that fixed it. **This single fact decides whether "the UI is done, just not applied" or "the UI needs building."**

## STEP 4 — two quick runtime checks (optional, 3 min)
- Q12 — **Cold-case save/reload:** after finishing Ep1, start a cold case (Storage Unit 44), then stop Play and Play again (or background/foreground on device). Does it **resume the active cold case**, or reset? __________
- Q13 — **FTUE cold-open:** open **`Assets/Scenes/Ep01_ColdOpen.unity`** and Play. Does a **first-merge "rig" choreography** play (guided ~45-second opening), or is the scene empty/stub? __________

---

## How to send it back
Paste this template into chat with your answers, and attach the 4 screenshots (+ any Console errors):

```
Scene used: 
Q1 board panel:        Q2 cells:        Q3 scrim:
Q4 top bar:            Q5 + buttons:    Q6 lead cards:
Q7 BOARD CHECKMARK:    Q8 chip counts/reward row:
Q9 popup skin:         Q10 plays end-to-end (+errors):
Q11 noir applied?:     (if unstyled, did Restyle fix it?):
Q12 cold-case resume:  Q13 FTUE cold-open:
```

That's everything. With this + the screenshots, Claude can turn every PARTIAL/UNKNOWN in the readiness audit into a confirmed done/build, and start the real UI work without guessing.
