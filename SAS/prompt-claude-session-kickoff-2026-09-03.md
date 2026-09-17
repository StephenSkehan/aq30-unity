# Session kickoff, 2026-09-03 (written at the close of 2026-09-02)

Read `MEMORY.md` and the Four Keys block of `project_state.md` first, then this. Stephen's rulings in memory are never reopened.

## Where 2026-09-02 left things

**The Four Keys chapter 1 slice is a real episode.** Catalog entry `fk01`, "The Friends with Four Keys", sits FIRST in `EpisodeCatalog` (Stephen-ruled: Four Keys is episode 1; the ep01 id transfers when it ships). It has its own leads database, package catalog, FTUE choreography config, steps, completion flag `fk.ch1.complete` (on the last card) and flag prefixes. Boot priority is the save's pointer, then the catalog's first playable entry, then the scene's legacy id. The old slice toggle, database swap and polling driver are deleted. Dev boot choice: menu **AQ > Dev Boot Episode** (Follow Save / The Listener / Four Keys Ch1; EditorPrefs `aq.dev.boot_episode`, builds ignore it).

**The FTUE choreography is per-episode data** (`FtueChoreographyConfig`, `Assets/App/FTUE/`). Null on an entry = The Listener's shipped constants (pinned by `FtueChoreographyConfigTests`). Four Keys is tap mode: package 1's beat plays up front as the intro, the generator pulses with the gold arrow, the first tap yields Audio T1 (placed deterministically if the drop misses), the card auto-proceeds, package 1 pays without repeating (flag `pkg.fk_p01_01.beat_preplayed`, set after display). The guided case loop then prefers the generator that feeds the current card and points at the Stash when that generator is stashed (two DRAFT banner lines await Stephen's copy ruling: "Place the Lab from the Stash." and "Tap the Lab. Every item helps.").

**Chapter 1 content after the playtest:** 12 packages over 14 cards (09 and 10 split after their third lines; titles ruled), one package never asks the same item twice, all beats carry the On Air studio backdrop (Del segments 9 and 9b: Del bench), cards carry Ally's badge, package 1 grants the lab. The evidence board pins completed packages as scenes (`BoardScene`). Verdict record with every disposition and the five rulings: `SAS/four-keys-ch1-playtest-verdict-2026-09-02.md`. Structure v2.2 chapter 1 table amended in place.

**Tests:** headless EditMode baseline is fully green after the eight week-2/3 scaffold tests were deleted (CLAUDE.md updated). Never run the suite through the mcp-unity bridge while Stephen's editor is open.

## First moves tomorrow

1. `git pull`; confirm main is at the last commit named in `project_state.md`.
2. If Stephen has played: collect the verdict on the opening (intro over the board, first tap at ~13s, lab to Stash, guided loop pointing at the Stash then the lab, evidence board after two beats). Fix bugs first, house way (verbatim record + disposition table in the verdict file).
3. Then the queue, one ruling at a time: the two DRAFT loop banner lines · the DRAFT closing summary on fk01 · chapter 1 line rulings in context (`SAS/four-keys-prose/ch01.md`; Stephen chose to rule after the next full playthrough) · the OPEN working-note lines.
4. Then the standing agenda from `prompt-claude-session-kickoff-2026-09-02.md`: chapter 2 prose via the re-runnable briefs kickoff (Fable blind to spine A to F), build queue (bar grouping for chapter 4, HUD selector scene pass, beat-art placeholder pipeline, per-package analytics), the tester question, the clinic (~Sep 9) then the submit-window ruling, the two season follow-ons before Ep2's spine.

## Known seams to watch

- The Listener now sits second and locks behind Four Keys for fresh saves; Stephen accepted this. The dev override boots it regardless.
- `WK2_BoardDemo.unity` was retired with the scaffold tests (7069957); Main Merge and SampleScene are the only scenes in build settings.
- Package member cards have no per-card dialogue by design; anything that keys on `aq.lead.<id>.seen` sees packages only through their `pkg.<id>.beat_seen` flags (the evidence board and the fk01 shop and dossier gates already do).
