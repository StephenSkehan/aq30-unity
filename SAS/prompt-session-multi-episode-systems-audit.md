# PROMPT: new Claude Code session — the multi-episode systems bill

**Paste everything below the line into a fresh Claude Code session in `C:\users\user\dev\aq30-unity`.**

Written 2026-08-27. Ground-truth facts in it were verified against the code that day; the session is told to re-verify rather than trust them.

---

## PASTE FROM HERE

You are working on **AQ30 (Ally Quinn: True Crime Merge)**. Read `CLAUDE.md` first. This is a code and architecture task with no story content in it.

### The constraint that creates this job

Stephen has ruled that **the first release ships AT LEAST FOUR EPISODES. The episode count is fixed; the 1 October date is not.** Every previous plan assumed one episode. The game currently has no multi-episode support of any kind, and nobody has ever measured what adding it costs.

**Your job is to measure that bill and design the cheapest thing that satisfies it. Your job is NOT to build it yet.** See "Scope" below, which is strict.

### Why measuring comes before building

Episode 1's story is one day old and may not survive its own adversarial review this week. Episodes 2 to 4 do not exist even as premises. **A general-purpose episode system built speculatively against unknown content is exactly the kind of work that eats three weeks and then gets thrown away.** What is needed first is an honest number and a design with a defensible minimum.

---

## Ground truth, verified 2026-08-27

**Verify each of these yourself before relying on it.** They were checked on 2026-08-27 and this repo moves.

**Leads**
- There is exactly **one** `LeadsDatabase.asset`, at `Assets/App/Leads/LeadsDatabase.asset`.
- ✅ Good news that changes the estimate: **`LeadsRepository` already supports runtime database swapping.** `SetDatabase(LeadsDatabase)` and `ReplaceFromDatabase(LeadsDatabase)` both exist (`Assets/App/Leads/LeadsRepository.cs`). The plumbing for "load a different episode's leads" is partly there already.
- `LeadsRepository.ApplySavedStates` restores lead state from a save after `ReplaceFromDatabase` has run. Understand this ordering before you touch anything.

**Save**
- `Assets/Scripts/UI/Board/Save/BoardSaveSystem.cs` writes **one file with a fixed name**, `board_state.json` (plus `board_state.prev.json`) in `Application.persistentDataPath`. Schema version is at `0.9.0`.
- The save DTO **records** an `episodeId` (see the DTO field and where `state.Episode.Value` is assigned) but **the file is not partitioned by episode.** Establish what actually happens today if the episode changes: does episode 2 overwrite episode 1's board, does it restore episode 1's lead states into episode 2's database, or something else. **Write the answer down with the line numbers that prove it.**

**Episode identity, and there is a live bug here**
- `CaseFlowOrchestratorMB.cs:18` has a serialized inspector field `public string episodeId = "Ep01"`, passed to `_svc.Begin(new EpisodeId(episodeId), steps)`.
- `CaseResolutionService.cs:12` has `const string EpisodeId = "e1_the_listener"`, used for `GameAnalytics.LogEpisodeComplete` and the `CaseResolvedEvent`.
- **Those are two different id namespaces that can already disagree**, and `"e1_the_listener"` names an episode that was demoted on 2026-08-23. So episode-complete analytics currently report a hardcoded id for a retired episode while the save records a different one.
- There **is** an `EpisodeId` value type in SharedKernel and `ICaseFlowService.Begin` takes it. Find it and establish what it is actually good for.

**Flags**
- `Assets/App/GameFlags.cs` stores flags in PlayerPrefs as `flag_` + name, **with no episode scoping**. Two episodes using the same flag name collide.
- ⚠ **PlayerPrefs cannot enumerate keys**, which is why the legacy migration in that file is lazy-on-read. That property makes "reset one episode's flags" genuinely hard, and it is probably the sharpest edge in this whole job. Do not hand-wave it.

**Scenes**
- `ProjectSettings/EditorBuildSettings.asset` contained only two scenes: `Assets/Scenes/SampleScene.unity` and `Assets/Scenes/Main Merge.unity`.
- `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/Ep01_ColdOpen.unity` and `Assets/Scenes/Case/Case_Board_Portrait.unity` exist on disk but were **not** in build settings. ⚠ That file shows as modified in git, so confirm current state.

**Content**
- `Assets/Content/` contains `Demo`, `Ep01`, `GhostStudent`, `TheListener`.
- ⚠ **Folder names do not imply case membership.** The cold cases and the Ep2 teaser live under `Content/GhostStudent/Leads/` but are Episode 1 *tail* content. Check `LeadsDatabase` and `SpawnLeadIds`, never the directory. This trap is documented in `CLAUDE.md` and it has caught people before.

---

## The questions to answer

Answer each with file and line references, not impressions.

1. **What breaks today if a second episode is added?** Enumerate every place episode identity is implicitly assumed to be singular: save, leads, flags, analytics, caseflow, scene loading, addressables, wallet, evidence locker, stash, case-kit specials, FTUE state, dossiers. Rank by severity.
2. **What is the correct save design?** Options include one aggregate with an episode-keyed section, one file per episode, or a global file plus per-episode files. **Robustness rule 1 in `CLAUDE.md` is binding**: anything that exchanges value with the board or the wallet lives inside `BoardSaveSystem`'s atomic aggregate, never its own file or PlayerPrefs. Say which option honours that and what the schema bump costs.
3. **What is the migration path for an existing player?** People have `board_state.json` at schema 0.9.0 today. A new schema must import it without losing anything. There is a documented fold-in pattern in `CLAUDE.md` rule 1 and three template test suites named in rule 2.
4. **How do flags get scoped without breaking the existing store?** Given PlayerPrefs cannot enumerate. Consider whether flags should move into the save aggregate entirely, and cost that.
5. **What does episode selection and progression actually need to be?** Assume the cheapest thing a player would accept: pick an episode, see which are complete, resume the one in progress. Do not design a season map.
6. **What is the minimum viable version?** ★ **This is the most important answer in the document.** If four episodes can ship with four `LeadsDatabase` assets, a selector, an episode-keyed save section and a flag prefix, say so and cost it in days. If it genuinely needs more, prove why.
7. **What is the bill?** Days of work, split into "must have to ship four episodes" and "would be nice". Assume a solo developer who is also writing the content.

---

## Scope, and it is strict

**Phase 1, which is what you are authorised to do: produce a costed audit and design document.** Write it to `SAS/feature-multi-episode-support-v1.md`. Read two or three existing `SAS/feature-*.md` files first and match their house style.

**Phase 2, implementation: DO NOT START IT. Stephen approves the design first.**

**Two exceptions you may do in phase 1**, because they remove defects and prejudge no design:

- **Fix the episode-id conflict.** Make `CaseResolutionService` take the id from the running caseflow service instead of a hardcoded const, so analytics stop reporting a retired episode. Small, strictly better, and independent of everything else. **Add an EditMode test.**
- **Any read-only diagnostic** you need: temporary scripts in the scratchpad, log statements you remove before finishing.

**Do not**: refactor the save schema, add episode partitioning, touch `Assets/Content/`, touch anything under `SAS/` other than creating your one new document, change scenes or build settings, or commit to `main` without asking. Another session may be working in `SAS/` on story documents at the same time.

---

## Tooling traps, each one cost this project real time

- ⚠ **`mcp-unity`'s `run_tests` bridge returns a DUMMY PASS for every filter in this project.** Verified 2026-08-24 against a known-good suite. **It cannot verify anything. Do not trust it.**
- ⚠ **`-quit` conflicts with `-runTests`.** Unity exits before the runner starts and returns 0, which means "batch mode closed cleanly", not "tests passed". Use the command in `CLAUDE.md` exactly as written, with no `-quit`.
- Batch mode only works with the Unity Editor closed, and it outlives shell timeouts, so launch it detached.
- **Baseline:** the last recorded batch run was 95 tests / 86 pass / 8 fail. Those 8 are pre-existing, scene-dependent legacy failures (`WK3_*`, a sanity spawn test, an Addressables smoke test) that always fail in batch mode's empty scene. **8 failures is the baseline, not a regression.** Anything above 8 is yours.
- The `LeadsBarView` twelve-lead counter is **already fixed** (`CaseArcTotal`, covered by `LeadsBarCounterTests`). Some documents still say it needs fixing. They are stale.

---

## Rules you must hold

Read the "Robustness Rules" section of `CLAUDE.md` in full. The two that will bite this job:

- **Rule 1, the save aggregate rule.** A crash must never separate a transaction's two halves.
- **Rule 2, crash-boundary tests are mandatory for any persisted system.** If phase 2 ever happens, every aggregate fold ships an EditMode suite covering memory-only mutation, export/import round-trip, `StateHash` changes-and-returns, legacy migration and legacy deletion, null-import resetting statics, and QA reset. **Your design document must say which suites the work would need**, even though you are not writing them yet.

Also binding: the assembly boundary table in `CLAUDE.md`. `AQ.SharedKernel` has **zero** Unity dependencies, ever.

---

## What good looks like

A document Stephen can read in ten minutes that ends with a number of days and a recommended minimum, where every claim about current behaviour carries a file and line reference, and where the hard parts (PlayerPrefs enumeration, save migration for existing players) are named as hard rather than smoothed over.

**If you find that the bill is much smaller than feared, say so plainly.** The `SetDatabase` API already existing is evidence that it might be. **If you find it is much larger, say that plainly too** — the episode count is fixed and the date is not, so an honest large number is useful rather than unwelcome.

## PASTE TO HERE
