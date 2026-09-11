# Session kickoff, 2026-09-06 (written at the close of 2026-09-05)

Read `MEMORY.md` and the Four Keys block of `project_state.md` first, then this. Stephen's rulings in memory are never reopened by you; when he reopens one himself it is recorded, as three were this week.

## Where 2026-09-05 left things

**The inciting threat is RULED: the dosed tobacco tin under the stairs.** Violet's two secret cigarettes a day, rolled from a tin kept in the cupboard under the stairs among old house paints and garden concentrates; on Day minus 38 Brad, with the spare from the meter box, doses it through with a concentrate from the shelf above; on minus 37 she catches a brown stain leaching through the paper as she licks the seam and does not light it; the lid was loose (could be her), the bottle above was capped (she does not leave bottles uncapped), the dose is through the tin, not pooled: she cannot be sure, which is why "I think". An invented period product; **no compound is ever named anywhere** (Stephen asked for one in passing, I declined, he laughed; the rule stands). The prior fall on her record is an innocent witnessed stumble on the weedy slip (Day minus 45). The Regent ladder is texture only. Recorded as the spine's v1.6 amendment; folded through spine, structure, synopsis; ch02 and the digest carry PENDING FORGERY REBUILD marks on Ruby's week-four line and the forged letter's ladder claim. Consequence recorded: nobody but Violet and Brad ever knew of the tin and its only record is ash; Ally cannot source it in this episode.

**Also ruled 2026-09-05:** the house spare key lives in a hiding place (the meter box by the front step) known to and used by all four; Liam holds no cut key; the workshop spare hangs on the hall hook; Brad holds the only cut workshop key. "Unit" is "workshop" everywhere (Australian English: a unit is a flat). The panel is her own alphabet, the one she teaches, done better than she can, signed with a mark that is not a name; no phrase on it (Stephen: "her own alphabet is fine").

**Records of the two threat rounds:** `four-keys-threat-scenarios-disposition-2026-09-03.md` (twenty invented, closed without a ruling; gate corrected to inside the house) and `four-keys-threat-real-cases-disposition-2026-09-05.md` (sixteen from real cases, Fable v1/v2 and GPT v2 verbatim beside it; my ranking put the shower first and Stephen ruled the tin; the scoring correction is in the file). Lesson recorded: author rankings, mine included, are hypotheses.

**The premise is under test, not changed.** Stephen's late concept: Violet did not suspect the four, she trusted them; the letters were insurance so they could put the pieces together; the accusation, the certainty and the police ban are all Brad's, in the forgery. `four-keys-premise-delta-trust-v0.1-2026-09-05.md` states it against premise v8.1 and spine v1.4+, sketches the three letters, closes five holes, lists the artefact impact (cold open middle, chapter 1 packages 4 to 6, the key letter ruled this morning, chapter 2 packages 2 and 3, spine C1/C2 and ledger rows) and ends with a five-front GPT attack prompt. **Stephen runs GPT on it tomorrow morning.** Nothing built or ruled is touched until the result file exists and he rules.

**Violet's genuine locker letter** (tin version) is back from Fable and shown to Stephen; his notes so far: shelf above (done), clearer "I've shown the panel round" line, the panel paragraph to carry "her own alphabet". It is held until the premise test resolves, because the trust version rewrites its frame. The separate Fable agent that wrote it holds graph facts; never merge its context with the chapter agent's.

**Still blocked behind the premise test:** the "I know" certainty ruling, Brad's forgery rebuild (its third claim replaces the ladder; under either premise it carries no physical threat), Ruby's week-four line, chapter 2 line rulings beyond the key letter, chapter 3 prose.

**Unaffected and open:** the two DRAFT guided-loop banner lines and the DRAFT closing summary on the Episode Closed screen; the deferred economy note (chapter 1 T1eq feels low; wait for chapters 2 and 3); the build queue; the tester question; the clinic around 9 September and the submit-window ruling after it.

## First moves

1. `git pull`; confirm main is at 0efc8aa or later.
2. Take GPT's attack result on the trust delta: save it verbatim beside the delta (`four-keys-premise-delta-trust-attack-chatgpt-2026-09-06.md`), build a disposition table, and put the verdict to Stephen with a recommendation. If he rules the trust version: premise v8.2 note, spine C1/C2/ledger fold, cold open middle rewritten for the three readers' sentence-level re-check, chapter 1 packages 4 to 6 re-cut, both letters re-briefed to Fable. If he rules against: the letters proceed as ruled (key letter stands; genuine locker letter gets the alphabet paragraph and the "shown round" line; then the certainty ruling; then the forgery).
3. Then the copy rulings and the standing items.

## Process notes

- Never run the test suite through the mcp-unity bridge while the editor is open. Headless EditMode baseline is green (146).
- Long heredocs break the Bash tool here: write scripts with the Write tool and run them.
- Every attack or generation result is saved verbatim beside its prompt before it is judged; a document counts as attacked only when the result file exists.
