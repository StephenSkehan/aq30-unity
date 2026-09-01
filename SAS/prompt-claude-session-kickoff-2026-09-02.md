<!-- pdf-title: Kickoff, session of 2026-09-02 -->

# KICKOFF PROMPT: NEXT CLAUDE CODE SESSION (after 2026-09-01)

*Written 2026-09-01 at session close, 97 per cent context spent, by the session that ran the Four Keys chain from spine kickoff to a playable chapter 1. Paste everything below the line into a fresh session in this repo. Memory (`project_state.md`) is current through tonight and is the authority where this prompt is stale.*

---

You are picking up AQ30 (Ally Quinn: True Crime Merge) the morning after a landmark day. Read `MEMORY.md` and the Four Keys block of `project_state.md` first; they are current. Then read this agenda. Stephen's rulings in memory are never reopened.

### Where everything stands (one paragraph each)

**Story.** Episode 1 is *The Friends with Four Keys*. The chain is complete and attack-hardened: premise v8.1 · spine v1.4 (three GPT rounds; the sweep passes on Margo's six-week silence, the episode's one named character lock) · cold open v0.4 (reader-checked, voice-research register) · economy model v1.1 (ruled: 1,600 T1eq, ~100 packages, no optionals) · structure v2.2 (100 played / 105 authored packages across 10 chapters; GPT five-front attack and the overnight critical review both folded; D3 forks chapter 7 and D4 forks the close, so the agency gate passes; Part F is fully ruled, including the season notch: the deckhand's photographs and Gerald's ask). The full-graph synopsis is `SAS/four-keys-episode-synopsis-v1.0.md`: orientation for you, **never** for Fable, readers, testers or marketing. Chapter 1's prose exists as DRAFT (`SAS/four-keys-prose/ch01.md`, briefs beside drafts, digest at `story-so-far.md`), unruled by design: Stephen chose to play before ruling lines.

**Code.** Everything is merged to `main` (at `59e3bda` tonight): the multi-episode system (play-verified; slot ep01 plays The Listener; HUD selector entry still owed a scene pass), the lead-package spine (`Assets/App/Leads/Packages/`: PackageData, PackageCatalog+Validate, PackageProgressService with the rule-5/rule-6 discipline, PackageRuntimeMB, PackageBeatPresenterMB, 13 EditMode tests), and the **chapter 1 vertical slice**: 12 cards, 10 packages, 10 beat CaseGraphs (v0.4 verbatim plus the Del steps scene; every node's voiceClip slot wired and empty per Stephen's text-first-but-VO-ready ruling), slice DB and catalog under `Resources/App/FourKeys/`, and a dev toggle (menu **AQ → Four Keys Slice → Enabled**, pref `aq.dev.fk_slice`, bootstrap swaps the database and installs the runtime at AfterSceneLoad; off = The Listener untouched). Headless EditMode baseline: 141/150, the 8 failures are the documented legacy scene-dependent set.

**The playable moment.** Stephen can now play chapter 1: pull main, open `Assets/Scenes/Main Merge.unity`, tick the AQ menu toggle, press Play. This is the project's rank-one experiment: does merging change what a player believes; does a beat every couple of minutes pay or interrupt; does the accusation landing on package 4's completion feel earned; does board-first Del land. Slice edges, known and deliberate: text advances on tap, no beat art, Del name-only portrait, the OPEN working-note lines deferred, the Listener caseflow idling harmlessly underneath.

### The agenda, in order

1. **Collect Stephen's playtest verdict** (feel, seams, lines, bugs) and dispose of it the house way: record verbatim in SAS with a disposition table, fix what is mechanical, put rulings to him one at a time with full explanation (he prefers being asked one by one), fold F4's seam re-ruling in the editor if the cut needs re-seaming (words never change without him).
2. **Chapter 1 line rulings in context**, against `SAS/four-keys-prose/ch01.md` (the file shows each brief beside its draft; the three weakest lines are already named in its header). Fold rulings into the file and the digest.
3. **Chapter 2 prose**: re-run `SAS/prompt-claude-fable-briefs-four-keys-kickoff.md`: it is re-runnable by design (resumes from the first missing chapter file) and its blindness discipline is the whole game: Fable subagents get ONLY the standing base plus the digest; you leak-check briefs before and prose after against the spine; chapter 10 last, close last. The previous session's orchestrator agent is gone with that session; the files are the state.
4. **Build queue, after the playtest verdict shapes it**: bar grouping (needed from chapter 4's three-carders), FTUE seam polish per the verdict, the HUD selector scene pass, beat-art placeholder pipeline, per-package analytics. The costing doc (`SAS/feature-lead-packages-v1.md`) prices it; two of its rulings remain (member-card toasts, MVP cut).
5. **Standing items, not yours to forget**: the tester question (cold open v0.4 to the twelve: Stephen's call, the critical reviewer's open objection) · the allergy clinic (~Sep 9) then the submit-window ruling · the two season follow-ons that must precede Ep2's spine: rebase the §3.3 ladder off The Listener, and decide what the deckhand's photograph holds.

### House discipline (the expensive lessons, compressed)

- **Ask Stephen rulings one at a time, with the full context and what-breaks-otherwise stated; options with a recommendation first.** He rejects batches and forms without context.
- **Reader verdicts outrank craft**: plain over crafted; a notch is an action, not a reaction; protect-list lines are not immune (all in `feedback_story_craft.md`).
- **Fable stays blind to spine Parts A to F** until the close briefs. Attack results are saved verbatim with disposition tables; a document only counts as attacked when a result file exists.
- **Check the code before trusting any spec** (`feedback_check_implementation_first.md`). The robustness rules in CLAUDE.md are load-bearing; the package code shows the pattern (state-scan, seen-after-display, paid-idempotence).
- **No em dashes anywhere, in any file.** Markdown renders to html/pdf via `SAS/tools/Convert-MdToPdf.ps1` (PowerShell + headless Chrome; Python 3.12 also exists). ⚠ The Bash tool here chokes on long heredocs: write files with the Write tool and run them.
- **Headless tests**: `Unity.exe -batchmode -nographics -projectPath <path> -runTests -testPlatform EditMode` (editor must be closed for the same project; a git worktree gives you an independent project at the cost of a first import). Code work while Stephen's editor is open goes in a worktree; never switch his checked-out branch without the editor closed and his word.
- **Commit as you go, push at day end, update `project_state.md` at every landing.** The daily nag and the critical reviewer cloud routines are current as of tonight; refresh their prompts when the queue shifts materially.

### First move

Say good morning, check `git log origin/main` and the routines' overnight runs for anything that moved, then ask Stephen for the playtest verdict. Everything else flows from it.
