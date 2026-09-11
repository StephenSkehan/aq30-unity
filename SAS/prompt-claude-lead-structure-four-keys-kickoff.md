<!-- pdf-title: Kickoff, the Four Keys lead structure -->

# ⚠ SUPERSEDED 2026-08-31, SAME DAY: Stephen re-ruled the constraints after v1.0 landed. 16 leads is retired; the model is now ~10 chapters × ~100 LEAD PACKAGES (1 to 5 cards each) per `four-keys-package-economy-model-v1.0.md`. Hours/sessions are guides not constraints; publish-beat session boundary is a nice-to-have; "the gap is Brad listening" dropped. Do not run this prompt; a v2 kickoff follows once the economy model is ruled.

# KICKOFF PROMPT: THE LEAD STRUCTURE FOR "THE FRIENDS WITH FOUR KEYS"

*2026-08-31. Paste everything below the line into a fresh session. It is the step after the spine: the sixteen leads, the sessions, the decisions and the gates, built against the spine's clock. No prose.*

---

We are building the **lead structure** for Episode One of Ally Quinn, *The Friends with Four Keys*. The spine exists, survived three adversarial rounds, and is the single source of truth for what happened and what Ally can reach. Your job is to turn its Part A clock and Part B6 sources into sixteen playable leads without breaking either. Read this whole prompt before opening a file.

### Read first, in this order

1. `SAS/episode-1-spine-four-keys-v1.4.md`. The spine. Part A5 is Ally's broadcast clock (Days 266 to 333, ten weekly episodes). Part B6 is her sources ledger: every fact she can air, from whom, on which day, and by what lawful route. Parts A to D are the whole graph, including everything the player must not learn early. **The player learns with Ally: nothing may surface in a lead before its B6 day, and nothing may reach the player that Ally never learns at all.**
2. `SAS/four-keys-the-premise-for-readers.md` (v8) and `SAS/four-keys-the-five-as-people-v1.2.md`. The premise and the five. Canon.
3. `SAS/four-keys-cold-open-v0.4.md`. The live open. Stephen ruled 2026-08-31 that it is **spread over multiple FTUE leads**, cost about one extra lead requirement in the tutorial. Its ~490 words are your budget for the opening leads' VO.
4. `SAS/ally-voice-reference-v1.0.md`. Not for this document (no prose here), but its Part A rules shape what a lead's on-air beat can be.
5. The bible: chapter 5 (§5.2 is the superseded 12-lead structure, read it for the mechanics it documents, not the count; §5.7 toast pacing), chapter 7 (§7.1 the loop as fiction, §7.2 requirement bands, §7.3 item families), chapter 3 (§3.3 the clue ladder, §3.4 arc rules).
6. Memory: `project_state.md` Four Keys headings (all rulings, including 16 leads, 4 to 5 hours, 8 to 10 sessions, session boundary = podcast episode boundary), `project_architecture.md` (RequiredLeadIds gates, lead tree flow, fragile areas), `project_economy.md` and `project_item_families.md` (bands, generators, families), `feedback_check_implementation_first.md`.
7. The code, before you trust any spec: `LeadsDatabase`, `LeadData`, `LeadsRepository.ApplySavedStates`, `CaseFlowOrchestratorMB`, `SpawnLeadIds`, and the Lead_E1_* assets in `Assets/Content/TheListener/` as the shape reference. The multi-episode system (EpisodeCatalog, ep01 slot ids) lives on `feature/multi-episode-audit`, pushed and NOT merged: design to its id scheme (slot ep01, story-neutral), but touch no code.

### What the lead structure is

Not prose, not VO, not assets. A structure document: sixteen leads plus a close, mapped to sessions and to Ally's broadcast clock, each lead carrying its purpose, its evidence turn, its sources, its gates, its requirement band, and its redaction line. The rule that governs everything: **the rank-one open question of this project is whether merging changes what a player believes.** Every lead must answer it on paper before anyone builds it: belief before, evidence gained on the board, belief after. A lead whose merge deliverable changes nothing the player believes is a toll booth and fails.

### Hard constraints, all ruled

- **Sixteen leads plus a close**, 4 to 5 hours total, ~30-minute sessions, 8 to 10 sessions across a week or more. **The session boundary is the podcast episode boundary:** Ally publishes, the player closes the app, and the gap between sessions is Brad listening. Map leads → sessions → A5 episodes → spine days explicitly; the map is Part A of your document.
- **Session 1 plays by different rules.** A fifth of installs die inside two minutes. The cold open v0.4 spreads across the opening FTUE leads; the first interaction target (35 to 40 seconds, I6) is an OPEN ruling: place your first interaction, state the seconds of VO before it, and flag the number for Stephen rather than assuming the ruling.
- **The reveal order is Ally's broadcast order.** The player may suspect anything; the game may confirm nothing ahead of A5. The episode's surprise is "he is alive" and it lands where the spine lands it.
- **Gates:** RequiredLeadIds arithmetic must be written out and checked, as the Same House Twice spine did for its decisions. No lock needing all keys, no gate the player can starve.
- **Economy:** assign each lead a requirement band from §7.2 against the canonical economy (Schedule B, 330 T1eq); families from `project_item_families.md`. Bands are drafts for tuning, but the total must live inside 4 to 5 hours at tuned drop rates: show the arithmetic.
- **Ids and content:** slot ep01, lead ids `Lead_FK_*` proposed (decide and say why), content folder `Assets/Content/FourKeys/` proposed. Folder names do not imply case membership; the database does.
- **Copy:** any player-facing line you draft is a placeholder and says so. No em dashes anywhere. Directive lines eight words or fewer. All final lines are Stephen-ruled later.
- **No replay (R6).** A completed lead's rewards never re-grant.

### Decisions

Design the decision points: how many, where they sit, what each costs, and the rejoin arithmetic, per the consequence-and-agency gate. Candidates the spine offers for free: whether Ally airs the fishmonger's tip or holds it for Del (she must hold it: is that Ally's choice or the player's, and what does choosing wrong cost?); what to do with Tessa's screenshot; whether to air the panel's description knowing the man who removes it will know someone saw it. A decision whose wrong branch costs nothing fails the gate. **Do not invent decisions that let the player break the spine's clock.**

### The redaction column

Every lead card carries one line: **what Fable may be told when this lead's prose is briefed.** Fable remains blind to spine Parts A to F for as long as possible; per-lead briefs will be cut from your redaction lines the way Part G was cut from the spine. A lead whose prose cannot be written without revealing the graph is misdesigned: fix the lead, not the redaction.

### Questions that are Stephen's, not yours

Write each as: the question, what the structure needs, what changes otherwise. Stop there.

1. **The season notch.** Bible §3.3 still lists The Listener as Ep1 with the coin on Dot's cradle; The Listener is demoted. What is the arc whisper of the new Ep1, where in the sixteen does it land, and what irreversible cost does it leave Ally (§3.4)? Propose a candidate; do not canonise it.
2. **How it ends and what it costs.** The spine stops at Day 333. The close lead needs the ending's shape (v8 open list). Propose one; the ruling is his.
3. **The first-interaction budget** (I6, 35 to 40 seconds vs the cold open's length as spread).
4. **Which stretch of the cold open lands in which FTUE lead**, once you have cut it: present the cut for ruling, since every Ally line is Stephen-ruled.

### Format

`SAS/episode-1-lead-structure-four-keys-v1.0.md`, html and pdf via `SAS/tools/Convert-MdToPdf.ps1` (PowerShell, no Python). Parts:

- **A. The map.** Lead × session × podcast episode × spine day × location × what Ally airs that week. One table.
- **B. The lead cards.** Per lead: id · working title (placeholder) · purpose · belief before → evidence on the board → belief after · sources (B6 row numbers) · requirement band and families · gates in and spawns out · the redaction line.
- **C. Decisions.** Placement, branches, costs, rejoins, gate arithmetic, checked.
- **D. Session 1.** The FTUE leads in seconds: VO, first interaction, first merge, first evidence turn.
- **E. Checks.** Every aired fact traced to a B6 row · no lead precedes its spine day · Kill Gate Zero · consequence-and-agency · the evidence-turn test answered per lead · gate arithmetic verified · band totals against the 4-to-5-hour budget.
- **F. The questions for Stephen**, in the form above.

### Rules

- The spine is not reopened. If a lead cannot be built without moving a spine fact, **stop and report**; do not repair the spine from below.
- Stephen's rulings in memory are not reopened.
- No menus where the choice is yours; one structure, with the tests shown. Options only inside Part F.
- Check the code before trusting any spec; specs go stale silently here.
- Commit as you go; push at the end; update `project_state.md` when it lands.

### When it is done

Report in this order: anything that could not be built without touching the spine (that is a stop, not a workaround) · the map in one paragraph · the evidence-turn test's weakest three leads, named honestly · the decision set and its arithmetic · Part F's questions · then propose the ChatGPT attack prompt on the structure, aimed at the evidence-turn table, the gate arithmetic and the session pacing, not at the spine.
