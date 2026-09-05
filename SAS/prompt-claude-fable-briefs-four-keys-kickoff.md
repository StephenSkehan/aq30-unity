<!-- pdf-title: Kickoff, the Four Keys package briefs and prose -->

# KICKOFF PROMPT: THE 105 PACKAGE BRIEFS AND PROSE FOR "THE FRIENDS WITH FOUR KEYS"

*2026-09-01. Paste everything below the line into a fresh session. It is pipeline step 4 at production scale: Fable writes every package's prose from redacted briefs, chapter by chapter, blind to the graph until the very end. This prompt is re-runnable: on resume, check which chapter files already exist in `SAS/four-keys-prose/` and continue from the first missing one.*

---

We are producing the prose for all **105 authored packages** (100 played; five branch pairs) of Episode One, *The Friends with Four Keys*. The structure is complete and fully ruled; your job is to cut a redacted brief per package, run Fable on them chapter by chapter, verify nothing leaks, and deliver each chapter to Stephen for line ruling. You hold the graph; Fable never does, until the final briefs end the blindness on purpose. Read this whole prompt before opening a file.

### Read first, in this order

1. `SAS/episode-1-structure-v2-four-keys.md` (v2.2, Part F all ruled). **Part B is your manifest:** per package: the beat, its payoff class, its source, and the **Fable column**, which is the redaction delta this whole process runs on: the standing base plus only what that package adds, and what is never told. Parts A, C, D, E, F govern; Part D's ten-way cut is the ruled chapter 1 text.
2. `SAS/episode-1-spine-four-keys-v1.4.md`. **You read it; Fable never does.** It is your leak-checking authority: anything in Parts A to F that is not explicitly licensed by a package's Fable column must not appear in a brief or survive in prose.
3. `SAS/four-keys-episode-synopsis-v1.0.md`: your own orientation. Full graph; never quoted to Fable.
4. **Fable's standing base, the only things it may ever be given before chapter 10:** spine **Part G only** (the public account) · `four-keys-the-five-as-people-v1.2.md` · `four-keys-cold-open-v0.4.md` · `ally-voice-reference-v1.0.md` · and the cumulative story-so-far digest you maintain (below).
5. Voices beyond Ally: the bible's locked registers: Del (§4.3: level, economical, honest about cost; "Quinny" is hers alone), Gerald (§4.1: warm, "love", method-not-facts), Mo, Arthur Finch. Quote the register blocks into briefs where those characters speak; do not paraphrase them.
6. Memory: `project_state.md` Four Keys headings; `feedback_copy_rules.md` (directive lines ≤8 words, no endearments to the PLAYER; in-story speech to Ally exempt); the em-dash ban is absolute.

### The discipline (this is the whole game)

- **Blindness is enforced by construction.** Fable runs as fresh subagents fed ONLY: the standing base + the story-so-far digest + the current chapter's briefs. No repo access, no memory, no tool use. Before every chapter run, you leak-check the briefs against the spine; after every run, you leak-check the prose. A brief that cannot be written without revealing the graph is a structure bug: **stop and report it**, never widen the licence yourself.
- **The story-so-far digest** is a document you maintain (`SAS/four-keys-prose/story-so-far.md`): after each chapter's prose drafts land, append only what a listener of the show now knows and what the room has experienced: never why. It is Fable's memory between chapters and it must stay as blind as Fable.
- **Chapter order is strict: 1 to 10, and chapter 10 last of all, close packages last within it.** The chapter 10 briefs are the only ones that reveal the ending (Brad alive, the identity, the accounting); they are drafted only after chapters 1 to 9's prose exists, per the standing rule. The D3/D4 branch packages are briefed in pairs with equal care; a branch is never the lesser draft.
- **What Fable writes per package:** the opening node line(s), the completion beat (dialogue, or the art caption for AC beats, or the character-fact delivery), and one §5.7-register toast line per member card on 3+ card packages. Chapter 1 is special: the open's words are v0.4 verbatim per Part D's ruled cut; Fable writes only the connective tissue and the p01_09 Del scene (Del interprets; she never repeats the narration).
- **Length discipline:** turn beats may breathe (a short scene); minor beats one to three sentences; micro beats and toasts a line. The whole episode's VO bill is downstream of this restraint.
- **Every line is a placeholder until Stephen rules it.** Deliver per chapter: one file `SAS/four-keys-prose/ch<NN>.md` containing, per package, the brief you sent (for audit) and the prose that returned, marked DRAFT. Batch to Stephen one chapter at a time: ten-ish packages is one review sitting, never more.

### The checks, run per chapter before delivery

1. **Leak check** (you, against the spine): no graph fact outside the licence; the standing never-tolds hold (never what the object was before chapter 9's composite, never that anyone is alive before chapter 10, never who forged, never the swap, never the deckhand's death, never what the photograph holds: that last one is never told to anyone, ever, including chapter 10).
2. **Voice check**: the ally-voice-reference Part B pass 4 (remove manufactured intrigue), the page-prose warning signs, the listening-speed test; locked registers for Del and Gerald; no em dashes; directive lines ≤8 words; evidential language (Ally never claims what she cannot source: Part B's Source column is her sourcing).
3. **Structure conformance**: every beat delivers what its Part B row says, turn beats actually turn, branch variants differ where the row says they differ (and only there).
4. **Ruled-text conformance**: chapter 1 uses v0.4's words untouched; the sign-off is the ruled one; the F1 Gerald lines and the F2 close beats appear as ruled, wordsmithed but not reinvented.

### Rules

- The structure is not reopened; if prose cannot be written inside a row's licence, stop and report (a licence widening is Stephen's, informed by you).
- Stephen's line rulings fold back into the chapter file and the digest before the next chapter depends on them; if he has not ruled a chapter yet, later chapters may draft against the DRAFT text, flagged as such.
- Commit per chapter; render md to html/pdf via `SAS/tools/Convert-MdToPdf.ps1`; push at session end; update `project_state.md` with the furthest chapter drafted and the furthest ruled.
- Token economy: this is multi-session work by design. End a session cleanly at a chapter boundary; the next session resumes from the files.

### When a chapter is done

Report: the chapter's package count and word count · any stop-and-report items · the three weakest lines in your own judgement (named honestly, Stephen reads these first) · what the digest gained. Then wait for Stephen's line ruling or continue to the next chapter's draft on his say-so.

### When it is all done

Chapters 1 to 10 drafted and ruled, the digest complete, and one final pass: read the whole episode's prose end to end against the synopsis for continuity, then propose the ChatGPT attack on the prose (aimed at voice drift, licence leaks a reader could infer, and beat-delivery failures; the structure and spine are out of scope).
