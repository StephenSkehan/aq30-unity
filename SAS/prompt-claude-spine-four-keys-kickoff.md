<!-- pdf-title: Kickoff: the Four Keys spine -->

# KICKOFF PROMPT: write the Four Keys spine

*For a fresh Claude Code session, 2026-08-30. Paste the block below as the first message.*

---

## PASTE FROM HERE

We are writing the **spine** for Episode One of Ally Quinn, working title *The Friend They Mourned* (Four Keys). This is pipeline step 1, the step this project had never done until last week and the one that killed two premises in a single day when it was finally run. Read this whole prompt before opening a file.

### Read first, in this order

1. `SAS/four-keys-the-premise-for-readers.md` (v8). The premise. Closed to premise-level attack after two ChatGPT rounds and a closure check. Treat its settled list as fixed; treat its open list as yours to decide.
2. `SAS/four-keys-the-five-as-people-v1.2.md`. The five people, Stephen's final names: Violet Moore, Liam Bryce, Ruby Walker, Margo Rivera, Brad Collins. Written blind by Fable, combed by Stephen. Everything in it is canon for the episode unless the spine finds it impossible, in which case you flag it, you do not silently change it.
3. `SAS/story-production-pipeline-v1.md`, specifically **Step 1b, the earliest-action sweep** (line 41), **the recipient column** (line 69), and **the thirteen objections** (line 134). The sweep runs before the pre-flight.
4. `SAS/four-keys-premise-attack-chatgpt-round2-2026-08-29.md`. Read the status table at the top and the closure check. The two PARTLY items are obligations on you.
5. `SAS/ally-quinn-bible.md`, chapters 2 and 3 for Havenbay process canon (R1 to R18; R16 to R18 are sitting at the head of chapter 3). The Old Docks and Harbor Ward is where the five live. The coast and the cliff are not yet built.
6. Memory: `project_state.md` carries every ruling from 28 to 30 August under the Four Keys headings. Rulings there are Stephen's and are not reopened.

### What the spine is

Not prose. A reconstruction of what actually happened, hour by hour where it matters and day by day where it does not, from the first day Brad began preparing his exit to the day Ally's episode identifies him alive. For every fact: who knows it, from what date, by what route, and who they could lawfully tell. That is the access ledger, and it has a **recipient column**: for every person and every week, the earliest competent action available to them and who it would reach.

The rule that killed the last two premises: **accurate nodes and a false graph.** Every row was checked locally and passed, and nobody recomputed the earliest action available from the rows combined. You recompute it. Ignore the authored order. Start at day one and for every person ask: given everything this person can lawfully know today, what is the earliest competent action they can take, and does it precede the planned turn? Include professional duties, not just powers (Ruby is a practice nurse; Margo does the accounts for half the harbour; Del is a serving officer). A duty renews when the facts change. Every suppression must be named and paid for; "the story does not visit them yet" is not a reason.

### The people, and their sharpest sweep cases

- **Liam** is held by self-interest, not obedience: the only thing he could hand police is a letter in Violet's hand pointing at him. Test that every week. Find the week it stops being enough, if there is one.
- **Ruby** holds "she was frightened, then she fell" hardest, because the alternative is that she stood in that kitchen a hundred times and saw nothing. She is also a nurse with a duty. Test whether her professional judgement ever obliges her to act on a written threat to a patient's life, even a dead one's.
- **Margo** thinks the object is wrong, and the only person that names is the man she believes she killed. Test whether her silence holds against Tessa, who is the one person she tells things to.
- **Brad** must never be the loudest voice for staying out of the police station. That is readable forwards. The loudest voice is Ruby.
- **Del** is canon and serving. Place every point at which the file could cross his desk, and what he can and cannot do with it.
- **Tessa** knows the object was wrong and never who. Test whether that holds when Margo dies: what does Tessa do, and who does she tell?

### What you decide, and what you never tell Fable

Two objects are chosen in this document and nowhere else:

- **X, what Violet hid.** Seven constraints in v8. It is evidence of Brad's prepared exit, meaningless alone, damning beside what the four separately knew, and it is not the motive. Fable's Violet gave us a candidate without knowing it: a brush she made herself, sable on a broken oar handle, green tape at the ferrule, recognisable from across the road. Test it against the seven; if it fails, choose something else and say why.
- **The decoy**, a genuine object of Violet's that Brad had access to and left beside the forged letter. The letter gives it a false meaning that points at Liam through something Violet told Brad, and fails through the one thing she told only Margo: where Violet's money came from. Liam secretly owns a third of the boatyard. The money is the axis.

**Neither object, nor which of the four did it, nor anything in this document, ever reaches Fable.** Fable writes prose from a redacted brief at step 4. The spine is the one place the whole graph exists.

### Rulings you must request from Stephen, not invent

Seven Havenbay process rulings are queued in v8's open list, plus one added by the closure check. For each, write the question, state what the spine needs the answer to be for the story to hold, state what happens if the answer goes the other way, and stop. Do not write the ruling. They are:

1. What supports an accidental-fall finding on an unwitnessed death with head trauma, and what reopens it.
2. Missing-person status, search duration, and reference-sample practice when there is no body.
3. Who can petition to have a missing man declared dead, and how fast.
4. Whether authentication of a material posthumous document is routine, discretionary, or on-dispute. (The letter passes in all three; you still need to know which.)
5. What a last-movements review reaches on an apparent suicide when the device is lost: carrier records, app metadata, the prepaid connection.
6. Whether a death at a known disappearance site automatically links the cases.
7. Camera coverage at the cliff and its approaches, and whether a review pulls it for an apparent suicide.

### The two open obligations from the closure check

- **Brad's journey.** Four hundred miles each way on cash, months after his face was in the local news, to a known site, at night, on foot for the last part, and back. Place it on the clock. Name what it leaves behind. By the three real cases we are mining (John Darwin, Patrick McDermott, Aubrey Lee Price; verify details before citing), every faker was found through the trip he could not resist. The residue of this one is a lead for Ally's listeners, never for the file.
- **The last-movements review** of Margo: what it finds (a phone in the sea, a partner nobody knew), what it does not, and why it stalls competently.

### Format

Markdown, in `SAS/episode-1-spine-four-keys-v1.0.md`, with html and pdf rendered via headless Chrome (the pattern is in git history; there is no Python on this machine, use PowerShell). Parts:

- **A. The clock.** Dated timeline from Brad's first preparation to Ally's last episode. Relative dates (Day −N, Week N) anchored to Violet's death as Day 0.
- **B. The access ledger.** One row per fact: fact · who holds it · from when · by what route · who they could tell · recipient's earliest action. Include the residue rows: everything Brad's swap, vanish, contact and return leave behind, and who could find each.
- **C. The sweep.** Per person, per week, earliest competent action and its recipient. Any action that precedes its planned turn is a FAIL and you stop and report it before writing further.
- **D. The objects.** X and the decoy, chosen, with the seven-constraint test shown.
- **E. The rulings.** The eight questions, in the form above.
- **F. Pre-flight.** The thirteen objections, applied to this spine, pass or fail each.
- **G. What Fable may be told.** The redacted brief for step 4: everything Fable needs to write the opening and nothing that reveals the graph.

### Rules

- No em dashes anywhere, in any file. Comma, colon, full stop, middle dot.
- No prose. If a sentence could go in the episode, it does not go in the spine.
- Stephen's rulings in memory are not reopened. If the spine finds one impossible, you stop and report; you do not repair it.
- No menus. Where a choice is yours, make it and show the test that made it. Where it is Stephen's, ask the one question and stop.
- Commit as you go; push at the end. Update `project_state.md` when the spine lands.
- If the sweep fails, that is the deliverable. A premise that dies at the spine has cost a day and no reader attention, which is what the step is for.

### When it is done

Say what failed, if anything, first. Then the objects chosen and why. Then the eight questions for Stephen. Then the pre-flight score. Then propose the ChatGPT step-3 attack prompt on the spine, aimed at the ledger and the sweep, not the premise.

## PASTE TO HERE
