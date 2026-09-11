<!-- pdf-title: The Friends with Four Keys, the Lead Structure v1.0 -->

# THE FRIENDS WITH FOUR KEYS
## THE LEAD STRUCTURE, v1.0

*2026-08-31. Built on spine v1.4 (three attack rounds, sweep PASSES), premise v8, the five v1.2, cold open v0.4, and the Four Keys rulings in memory (16 leads, 4 to 5 hours, 8 to 10 sessions, session boundary = podcast episode boundary). No prose. Every player-facing line in this document is a placeholder and says so. The spine is not reopened here; nothing in this document moves a spine fact, and nothing needed to.*

**Code checked before trusting any spec (2026-08-31):** `LeadData` (leadId, RequiredLeadIds AND-gate, SpawnLeadIds, requiresFlag/forbidsFlag, NarrativeFlags, banded rewards, generator and special rewards, boardPhase) · `LeadsRepository` (CheckAndUnlockBlockedLeads state-scan, SpawnLead spawns Blocked when gated, ApplySavedStates re-evaluates gates) · `LeadsDatabase.FindById` (the database, not the folder, decides membership) · `Lead_E1_*` assets as the shape reference (`e1_close` spawns `cold_case_a` + `ep2_teaser`, sets `e1.ep01.complete`) · the branch pattern as shipped (flags set in dialogue, variant nodes pay at later leads; no BranchOutcomes runtime UI exists). The multi-episode system (EpisodeCatalog, slot ep01) is on `feature/multi-episode-audit`, pushed, NOT merged: this document designs to its id scheme and touches no code.

**Governing rule, applied to every card in Part B:** the rank-one open question of this project is whether merging changes what a player believes. Every lead answers it on paper: belief before, evidence gained on the board, belief after. The three weakest answers are named honestly in Part E.

---

## THE STRUCTURAL DECISIONS, STATED ONCE

1. **Ids.** Runtime lead ids are `fk_*`, asset names `Lead_FK_*`, content folder `Assets/Content/FourKeys/`. Why `fk` and not `e1`: The Listener's `e1_*` ids hardcode an episode slot the story no longer holds, and R5 ruled slot ids story-neutral for exactly this reason. A story-scoped prefix survives renumbering. The `LeadsDatabase` for slot ep01 decides membership; the folder name implies nothing (the standing content-folder rule).
2. **The publish beat is the session gap, not a lead.** Completing a session's last lead hands Ally enough to cut that week's episode. She publishes in the gap; the next session's opening dialogue is the aired episode and what came back. This is the ruled session boundary made literal: the gap between sessions is Brad listening. No wall-clock gate exists or is added; the boundary is enforced economically, because each session's requirement mass is sized to roughly one energy tank (arithmetic in Part E). A binge player flows through without a wall, which is deliberate: R6 forbids replay, not pace.
3. **Ten sessions, sixteen leads plus a close.** Broadcast episodes 1 to 9 air in the gaps after sessions 1 to 9. Episode 10, ruled indistinguishable from episode 9, airs inside the close lead's narration, because a merge lead whose deliverable is a deliberately empty broadcast would fail the evidence-turn test by construction. The player instead holds the answer through it, which is the point of that week.
4. **Decisions are Class A and B only.** The spine's clock (Days 266 to 333) survived three adversarial rounds by being inevitable; no player decision may move it, so no decision changes who is arrested or when. The four decisions (Part C) change who is protected, exposed and believed along the way, and what the evidence record ends up holding. That satisfies the consequence-and-agency gate on its protected/exposed/believed axes; the honest caveat is stated in Part C and again in Part E.
5. **Requirement families** (per `project_item_families.md` and the shipped economy): **A** = Audio Investigation (lab, T1 to T6), **F** = Forensic Tools (lab, T1 to T5), **R** = Rusty Anchor (junk drawer, T1 to T10), **D** = Kestrel Corner Diner food (diner, T1 to T12). T1eq of tier n = 2^(n-1). Diegetic reads for this episode: A = the tape and the show (the tip line, the letters read aloud, the broadcast itself); F = documents and scene work (letters, the slip, photographs, the side-by-side spread); R = the harbour (the boatyard, the slip swimmers, the fish market, the pub that changes its name); D = door-knocking (the café, the cinema, Tessa's kitchen, a witness's coffee).

---

# A. THE MAP

One table. Sessions air their episode in the gap that follows them. Days are Ally's working days from the spine's clock; B6 rows are the sources ledger in spine v1.4.

| # | Lead id | Title (placeholder) | Type | Session | Spine days | Location | Airs in the gap after (A5) | What Ally airs that week |
|---|---|---|---|---|---|---|---|---|
| 1 | `fk_frontpage` | The Front Page | Podcast | S1 | 266 | Ally's studio | | |
| 2 | `fk_fourkeys` | Four Keys | Evidence | S1 | 267 | Ally's studio | | |
| 3 | `fk_sergeant` | We Looked, Properly | Interview (Del) | S1 | 268 to 269 | Precinct steps | **Ep 1, Day 269** | The cold open v0.4: the death, the keys, the door, two survivors afraid of each other |
| 4 | `fk_kitchen` | The Kitchen Table | Interview (Ruby) | S2 | 270 to 275 | Ruby's kitchen | | |
| 5 | `fk_boxes` | The Boxes | Evidence | S2 | 270 to 275 | Ruby's spare room | **Ep 2, Day 276** | The room, the letters read out (D1), the boxes, Brad's habits |
| 6 | `fk_calling_in` | Calling It In | Money Trail (café owner) | S3 | 278 to 282 | The harbour café | | |
| 7 | `fk_gold_panel` | The Gold Panel | Interview (cinema owner) | S3 | 278 to 282 | The Regent cinema | **Ep 3, Day 283** | She was calling money in; she carried a gold panel round asking whose hand |
| 8 | `fk_liam` | The Man It Points At | Interview (Liam) | S4 | 288 | The boatyard | | |
| 9 | `fk_bay3` | The Wednesday Visitor | Location (fish market) | S4 | 290 | Bay 3, the market | **Ep 4, Day 290** | Liam's side. The fishmonger's tip goes to Del and is never aired (D2) |
| 10 | `fk_asking` | Did She Say Anything | Podcast · milestone | S5 | 291 to 297 | Studio / Mariner's Row lane | **Ep 5, Day 297** | The ask about Margo; a listener: the build No. 14 gave is wrong for Liam |
| 11 | `fk_tessa` | Two Hours Up the Coast | Interview (Tessa) | S6 | 300 | Tessa's kitchen | | |
| 12 | `fk_wrong_thing` | The Thing in the Cupboard | Data | S6 | 301 to 304 | Studio | **Ep 6, Day 304** | Margo's words, anonymised (D3); if the thing was wrong there was a real thing |
| 13 | `fk_ithink` | I Think Against I Know | Data · wall | S7 | 305 to 311 | Studio | **Ep 7, Day 311** | The certainty gap; the slip's date; the letter kind to exactly one man; nobody ever saw Brad write |
| 14 | `fk_skipper` | Three Years Before | Evidence | S8 | 311 to 313 | The harbour master's office | | |
| 15 | `fk_platform` | The First Train | Evidence | S8 | 314 to 318 | The inland halt | **Ep 8, Day 318** | The 05:47 and the boots go to Del, not on air; on air: if you were on that train, write to me |
| 16 | `fk_one_of_a_kind` | One of a Kind | Podcast · climax | S9 | 319 to 325 | Studio | **Ep 9, Day 325** | The panel described (D4). One of a kind |
| 17 | `fk_close` | The Man on the Wall | Podcast · close | S10 | 326 to 333 | Studio | *(Ep 10, Day 332, airs inside this lead's narration)* | The accounting, after the arrest is public |

**The gap that matters most is the last one:** between S9 and S10 the panel burns (Day 325 night), the landlady dusts the room (Day 326), and she writes. The player left the app having aired the description; they return to her letter.

**Evidence board phases** (`boardPhase`): Phase 1 = leads 1 to 7 (the public case) · Phase 2 = leads 8 to 13 (the doubt) · Phase 3 = leads 14 to 17 (the man).

**Locations and art cost** (new builds flagged): Ally's studio (exists), precinct (exists), Ruby's kitchen ⚠ new, the harbour café ⚠ new or redress, the Regent cinema frontage ⚠ new, the boatyard ⚠ new or redress, the fish market corridor ⚠ new, Mariner's Row lane ⚠ new, Tessa's kitchen ⚠ new or a Ruby-kitchen redress, the harbour master's office ⚠ new or redress, the inland halt platform ⚠ new. Kestrel Head is already flagged unbuilt at the spine and is needed only as photographs or narration in this structure, never as a playable location: that is deliberate, it keeps the cliff off the art bill.

---

# B. THE LEAD CARDS

Format per card: purpose · belief before → evidence gained on the board → belief after · sources (B6 row numbers from spine v1.4) · requirement (band, families, T1eq) · gates in / spawns out · decision if any · the redaction line (what Fable may be told when this lead's prose is briefed; the standing base for every brief is spine Part G plus all earlier briefs, so each line states only the delta). All titles and subtitles are placeholders. Rewards are draft values, tuned to hold the shipped earn rate (Part E).

### 1 · `fk_frontpage` · The Front Page · Podcast · S1

- **Purpose:** cold install to first merge inside forty seconds; the death and the ruling on the table; FTUE entitlements and the first generator grant (mirrors the shipped `e1_tip` duties: junk drawer via overflow, family flags per the systems note in Part E).
- **Belief:** nothing (cold install) → the front page assembled on the board, the wake photograph pinned (choreographed deterministic first merge, §5.7 pattern) → *a woman died on her stairs nine months ago, it was ruled an accident, and four friends got keys.* Orientation, not a turn; named as such in Part E.
- **Sources:** row 54 (the Gazette front page).
- **Requirement:** Easy · 1× A-T2 · 2 T1eq. Reward draft: 20 CC.
- **Gates/spawns:** initial Available lead (design-time state 1). Spawns `fk_fourkeys`.
- **VO:** cold open v0.4 segment 1 (Part D).
- **Redaction line:** Fable may be told: nothing new. The VO is v0.4 as ruled; Fable writes only Ally's connective working notes from Part G.

### 2 · `fk_fourkeys` · Four Keys · Evidence · S1

- **Purpose:** the arrangement lands; the dramatic question is set (Pilot Doctrine #1).
- **Belief:** an accident, a sad thing → the letter's public outline and the four keys laid out as an evidence spread → *the dead woman predicted her death and built an arrangement around it. Accident is now only one of two readings.* First real turn.
- **Sources:** row 54 (keys, letters, the locked door, the Saturday agreement are all in the public account, Part G).
- **Requirement:** Easy · 1× A-T3 · 4 T1eq. Reward draft: 20 CC.
- **Gates/spawns:** requires nothing (spawned Available by 1). Spawns `fk_sergeant`.
- **VO:** cold open v0.4 segment 2 (Part D).
- **Redaction line:** Fable may be told: nothing new beyond Part G.

### 3 · `fk_sergeant` · We Looked, Properly · Interview (Del) · S1

- **Purpose:** close the police door honestly, so the whole season's sourcing logic is taught in one scene: nothing will come from the file; everything will come from people.
- **Belief:** the police must have missed something → the outline of what was actually checked, pinned (letters examined and consistent, solicitor confirmed, movements reviewed, no further action) → *the police looked properly and found nothing. Whatever is wrong here is invisible to procedure. Only people hold it.* This is the turn that makes the podcast the instrument rather than a commentary on one.
- **Sources:** rows 64 (Del: we looked, properly, there's nothing), 54.
- **Requirement:** Easy · 1× A-T4 + 1× A-T2 · 10 T1eq. Reward draft: 20 CC.
- **Gates/spawns:** requires `fk_fourkeys` (linear FTUE). Spawns `fk_kitchen`, `fk_boxes` (parallel pair). **Ep 1 airs in the gap.**
- **VO:** cold open v0.4 segment 3 including the ruled sign-off (Part D).
- **Redaction line:** Fable may be told: Del exists as the sergeant from Part G and says only what Part G gives her. Never: anything the file holds (Ally never gets it either).

### 4 · `fk_kitchen` · The Kitchen Table · Interview (Ruby) · S2

- **Purpose:** the room. Five months of meetings, the reading, and the man who defended Liam, delivered as Ruby's testimony with no interpretation from Ally.
- **Belief:** the letter is a dead woman's warning and the reading landed on Liam because it fit → the room reconstructed on the board: meeting by meeting, claim by claim, who argued what (the letters' claims as the survivors hold them: the yard, the tin, the ladder, the board) → *the reading landed on Liam because the letter's own lines lean that way, and the one man who argued his side lost every time.* The inversion enters as texture, unremarked.
- **Sources:** row 55 (Ruby on record: the room, the letters read out, the meetings).
- **Requirement:** Standard · 2× D-T4 + 3× D-T3 + 2× A-T3 · 36 T1eq. Reward draft: 170 CC.
- **Gates/spawns:** spawned by 3, parallel with 5. Spawns nothing (6 and 7 gate on both).
- **Decision:** **D1, the letter on air** (Part C).
- **Redaction line:** Fable may be told: Ruby's account of the room and the meetings; the full text of the key letters and the cage letter *as the survivors hold them* (the claims about the yard, the tin, the ladder, the board). Never: that any of it is forged, or by whom.

### 5 · `fk_boxes` · The Boxes · Evidence · S2

- **Purpose:** Brad becomes a person, in absence, through objects and habits; and the episode gets its first hard date.
- **Belief:** Brad is the tragic friend who broke → the boxes catalogued (nothing in them), his habits pinned (keys on the table moved twice, thank you like it cost him, up at five to run), and in Ruby's envelope the solicitor's covering slip → *Brad is a specific man, not a grief; and the letters were lodged three weeks before Violet died. Her fear predates everything.* The player holds the slip's date from here; its full meaning lands at lead 13, which is honest, because that is when Ally airs it.
- **Sources:** rows 55, 41 (the boxes hold nothing), 62 (the slip and postmark, from Ruby's envelope, Day 276).
- **Requirement:** Hard · 3× F-T5 + 3× F-T4 + 2× F-T3 · 80 T1eq. Reward draft: 380 CC.
- **Gates/spawns:** spawned by 3, parallel with 4. **Ep 2 airs in the gap** once 4 and 5 are both activated. Spawns nothing (gate carried by 6 and 7).
- **Redaction line:** Fable may be told: the boxes and their nothing; Brad's habits (Fable wrote them in the five v1.2); the slip, its date, and that Ally notes it without yet knowing what it proves. Never: what it will prove.

### 6 · `fk_calling_in` · Calling It In · Money Trail (café owner) · S3

- **Purpose:** money enters the story, from a witness who owed her nothing.
- **Belief:** nobody had a reason to want Violet dead → the debt conversation reconstructed from the café (she was calling in what she was owed, for the lawyer, in her last months) → *money moved in her last months. Someone owed her, and she was collecting.* No name attaches; the player's suspicion has a category now, not a face.
- **Sources:** row 56 (café owner, on record).
- **Requirement:** Standard · 2× D-T5 + 1× D-T4 · 40 T1eq. Reward draft: 170 CC.
- **Gates/spawns:** requires `fk_kitchen` + `fk_boxes`. Spawned by 4 and 5 (idempotent), parallel with 7.
- **Redaction line:** Fable may be told: the café owner's account: she was calling her money in. Never: who owed most, or why it matters.

### 7 · `fk_gold_panel` · The Gold Panel · Interview (cinema owner) · S3

- **Purpose:** what Violet found gets a shape; and the download map plants the far town, player-only, with Ally.
- **Belief:** what she found is unknowable, gone behind the door → a month of whose-hand sightings plotted (the cinema owner, the café, the panel described loosely: gold letters on a dark board, a hand she could not place) plus Ally's own download map, one town four hundred miles away listening every week → *what she found had a shape, and she showed it round town for a month. Whoever hid it heard her asking. And somewhere far away, one town is listening very hard.* The map is Ally's own data (row 61); it is never aired and never explained here.
- **Sources:** rows 56, 61 (download map, Day 283 onward, her own analytics).
- **Requirement:** Hard · 2× R-T5 + 2× D-T5 + 3× D-T4 · 88 T1eq. Reward draft: 380 CC.
- **Gates/spawns:** requires `fk_kitchen` + `fk_boxes`, parallel with 6. **Ep 3 airs in the gap** once 6 and 7 are both activated. 8 and 9 gate on both.
- **Redaction line:** Fable may be told: the whose-hand month; the panel only as witnesses describe it (gold letters, dark board); that the download map shows one persistent far town and Ally files it as odd. Never: what the panel is, whose hand, or what the town means.

### 8 · `fk_liam` · The Man It Points At · Interview (Liam) · S4

- **Purpose:** Liam talks (spine Day 288). The letter's case against him is tested claim by claim, in his own flat voice.
- **Belief:** the letter's case against Liam is at least arguable → each claim laid against his account on the board: the third (true, lawful, confirmed by him), the tin (I never gave her a penny: undisprovable either way), the ladder (she told everyone which ladder) → *every claim is either true but innocent, or unfalsifiable. The letter reads like a case built to fit him.* The word "built" enters the player's vocabulary and is not yet aired anywhere.
- **Sources:** row 58 (Liam, Day 288).
- **Requirement:** Hard · 2× R-T6 + 1× R-T5 + 1× R-T4 · 88 T1eq. Reward draft: 380 CC.
- **Gates/spawns:** requires `fk_calling_in` + `fk_gold_panel`. Parallel with 9 (9 spawns Blocked beside it, visible, gated on 8: the tip card sits on the bar as anticipation).
- **Redaction line:** Fable may be told: Liam's side as he gives it, including the confirmed third and his refusals. Never: whether the letter is right.

### 9 · `fk_bay3` · The Wednesday Visitor · Location (fish market) · S4

- **Purpose:** the floor moves. The fishmonger's tip: Brad was at the cage on the Wednesday, three days before the Saturday.
- **Belief:** the cage was sealed until the four opened it together → the market morning reconstructed (Bay 3 at 05:40, the corridor door round the side where no camera looks, market hours, anyone with a key) → *Brad was inside the arrangement before it opened. The dead man went early.* The single largest belief turn before the close, and it is held, not aired: it goes to Del the same day (spine row 57, Ally's protocol as canon). The player now suspects Brad of something and cannot say what death has to do with it, which is exactly the ruled shape: the surprise the episode protects is not "it was him", it is "he is alive".
- **Sources:** row 57 (Bay 3 fishmonger, Tip Line, Day 290, caller consents; to Del, never aired).
- **Requirement:** Standard · 2× F-T4 + 2× D-T4 + 2× A-T3 · 40 T1eq. Reward draft: 170 CC.
- **Gates/spawns:** requires `fk_liam` (spawned Blocked beside it by 6/7's completion; unblocks on 8). **Ep 4 airs in the gap** (Liam's side; the tip is not in it). Spawns `fk_asking`.
- **Decision:** **D2, the Wednesday tip** (Part C).
- **Redaction line:** Fable may be told: the tip verbatim; that it goes to Del within the day and is never aired; Ally's private vertigo, unexplained. Never: what the visit was for, or that anything was swapped.

### 10 · `fk_asking` · Did She Say Anything · Podcast · milestone · S5

- **Purpose:** mid-season milestone (the halfway wall). Margo becomes the missing voice; the lane man stops being Liam.
- **Belief:** Margo jumped from guilt and left nothing; the man in the lane could have been Liam → the lane sighting re-measured on the board (No. 14's build set against Liam's, from a listener's letter, spine row R15 Day 297) and Margo's last months mapped: the withdrawals, the every-second-Sunday drives, the silence → *the man in the lane was the wrong build for Liam, and Margo, the first to go quiet, is the person nobody ever heard from. Ally asks the world: did she say anything to anyone.* The board work here is re-measurement rather than new physical evidence, and Part E names this lead among the three weakest for it; the ask itself is the week's broadcast.
- **Sources:** rows 55, 58 (follow-ups), R15 (the listener on the build, Day 297), 18 (the lane sighting as public door-knock fact).
- **Requirement:** Very Hard wall · 2× R-T6 + 2× R-T5 + 2× D-T5 · 128 T1eq. Reward draft: **milestone 400 CC + 20 energy**.
- **Gates/spawns:** requires `fk_bay3`. **Ep 5 airs in the gap** (the ask). Spawns `fk_tessa` and `fk_wrong_thing` (12 arrives Blocked, gated on 11, visible).
- **Redaction line:** Fable may be told: the listener's letter about the build; Margo's mapped last months as the survivors describe them; the on-air ask, word for word once ruled. Never: who contacted Margo or why she went to the point.

### 11 · `fk_tessa` · Two Hours Up the Coast · Interview (Tessa) · S6

- **Purpose:** the person nobody in Havenbay knew exists. Eleven years, every second Sunday, and one kept screenshot.
- **Belief:** Margo left nothing → the drive up the coast (door-knock kit), Tessa herself, and the screenshot authenticated on the board: *the thing in the cupboard is wrong. It can't mean that.* → *Margo doubted the letter's meaning, said so once, to one person, and never said who she suspected.* The first voice from inside the silence.
- **Sources:** row 59 (Tessa writes Day 300, owns the screenshot, consents anonymised).
- **Requirement:** Standard · 2× D-T5 + 1× D-T4 + 2× A-T3 · 48 T1eq. Reward draft: 170 CC.
- **Gates/spawns:** spawned by 10, parallel-visible with 12 (12 gated on 11). 
- **Decision:** **D3, Margo's words** (Part C).
- **Redaction line:** Fable may be told: Tessa as a person (the card in the drawer is hers to mention or not); the screenshot line verbatim; the channel's nature (self-deleting, eleven years). Never: who else ever used that channel.

### 12 · `fk_wrong_thing` · The Thing in the Cupboard · Data · S6

- **Purpose:** the deduction airs. If the thing was wrong, there was a real thing, and nobody has found one.
- **Belief:** the cupboard held Violet's proof and the letter explains it → the cage letter's claims laid out against everything the room verified, with Margo's line pinned across them → *the letter and the thing do not have to be one testimony. If the thing was wrong there was a real thing, and it is not in the cage, not in the boxes, not in the estate. Something is missing from the world.* Two signwriters write in about the panel in response (one, on D3's paraphrase branch). The board work assembles facts the player already holds into a new reading; Part E names this lead among the three weakest for exactly that reason, and keeps it because the reading is Ally's Ep 6 and the player must build it with her rather than be told it.
- **Sources:** rows 59 (aired anonymised, with consent), 56 (the signwriters' letters).
- **Requirement:** Hard · 2× A-T5 + 2× F-T5 + 3× F-T4 · 88 T1eq. Reward draft: 380 CC.
- **Gates/spawns:** requires `fk_tessa`. **Ep 6 airs in the gap.** Spawns `fk_ithink`.
- **Redaction line:** Fable may be told: the deduction exactly as aired; that two signwriters wrote in describing a gilded panel. Never: that the deduction is correct, or what the real thing is.

### 13 · `fk_ithink` · I Think Against I Know · Data · wall · S7

- **Purpose:** the letter stops being Violet's. The episode's intellectual climax, one week before its physical one.
- **Belief:** both letters are Violet's; one frightened voice → the two letters side by side on the board, dated by the slip: the first says *I think, and I may be wrong*, lodged three weeks before she died; the second says *I know*; the claims of the second are kind to exactly one man; and a listener's question lands: what did Brad's handwriting look like? Ruby cannot answer. Nobody can. → *the second letter is probably not hers, it is kind to the man who is dead, and nobody alive ever saw that man write cursive.* The player arrives where Margo arrived, six weeks after her, by the public route. A Gerald beat is proposed inside this lead's dialogue (Discuss register, canon-first; not a separate lead).
- **Sources:** rows 55, 62 (the slip dates the lodging), R3a (nobody saw Brad write; aired Day 311), 30 (the cage letter's text as the survivors hold it).
- **Requirement:** Very Hard wall · 2× A-T6 + 3× F-T5 + 2× F-T4 · 128 T1eq. Reward draft: 560 CC + 20 energy.
- **Gates/spawns:** requires `fk_wrong_thing`. **Ep 7 airs in the gap.** Spawns `fk_skipper` and `fk_platform` (parallel pair).
- **Redaction line:** Fable may be told: everything aired on Day 311, which necessarily includes "the second letter may not be Violet's and its kindness points at Brad". This is the widest brief before the close and it is licensed because Ally airs it; Fable still holds nothing of the swap's mechanics, the panel's meaning, or that Brad is alive. Never: those three things.

### 14 · `fk_skipper` · Three Years Before · Evidence · S8

- **Purpose:** Brad acquires a shadow. A letter from a man who was never asked a question in his life.
- **Belief:** Brad's story starts at Violet's unit seven years ago → the skipper's letter pinned and the harbour years mapped: a deckhand who lived aboard, drank at Brad the signwriter's, missed a sailing three years before Violet died, and was never found → *someone vanished out of Brad's life before any of this began, and nobody ever connected the two until a podcast said his name often enough.* To Del, never aired (spine Day 311; Del revives the file Day 312, off-page). The thinnest full lead in the set and named in Part E; it stays because the close's identification is unbuyable without it and the player must hold it as an unexplained weight, exactly as Ally does. **Season notch candidate sits here (Part F, question 1).**
- **Sources:** rows 10c, R6a (the skipper writes via the show, Day 311; to Del, never aired).
- **Requirement:** Standard · 2× R-T5 + 1× R-T4 · 40 T1eq. Reward draft: 170 CC.
- **Gates/spawns:** spawned by 13, parallel with 15.
- **Redaction line:** Fable may be told: the skipper's letter verbatim; that it goes to Del and is never aired; that Ally cannot see what it means and files it. Never: what it means.

### 15 · `fk_platform` · The First Train · Evidence · S8

- **Purpose:** alive becomes thinkable. The platform woman's letter: the Saturday the car was found, the first train, wet boots, a cash single.
- **Belief:** Brad went into the water on the Friday night → the 05:47 reconstructed on the board: the clerk's complaint about the change, the boots, the timetable against the search timeline → *someone walked out of Kestrel Head the morning after Brad's car was found. Ally does not say the sentence. She asks the train to speak: if you were on that train, write to me.* To Del, not aired; the ask is aired (spine row 60). The player may now believe anything; the game confirms nothing, per the ruled reveal order.
- **Sources:** row 60 (the platform woman, Day 318; to Del; the on-air ask).
- **Requirement:** Hard · 2× R-T6 + 2× A-T4 + 2× F-T4 · 96 T1eq. Reward draft: 380 CC.
- **Gates/spawns:** spawned by 13, parallel with 14. **Ep 8 airs in the gap** once 14 and 15 are both activated. 16 gates on both.
- **Redaction line:** Fable may be told: the letter, the boots, the cash single, the on-air ask. Never: who was on the train.

### 16 · `fk_one_of_a_kind` · One of a Kind · Podcast · climax · S9

- **Purpose:** the climax build. Four descriptions become one object, and describing it out loud is a decision with a cost.
- **Belief:** the panel is a rumour with no owner → the climax assembly: the café's memory, the cinema owner's, the two signwriters', cross-checked detail by detail into one describable thing: black ground, twenty-six gold capitals and an ampersand, a crack across one corner, a small mark bottom right that was not her name and not his → *the thing is real, unique, and about to be spoken into every kitchen in the country, including one Ally cannot know about. Airing it is the only way to find it, and airing it warns whoever holds it.* Ep 9 airs it. That night, off-page, it burns.
- **Sources:** row 56 (the four describers; aired Ep 9, Day 325).
- **Requirement:** Very Hard climax wall · 3× R-T6 + 2× F-T5 + 2× A-T5 · 160 T1eq. Reward draft: 560 CC + **2 Platinum Ingots**. Board-space guardrail: three concurrent T6 builds plus two T5s is the heaviest ask in the episode, same class as The Listener's L11-B; playtest before lock, fallback shape 2× R-T6 + 2× R-T5 + 2× F-T5 (144).
- **Gates/spawns:** requires `fk_skipper` + `fk_platform`. **Ep 9 airs in the gap.** Spawns `fk_close`.
- **Decision:** **D4, the mark** (Part C).
- **Redaction line:** Fable may be told: the full description as assembled; Del's request to hold one detail back; that Ally airs the description knowing whoever has it will hear. Never: whose mark it is, what the panel proves, who hears it.

### 17 · `fk_close` · The Man on the Wall · Podcast · close · S10

- **Purpose:** the accounting. The letter from the landlady arrives in the gap; the player holds the answer through a deliberately quiet week; the arrest lands; Ally tells Havenbay he is alive.
- **Belief:** the description is out and there is nothing to do but wait → ceremonial requirement (one item: the studio, one more time); the content is narrative: her letter (the wall, the gap where the panel hung, an address, never aired, never named), Del at 07:00, six days of silence, Ep 10 indistinguishable from Ep 9 by design, and then the news: arrested four hundred miles away, under a name that sat for three years in a missing-person file in Del's own precinct. *Brad Collins, alive.* → *he was alive the whole time; the reading was his; Margo was right and it killed her; and the two survivors were afraid of the only two people in the room who were innocent.* The close airs the accounting: what is public, plus everything Ally held that can now be said, minus the landlady, who is never named, even now. Ending shape and its named cost are Stephen's ruling (Part F, question 2).
- **Sources:** rows 63 (the landlady's message, held), 54-class public record of the arrest (Day 333); everything previously held becomes airable only where the source consented.
- **Requirement:** ceremonial · 1× A-T2 · 2 T1eq (the separate-axes rule, §7.2: Very Hard rewards over a ceremonial requirement). Reward draft: **500 CC + 20 energy + 3 Platinum Ingots**.
- **Gates/spawns:** requires `fk_one_of_a_kind`. Sets `fk.ep01.complete` (mirrors `e1.ep01.complete`). Spawns `cold_case_a` + `ep2_teaser` (the shipped tail assets, reused; story-neutral, and the standing content-folder note applies).
- **Redaction line:** the last brief, and the only one that ends the blindness: Fable may be told the ending as the public knows it (alive, the arrest, the identity offences, the revived file), plus Ally's held material with its consent boundaries marked. This brief is written last and only after every other lead's prose is locked.

---

# C. THE DECISIONS

Four decisions, placed S2 / S4 / S6 / S9. None moves the spine's clock: the same man is arrested on the same day in every reachable state, because the spine survived three adversarial rounds by being inevitable and this document does not reopen it. What the decisions change is who is protected, exposed and believed along the way, and what the public record ends up holding, which is the axis the consequence-and-agency gate names ("differ materially in who is protected, exposed, believed, harmed"). The honest caveat: the gate's strongest reading, decisions that change the ending, is deliberately unavailable here. That trade is structural, not accidental, and it goes to the attack (Part E and the attack prompt).

Implementation: each decision is the shipped pattern (two buttons in dialogue, one flag pair, variant nodes at later leads; no BranchOutcomes UI needed, no flag ever gates a mainline lead, so no branch can starve the chain). Flags: `aq.fk.d1.aired` / `aq.fk.d1.held`, `aq.fk.d2.direct` / `aq.fk.d2.market`, `aq.fk.d3.verbatim` / `aq.fk.d3.paraphrase`, `aq.fk.d4.mark_aired` / `aq.fk.d4.mark_held`.

### D1 · The letter on air · lead 4, Day ~272 · Class B

Ruby wants her key letter read whole on the show: Violet's words in Violet's order, including *do not go to the police, and here is why*.

- **A (spine's account, Ep 2 "the letters read out"): read it whole.** The city hears that four friends sat on a murder warning for months. Cost, named and irreversible: Ruby and Liam are savaged for the holding; the doorstepping hardens from S3; variant lines through S3 and S4 carry it, and one close-lead line accounts for it. Gain: Violet speaks for herself, and every later broadcast stands on her exact words.
- **B: air only what Ally can verify** (the arrival, the solicitor's slip, the outline) and hold the text. Cost, named and perceivable by S6: a partial version of the letter leaks anyway (Ruby has read it aloud to others before Ally existed), the city's version is garbled and crueller, and Ep 7 must spend its opening correcting the record instead of advancing it; Ruby's variant lines carry the exposure to a worse version of her own evidence. Gain: the survivors are not the ones who put the warning in the city's mouth.
- **Fairness:** both defensible at the moment of choice (let Violet speak vs shield the living); each option's risk is stated by Ruby and Ally respectively before the buttons; the later leak is signalled (Ruby says other people have heard her read it). Rejoin: immediate, shared topology; variant lines only. Clock check: Ep 7 airs the full text on Day 311 in both branches; no B6 day moves.

### D2 · The Wednesday tip · lead 9, Day 290 · Class A

The tip names Brad at the cage on the Wednesday. It goes to Del the same day and never airs in both branches: that is Ally's protocol and it is canon, not the player's to spend. The choice is how it travels.

- **A (spine's account): hand it to Del raw, stand back.** Del takes the fishmonger's first telling as a clean statement (spine Day 292). Cost: Ally never holds the texture; the evidence board's market cluster stays thin; the player's private certainty rests on one relayed sentence.
- **B: go to Bay 3 first, take his account on tape, then hand both over.** Gain: the corridor door, the early-for-you line, the market morning on the board in detail. Cost, named: the market learns the podcast is asking about the cage; within the week the city knows there is a cage question; Liam, who has just bought his quiet by talking, is doorstepped with it and goes silent toward the show until Day 311 (his contribution drops out of the S5 material; the board marks him gone quiet; Del's single friction line notes the first telling is now on Ally's tape, not a statement).
- **Fairness:** both defensible (evidence hygiene vs witness texture); risks stated by Del's standing rule and by the fishmonger's own "I'll tell you what I told the machine". Rejoin: by lead 13. Clock check: the tip reaches Del Day 290 in both branches; the fishmonger's statement lands Day 292 in both; the survivors learn nothing of its content in either; no B6 day moves. Class A: the differences ride on shared assets (board annotations, card subtitle, access state, one flat line), no exclusive scene.

### D3 · Margo's words · lead 11, Days 300 to 304 · Class B

Tessa consents to the words going on air, anonymised, and is frightened of the phrasing itself: eleven years of secrecy has trained her to believe a sentence can be an address.

- **A (spine's account): air the words verbatim, anonymised.** Gain: the exact words do the work; two signwriters connect the gold panel to "the thing in the cupboard"; Margo enters the record as someone who doubted, not someone who drowned in guilt. Cost, named: the channel is described on air (an app that deletes everything, a woman who was nobody's public anything), and Tessa spends the rest of the episode knowing the description exists; her variant lines carry it. The spine holds that no danger reaches her; the fear is real and is the cost.
- **B: air the substance, paraphrased; shield the phrasing and the channel.** Cost, named and perceivable at S9 and the close: the deduction lands softer; one signwriter writes in instead of two; the Ep 9 description assembles from three sources and drops the crack across the corner; at the close, Tessa's variant line lands: *you made her sound like a rumour.* Margo's own words never enter the record. Gain: the channel is never described to the city.
- **Fairness:** both defensible (the record vs the witness); Tessa states her fear, Ally states what verbatim words historically do (strangers answer). Rejoin: immediate; the S9 evidence-quality difference is the perceivable tail. Clock check: Ep 6 airs Day 304 in both; the panel description airs Day 325 in both; the burn and the letter (Days 325 to 326) hold in both, because a near-match is a risk the man cannot take; no B6 day moves.

### D4 · The mark · lead 16, Day 325 · Class A

Del asks Ally to keep one detail off air so that claims can be tested: the small mark bottom right that was not Violet's name and not Brad's. Del does not say why she is asking (she has held a private hypothesis since Day 292 and it never leaves her, per C7).

- **A (spine's account): air everything, including the mark.** Gain: maximum recognition reach (Ally cannot know whether the person who matters saw it daily or once), and the public record carries the strangest fact: somebody signed this thing as somebody else. Cost, named: the description is unfalsifiable in the wild; claims flood the week; Del sifts forty letters; the two signwriters who described it publicly are doorstepped, and the cinema owner shuts his booth to the press (board annotation and one close line).
- **B: hold the mark back at Del's request.** Gain: the one letter that supplies the unaired detail is instantly credible; the close's accounting shows a clean identification instead of a sift. Cost, named: the broadcast record never carries the mark; the city's doubt runs the final week (Ruby's variant: nobody I know ever saw this thing); and the audience, in-world and at the handset, never hears the pre-echo.
- **Fairness:** genuinely balanced; Del argues B, the season's own protocol history argues A. Rejoin: within the close. Clock check: the burn happens on the night of Day 325 in both branches (a partial description is still a description of the only such object in the world); the landlady looks on Day 326 in both (black ground and gold capitals is her wall either way); Del moves at 07:00 on Day 327 in both; arrest Day 333 in both. Class A: board and risk-state differences through shared assets plus close-lead variant nodes.

**Gate arithmetic for the set:** four decisions (minimum met) · Classes A, B, B, A (two Class A met; zero Class C, under the cap of one) · every branch rejoins in at most three leads (D1 immediate, D2 by lead 13, D3 by the close via S9's shared assembly, D4 within the close) · no nested forks (one live decision at a time; the pairs never overlap because each is resolved inside its own lead's dialogue) · no single end-of-episode branch (S2, S4, S6, S9) · no flag gates any mainline lead, so no reachable state starves the chain: verified against the Part E gate table. Material loss on a named person, per branch: Ruby (D1-A), the survivors' record (D1-B), Liam's quiet (D2-B), evidence texture (D2-A), Tessa's fear (D3-A), Margo's voice and Tessa's trust (D3-B), the sources' privacy (D4-A), the public record and the pre-echo (D4-B). Every loss is surfaced in a scene or on the board before the episode ends; none is design-document-only.

---

# D. SESSION 1, IN SECONDS

The cold open v0.4 is ~470 words, ~3:20 at 140 wpm. Ruled 2026-08-31: spread over multiple FTUE leads at a cost of about one extra lead requirement. The cut below is the proposal for ruling (Part F, questions 3 and 4). Every word is v0.4 as adopted; nothing is rewritten.

**The cut:**

- **Segment 1 → lead 1 (`fk_frontpage`).** From *"This is Echoes of Havenbay"* through *"…four people received something in the post."* ≈ 90 words ≈ **38 to 39 seconds** at 140 wpm. The segment ends on the hook line so the first tap lands with the keys already in the air.
- **Segment 2 → lead 2 (`fk_fourkeys`).** From *"Each of them got an identical key…"* through *"…neither one will say what that object was."* ≈ 160 words ≈ 68 seconds, played across the lead (opening nodes and completion nodes; the deterministic merge sits between them, §5.7's choreographed pattern).
- **Segment 3 → lead 3 (`fk_sergeant`).** From *"Then the group began to disappear."* to the ruled sign-off ≈ 220 words ≈ 95 seconds, split around the Del scene, sign-off on completion. Ep 1 airs in the gap.

**First interaction:** the seeded pair begins pulsing under segment 1's third paragraph (about 0:25); the board accepts the tap from the moment segment 1 ends. **VO before first required interaction ≈ 38 to 39 seconds**, against I6's open 35-to-40-second target: inside the band, at its top edge. The alternative cut (end segment 1 at *"…nothing more."*, ≈ 78 words ≈ 33 seconds) buys 6 seconds and costs the keys landing before play. Both numbers are flagged for Stephen rather than assumed (Part F, question 3).

**Timeline (draft):** 0:00 ident and the death line · 0:25 board visible, pair pulsing under VO · ~0:39 first tap (lead 1's requirement is one A-T2: one deterministic merge) · ~1:00 lead 1 resolution nodes · ~1:15 segment 2 opens lead 2 (the keys, the letter, the door) · first self-directed merges (1× A-T3) · ~2:30 lead 2 resolution: the object, and the two survivors who will not say · ~3:00 lead 3: Del on the steps, segment 3, the requirement (A-T4 + A-T2) teaches a two-slot card · ~5:30 sign-off, Ep 1 publishes in the gap, session 1 can end. Total session 1 VO = the whole ruled open, unchanged; total new FTUE VO written for this structure: none.

Mechanical duties (systems pass, not this document): lead 1 carries the FTUE entitlement hooks and the junk-drawer generator grant exactly as `e1_tip` does; the first R-family requirement does not appear until lead 7, so the rusty-anchor availability flag must be set by lead 6's activation at the latest; A and F flow from the lab from lead 1. To be wired and verified in the editor, not asserted here.

---

# E. THE CHECKS

### E1. Every aired fact traces to a B6 row

| Broadcast | Airs | B6 sources |
|---|---|---|
| Ep 1 (269) | The public account, Del's nothing | 54, 64 |
| Ep 2 (276) | The room, the letters read out, the boxes, the habits | 55, 41 |
| Ep 3 (283) | Debts called in; the panel carried round | 56 |
| Ep 4 (290) | Liam's side (the tip is to Del, not aired) | 58; 57 held |
| Ep 5 (297) | The ask about Margo; the build correction | 55/58, R15 |
| Ep 6 (304) | The words, anonymised, with consent; the deduction | 59, 56 |
| Ep 7 (311) | I think against I know; the slip's date; the kindness; the handwriting question | 55, 62, R3a, 30 |
| Ep 8 (318) | The on-air ask about the train (the letter itself to Del) | 60 held; the ask is hers |
| Ep 9 (325) | The description | 56 |
| Ep 10 (332) | Indistinguishable from Ep 9 | by design |
| Close (333+) | The public arrest; held material within consent; the landlady never | public record; 63 never aired |

Never aired, held with Ally, shown to the player as held: rows 57 (the tip), 60 (the letter), 61 (the map), 63 (the landlady), 10c/R6a (the skipper). Nothing reaches the player that Ally never learns; nothing is aired ahead of its A5 episode; the only facts the player holds ahead of the audience are the ones Ally holds, which is the ruled shape: the player learns with Ally.

### E2. No lead precedes its spine day

Checked lead by lead against B6 and A5: lead 5 uses row 62 (Day 276) inside S2 (270 to 276) · lead 7 uses row 61 (283 onward) at Day 282-283 · lead 9 sits on Day 290 exactly · lead 10 uses R15 (Day 297) · lead 11 sits on Day 300 · lead 13 airs R3a on Day 311 · lead 14 opens after Day 311 · lead 15 sits on Day 318 · lead 16 assembles sources dated 283 to 311 and airs Day 325 · the close consumes nothing dated before Day 326. **Pass.** The tightest joints are lead 5 (the slip arrives the same week it is used) and lead 14 (the skipper's letter is written on the day Ep 7 airs and read at the top of the next session); both are inside their windows.

### E3. Kill Gate Zero (operational reality)

The structure invents no institutional power and asks no professional to skip a step. Every route used is one the spine already computed: the Tip Line (canon), witnesses speaking about their own knowledge (lawful throughout B6), Ally's own analytics (row 61), Del receiving and acting on tips (C7). The two decisions that touch process (D2's market visit, D4's held-back detail) use powers Ally and Del actually have: asking questions and choosing what to broadcast. The why-was-it-missed answer is the spine's pre-flight #1 and is not weakened by any lead: nothing here finds anything before its day.

### E4. Consequence and agency

Four decisions, two Class A, zero Class C, all rejoining within three leads, no consequence-free branch, every cost surfaced in-scene or on-board before the end (Part C's arithmetic). Caveat, stated plainly: no decision changes the arrest or its day. The differing end states are who was exposed, protected and believed on the way, and what the record holds. Whether that satisfies the gate as ruled is folded into the attack prompt; it is the one place this structure knowingly trades the gate's strongest reading for the spine's inevitability.

### E5. The evidence-turn test, answered per lead

Answered on every card in Part B (belief before → board → belief after). **The three weakest, named honestly:**

1. **Lead 1 `fk_frontpage`.** Orientation, not a turn: the player believes nothing yet, so nothing can change. Defended as the FTUE's price and kept to one merge and forty seconds; it must never grow.
2. **Lead 10 `fk_asking`.** The board work is re-measurement (a build compared, a timeline mapped) and the belief turn arrives partly in dialogue (the listener's letter). It carries the mid-season milestone and the largest wall to that point, which risks toll-booth reading. Mitigation: the lane re-measurement is the merge deliverable itself (the player assembles the comparison that clears Liam of the lane), and the ask is the payoff, not the work.
3. **Lead 12 `fk_wrong_thing`.** A deduction lead: the merge assembles facts already held into a new reading. The belief does change (decoy enters the world), but the board contributes arrangement, not evidence. Kept because the player must build Ally's Ep 6 reasoning to own it; flagged as the first candidate for redesign if the evidence-turn play test says toll booth.

Also thin, watched but defended: lead 14 (a single letter; the turn is real but small, and the close cannot exist without it).

### E6. Gate arithmetic, written out

| Lead | RequiredLeadIds | Spawned by | Spawns |
|---|---|---|---|
| 1 | (none, initial Available) | | 2 |
| 2 | (none) | 1 | 3 |
| 3 | 2 | 2 | 4, 5 |
| 4 | (none) | 3 | |
| 5 | (none) | 3 | |
| 6 | 4 AND 5 | 4, 5 (idempotent) | |
| 7 | 4 AND 5 | 4, 5 (idempotent) | |
| 8 | 6 AND 7 | 6, 7 | |
| 9 | 8 | 6, 7 (arrives Blocked, visible) | 10 |
| 10 | 9 | 9 | 11, 12 |
| 11 | (none) | 10 | |
| 12 | 11 | 10 (arrives Blocked, visible) | 13 |
| 13 | 12 | 12 | 14, 15 |
| 14 | (none) | 13 | |
| 15 | (none) | 13 | |
| 16 | 14 AND 15 | 14, 15 | 17 |
| 17 (close) | 16 | 16 | cold_case_a, ep2_teaser |

Checks: every referenced id exists in the table (auditor's rule) · the graph is acyclic and single-rooted at lead 1 · every lead is reachable (each gate's members are themselves reachable and completable) · no gate needs more than two keys and no lock needs all keys anywhere (the ruled anti-puzzle-box rule; the widest gate is 2) · no flag gates a mainline lead, so no decision can starve the chain · maximum simultaneous Available leads is 2, within the bar's shipped comfort · `CheckAndUnlockBlockedLeads` covers the two Blocked-visible teases (9, 12) on the state-scan guarantee, and `ApplySavedStates` re-evaluates both on restore (verified in code, lines cited at the head of this document).

### E7. Bands, families and the 4-to-5-hour budget

Requirement mass (T1eq of tier n = 2^(n-1); shapes on the cards): **total 1,078 T1eq** across 16 leads + ceremonial close. By family: A 186 · F 264 · R 416 · D 212. By session: S1 16 · S2 116 · S3 128 · S4 128 · S5 128 · S6 136 · S7 128 · S8 136 · S9 160 · S10 2.

Production taps at the tuned yields (lab 1.3355, junk 1.7427, diner 1.6975 T1eq/tap; method and 1.10 overhead per `ep1-economy-rebalance.md`): (186+264)/1.3355 + 416/1.7427 + 212/1.6975 = 700.6 × 1.10 ≈ **771 taps ≈ 771 energy**, less 40 granted (leads 10, 13, close) ≈ **731 net external energy**.

Per-session energy demand (same method, per session's family mix): 13 · 91 · 83 · 86 · 81 · 105 · 105 · 92 · 113 · 2. A session opens on a full 100 tank plus ~12 regen across 30 minutes: **sessions 6, 7 and 9 exceed the tank and are the episode's three deliberate EnergyOutPopup encounters**, mirroring The Listener's climax-wall philosophy; every other session clears free. Ten sessions ≈ ten days for a light one-session player, ≈ five for an engaged two-session player: inside the ruled week-or-more.

Playtime: 771 taps plus roughly 700 merges at 3 to 4 seconds of board action each ≈ 1.6 to 2.0 hours of pure board work; board management, locker and toasts ≈ ×1.4 ≈ 2.3 to 2.8 hours; dialogue, VO, decisions and the evidence board ≈ 1.2 to 1.6 hours. **Total ≈ 3.7 to 4.5 hours: inside the ruled 4-to-5-hour band, with the assumptions written down so the tuning pass can attack them.** All bands are drafts for tuning.

Rewards (draft, scaled to hold the shipped earn rate): Easy 20 ×3 · Standard 170 ×5 · Hard 380 ×5 · Very Hard 560 ×2 · milestone 400 (lead 10) · close 500. **Total 4,830 CC ≈ 4.48 CC per T1eq** against The Listener's 4.27, so every sink (locker, Mo's Back Room at 20 CC/T1eq, Case Kit) keeps its intended price feel. Energy grants +40 total; ingots 5 total (2 climax + 3 close), matching the shipped episode. Note for the tuning pass: the §7.2 band midpoints (20/50/95/185) were sized for a 330 T1eq episode; holding them at 3.3× the mass would collapse the earn rate to 1.7 CC/T1eq and silently reprice every sink, which is why the draft scales the midpoints instead. Stephen's call.

VO budget note: unique spoken VO ≈ the 470-word open + the lead 10 milestone + Ep 9's description passage + the close ≈ 1,100 to 1,400 words, roughly 2.5× The Listener's 515. Priced here, not approved here.

### E8. Copy rules

All titles ≤ 32 characters, no puns; subtitles to be written ≤ 140 characters as case-file labels; directive lines ≤ 8 words; no em dashes anywhere in this document or in any line it proposes; every player-facing line here is a placeholder awaiting Stephen's ruling; no endearments or gendered address toward the player.

---

# F. THE QUESTIONS FOR STEPHEN

Each in the ruled form: the question · what the structure needs · what changes otherwise. Stop there.

### 1. The season notch

**The question:** §3.3 still lists The Listener as Ep 1 with the coin on Dot's cradle. What is the arc whisper of the new Ep 1, where in the sixteen does it land, and what irreversible cost does it leave Ally?

**Candidate, not canonised:** the whisper lands in lead 14. The skipper's letter carries one line Ally does not follow: the deckhand had once been paid to row a Thursday run out of the harbour, quit it, and lived aboard broke ever after. It is one sentence, unexplained, never mentioned again; the player who continues is the one it was for. This threads the season's Thursday-route machinery (the 2006 Reyes crossing) through a dead itinerant man without naming anything, but it brushes the street-layer/apex separation, and §3.4 says a finger connects to the head only by explicit season decision, which is exactly what this asks for. **The cost candidate:** Ally's description is what erased the panel. She knows by the close that the wall was cleared the day after she aired it: the one physical thing Violet's death left behind is gone because the show spoke it out loud. What the podcast touches, it changes; she cannot take that back.

**What the structure needs:** one dialogue node in lead 14 and one in the close. **What changes otherwise:** the episode ships arc-silent and fails §3.4's one-notch rule at review.

### 2. How it ends and what it costs

**The question:** the spine stops at Day 333. What is the close's shape?

**Candidate, not canonised:** the close airs after the arrest is public. Its beats: he is alive, and what that un-writes (the grief, the reading, the two survivors' war); Margo read it right first and paid first, said so once, and the record now holds her words; the landlady is never named, even now, and the close says only that someone wrote, which is itself the season's thesis about strangers; and the honest limit: the arrest is for the identity offences and a revived file, and whether Violet's staircase is ever charged is not Ally's to promise. Truth delivered where justice is only partial, per the accountable-resolution element. Final image candidate: Liam and Ruby, who have not spoken since Day 96, standing in the same harbour crowd when the news breaks, with nothing left between them but the space where the accusation was.

**What the structure needs:** the close card's beats confirmed so its dialogue and VO can be briefed. **What changes otherwise:** the close lead and the final Fable brief cannot be written, and S10 ships as a stub.

### 3. The first-interaction budget

**The question:** I6's 35-to-40-second first-interaction target is an open ruling; the cold open as cut puts ~38 to 39 seconds of VO before the first required tap (segment 1 ending on the keys hook), or ~33 seconds on the shorter alternative (ending at "nothing more").

**What the structure needs:** one number ruled. **What changes otherwise:** Part D's timeline and the segment-1 boundary stay provisional and the FTUE build cannot lock.

### 4. The cold-open cut

**The question:** which stretch of v0.4 lands in which FTUE lead. The proposed cut: segment 1 ends on *"…four people received something in the post."* (lead 1); segment 2 ends on *"…neither one will say what that object was."* (lead 2); segment 3 runs to the sign-off (lead 3).

**What the structure needs:** the two boundary rulings (every Ally line is Stephen-ruled, and a cut is an edit). **What changes otherwise:** the FTUE leads' dialogue assets cannot be assembled and session 1 cannot be built.

---

# APPENDIX: SYSTEMS NOTES (design-to, touch no code)

- Slot **ep01** (story-neutral, R5); this structure becomes ep01's `LeadsDatabase` content when Four Keys ships as Ep 1. Alias table handles `e1_the_listener` saves per the multi-episode build; nothing here migrates anything.
- Lead ids `fk_*`, assets `Lead_FK_*`, folder `Assets/Content/FourKeys/`, dialogue `Resolve_FK_*`. The database decides membership.
- Close sets `fk.ep01.complete` and spawns the shipped tail (`cold_case_a`, `ep2_teaser`); R6 no-replay applies to every lead.
- Decision flags namespaced `aq.fk.d1..d4`; set on Proceed in dialogue; never gate a mainline lead.
- FTUE mechanical duties (entitlements, generator grants, family availability flags) mirror the shipped `e1_tip` pattern and are wired in a systems pass with editor verification, not asserted from this document.
- The board-space guardrail on lead 16 and the quantity cap [Range(1,3)] were checked against every requirement shape in Part B: no slot exceeds quantity 3, no card exceeds 3 slots.
