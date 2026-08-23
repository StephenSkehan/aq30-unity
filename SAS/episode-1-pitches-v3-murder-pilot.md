# Episode 1 Pitches, Round 3.1 — The Murder Pilot, with Agency Maps

**Date:** 2026-08-23 (v3 body) · **Revised:** 2026-08-23 (v3.1 agency and charter pass) · **Status:** AWAITING PREMISE RULING · **Decision input:** the table-read in Appendix A, not this document.

**Trigger:** 12/12 F&F testers on The Listener: "no jeopardy, too safe, not real true crime, didn't finish." Stephen-ruled 2026-08-23: dates shelved, compelling pilot or no release; murder-led; The Listener demotes to mid-season (dread pass later); episode length is decided by the story, not the skeleton (expect 15-20+ leads); economy models regenerate to fit.

Doctrine authority: bible Ch 5.4 (Pilot Doctrine) plus episode-1-pilot-spec.md. Tone authority: Ch 5.9 ("restrained murder yes, graphic violence no"). Conventions: episode-1-vo-lock-v1.1-WIP.md (all still binding: no em dashes, no exclamation marks, evidential language, announcer format, endearment map, absolute clock). Review authority: SAS/critical-reviewer-charter.md.

---

## 0. What v3.1 changes, and what is now void

Four things, in order of consequence.

**0.1 The scorecards are VOID.** Every "Doctrine score: 47/50 / 46/50 / 47/50" line in the pitch bodies below is struck. Stephen ruled it on the day: three near-identical percentages discriminate nothing, and the author of the pitches wrote the scores. Per charter rule 1, author scores are hypotheses, never evidence. They are left visible in the text only so the record shows what was replaced. **Do not cite them.** Section 5 replaces them with kill-gates and forced rankings.

**0.2 As committed, all three pitches FAIL the agency kill-gate.** The charter gate reads: count the decisions that change play, fewer than four is a fail, and a single end-of-episode gate is an automatic fail (The Listener precedent). Counted honestly, v3 gave Pitch G one branch (air or seal, Phase 3), Pitch H one branch (broadcast or decode, Phase 2), Pitch I one branch (hold or publish, Phase 3). One each, all late. That is the exact defect the pivot exists to fix, reproduced in the document written to fix it. Each pitch below now carries a six-decision agency map that clears the gate.

**0.3 The agency maps are proposals, not premise changes.** Nothing in a map alters a logline, a victim, a cast or an art bill. Each map answers one question: given this premise, what are the cheapest six decisions that change what the player does, and how does the player feel each one later. Ruling on a premise does not commit to its map; the map is the first thing the skeleton pass would negotiate.

**0.4 There is a technical gap, and it is small but it is not zero.** The previous session recorded that choice-gated lead spawns are "near-free" because RequiredLeadIds, setsFlag and sticky-choice recovery all exist. Half of that is true. Verified in the codebase today:

- Dialogue choices set persistent flags. `CaseGraph.Node.setsFlag` writes through `DialogueFlags` into the unified `GameFlags` store, and `DialogueRunner` has crash-safe sticky-choice recovery: a choice whose target node's flag is already down is auto-followed, so an interrupted run cannot set both halves of a mutually exclusive pair. This is shipping, tested, and it is the hard part. `Assets/App/UI/Dialogue/DialogueRunner.cs:504`.
- Lead gating reads lead completion only. `LeadsRepository` evaluates `RequiredLeadIds` and nothing else (`LeadsRepository.cs:39`), and `LeadOutcomeMB.SpawnFollowUpLeads` spawns every id in `SpawnLeadIds` unconditionally (`LeadOutcomeMB.cs:91`).
- `LeadData.BranchOutcomes` (a label plus SpawnLeadIds) exists as a serialized field with **no runtime consumer anywhere in the project**. It is a stub.

**So no flag can currently change which leads exist.** The bridge every map below depends on is one work item, specified in 1.5. It is small. It must land before any agency map is real, and it should be built and tested before the skeleton pass, not during it.

---

## 1. The agency contract

### 1.1 What counts as a decision

A decision counts toward the kill-gate only if the answer to "what is different afterwards" is one of:

- **which leads exist** (a lead spawns, or does not, or is replaced by a sibling),
- **what the player does on the board** (different requirement families, tiers, quantities, generator grants, Case Kit specials, energy cost),
- **what the ending is** (which close lead runs, or which variant of it).

A decision that changes only which line of dialogue plays does not count. This is the rule that would have caught The Listener before the testers did.

### 1.2 Cost classes

Every decision in the maps below is tagged with one:

| Class | What it costs | Rule of thumb |
|---|---|---|
| **A** | Zero VO. The choice label plus data on two sibling LeadData assets: requirements, economy, generator grant, board work. | Free once the bridge exists. Use these to hit the gate count. |
| **B** | Two to six variant lines at the divergence and the rejoin. | Cheap. Use for the felt callbacks. |
| **C** | A whole lead of exclusive dialogue, or a short exclusive chain. | Expensive. One per episode, two at the outside. Reserve it for the branch the episode is named after. |

**Target mix per episode: three class A, two class B, one class C.** Six decisions, one expensive branch, and a total added VO bill of roughly 12 to 20 lines over the unbranched baseline.

### 1.3 Branch and rejoin, always

Divergence lasts at most three leads and then converges on a shared lead. No branch may fork a second time before it rejoins. This is not an artistic preference. It is the only structure a solo dev can finish, because authored content then grows linearly with decisions instead of exponentially.

### 1.4 The felt-consequence rule

A decision the player cannot feel is a decision the player did not make. Every entry in the maps below carries a **callback**: the specific later moment where the game says back to the player what they chose. Preferred callbacks in descending order of strength:

1. The board looks different (a lead card that is not there, a cold-trail card that is).
2. A character refers to the choice by its content, not by its label.
3. The close podcast is a different recording.
4. The next episode opens differently.

The Listener's branch had none of these, which is why it read as "a tacked on afterthought" even though it was mechanically real.

### 1.5 The Agency Bridge (engineering work item, do first)

Scope, in order:

1. **`LeadData.requiresFlag` and `LeadData.forbidsFlag`** (two strings). Checked in `LeadsRepository.SpawnLead` and in the gate evaluation beside `RequiredLeadIds`. A lead whose flag condition fails stays Blocked and is excluded from case progress.
2. **Re-evaluate on restore.** Hook the same state-scan that already re-evaluates `RequiredLeadIds` gates a save may predate (`LeadsRepository.cs:210`). Per CLAUDE.md robustness rule 6, the scan is the guarantee and the spawn-time check is the optimization.
3. **Consume `BranchOutcomes`, or delete the field.** Selection by flag: the first outcome whose label matches a set flag wins, otherwise index 0. Leaving a serialized stub in shipping data is how the next person is misled about what exists.
4. **Flag registry.** One document listing every `flag_e1_*` an episode sets, its two values, which leads read it, and whether Ep2 or later reads it. Without this the cross-episode callbacks in 1.4 quietly rot.

No new persisted state is introduced: flags already live in `GameFlags`, leads already persist through `BoardSaveSystem`. So the CLAUDE.md crash-boundary suite (rule 2) is not triggered by a new aggregate, but the gate needs EditMode coverage for flag-set-before-spawn, flag-set-after-spawn, flag-never-set, and restore-path re-evaluation. Estimate: half a day of code, half a day of tests, and it unblocks all three premises equally. **It is not premise-specific, so it can be built before the ruling.**

### 1.6 The authoring multiplier nobody has costed yet

Agency is not free in content even when it is free in VO. Authored leads exceed played leads:

| | Played leads | Authored lead assets | Multiplier |
|---|---|---|---|
| G | 17 | ~21 | 1.24 |
| H | 15 | ~20 | 1.33 |
| I | 18 | ~24 | 1.33 |

Sibling leads, cold-trail stubs and exclusive chain leads are all real assets needing requirements, art references, board wiring and an audit pass. **Ruling on a premise is ruling on the right-hand column, not the left.** The economy regeneration in section 7 uses played leads; the schedule must use authored leads.

### 1.7 Propulsion discipline: the lead ledger

Added 2026-08-23 after a craft note on thriller propulsion (Dan Brown's engine, not his prose). The useful half of that model is information control: the player should always know the immediate objective and never be finished interpreting what they just found. Three rules, and they are authoring gates for the skeleton pass, not style advice.

**Rule 1: the ledger.** Before any dialogue is written, every lead gets one row: entering question · what the player's board work physically does · the answer obtained · the better question created. A lead with an empty fourth column is connective tissue and should be cut or merged.

**Rule 2: two of four functions.** Every lead must do at least two of: **answer · complicate · endanger · humanise.** One function is thin. Zero is a toll booth wearing a detective hat.

**Rule 3: recontextualise rather than surprise.** The strongest reveal changes the meaning of evidence the player already holds instead of adding new evidence. The signature was his; the ink was made twelve years after he disappeared. This is also the fairness gate: nothing may be explained that was not planted.

**What we deliberately do not borrow:** withholding obvious information to manufacture a cliffhanger, exposition delivered as convenient dialogue, escalating stakes for their own sake, or thin secondary characters who exist to hand over clues. Our differentiator is the keeper's humanity, and the keeper must never become a puzzle delivery mechanism. Brown makes a reader ask what happens next. This show also has to make them ask what the right thing to do with the truth is, which is precisely what the agency maps are for.

---
## PITCH G — "The Confession" (Pitch B revived and re-engineered)

**Logline:** An eighty-one-year-old man confesses to a thirty-seven-year-old killing in the back booth of the Rusty Anchor, and by morning he is dead of a heart attack that Dr. Cortez can prove was murder. Whoever silenced Frank Doyle waited thirty-seven years for him to talk. They were still listening. And the booth he talked in belongs to the man teaching Ally everything she knows.

**Why round 1 killed it, and why that reason is now void:** its engine was pre-existing love of Gerald, which a new player lacked. As of build b7, Gerald IS the tutorial: his face fronts every hint, banner and help card from the first ninety seconds. The systems now build the attachment before the story spends it. The objection has inverted into the pitch's strongest feature: *the man who has been guiding your hand is the man whose booth becomes a crime scene.*

> **v3.1 reviewer note on the paragraph above:** this rebuttal is an assumption presented as settled. Ninety seconds of hint chips carrying a portrait produces *familiarity*, not *attachment*, and the difference is exactly what the round-1 objection was about. It is cheap to test, and the same table-read tests it (Appendix A, question 3). Until then the objection is suspended, not void.

**Cold open (the hook is lines, not premise, first 60 seconds):**

> ANNOUNCER: "Message received: Saturday, eleven fifty-one p.m."
> MO (voicemail, low, the bar noise gone): "Ally. It's Mo. Not an emergency. Except I think it might be. There's a man in Gerald's booth who's been nursing one whiskey for four hours, and he just told your grandad something I wish I hadn't heard. Gerald's writing it down. Come when you can, my lovely. Come tonight."
> ALLY (on-mic): "By the time I got there, the man was gone. Gerald was still in the booth. He hadn't touched his glass. He slid his notebook across the table and said: read it twice, love. Because tomorrow he'll deny it, and by Monday he'll be dead. He was wrong about one thing. Frank Doyle didn't live until Monday."

**The case, two threads braided:**
- **1989 (the confession):** Doyle, a foundry night-shift foreman, killed a man named Wren Kellaway in a dispute that was staged as a drunken fall into the harbour. Kellaway was written off as a drunk; his younger sister **June Kellaway** (the keeper) has spent thirty-seven years being told her brother drowned in shame. Doyle names the thing 1989 could not: he was PAID to make it look that way. He never knew the payer's face. Only a voice, an envelope, and a coin left in his coat pocket he never dared spend.
- **Present (the silencing):** Doyle dies within hours of confessing. "Tidy heart attack." Dr. Cortez (his first appearance in the series; the casting block exists) finds the injection site behind the knee. Someone in the bar heard, or someone has watched Doyle for thirty-seven years waiting for the day his conscience won. The braid: solving 1989 is how Ally identifies who needed it silenced in the present.

**Doctrine beats (selected):**
- *Dramatic question in two nodes or fewer:* "Who needed a dying man's thirty-seven-year-old secret to die with him tonight?"
- *Grievable victim from ONE detail:* June has paid the mooring fee on her brother's ruined dinghy every year since 1989. Thirty-seven receipts in a biscuit tin. "You don't keep a boat for a drunk. You keep a boat for a sailor."
- *Warmth beat:* Mo closing the bar to strangers and cooking for the wake nobody else will hold.
- *Competence beat:* Ally reconstructing Doyle's last four hours from till receipts, the jukebox log and one whiskey that never got refilled.
- *Midpoint jeopardy reaches Ally:* Gerald's notebook is the only record of the confession, which makes Gerald the last living witness. A pale sedan parks across from the Rusty Anchor two nights running. The episode's spine question flips from who killed Doyle to *how do I keep my grandfather alive.*
- *The branch:* air Gerald's recording of the confession, making the whole city a witness so killing Gerald becomes pointless (loud shield), or keep it sealed and let Del work the injection-site evidence quietly while Gerald sleeps at Mo's (quiet shield). Both converge: the paymaster's proxy is caught by the mooring-fee thread; both cost something (aired means June hears her brother's death sold as content before she hears it as truth; sealed means the killer's name surfaces two days later and a second grave is nearly needed).
- *Whiff of change and the coin:* Doyle's unspent 1989 coin, wrapped in the confession page Gerald tore out for Ally. Brass, one face worn blank, a crescent nick in the rim. Ally does not know what it means. Episode 2 does.
- *Conversion hook (the close):* the paymaster's proxy is charged; the paymaster is a name on dissolved 1980s paperwork; and Ally's tip line receives one message the night the episode airs. "Message received: Sunday, one a.m." Coins dropping into a payphone. Then a voice, elderly, unhurried: "Frank always was sentimental. Goodnight, Miss Quinn." Click.
- *Defeatable antagonist:* the proxy who administered the injection is an episode-level face (the bible allows Damien Kroll here if we want the recurring heavy, or an episode-invented nurse-turned-fixer). The paymaster stays a terrifying absence, per the season-villain firewall.

**Natural length: 17 leads, 3 phases** (Phase 1 the confession and the body, 6 · Phase 2 1989 exhumed, 6 · Phase 3 the shield and the catch, 5). The third phase exists because the case genuinely turns at the midpoint from investigation to protection, which is a different kind of play and earns its own phase podcast.

**Cast and art cost:** Ally, Gerald, Mo, Del, Tip Line, all existing final art. **Cortez, new portrait (his entrance, prompt block ready).** June Kellaway, new, episode-level, allowed. Doyle, VOICE ONLY plus an evidence-board polaroid (the Tip Line object-character precedent): he exists as a recording and a notebook, which is both cheap and thematically perfect. Two new backgrounds (foundry ruin, June's kitchen); Rusty Anchor and studio masters reused heavily. **Verdict: cheapest of the three pitches in art.**

**Mechanical vocabulary:** audio (the confession tape, the jukebox log), forensic (injection site, exhumed 1989 file), rusty_anchor family (the bar IS the crime scene, so the gated family finally earns its story turn), food_gifts (Mo's wake, June interviews). Optional: press family (Arthur pulls the 1989 archive, art live, flag flip only).

**VO scale:** ~30-34 cues, 2 sessions (Ally plus one gravel male voice for Doyle; Mo, Gerald and Del lines follow the Ep1 text-only convention where not podcast-facing). Add the agency map's bill: +14 lines.

~~Doctrine score: 47/50~~ **VOID per 0.1. See section 5.**

**Season notch planted:** the coin plus "he was paid". Organized silence-buying existed in Havenbay in 1989. One notch, no Thomas spend.

### AGENCY MAP G — flavour: CUSTODY

Every decision in G is the same question in a different costume: **who do you trust with this.** A page, a truth, a grandfather, a tape, a coin. That through-line is why G's agency reads as authored rather than bolted on, and it is the thing to protect if the map is negotiated down.

| # | Phase | The decision | What changes in PLAY | Persists | Class | Callback |
|---|---|---|---|---|---|---|
| **G1** | P1, lead 2 | **The notebook.** Hand Gerald's page to Del now, or transcribe it yourself first. | Swaps the next lead for its sibling. DEL: `Lead_E1G_Chain`, forensic family requirements, Cortez enters one lead earlier, lower CC. KEEP: `Lead_E1G_Transcribe`, audio family requirements, reusing the audio-forensics vocabulary already built for The Listener, and it yields the paymaster's *voice* detail the police route never gets. | `flag_e1g_notebook_police` / `_kept`. Ep2 reads it for Del's willingness to do a favour. | A + B (two 6-node sibling leads, one authored either way) | P3 arrest: Del either says the chain held, or that a podcaster's transcript is inadmissible and they had to build it on the mooring thread instead. |
| **G2** | P1 close | **June.** Tell June Kellaway what Doyle confessed before you can prove it, or hold it until you can. | TELL: June becomes an active source. `Lead_E1G_BiscuitTin` spawns early, the thirty-seven mooring receipts arrive as press-family merge material, and one Phase 2 wall is removed. HOLD: the receipts come later out of the harbour office, a harder lead that **consumes a SearchWarrant special** from the Case Kit. | `flag_e1g_june_told_early` | **A** (one variant node, two sibling leads, zero VO on the mechanical half) | Close: June either says she has had three days to be angry with a dead man, or that Ally made her wait and she would have liked those three days. |
| **G3** | P2 midpoint | **The pale sedan.** Run the plate through Del (legal, slow) · have Vega pull the dissolved-company records (fast, dirty) · put a callout on the tip line asking who owns a pale sedan on Harbour Row (loud, and it tells the watcher he was seen). | Three-way. DEL and VEGA each spawn a different identification lead. CALLOUT spawns neither: it triggers the watcher's escalation one lead early, so **Phase 3 opens at 4 leads instead of 5, under time pressure, with the shield decision forced before Ally is ready.** | `flag_e1g_sedan_del` / `_vega` / `_callout` | B (three 4-node variants; Vega by phone-call convention, no new portrait) | The P3 arrest is built on whichever route named the proxy, and Del names the route out loud. |
| **G4** | P2 to P3 hinge | **Where does Gerald sleep.** Mo's back room · his own house with Del's drive-bys · the studio with Ally. | The strongest structural one. MO'S: Gerald is safest, but the Rusty Anchor closes to Ally as a working space for two leads, so **the rusty_anchor generator goes offline and the board's requirement mix changes.** HOUSE: keeps the bar, but the sedan resolves as a break-in that destroys one evidence item, removing a P3 shortcut. STUDIO: keeps everything, and the tip line receives the taunting message a day early, which moves the season notch forward and unsettles the close. | `flag_e1g_gerald_*` | A + B | The Phase 3 podcast opens differently in each, and Gerald has one line about where he slept. |
| **G5** | P3 | **Air the tape, or shield quietly.** The named branch, now play-changing rather than dialogue-changing. | AIR: a 3-lead "the city as witness" chain. Tip line floods, listener-sourced evidence, press-family requirements, subscribers surge (a real economy reward), and June hears her brother's death as content before she hears it as truth. SEAL: a 3-lead "quiet net" chain. Del's warrant, Cortez, slower, a smaller subscriber payoff, June's trust intact, and the proxy gives a confession instead of an ambush. Both converge on the arrest lead. | `flag_e1g_aired`. **Season-level:** Ally's public-pressure doctrine, readable by Ep2 and by Ep3 when The Listener is dread-passed. | **C** (the one expensive branch: two 3-lead chains, ~24 nodes, ~12 VO cues) | The close podcast is a different recording. June's last line is different. Ep3 can quote it. |
| **G6** | P3 close | **The coin.** Keep it, or log it into evidence with Del. | KEEP: the coin becomes a permanent evidence-board card that carries into Ep2 and opens Ep2's first lead one step earlier. LOG: HPD hold it, so Ep2 needs an extra lead to get it back, but Del's trust is maxed and one Ep2 favour is free. | `flag_e1g_coin_kept`. Cross-episode by design. | **A** (choice label plus one line) | **Episode 2 opens differently.** The cheapest decision in the map and probably the most felt. |

**Count: six play-changing decisions, spread 2 / 2 / 2 across three phases. Gate cleared.** Mix: three class A, two class B, one class C. Added VO ~14 lines. Added authored leads +4 (two sibling pairs; the two P3 chains net out against the unbranched P3).

**What G's map does not do:** it never lets the player be wrong. Every route reaches the arrest. That is a defensible protective-genre choice, because custody stories are about cost rather than failure, but it means G carries the least *risk* of the three maps, and risk is a large part of what agency feels like.

---

## PITCH H — "The Overnight Girl" (Pitch A revived)

**Logline:** Nina Vasquez, twenty-four, works the overnight counter at the Kestrel Corner Diner. Every night at three ten a.m. she rings her kid brother twice and hangs up: the all-safe. Tonight the phone rang once. Through the diner window, four hours earlier, Nina watched two men load something man-shaped into the fish-market ice store. Nina is not missing. Nina is hiding. And the man looking for her has done this before.

**Cold open:**

> ANNOUNCER: "Message received: Thursday, three twelve a.m."
> BOY (young, steady the way children are when they have rehearsed being brave): "My sister rings twice every night. Two rings means safe. It rang once tonight. She made me learn this number in case it ever rang once. She said ask for Ally. She said say the diner, say the ice store, say run."
> ALLY (on-mic): "He recited it like a times table. Somebody taught a twelve-year-old to be a dead man's switch. That somebody is out there right now, with a head start measured in hours, and so is whoever she built the switch against."

**The case:** a body IS in the ice store, the first on-page present-tense body of the series: found frozen, restrained rendering, a sheet, a hand, Cortez's gurney, nothing more. The victim is a harbour customs clerk who had begun photographing manifests. Nina saw the disposal, understood instantly what seeing it meant, and executed the escape plan she had rehearsed since childhood. She grew up in witness-protection fallout; her family fled something once before, so her competence is her backstory. The episode is a two-clock chase: Ally racing the hunter to Nina, using breadcrumbs Nina left only for someone who listens, keyed to episodes of the show. The hunter is a patient professional (Kroll fits perfectly) who cleans, never hurries, and is methodically visiting everyone Nina ever trusted, one conversation at a time, always polite, always a day ahead.

**Doctrine beats (selected):**
- *Grievable victim from ONE detail:* the brother's twice-nightly phone ritual. Also its inversion at the close.
- *Midpoint jeopardy reaches Ally:* Kroll sits at the diner counter during Ally's stakeout and orders coffee. Never threatens. Pays exact change. "You do good work, Miss Quinn. Thorough. He appreciates thorough." Leaves a tip: exact change again. The scene is polite the way a scalpel is clean.
- *The branch:* broadcast Nina's breadcrumb key to flush the hunter into the open (burn the trail to burn the tail), or decode quietly and reach her first, knowing that if you are wrong about being faster, the last person to follow the trail is not you. Converges at the marsh boathouse.
- *Competence beat and effort engine:* every lead is a breadcrumb Nina keyed to the show's archive, so the merge requirements ARE the decoding. Audio family, literal, medium-native.
- *Resolution with emotional honesty:* Nina is found alive. The series does not kill women in fridges; the clerk in the ice store is the body, and HIS keeper, a wife who thought he was having an affair rather than a conscience, carries the grief thread. Kroll is taken at the boathouse but gives nothing: hired, layered, a receipt not a reason. The manifests point at a shell that dissolves under Del's warrant.
- *Whiff of change and the coin:* among the clerk's photographed manifests, one Thursday-tide entry from Pier 13 flagged with a hand-drawn circle. Pinned beside it in his effects, a brass coin, one face blank, a nick in the rim. Nobody in the episode explains it.
- *Conversion hook:* Nina, safe, records one line for the show. "Two rings tonight." Then the stinger: somewhere, a phone photographed on a manifest desk rings twice. Someone else has learned her code.

**Natural length: 15 leads, 2 phases.** The chase structure resists a third phase; it wants acceleration, not plateau.

**Cast and art cost:** Ally, Del, Gerald, Mo, Tip Line existing. **New: Nina (full sheet, she survives into the series as a canon caller and ally), the brother (one frame or voice-only), Kroll (his canon entrance, antagonist lighting grammar), Cortez (shared with G), the clerk's wife (episode-level).** Two or more new backgrounds (ice store exterior at night, marsh boathouse; the diner master EXISTS). **Verdict: heaviest art bill of the three.**

**VO scale:** ~30 cues. The boy's cold-open voicemail is the casting risk, a child actor cue: source carefully, or restage as Nina's own pre-recorded failsafe message, which also works and may be stronger. Add the agency map's bill: +12 lines.

~~Doctrine score: 46/50~~ **VOID per 0.1. See section 5.**

**Season notch planted:** Pier 13, Thursday tide, photographed and circled by a man who is now dead. The route exists and it kills. No Thomas spend.

### AGENCY MAP H — flavour: TRIAGE

H's agency is not a set of menus. It is one mechanic used three times: **you cannot reach everything before he does.** Every choice in H is the same question: **what do you give up.**

**The triage mechanic.** At three points the board offers two open leads and the case offers time for one. Resolving either advances the hunter one tick. The unchosen lead does not vanish; it converts to a **cold-trail card** on the evidence board, resolved-negative, worth two or three nodes: the hunter got there first, the person will not talk now, and the thing they held is gone. The cold trail still tells Ally something, because it is the hunter's own path drawn in the negative, but the evidence it held is lost, which raises the requirement on a later lead.

This is the cheapest agency design in the document, and the only one whose ending is *earned* rather than *picked*.

| # | Phase | The decision | What changes in PLAY | Persists | Class | Callback |
|---|---|---|---|---|---|---|
| **H1** | P1, lead 2 | **The brother.** Hand him to Del formally, so he goes into care, safe, and Ally loses the only person who can read Nina's code. Or park him with Helen Quinn off the books. | DEL: two later breadcrumb leads have their requirements raised, Ally decodes alone, more merge work per lead. HELEN: those leads are cheaper, and an **exclusive Phase 2 lead spawns** in which Kroll visits Helen's school, politely, at pickup time. | `flag_e1h_boy_with_helen` | **C** (one exclusive lead) | The close changes, and Helen's disposition carries into later episodes. |
| **H2** | P1 | **Triage tick 1.** The trailer park where Nina's aunt lives, or the pawn shop that took her phone. | One resolves, the other becomes a cold trail. Different evidence, different family requirements downstream. | `flag_e1h_tick1_*` | **A** (plus one 3-node cold-trail stub, a reused pattern) | The cold-trail card sits on the evidence board for the rest of the episode. The board *looks* like your choice. |
| **H3** | P2 | **Triage tick 2.** The night bus driver, or the church that runs the food bank. | As above, and the loser's evidence is what makes the boathouse identification cheap. Lose it and H6 gets harder. | `flag_e1h_tick2_*` | **A** | As above, and Kroll is one step nearer in the P2 podcast. |
| **H4** | P2 | **Triage tick 3.** The clerk's widow, or Nina's old caseworker. | As above. Losing the widow costs the grief thread its best scene. Losing the caseworker costs the witness-protection backstory, which is the emotional key to the ending. | `flag_e1h_tick3_*` | **A** | As above. |
| **H5** | P2 | **Broadcast the breadcrumb key, or decode quietly.** The named branch, now play-changing. | BROADCAST: the tip line floods, two listener-sourced leads spawn (fast, cheap, subscribers surge), and the hunter hears it too, so **the next triage tick offers only one lead, because he has already taken the other.** DECODE: no flood, the archive leads need audio family at a higher tier (expensive merges), and the hunter stays a day behind. | `flag_e1h_aired`. Same public-pressure doctrine class as G5 and I4. | **B** | The tick that follows is visibly poorer. Nina says which one she heard. |
| **H6** | P2 climax | **The last mile.** Go to the marsh boathouse now, or spend one more lead confirming it is the right place. | NOW: skip a lead, arrive at the ambiguous ending. CONFIRM: one more lead of board work, arrive certain, Kroll taken cleanly. Pure risk and reward, zero VO on the mechanic. | `flag_e1h_rushed` | **A** | Which version of the boathouse you get. |

**The accumulated ending.** `flag_e1h_ticks_won` counts H2 to H4 (0 to 3), modified by H5 and H6.

- **3 of 3:** Ally reaches the boathouse first. Nina is calm, and she is calm because somebody finally kept up with her.
- **2 of 3:** Ally arrives during. The hunter is there. Del is four minutes out.
- **0 or 1 of 3:** Nina saves herself, exactly as she has planned to since she was nine, and Ally arrives to a woman who did not need her. This is the humbler close podcast, and it is the better one to have written.

**Count: six play-changing decisions, and the ending is a function of play rather than a menu. Gate cleared, and cleared with the strongest mechanic in the document.** Mix: four class A, one B, one C. Added VO ~12 lines plus three 3-node stubs. Added authored leads +5.

**Why this map is not optional for H.** See 5.4.1. The chase clock is H's biggest strength on the page and its biggest liability in a merge game played across days. Triage is the fix, because it makes the clock diegetic to *lead count* rather than to wall-clock time: the hunter advances when you advance. Without triage, H's "head start measured in hours" collides with an energy economy that meters play across a week. **Ruling for H is ruling for triage.**

---
## PITCH I — "The Last Caller" (engineered hybrid: G's present-tense murder braided with H's chase, on The Listener's best device)

**Logline:** The tip line's midnight message is a confession. An old man, naming a killing from decades ago, talking like a man out of time. The message cuts off mid-sentence. Not hung up: interrupted. By morning there is a body, and the only person who can finish the dead man's sentence is the caller he named as his witness, who has just stopped answering her phone.

**Cold open:**

> ANNOUNCER: "Message received: Wednesday, twelve oh four a.m."
> CALLER (elderly, deliberate, reading from something written): "My name doesn't matter yet. Write this down instead. In August of nineteen eighty-nine a man went into the harbour at the foundry basin and it was called a fall. It was not a fall. I was paid to say it was. The one who paid me had a voice I will hear when I die, which I am told is soon, so I am spending it. There was a witness. She was nine years old. Her name is..."
> [The line does not click. It muffles: a hand over a receiver. Four seconds of a room with two people breathing in it. Then dial tone.]
> ALLY (on-mic): "The tip line keeps voices, timestamps and nothing else. That has always been the promise. Tonight I would trade the whole archive for one more second of tape. Somewhere in this city is a woman who watched a murder when she was nine years old. Two people are looking for her tonight. I am the slower one."

**The case:** identifying the dead caller, a Doyle-figure, VOICE ONLY, found dead by morning, restrained murder, Cortez proof, from the message's room tone, medications heard rattling, a foghorn interval. Audio forensics as the spine, the show's own craft as the weapon. Then the chase: find the witness, now mid-forties, the keeper AND the endangered, one character carrying both doctrine loads, before the interrupter does. The interrupter was IN THE ROOM at 12:04. The episode's chill is that the confession was permitted up to the name. Someone let a dying man have his conscience, except for the only sentence that mattered.

**Structure: 3 phases, natural length 18 leads** (Phase 1 the tape and the body, 6 · Phase 2 nineteen eighty-nine and the hunt for her, 7 · Phase 3 she is found, and what she saw finishes the sentence, 5). The branch in Phase 3: she will only testify if Ally holds the episode until the arrest (protect), or the episode is the only pressure that forces the arrest (publish). The Listener's loud-truth and quiet-truth axis, aimed at a witness's life instead of a van.

**What it borrows:** The Listener's wordless-interruption horror and tip-line-native evidence, the strongest devices we built, now with a body under them · G's confession-and-silencing braid, Cortez entrance, and voice-only victim economy · H's hunted-witness clock and polite midpoint visitor. **What it risks:** doing everything and therefore having no identity of its own; three casts of load on one witness character; the 12:04 interrupter needing an eventual face without spending season villains, so an episode-level proxy is required, Kroll eligible.

**Cast and art cost:** between G and H. The caller is voice-only; the witness is one new full sheet; Cortez; an episode proxy. Backgrounds: foundry basin (shared with G if both advance), the witness's inland town (one master).

**Coin plant:** in the dead caller's effects, sewn into a coat lining, the unspent 1989 coin. Same mint as G's.

~~Doctrine score: 47/50~~ **VOID per 0.1. See section 5.**

**Season notch planted:** identical class to G. Paid silence, 1989, the coin.

### AGENCY MAP I — flavour: EDITORIAL CONTROL

I is the only premise where the player's *show* is the instrument. So the signature is not a branch, it is a desk: **what do you put on air, and who is listening when you do.** Every choice in I is broadcast, deduction, or the promise the tip line makes.

| # | Phase | The decision | What changes in PLAY | Persists | Class | Callback |
|---|---|---|---|---|---|---|
| **I1** | P1, after the tape | **The first bulletin. Choose TWO of three** pieces of the 12:04 tape to air: the foghorn interval · the medication rattle · the sentence "I was paid to say it was." | Each aired item spawns its own listener-response lead in P2: a harbour pilot who knows that foghorn, a pharmacist who knows that rattle, and the third, which reaches the wrong ears and **moves the interrupter one lead closer to the witness.** The unaired item must instead be worked as an expensive solo lead, or not at all. Three combinations, three different Phase 2 shapes. | `flag_e1i_aired_foghorn` / `_meds` / `_paid` | **A + B** (three short leads, the player sees two; the unaired one banks for a later episode) | Each responder says which broadcast they heard, and the interrupter's proximity is visible on the board. |
| **I2** | P1 close | **Name the dead caller on air, or protect his family.** | NAME: the 1989 file opens faster, and Arthur plus the press family carry a whole lead's worth of work for free. PROTECT: the family stays cooperative and hands over the coat, so **the coin plant lands in Phase 1 instead of Phase 3** and the season notch is set before the hunt starts. | `flag_e1i_named_caller` | A + B | The family is either at the arrest or they are not. |
| **I3** | P2, the spine | **The nine-year-old's name.** Three candidate women fit the evidence. Pick who to pursue first. | The best detective beat in the document. A wrong first pick is not a fail state: it costs a phase tick, gives the interrupter ground, and produces a real scene, a woman who was not the witness but was in her class, who yields the school photograph the right identification needs. Order of leads changes; endgame cost changes. | `flag_e1i_first_pick_*` | **C** (three candidate leads: two short at ~5 nodes, one full) | The witness herself: "You went to Marion first. She rang me. That is the only reason I picked up." One line, and the whole choice lands. |
| **I4** | P3 | **Hold the episode, or publish.** The named branch, now play-changing. | HOLD: a 3-lead protect chain, the witness's safety, a wire, Del. PUBLISH: a 3-lead pressure chain, the city looks, the proxy panics, a second grave is nearly needed. Converge at the arrest. | `flag_e1i_published`. Same doctrine class as G5 and H5. | **C** | The close is a different recording. |
| **I5** | P2 or P3 | **The four seconds.** Spend a full lead re-analysing the room tone at 12:04, two people breathing, or skip it. | An optional lead with a real cost and a real reward. Done: the arrest is airtight and the proxy gives up the chain above him, the episode's deepest season notch. Skipped: the arrest still holds, the notch is thinner, and Ep2 opens with less. | `flag_e1i_four_seconds` | **A** (plus two variant lines at the arrest) | The player who did the work gets the Ferryman-shaped sentence. The player who did not, does not. |
| **I6** | P3 close | **The archive promise.** "The tip line keeps voices, timestamps and nothing else." Hand the 12:04 tape to HPD and break the promise, or keep it. | BREAK: the case is materially stronger, and **the tip line's inbound volume drops in Ep2**, a real, visible effect on the subscribers ladder and on how many leads the tip line can seed next episode. KEEP: a weaker case, and the tip line stays the show's engine. | `flag_e1i_promise_kept`. The strongest cross-episode flag in the document. | **A** | **Episode 2's tip line is quieter, or it is not.** The show itself is the consequence. |

**Count: six play-changing decisions, spread 2 / 2 / 2, with the highest agency density of the three. Gate cleared.** Mix: three class A, one B, two class C. Added VO ~18 lines. Added authored leads +6, the largest bill in the document.

**The structural problem the map must solve.** I's most important character does not appear until Phase 3. A keeper the player has not met cannot carry two thirds of an episode. The mitigation is the Tip Line precedent used again: **the witness is present from Phase 1 as artefacts.** A school photograph, her handwriting on a class register, her voice on a 1989 recording, the name three people almost say. By the time she opens a door, the player should feel they already know her, exactly as they will know Doyle in G without seeing his face. If that is not built in deliberately, I's Phase 3 lands on a stranger.

---

## 5. Charter re-evaluation (this replaces the scorecards)

Conducted per SAS/critical-reviewer-charter.md. Kill-gates first, then forced ranking with no ties, then external benchmark, then the strongest case against.

### 5.1 Kill-gates

| Gate | G Confession | H Overnight Girl | I Last Caller |
|---|---|---|---|
| **1. Cold open read to a stranger produces "what happens next" unprompted** | **PENDING TEST** | **PENDING TEST** | **PENDING TEST** |
| **2. Central jeopardy in one sentence with a countdown or closing window** | PASS | PASS | PASS |
| **3. The midpoint changes what the player is DOING** | PASS | **CONDITIONAL** | PASS |
| **4. Four or more play-changing decisions, not a single end gate** | FAIL as v3 · **PASS with map** | FAIL as v3 · **PASS with map** | FAIL as v3 · **PASS with map** |
| **5. Present-tense murder with dual-track jeopardy** | PASS | PASS | PASS |

**Gate 1 is not mine to pass.** The gate says a *stranger* produces the question. There is no tester data, so the honest answer for all three is: I do not know. That is precisely why Appendix A exists, and why every verdict in this section is capped until the read is run. Charter rule 5 is explicit: when tester data exists, internal scoring is advisory only. Right now there is none, so this entire section is advisory.

**Gate 2 passes for all three,** which is worth naming as a failure of the gate rather than a success of the pitches. Three for three is the same non-discrimination that voided the scorecards. For the record, the one-sentence statements:

- G: whoever paid Frank Doyle to lie in 1989 killed him within hours of him talking, and the only other person who heard the confession is a seventy-four-year-old man who wrote it down.
- H: Nina is hiding, the hunter is a day ahead, and he is working through everyone she ever trusted one polite conversation at a time.
- I: a dying man was cut off before he could say her name, the people who cut him off know it, and she does not know she is being looked for.

I is the strongest of the three sentences, because its victim does not know she is one.

**Gate 3 is where the gates finally discriminate.** G passes explicitly and mechanically: the case turns from investigation to protection, and with G4 the available generator set literally changes. I passes twice, at both phase turns. **H is conditional.** As pitched, H is two phases and the doc itself says the chase "resists a third phase, it wants acceleration, not plateau." A chase's midpoint is more of the same but faster, and Kroll at the counter changes what Ally *knows and fears*, not what the player *does*. With triage in place H passes cleanly, because the midpoint becomes the point where the hunter starts pre-empting and the player switches from gathering to denying. Without triage, H fails gate 3. This is the single most load-bearing conditional in the document.

**Gate 4: the v3 document as committed fails for all three.** One branch each, at or near the end, The Listener's exact defect. That finding is the reason for this revision, and it should be recorded rather than quietly fixed.

### 5.2 Forced ranking, no ties

Ranked per dimension, first place best. The reasons are the point; the ordinals are just the discipline.

| Dimension | 1st | 2nd | 3rd | Reason |
|---|---|---|---|---|
| **Cold-open pull** (predicted; the least reliable row in this table) | **I** | H | G | I's interruption is the most unusual thing in the document: a confession physically stopped by a hand over a receiver, four seconds of two people breathing. It needs no prior context and it is wordless horror, which is the device our own testers responded to in The Listener before the rest of the episode let it down. H's boy is strong, but "dead man's switch" is a writerly phrase doing work a child's voice should do alone. G asks a stranger to care about a booth and a grandfather they have not met. |
| **Present-tense jeopardy legibility** | **H** | I | G | H's clock is felt without being explained: he is a day ahead and he is working a list. I's is felt but must be inferred, because she does not know. G's arrives at the midpoint rather than at minute one. |
| **Medium fit** (does it survive a merge economy played across days) | **G** | I | H | G is day-structured and protection has natural downtime built into it. I's hunt plausibly takes days. H's chase fights the format hardest. See 5.4.1. |
| **Agency per authoring dollar** | **H** | I | G | H's triage is one mechanic used three times, costing three 3-node stubs, and it buys an ending the player earned. I has the highest agency density but pays two class C branches for it. G's map is elegant and thematically unified, but its decisions are mostly different routes to the same arrest and it never lets the player be wrong. |
| **Production cost** (cheapest first) | **G** | I | H | Per the pitch bodies' own art analysis. Doyle voice-only is the cheapest victim design in the document. H needs Nina as a full sheet, Kroll's canon entrance, the brother, the widow, and two new backgrounds. |
| **Tutorial and systems synergy** | **G** | H | I | G puts the rusty_anchor family, gated since it was built, at the centre of the case, and its crime scene is the room the tutorial taught in. H's synergy is real but inverted: the Kestrel Corner Diner is Del's canon table, the food_gifts generator and a tutorial space, and week one turns it into a crime-adjacent location. That is a tonal collision, not a bonus. |
| **Season-arc leverage into Ep2 Ferryman** | **I** | G | H | "Someone let a dying man have his conscience, except for the sentence that mattered" is the most Ferryman-shaped sentence anyone has written for this project. G's paid-silence notch is the same class but blunter. H's circled Thursday tide is good, and procedural. |
| **Distance from The Listener's failure mode** | **H** | I | G | The Listener failed as: an elderly person, a warm relationship, a decades-old wrong, and a threat the player had to be told about. H shares none of that shape. I shares the tip line and an elderly voice but inverts both by killing him in the first minute. G shares the most surface DNA, an old man, a warm bar, a 1989 wrong, and although its jeopardy is genuinely different, surface shape is what a tester meets first. |

**Cluster read.** Each pitch wins a coherent cluster and no pitch wins two, which is what a real ranking looks like:

- **G wins PRODUCTION** (cost, medium fit, systems synergy).
- **H wins AUDIENCE** (jeopardy legibility, agency per dollar, distance from the failure).
- **I wins STORY** (cold-open pull, season leverage).

### 5.3 External benchmark

Charter rule 4 forbids judging these against each other or against The Listener. Against the genre's actual pilots:

- **Serial S1E1 "The Alibi"** opens on the host's own problem, whether anyone can remember an ordinary day six weeks ago, and only then on a teenager in prison for a murder he says he did not commit. The engine is audible uncertainty in the host.
- **Dirty John E1** opens on ordinary loneliness and a man who is too good to be true. The engine is dramatic irony, because the title has already told you.
- **Teacher's Pet E1** opens on a woman who vanished in 1982 and a suspect everyone can name. The engine is unpunished obviousness.
- **In the Dark S1** opens on a child taken and twenty-seven years of institutional failure. The engine is a system that did not work.

**The uncomfortable finding: three of those four are cold cases with no present-tense danger at all.** What they have instead is an *unbearable asymmetry*. Someone knows and will not say, or an institution failed and will not admit it. Our pivot doctrine, present-tense jeopardy, is one way to manufacture that asymmetry, and it is not the way the genre's biggest pilots do it.

This does not overturn the pivot. Twelve of twelve testers is real evidence and internal doctrine is not. But it sharpens what the testers were asking for. "No jeopardy, too safe" is not necessarily a request for a chase. It is a request for someone in the story to be *getting away with it while you listen*. Read that way:

- **I is closest to the genre's proven grammar.** Somebody was in the room and permitted a confession up to the name. The asymmetry is stated in the first sixty seconds.
- **G is second.** Someone has been waiting thirty-seven years and is still waiting, patiently, in the present.
- **H is a thriller, not a true-crime pilot.** That is not a defect, it is a different product, and with a mobile audience who did not come for Serial it may well outperform the benchmarks. But it departs furthest from what the genre has proven, so it carries genre risk that G and I do not.

### 5.4 The strongest case against each

**5.4.1 The clock and the medium (hits H hardest, all three somewhat).** Charter gate 2 demands a countdown. The game meters play at 150-second energy regen across a 15 to 18 lead episode with four walls, so realistically the player experiences Episode 1 over several days and several sessions. A hunter with "a head start measured in hours" is being outrun by a player who is asleep. The Listener solved this with in-fiction day stamps, Day 1 Monday through Day 6 Saturday, which converts hours into days, and G and I are already day-structured. H cannot take that fix without weakening its own hook. Its only real fix is triage, which advances the clock on lead resolution instead of on time. **H's greatest strength on the page is the thing that fights the medium hardest, and the fix is not optional.**

**5.4.2 G's revival rests on an untested assumption.** The pitch body argues the round-1 objection is "void" because Gerald now fronts the tutorial. Ninety seconds of hint chips carrying a portrait produces recognition, not attachment. The claim may well be true and it is cheap to test: Appendix A question 3 asks each tester, after the read, who they would least like to see hurt. If G's readers do not name Gerald, the revival's central argument has failed and G should not be Ep1.

**5.4.3 I's protagonist-of-the-case is absent for two thirds of it.** The witness carries the grievable-victim load, the endangered-person load and the resolution, and she does not appear until Phase 3. The map's artefact mitigation is a real fix, but it has to be *designed in*, and if it is skipped under schedule pressure the episode's emotional payoff lands on a stranger. I also carries the identity risk its own pitch body admits: it borrows from all three predecessors and could end up being a hybrid rather than a thing.

**5.4.4 The split keeper problem (H).** H gives the grievable victim, the clerk, a keeper, his widow, who is disconnected from the chase, while the person in danger, Nina, has a separate emotional thread through her brother. Two grief threads, neither of them the spine. G concentrates grief in June. I concentrates it in the witness. H spreads it, and spread grief is thin grief.

**5.4.5 A production risk that outranks all of the above.** The launch plan is under review, surgery is at or after mid-September, and this is a solo project. Section 1.6 shows agency raises authored leads by a quarter to a third, and the Agency Bridge is new engineering. H is simultaneously the heaviest art bill and the most structurally novel design. Ranking it first on audience merit does not make it the right thing to attempt in this window, and that tension is a ruling for Stephen, not a judgement to bury inside a recommendation.

### 5.5 The three changes that would most improve this slate

1. **Build the Agency Bridge before the premise ruling** (1.5). It is premise-independent, half a day of code plus tests, and until it exists every agency map here is fiction. Building it first also means the skeleton pass is authored against a working gate rather than against a promise.
2. **Run the table-read before choosing** (Appendix A). Every conclusion in section 5 is advisory by the charter's own rule 5. Five testers, three reads, twenty minutes each. It is the cheapest decisive act available and the only one that produces evidence rather than opinion.
3. **Make the artefact-presence design explicit for whichever premise wins.** G already does it, because Doyle is a voice and a notebook. I needs it built deliberately (5.4.3). H needs it least, and that is a hint about where H's emotional risk sits.

### 5.6 Verdict

**TEST BEFORE COMMITTING.**

The slate is materially stronger than v3: gate 4 is cleared for all three, gate 3 now discriminates, and the rankings separate the pitches into three coherent clusters instead of three near-identical percentages. What is missing is not more analysis. It is the one input the charter says outranks all of this, and it does not exist yet.

**Recommendation, conditional, and for Stephen to rule:**

- **If the schedule can absorb the heaviest art bill in the document: H,** and ruling for H means ruling for triage (5.1 gate 3, 5.4.1). H wins the audience cluster, and the audience cluster is the one that killed the last pilot. H also preserves maximum optionality: G and I share a 1989 spine and are mutually exclusive, so choosing H keeps both in the bank, whereas choosing either of them spends the other.
- **If it cannot: I, not G.** I is mid-cost, wins the story cluster, sits closest to the genre's proven grammar (5.3), and stands furthest from The Listener's surface shape. Its risks (5.4.3) are design risks, fixable at the skeleton pass. G's central risk (5.4.2) is an audience risk, only fixable by finding out.
- **G is not third on quality.** G is the best-engineered pitch in the document, the cheapest, and its map has the tightest thematic through-line. It ranks below the other two here for one reason: its appeal depends most on a relationship the player has not had time to form, and that is the exact class of assumption that produced a twelve out of twelve rejection eight days ago.

**Confidence: low to moderate, deliberately.** High confidence in the kill-gate findings and the cost analysis, because those are verifiable. Low confidence in the cold-open ranking and therefore in the recommendation, because the decisive evidence has not been gathered.

**What would change the verdict:** a table-read in which one cold open is picked by four or more of five testers flips this to PROCEED on that premise regardless of anything above. A read in which G's listeners name Gerald in question 3 restores G to first on the audience cluster and makes it the clear overall winner given its cost advantage. A read in which no cold open pulls clearly means none of the three is the pilot, and the slate needs a fourth.

---
## 6. Comparison at a glance (revised)

| | G Confession | H Overnight Girl | I Last Caller |
|---|---|---|---|
| Present-tense murder on-page | yes, night one | yes, ice store body | yes, by morning |
| Victim-track jeopardy | June's truth and Gerald's life | Nina hunted now | witness hunted now |
| Ally-track jeopardy | family (Gerald) | Kroll at the counter | racing the interrupter |
| Played length | 17 leads / 3 phases | 15 / 2 | 18 / 3 |
| **Authored lead assets** | **~21** | **~20** | **~24** |
| **Play-changing decisions** | **6 (2/2/2)** | **6, ending accumulates** | **6 (2/2/2)** |
| **Agency flavour** | **custody: who do you trust with this** | **triage: what do you give up** | **editorial: what goes on air** |
| **Expensive (class C) branches** | 1 | 1 | 2 |
| **Added VO over baseline** | ~14 lines | ~12 lines plus 3 stubs | ~18 lines |
| New art | lightest (Cortez, June) | heaviest (Nina, Kroll, brother, widow) | middle |
| Tutorial synergy | strongest, Gerald-centred | inverted, the diner becomes crime-adjacent | neutral |
| Reuses Listener devices | announcer, bar | archive breadcrumbs | strongest, the interrupted tape |
| Cluster won | **production** | **audience** | **story** |
| ~~Score /50~~ | ~~47~~ VOID | ~~46~~ VOID | ~~47~~ VOID |

## 7. Economy consequences (per ruling 2: reported, not pre-decided)

At 15 / 17 / 18 played leads versus the tuned 13, total T1eq scales to roughly 380 / 430 / 460, keeping the 5-10 minute lead beat and the Act-1 frictionless hook, with the first five leads staying under ~36 T1eq combined. Wall placement: keep four walls, re-sited between story beats per pitch, drafted in the skeleton pass after the premise ruling.

**Added by v3.1.** The agency maps put economy inside the fiction, which is the cheapest agency there is, and it means Schedule B can no longer be a single column. Three consequences to carry into the regeneration:

- **Branch-divergent requirement cost.** G2, H2 to H4 and I1 change which family a lead needs, so Schedule B needs a per-branch T1eq band rather than one number per lead, and the band's width is the tuning risk. Rule of thumb to hold: **no branch may be more than 15% more expensive than its sibling**, or the choice becomes an economy decision wearing a story costume.
- **Special-item consumption as a choice** (G2's SearchWarrant). This is the first time a Case Kit special would be spent by a narrative decision rather than by a puzzle. It is a good idea and it needs its own line in the specials budget.
- **Subscriber payoff asymmetry.** G5, H5 and I4 all make the loud branch pay more subscribers than the quiet branch. That is correct fiction and dangerous economy: if loud always pays better, the quiet branch is a tax on players who chose with their conscience. Compensate the quiet branch in a different currency, CC or a special or an extra evidence card, never in subscribers.

Obligations triggered in all three cases, to be executed together after the premise ruling: Schedule B regeneration (new CSV grid, now per-branch), subscribers ladder recompute (episode-close projection moves from ~8,400 to ~10-11k, all eight tier thresholds re-derived, conversion-moment fiction re-ruled onto the new pilot), cold-case tail re-sourced from the new close lead. Hard limits respected in all skeletons: quantity three or fewer per requirement line, current board geometry, one generator grant per lead.

## 8. Season 1 straw man (re-based, all premises compatible)

**Ep1** murder pilot (coin whisper, "someone knows her name") → **Ep2 The Ferryman** (detonation; its five Listener re-points reversed to point at the new pilot's coin, and the v3/v4 change tables make this auditable) → **Ep3** The Listener, demoted and dread-passed ("the woman who calls my show every night went silent" lands harder on an audience that now knows the show) → **Ep4** Pitch F "The Boy Who Said Yes" (banked; the systemic register works mid-season) → **~Ep5** Ghost Student (Voss Bio surfaces per the existing ladder) → mid-season red-herring spend → **Finale** reserved, the Ferryman unmasked live. One notch per episode, costs compound, and whichever murder pitch is not chosen for Ep1 banks as season stock. G and I share a 1989 spine and are mutually exclusive; H banks cleanly alongside either.

**Added by v3.1.** The public-pressure doctrine flag (`flag_e1*_aired` / `_published`) is the same class of decision in all three premises. Whichever wins, that flag should be renamed to a season-level name at the skeleton pass (`flag_ally_doctrine_loud`) and read by Ep2 and Ep3, so the first thing a returning player learns is that the game remembered what kind of journalist they decided to be. That is the cheapest cross-episode payoff available and it costs one variant line per episode.

## 9. Rulings requested

1. **The premise:** G, H or I, or a directed cross. The table-read in Appendix A is the recommended input; section 5.6 is advisory only.
2. **The agency maps:** accept, trim or replace, per premise. Accepting a premise does not accept its map.
3. **The Agency Bridge (1.5):** authorise it now, before the premise ruling, as premise-independent engineering. Recommended yes.
4. **On the winner:** green-light the full skeleton pass (lead-by-lead beat sheet, the lead ledger of 1.7, the agency map wired to lead ids and flags, economy regeneration, art and VO manifest) as the next artifact, before any script writing.
5. **The clock convention:** confirm that day-stamped structure (Day 1 through Day 6, as in The Listener) is the house solution to 5.4.1, and that any premise choosing an hours-scale clock must convert it to a lead-count clock instead.

---

## Appendix A — The table-read kit

**Purpose.** This is the decision input. Per charter rule 5, tester evidence outranks every ranking in section 5, including the recommendation.

**Who.** Five or more of the twelve F&F testers who read The Listener. Prefer the ones who were most specific in their criticism, not the kindest.

**How.**

1. Read aloud. Do not hand them the page. The cold opens are written to be heard, and reading silently changes what a hook does.
2. **Do not say the titles, the pitch letters, or anything about the episode.** Titles bias. So does "this is the murder one."
3. Give only the framing line at the top of each read.
4. **Rotate the order for every tester** using the table below. Whatever is read first has an advantage; rotating cancels it.
5. Ask questions 1 and 2 after each read, while it is fresh. Ask question 3 only after all three are done.
6. Write down their words, not your summary of their words.

**Rotation table**

| Tester | First | Second | Third |
|---|---|---|---|
| 1 | Read 1 | Read 2 | Read 3 |
| 2 | Read 2 | Read 3 | Read 1 |
| 3 | Read 3 | Read 1 | Read 2 |
| 4 | Read 3 | Read 2 | Read 1 |
| 5 | Read 1 | Read 3 | Read 2 |
| 6 | Read 2 | Read 1 | Read 3 |

**The questions**

- **Q1, after each read:** in one sentence, what do you think is going on. (This is a comprehension check, not an opinion question. If they cannot say it, the open is not working, no matter how much they liked it.)
- **Q2, after each read:** who in that would you least like to see get hurt. (This is the attachment probe, and it is the specific test of 5.4.2. If Read 1's answer is not "the grandfather", Pitch G's revival argument has failed.)
- **Q3, after all three:** which one do you need to hear the rest of, and would you come back tomorrow for it.

**Scoring.** None. Count Q3 first-choices, and read the Q1 answers for confusion. Four or more of five converging on one read is decisive. A three-two split is not a result; it means run three more testers.

**Private key, not to be shown to testers:** Read 1 = Pitch G "The Confession" · Read 2 = Pitch H "The Overnight Girl" · Read 3 = Pitch I "The Last Caller".

---

### READ 1

*Framing line to say first: "This is the first minute of a true crime podcast episode. The host is a woman called Ally. Tell me what you think afterwards."*

> **ANNOUNCER (flat, synthetic):**
> Message received: Saturday, eleven fifty-one p.m.
>
> **MO (a voicemail, low, no bar noise behind her):**
> Ally. It's Mo. Not an emergency. Except I think it might be. There's a man in Gerald's booth who's been nursing one whiskey for four hours, and he just told your grandad something I wish I hadn't heard. Gerald's writing it down. Come when you can, my lovely. Come tonight.
>
> **ALLY (on-mic):**
> By the time I got there, the man was gone. Gerald was still in the booth. He hadn't touched his glass.
>
> He slid his notebook across the table and said: read it twice, love. Because tomorrow he'll deny it, and by Monday he'll be dead.
>
> He was wrong about one thing.
>
> Frank Doyle didn't live until Monday.

*Q1: in one sentence, what is going on. · Q2: who would you least like to see get hurt.*

---

### READ 2

*Framing line to say first: "This is the first minute of a true crime podcast episode. The host is a woman called Ally. Tell me what you think afterwards."*

> **ANNOUNCER (flat, synthetic):**
> Message received: Thursday, three twelve a.m.
>
> **BOY (young, steady the way children are when they have rehearsed being brave):**
> My sister rings twice every night. Two rings means safe.
>
> It rang once tonight.
>
> She made me learn this number in case it ever rang once. She said ask for Ally. She said say the diner, say the ice store, say run.
>
> **ALLY (on-mic):**
> He recited it like a times table. Somebody taught a twelve-year-old to be a dead man's switch.
>
> That somebody is out there right now, with a head start measured in hours.
>
> And so is whoever she built the switch against.

*Q1: in one sentence, what is going on. · Q2: who would you least like to see get hurt.*

---

### READ 3

*Framing line to say first: "This is the first minute of a true crime podcast episode. The host is a woman called Ally. Tell me what you think afterwards."*

> **ANNOUNCER (flat, synthetic):**
> Message received: Wednesday, twelve oh four a.m.
>
> **CALLER (elderly, deliberate, reading from something he has written down):**
> My name doesn't matter yet. Write this down instead.
>
> In August of nineteen eighty-nine a man went into the harbour at the foundry basin and it was called a fall. It was not a fall. I was paid to say it was.
>
> The one who paid me had a voice I will hear when I die, which I am told is soon, so I am spending it.
>
> There was a witness. She was nine years old. Her name is...
>
> **[The line does not click. It muffles: a hand over a receiver. Four seconds of a room with two people breathing in it. Then dial tone.]**
>
> **ALLY (on-mic):**
> The tip line keeps voices, timestamps and nothing else. That has always been the promise. Tonight I would trade the whole archive for one more second of tape.
>
> Somewhere in this city is a woman who watched a murder when she was nine years old.
>
> Two people are looking for her tonight.
>
> I am the slower one.

*Q1: in one sentence, what is going on. · Q2: who would you least like to see get hurt.*

---

*All quoted script lines above are draft register sketches, not locked copy. They follow the no-em-dash and no-exclamation rules and will go through the standard line-by-line ruling at script pass.*
