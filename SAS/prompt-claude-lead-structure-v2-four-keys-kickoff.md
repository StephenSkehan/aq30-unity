<!-- pdf-title: Kickoff, the Four Keys structure v2, chapters and packages -->

# KICKOFF PROMPT: STRUCTURE v2.0 FOR "THE FRIENDS WITH FOUR KEYS": CHAPTERS × PACKAGES

*2026-08-31. Paste everything below the line into a fresh session. This replaces the superseded 16-lead kickoff and the superseded structure v1.0: Stephen re-ruled the constraints and introduced lead packages the same day v1.0 landed. The story spine and the v1.0 turn-beats survive; the container changes.*

---

We are rebuilding the structure for Episode One of Ally Quinn, *The Friends with Four Keys*, as **about 10 chapters containing about 100 lead packages**. The spine is untouchable; the economy model is ruled; your job is to lay 100 story beats and their packages against both. Read this whole prompt before opening a file.

### Read first, in this order

1. `SAS/four-keys-package-economy-model-v1.1.md`. **The ruled container.** A lead package is 1 to 5 lead cards across characters and item families; fulfilling the PACKAGE shows the story beat; every package fulfilment pays off with something. Ruled numbers: ~100 packages · ~10 chapters · **1,600 T1eq total** (tunable once your beats exist) · CC at the 4.3 rate, ≈6,880 banded per package · **no optionals, the 100 are everything** · the ramp table is a guide and **variety is mandated**: never a mechanical size progression; quick wins beside five-carders late, a spike early, sizes and tiers mixed within every chapter.
2. `SAS/episode-1-spine-four-keys-v1.4.md`. The truth. Part A5 is Ally's broadcast clock; Part B6 is her sources ledger. **The player learns with Ally: no beat may surface a fact before its B6 day, and nothing may reach the player that Ally never learns.** Parts A to F are the whole graph and never reach Fable.
3. `SAS/episode-1-lead-structure-four-keys-v1.0.md`, superseded but load-bearing. Salvage: the **16 turn leads** (they become the spine of your beat list: the moments belief changes), the **four decisions** D1 to D4 (re-place them; their class analysis stands), the broadcast map (Eps 1 to 9 in the gaps, S10 = Days 326 to 333), the gate logic, the redaction lines, and its Part F questions, which are STILL OPEN and carry forward.
4. `SAS/four-keys-cold-open-v0.4.md` (the live open, spread across chapter 1's packages; its three-segment cut from v1.0 is a proposal awaiting Stephen), `SAS/four-keys-the-five-as-people-v1.2.md` (the five: your quarry for character-fact beats), `SAS/ally-voice-reference-v1.0.md` (register for any placeholder line).
5. The bible: §5.7 toast pacing, §7.2 requirement bands, §7.3 item families and their character ties; §3.3 and §3.4 for the arc rules. Memory: `project_state.md` Four Keys headings, `project_economy.md`, `project_item_families.md` (the canonical 3 generators and 7 families: packages mix these), `project_architecture.md`, `feedback_check_implementation_first.md`.
6. The code, before trusting any spec: the lead/requirement shape (`LeadData`, `LeadRequirement` quantity cap 3, `RequiredLeadIds`), `LeadsRepository`, the caseflow orchestrator. **There is no package container in code yet**; you are writing the content design it will be costed and built against, so name every assumption you make about it in one place.

### What you are producing

**The beat-and-package table for the whole episode.** For every one of the ~100 packages: id (`fk_p<chapter>_<n>` proposed; decide and say why) · chapter · the cards (count, family, tier, quantity, T1eq each) · gates in and unlock out · CC band · **the beat**: its type (evidence turn / character fact / Ally line / art with caption) · a one-line statement of its content · its source (a B6 row, the five, or new texture) · **the redaction line**: what Fable may be told when this beat's prose or caption is briefed.

The discipline that governs the beats:

- **Every chapter turns belief at least once.** The 16 turn leads carry this; place them, and mark each package's beat honestly as turn or texture. Texture is allowed; unmarked texture is not.
- **Character-fact beats reveal previously unknown facts about the five, and none may touch the graph.** Mine the five v1.2 first (it is full of unexploited texture: the tenor voice, the grade eight piano, the county swimming, the 2014 kitchen date). Where you invent new facts, they must be safe at every future reveal, flagged as new, and they go to Stephen in a batch for ruling, not one at a time.
- **Art-with-caption beats are allowed freely** (Stephen: budgets later, cost does not constrain the design). Mark them so the eventual art bill can be counted, but do not self-censor the count.
- **No beat spoils the spine.** The surprise is "he is alive" and it lands where A5 lands it. Check every beat against B6's day.

### Hard constraints and guides, as re-ruled 2026-08-31

- **Hard:** ~100 packages, ~10 chapters, 1,600 T1eq, no optionals, no replay, ep01 slot ids, quantity ≤ 3 per card, the v1.1 CC total banded, variety within chapters, cold open spread across chapter 1, every package pays off.
- **Guides, not constraints:** 4 to 5 hours; 8 to 10 sessions; chapter-end = Ally publishes is a **nice-to-have** where it falls naturally (the arithmetic makes a chapter roughly a session anyway); do not build anything that only exists to enforce a session boundary.
- **Dropped:** the "gap between sessions is Brad listening" framing. Do not design to it.

### Decisions

Re-place v1.0's four decisions (D1 Ruby's letter aired vs verified · D2 the tip to Del raw vs market first · D3 Margo's words verbatim vs paraphrase · D4 the mark aired vs held) into the chapter flow, at package boundaries. Their known limitation stands and is named, not hidden: the spine's clock and arrest are fixed, so decisions redistribute who is protected, exposed and believed; the ending-changing axis is unavailable. Send that to the attack again.

### Economy conformance (Part E of your document)

Show per chapter: packages, cards, T1eq, net energy, CC, against the v1.1 envelope; show the variety evidence (size and tier spread inside each chapter, the late-chapter quick wins, the early spike); total to 1,600 and ≈6,880 within rounding. Where your beats want a different chapter budget than the envelope, take it: the envelope is a guide, and say what you took and why.

### Questions that are Stephen's, not yours (Part F)

Carried from v1.0, still open: **F1** the season notch (candidate: the skipper letter's Thursday-run line; it brushes the street/apex separation and needs an explicit season decision) and its irreversible cost to Ally · **F2** the close's shape (and now: whether an Ep2 teaser beat lives inside the close package) · **F3** the first interaction at 38 to 39 seconds against I6's open 35-to-40 ruling · **F4** the v0.4 cut boundaries. New: **F5** the new-character-facts batch for ruling · **F6** any beat whose art-with-caption is load-bearing rather than decorative, listed for the art bill.

### Rules

- The spine is not reopened; if a beat cannot exist without moving a spine fact, **stop and report**. The v1.1 economy rulings are not reopened. Stephen's rulings in memory are not reopened.
- No menus where the choice is yours; one structure, tests shown; options only inside Part F.
- Check the code before trusting any spec. Name your package-container assumptions in one section; the costing doc will price exactly those.
- Format: `SAS/episode-1-structure-v2-four-keys.md` + html/pdf via `SAS/tools/Convert-MdToPdf.ps1`. Parts: A chapter map (chapter × A5 episode × spine days × the chapter's turn) · B the beat-and-package tables, one per chapter · C decisions · D chapter 1 in seconds · E economy conformance · F Stephen's questions · G package-container assumptions for costing. No em dashes anywhere. Commit as you go; push at the end; update `project_state.md`.

### When it is done

Report in this order: any stop against the spine · the chapter map in one paragraph · the beat-type census (turns / character facts / Ally lines / art-caption, and how many are texture) · the three weakest beats, named honestly · the decision placement · Part F · then propose the ChatGPT attack prompt aimed at the beat census (is every chapter's turn real), the economy conformance, the variety evidence, and the pacing of chapter 1, with the spine out of scope.
