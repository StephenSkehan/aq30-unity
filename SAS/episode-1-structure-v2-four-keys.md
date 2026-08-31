<!-- pdf-title: The Friends with Four Keys, Structure v2.0 -->

# THE FRIENDS WITH FOUR KEYS
## THE STRUCTURE, v2.0: CHAPTERS AND LEAD PACKAGES

*2026-08-31. Built on the ruled package economy model v1.1 (100 packages, 10 chapters, 1,600 T1eq, CC ≈6,880 at 4.3, no optionals, variety mandated), spine v1.4 (three attack rounds, sweep PASSES on Margo's named lock), lead structure v1.0 (superseded; its 16 turn leads, four decisions, broadcast map, gate logic, redaction lines and Part F questions are salvaged here), cold open v0.4, the five v1.2, and the Four Keys rulings in memory. No prose. Every player-facing line is a placeholder and says so. The spine is not reopened; no beat in this document moves a spine fact, and none needed to. The v1.1 economy rulings are not reopened.*

**Code checked before trusting any spec (2026-08-31, this session):** `LeadData` (leadId, RequiredLeadIds AND-gate, SpawnLeadIds, requiresFlag/forbidsFlag, banded rewards, generator and special rewards, boardPhase; requirements header says "max 3 recommended", not enforced) · `LeadRequirement.quantity` is `[Range(1,3)]`: no card asks more than 3 of an item · `LeadsRepository` (CheckAndUnlockBlockedLeads state-scan; flag gates re-checked in the same scan per the robustness rules) · `audio_investigation` T1 to T6 assets live in `Assets/ScriptableObjects/Items/` · **no package container exists anywhere in code** (no PackageData, no grouping id): Part G names every assumption this design makes about it, and the costing doc prices exactly those.

**Governing rules, applied to every package in Part B:**

1. **Every chapter turns belief at least once.** The 16 turn leads of v1.0 carry it; each package's beat is marked ★TURN or (t) texture. Unmarked texture does not exist in this document.
2. **Character-fact beats reveal previously unknown facts about the five and never touch the graph.** The five v1.2 is mined first; invented facts and invented routes are flagged NEW and batched in Part F5 for ruling.
3. **No beat surfaces a fact before its B6 day, and nothing reaches the player that Ally never learns.** Checked per package; the tight joints are named in the chapter notes.
4. **The surprise is "he is alive" and it lands where A5 lands it** (the landlady's letter opens the door on Day 326; the arrest closes it on Day 333).
5. **Art-with-caption payoffs are counted, not rationed** (Stephen: budgets later). The load-bearing ones are listed in Part F6 for the art bill.

**Id decision.** Packages are `fk_p<chapter 01..10>_<n 01..10>`, e.g. `fk_p04_09`. Cards inside a package are `fk_p04_09a`, `b`, `c` in bar order; assets `Lead_FK_P04_09a`; folder `Assets/Content/FourKeys/`; dialogue `Resolve_FK_*`. Why: `fk` is story-scoped and survives slot renumbering (R5, as v1.0 ruled); zero-padded chapter and index sort correctly in the editor and in `LeadsDatabase` tooling with ten chapters and ten packages; the card letter keeps the card id derivable from its package by eye. The database decides membership, never the folder (standing rule). Slot **ep01**; close sets `fk.ep01.complete`.

---

# A. THE CHAPTER MAP

A chapter is roughly one sitting (the arithmetic in Part E makes it so; nothing enforces it). Ally's episode airs at the chapter boundary where that falls naturally, which is chapters 1 to 9; Ep 10 airs inside chapter 10, because a package whose deliverable is a deliberately empty broadcast is the point of that week (v1.0's ruling, kept). Days are the spine's A5 clock.

| Ch | Title (placeholder) | Spine days | Airs at boundary | The chapter's turn(s) | Pkgs | T1eq |
|---|---|---|---|---|---|---|
| 1 | The Front Page | 266 to 269 | **Ep 1 (269)** | The keys land: accident becomes one of two readings · the police door closes honestly | 10 | 35 |
| 2 | The Kitchen Table | 270 to 276 | **Ep 2 (276)** | The reading landed on Liam because the letter leans · the slip: her fear predates everything | 10 | 70 |
| 3 | Whose Hand | 277 to 283 | **Ep 3 (283)** | Money moved in her last months · what she found had a shape (and one far town is listening) | 10 | 105 |
| 4 | The Wednesday Visitor | 284 to 290 | **Ep 4 (290)** | The letter reads built to fit Liam · Brad was inside the arrangement before it opened | 10 | 135 |
| 5 | Did She Say Anything | 291 to 297 | **Ep 5 (297)** | The lane man was the wrong build for Liam; Margo is the missing voice | 10 | 170 |
| 6 | Two Hours Up the Coast | 298 to 304 | **Ep 6 (304)** | Margo doubted the letter's meaning · if the thing was wrong there was a real thing, and it is missing | 10 | 205 |
| 7 | I Think Against I Know | 305 to 311 | **Ep 7 (311)** | The second letter is probably not hers, and nobody alive ever saw Brad write | 10 | 215 |
| 8 | The First Train | 311 to 318 | **Ep 8 (318)** | A man vanished out of Brad's life three years early · someone walked out of Kestrel Head at dawn | 10 | 220 |
| 9 | One of a Kind | 319 to 325 | **Ep 9 (325)** | The object is real, unique, and about to be spoken into every kitchen in the country | 10 | 220 |
| 10 | The Man on the Wall | 326 to 333 | *(Ep 10, Day 332, airs inside the chapter)* | The landlady's letter · Brad Collins, alive | 10 | 225 |

**How v1.0's 16 turn leads map in** (the skeleton, preserved): fk_frontpage → p01_01 (orientation, still not a turn) · fk_fourkeys → p01_04 · fk_sergeant → p01_09 · fk_kitchen → p02_03 · fk_boxes → p02_06 + p02_08 (the slip carries the turn) · fk_calling_in → p03_02 · fk_gold_panel → p03_06 + p03_09 (the map) · fk_liam → p04_06 · fk_bay3 → p04_09 · fk_asking → p05_06 + p05_08 · fk_tessa → p06_03 + p06_06 · fk_wrong_thing → p06_08 · fk_ithink → p07_03/04/05 building to p07_07 · fk_skipper → p08_01 · fk_platform → p08_05 · fk_one_of_a_kind → p09_08 · fk_close → p10_01 + p10_07 + p10_10.

**Evidence board phases** (`boardPhase`): Phase 1 = chapters 1 to 3 (the public case) · Phase 2 = chapters 4 to 7 (the doubt) · Phase 3 = chapters 8 to 10 (the man).

**Broadcast map** (unchanged from v1.0): Eps 1 to 9 air at the ends of chapters 1 to 9 on Days 269, 276, 283, 290, 297, 304, 311, 318, 325; Ep 10 (Day 332) airs inside chapter 10 and is indistinguishable from Ep 9 by design; the arrest is Day 333.

**Locations** (new builds flagged, carried from v1.0): Ally's studio (exists), precinct steps (exists), Ruby's kitchen ⚠, harbour café ⚠, Regent cinema frontage ⚠, boatyard ⚠, fish market corridor ⚠, Mariner's Row lane ⚠, Tessa's kitchen ⚠, harbour master's office ⚠, inland halt platform ⚠. Kestrel Head stays off the art bill: photographs and narration only, never playable.

---

# B. THE BEAT AND PACKAGE TABLES

Format per row: **#** (package id is `fk_p<ch>_<n>`) · **Cards** (family letter + tier, ×q for quantity; A = Audio Investigation, lab, T1 to T6 · F = Forensic Tools, lab, T1 to T5 · D = Kestrel Corner Diner food, T1 to T12 · R = Rusty Anchor, junk drawer, T1 to T10; T1eq of tier n = 2^(n−1) per item) · **T1eq** package total · **CC** on package completion (cards pay 0; Part G assumption 4) · **Beat**: ★TURN or (t) texture, payoff class in brackets (ET evidence turn · CF character fact · AL Ally line · AC art with caption), then the beat in one line · **Source** (B6 row numbers from spine v1.4; "v1.2" = the five; NEW = Part F5 batch) · **Fable** (the redaction delta: the standing base is spine Part G plus all earlier packages' briefs; each line states only what is added, and what is never told).

Diegetic family reads (v1.0, kept): A = the tape and the show (tip line, letters read aloud, the broadcast itself) · F = documents and scene work · R = the harbour (yard, slip, market, the pub that changes its name) · D = door-knocking (cafés, kitchens, a witness's coffee).

Gating: within a chapter, packages chain (each requires the previous) except the parallel pairs named under each table; the last package of a chapter spawns the first of the next. Maximum simultaneous Available packages is 2, the bar's shipped comfort. Decisions (Part C) resolve inside the named package's completion dialogue; no flag ever gates a mainline package.

## Chapter 1 · The Front Page · Days 266 to 269 · 10 pkgs · 10 cards · 35 T1eq · 150 CC

FTUE. All single-card packages, lab families only (A and F flow from the first generator grant). The cold open v0.4 is spread across packages 1 to 10; the cut is Part D and its boundaries are Stephen's (F4). First deterministic merge choreographed per §5.7.

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | A1 | 1 | 10 | (t)[AL] Ident and the death line: a signwriter, her stairs, nine months ago. Orientation, not a turn; it must never grow | 54 | Nothing new; VO is v0.4 as ruled |
| 02 | A2 | 2 | 10 | (t)[AC] Her work everywhere: shopfronts, boats, the bus, pinned as a town-wide map of her hand | 54, v1.2 | Violet's public work only; never the panel |
| 03 | F1×2 | 2 | 10 | (t)[ET] The ruling pinned: accident, a fall, nothing more | 54 | Nothing new |
| 04 | A2 | 2 | 15 | ★TURN [ET] Four keys and a letter in her handwriting: the dead woman predicted her death. Accident is now one of two readings | 54 | Nothing new beyond Part G |
| 05 | F2×2 | 4 | 15 | (t)[AC] The wake photograph: four names become four faces | 54 | The photo as public record; never which face matters |
| 06 | A2×2 | 4 | 15 | (t)[ET] Not the police, and a locked door: the pact, the Saturday, nobody alone | 54 | Nothing new |
| 07 | F2×2 | 4 | 15 | (t)[ET] The second letter said she knew; a thing of hers beside it that two survivors will not name | 54 | Never what the object was |
| 08 | A3 | 4 | 15 | (t)[ET] Then the group began to disappear: Brad's car, Margo's car, the same car park | 54 | Nothing new |
| 09 | A4 | 8 | 25 | ★TURN [ET] Del on the steps: we looked, properly, there's nothing. The file is closed to everyone; whatever is wrong here is invisible to procedure. Only people hold it | 64 | Del says only what Part G gives her; never anything the file holds |
| 10 | A3 | 4 | 20 | (t)[AL] Ep 1 publishes: sign-off as ruled; the show is live and the city is listening | 54 | Nothing new |

Chain is strict (FTUE). Chapter notes: mechanical duties mirror `e1_tip` (entitlements, lab grant at p01, junk-drawer generator granted via p10's overflow, diner granted at p10 for chapter 2; `aq.loc.rusty_anchor.active` set by chapter 2 open). Two turns; the rest is the open doing its work.

## Chapter 2 · The Kitchen Table · Days 270 to 276 · 10 pkgs · 15 cards · 70 T1eq · 300 CC

Ruby. The room, the letters, the boxes, and the first hard date. Decision D1 in p03. The chapter's deliberate spike is p06 (the model's ruled early spike).

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | D2 | 2 | 15 | (t)[AL] Ruby's kitchen: the table the letters were worked at, the heater ritual Tuesdays held for years | 55, v1.2 | Ruby's kitchen and manner; never what the room concluded rightly or wrongly |
| 02 | F2, A2 | 4 | 20 | (t)[AC] The key letter photographed: Violet's handwriting, her exact words, as an object on the board | 55 | The letters' full text as the survivors hold them; never that any of it is forged, or by whom |
| 03 | A3, D3 | 8 | 45 | ★TURN [ET] The room reconstructed meeting by meeting: the reading landed on Liam because the letter's own lines lean that way, and the one man who argued his side lost every time. **D1 resolves here** | 55 | Ruby's account of the meetings; the claims (yard, tin, ladder, board); never whether any claim is true |
| 04 | D2×3 | 6 | 20 | (t)[CF] Frightened, Ruby talks: fills the room, allocates jobs, ends sentences with "anyway"; and the glass she holds is tonic, six years now | v1.2 | The fact as hers to state plainly; never link her sobriety to any case fact |
| 05 | A3 | 4 | 20 | (t)[CF] The four in the morning call, three years ago: Violet rang, Ruby drove over, and neither ever said what it was | v1.2 | The call happened and stayed private; it is never explained, to anyone, ever |
| 06 | R4, R3, R3 | 16 | 60 | (t)[ET] The boxes: a vanished man's whole flat catalogued, and nothing in it. The spike: days of careful nothing | 41, 55 | The boxes and their nothing; never why the nothing matters |
| 07 | A4 | 8 | 25 | (t)[CF] Brad's habits, from Ruby: keys on the table moved twice, thank you like it cost him, up at five to run | 55, v1.2 | The habits verbatim (Fable wrote them); never that anyone else will recognise them |
| 08 | F4, F2 | 10 | 45 | ★TURN [ET] In Ruby's envelope, the solicitor's covering slip: the letters were lodged three weeks before she died. Her fear predates everything | 62 (D276) | The slip and its date; Ally notes it without knowing what it proves; never what it will prove |
| 09 | D4 | 8 | 30 | (t)[CF] The piano: sheet music past carols on Ruby's stand; grade eight at fourteen, and she lets the surgery think otherwise | v1.2, NEW route | The fact; never any bearing on the case (it has none) |
| 10 | A3 | 4 | 20 | (t)[AL] Ep 2 publishes: the room, the letters (per D1), the boxes, the habits; and what airing costs Ruby | 55 | What aired per branch; never the audience it reaches |

Parallel pair: p04 ∥ p05 (both from p03; p06 requires both). Tight joint named: p08 uses row 62 the week it arrives (Day 276), as v1.0 did.

## Chapter 3 · Whose Hand · Days 277 to 283 · 10 pkgs · 20 cards · 105 T1eq · 450 CC

The café, the cinema, the panel, and the download map. Money enters; what she found gets a shape.

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | D2×2 | 4 | 20 | (t)[AC] The harbour café at opening: her menu boards still out front; you eat under her lettering all over this town | 56, v1.2, NEW route | The boards as public texture; nothing else |
| 02 | D4, A3 | 12 | 55 | ★TURN [ET] The café owner on record: she was calling her money in, for the lawyer, in her last months. Money moved; suspicion gets a category, not a face | 56 | She was calling money in; never who owed most, or why it matters |
| 03 | D3, F2 | 6 | 30 | (t)[ET] What the money was for: the sister, the house, a solicitor on Civic Row; a fight she meant to win | 56 | The dispute in public outline; never where the tin's cash came from |
| 04 | F3×2 | 8 | 20 | (t)[AC] The Regent marquee: three of six letters back, the ladder still chained under them, nothing finished. That is how you know she was there | v1.2 | The unfinished job as image; never the ladder's history |
| 05 | D3, D2, A2 | 8 | 35 | (t)[CF] The colour of the R: the cinema owner's running argument with her; she ranked people's handwriting out loud and told the truth about your kitchen | v1.2, 56 | Her expertise and her manner; never that her judgement of a hand was ever wrong |
| 06 | A4, D4 | 16 | 70 | ★TURN [ET] The panel: a month of whose-hand sightings plotted; she carried a gold thing round town asking a question nobody could answer. Whoever hid it heard her asking | 56 | The whose-hand month; the panel only as witnesses loosely describe it; never what it is or whose hand |
| 07 | R4, D3 | 12 | 50 | (t)[ET] Two signwriters up the coast, first pass: gold letters on a dark board, a hand neither could place | 56 | Their loose descriptions; never the mark's meaning |
| 08 | R4, R2, R1 | 11 | 45 | (t)[ET] The ladder she let nobody climb threw her off it six weeks before she died: the fall on the record, the hip, the coincidence nobody called anything | 21, 55 | The fall as public-inquest fact and Ruby's memory of treating it; never the bolt |
| 09 | A5 | 16 | 75 | ★TURN(minor) [ET] Ally's own download map: one town, four hundred miles away, listening every week. Held, never aired, never explained | 61 (D283) | The map shows one persistent far town and Ally files it as odd; never what the town means |
| 10 | A4, F2, F2 | 12 | 50 | (t)[AL] Ep 3 publishes: the money and the panel go on air | 56 | What aired; nothing else |

Parallel pair: p04 ∥ p05 (both from p03; p06 requires both). p09 sits on Day 283 exactly, the map's first week.

## Chapter 4 · The Wednesday Visitor · Days 284 to 290 · 10 pkgs · 21 cards · 135 T1eq · 580 CC

Liam talks; the floor moves. Decision D2 in p09. The R family carries the yard and the market.

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | R3, D2×2 | 8 | 35 | (t)[AC] The boatyard: the crane, the slings, boats out of water; the place Liam is called staff and is not | 58 | The yard as place; never the third yet |
| 02 | R4 | 8 | 35 | (t)[CF] A yard hand mentions it: Liam sings, real tenor, only in the crane cab with the door shut. Nobody else has ever heard it | v1.2, NEW route | The fact; never any bearing on the case (it has none) |
| 03 | A4, D3 | 12 | 50 | (t)[ET] Liam talks: thirty-five years from the school bus, the third confirmed and owned, and why he let the town think he was staff | 58, 32 | Liam's side as he gives it, including the confirmed third; never whether the letter is right |
| 04 | A3, F3 | 8 | 35 | (t)[ET] The tin claim tested: I never gave her a penny. Undisprovable either way, and he knows it | 58 | His denial and its unfalsifiability; nothing else |
| 05 | R4, A3 | 12 | 50 | (t)[ET] The ladder claim tested: she told everyone which ladder. True, and useless | 58 | His answer; nothing else |
| 06 | A5, R4 | 24 | 100 | ★TURN [ET] The claims laid side by side: every one is true but innocent, or unfalsifiable. The letter reads like a case built to fit him. The word "built" enters the player's vocabulary, unaired | 58 | The laid-out spread as Ally assembles it; never whether "built" is correct |
| 07 | D4 | 8 | 35 | (t)[CF] August 2014, Violet's kitchen, a bottle open, one question about her mother's house, and a laugh two people have spent twelve years hearing differently | v1.2, NEW route | The scene as Liam flatly gives his half; never whose side Violet was on (nobody knows) |
| 08 | R4, D3, F2 | 14 | 60 | (t)[AC] The market at 05:40: Bay 3, the cages, the corridor door round the side where no camera looks, anyone with a key in market hours | 57 setting, 27 | The geography as Ally lawfully observes it; never who used it |
| 09 | R5, F4, A3 | 28 | 120 | ★TURN [ET] The fishmonger's tip: Brad was at the cage on the Wednesday, three days before the Saturday. The dead man went early. To Del the same day, never aired. **D2 resolves here** | 57 (D290) | The tip verbatim; that it goes to Del within the day and is never aired; Ally's private vertigo, unexplained; never what the visit was for, or that anything was swapped |
| 10 | A4, D3, D1 | 13 | 60 | (t)[AL] Ep 4 publishes: Liam's side airs; the tip is not in it; Ally sits with what she is holding | 58; 57 held | What aired and what is held; never the held thing's meaning |

Parallel pair: p04 ∥ p05 (both from p03; p06 requires both). p09 sits on Day 290 exactly.

## Chapter 5 · Did She Say Anything · Days 291 to 297 · 10 pkgs · 24 cards · 170 T1eq · 730 CC

Margo becomes the missing voice. The mid-episode milestone closes the chapter (+20 energy).

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | D3, R3 | 8 | 35 | (t)[AC] Margo's office, shut: accounts for half the harbour, a desk squared to the millimetre, and nobody who can say what she was like at it | 55, 58, NEW route | The office as image; never the file inside it |
| 02 | R4, A3 | 12 | 50 | (t)[CF] A listener who swam with her for the county at fifteen writes in: the woman the town watched stand on the slip with a flask could outswim all three of them, and never corrected anyone | v1.2, NEW route | The fact and the letter; never why she stood and watched |
| 03 | D4 | 8 | 35 | (t)[CF] How Margo and Violet met: the undercharged door, the posted cheque, the note that said don't be a martyr. Fourteen years from that | v1.2 via 55/58 | The story as the survivors tell it; nothing else |
| 04 | F3, D3 | 8 | 35 | (t)[CF] The receipts bag: left every Tuesday for sorting, collected untouched every Friday, sorted by Margo herself. It had been a joke once | v1.2 via 55/58 | The ritual; never the accounts it fed |
| 05 | D4, A4, R3×2 | 24 | 100 | (t)[ET] Her last months mapped: the withdrawal from Ruby, the sharpened edges, and Sundays that were spoken for, nobody knew where. A silence with a shape | 55, 58 | The withdrawal as the survivors describe it; never who contacted her, never where the Sundays went |
| 06 | F4, A5, R4 | 32 | 140 | ★TURN [ET] A listener's letter re-measures the lane: No. 14's build set against Liam's. The man in the lane was the wrong build for Liam | R15 (D297), 18 | The build letter and the comparison; never who the lane man was |
| 07 | F4, D3, A2 | 14 | 60 | (t)[ET] The dashboard clock: ten minutes early to everything, always ten, sitting in the car until the hour. She went to that car park the way she went everywhere. Ally airs the oddity and does not resolve it | public account, v1.2 | The habit and the juxtaposition as an open question; never the meeting |
| 08 | A5, A3, D3 | 24 | 100 | (t)[AL] The ask composed: did she say anything, to anyone. Word for word once ruled; the week's broadcast is a question | A5 D297 | The ask verbatim once ruled; never who will answer |
| 09 | D4, D3 | 12 | 50 | (t)[CF] Since the funeral Margo made Ruby cry twice and apologised once: the true thing in the worst way, from the survivors who took it | v1.2 via 55 | The sharpness as texture; never what the true things were |
| 10 | A5, R4, F3 | 28 | 125 | (t)[AL] Ep 5 publishes, then three days of nothing: asking a city about a private woman gets you her dry cleaner. **Milestone: +20 energy** | A5 D297 | The silence; never that an answer is coming |

Parallel pair: p03 ∥ p04 (both from p02; p05 requires both). p06 sits on Day 297; p07 to p10 complete the same broadcast week.

## Chapter 6 · Two Hours Up the Coast · Days 298 to 304 · 10 pkgs · 23 cards · 205 T1eq · 880 CC

Tessa, the screenshot, the deduction. Decision D3 in p06.

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | A4, F3, F1 | 13 | 55 | (t)[ET] A letter with a screenshot attached arrives from a woman two hours up the coast (Day 300). The ask was answered once | 59 (D300) | The letter's existence and its writer's fear of being found; never her history yet |
| 02 | D5, D3 | 20 | 85 | (t)[AC] The drive: the coast road Margo took every second Sunday for eleven years, to a person none of the three ever heard named | 59 | The road and the ritual; never what the visits held |
| 03 | D4, A4 | 16 | 70 | (t)[CF] Tessa: eleven years, a partner nobody in Havenbay knew existed. Margo had a whole second room to her life and kept the door shut without effort | 59 | Tessa as a person (v1.2-consistent); never who else used the channel |
| 04 | F4, A3 | 12 | 50 | (t)[CF] The card in the kitchen drawer: in case, ring Tessa, a number. Margo prepared for exactly this eleven years ago; that is how Tessa learned Margo was dead | 49 via Tessa | The card as Tessa's own experience; never the police file around it |
| 05 | A4 | 8 | 35 | (t)[CF] Tessa's fear, stated: eleven years of secrecy taught her a sentence can be an address. She consents anyway | 59, carried from v1.0 D3 | Her fear in her words; never whether the fear is justified (the spine holds no danger reaches her) |
| 06 | A5, F4, F3 | 28 | 120 | ★TURN [ET] The screenshot authenticated and dated (Day 77): the thing in the cupboard is wrong. It can't mean that. Margo doubted the letter's meaning, said so once, to one person. **D3 resolves here** | 59, 42 | The line verbatim; the channel's nature; never what Margo knew or when |
| 07 | F5, A4 | 24 | 100 | (t)[ET] The deduction built: the cage letter's claims laid against everything the room ever verified, with Margo's line pinned across them | 59, 30 | The layout as Ally builds it; never that the deduction is correct |
| 08 | F5, A5, D4 | 40 | 170 | ★TURN [ET] If the thing was wrong there was a real thing, and it is not in the cage, not in the boxes, not in the estate. Something is missing from the world. The chapter's wall | 59, 56 | The deduction exactly as it will air; never what the real thing is |
| 09 | F4, A4, D3 | 20 | 85 | (t)[ET] Close reading of "that": what did Margo think the thing could not mean? The board holds the question open; nobody alive can answer it | 42, 59 | The question as a question; never its answer |
| 10 | A5, D3×2 | 24 | 110 | (t)[AL] Ep 6 publishes: the words air anonymised (per D3), the deduction airs, and the mailbox is watched | 59 | What aired per branch; never who is listening |

Parallel pair: p03 ∥ p04 (both from p02; p05 requires both). All of chapter 6 sits inside Days 300 to 304.

## Chapter 7 · I Think Against I Know · Days 305 to 311 · 10 pkgs · 27 cards · 215 T1eq · 925 CC

The intellectual climax: the letter stops being Violet's. The turn is p07; p03 to p05 build it in the open. Chapter wall at p07 (+20 energy).

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | F4, A3, D3 | 16 | 70 | (t)[ET] Two signwriters answer Ep 6 (one, on D3's paraphrase branch): independent descriptions of a gilded panel Violet carried. The rumour becomes testimony | 56 | Their letters; never the mark's meaning |
| 02 | F5, F3 | 20 | 85 | (t)[AC] The side-by-side spread built: both letters, photographed, aligned line against line. The episode's defining image | 55, 30 | The spread as an object; never its conclusion yet |
| 03 | A5, F4 | 24 | 100 | (t)[ET] The certainty gap: the first letter says I think, and I may be wrong; the second says I know. Dead women do not usually get surer | 55, 30, 62 | The gap as Ally reads it aloud; never its cause |
| 04 | F4, A4 | 16 | 70 | (t)[ET] The slip dates the first: lodged three weeks before she died, calm, provisioned, sealed. Fear with a filing system | 62 | The date doing its work; nothing else |
| 05 | A5, F3 | 20 | 85 | (t)[ET] The second letter is kind to exactly one man: every line leans away from the one it mentions kindly, once, as the one she sent up the ladder | 30 | The kindness observed; never its author |
| 06 | R4, D4, A2 | 18 | 75 | (t)[CF] The wedding that was a marathon: Ruby saw the results page in spring and said nothing. He lied about where he went, once, provably, about nothing that mattered. Or so it seemed | 8 via 55 | The fact as Ruby's held pebble; never what the lie was for |
| 07 | A5, F5, F3 | 36 | 155 | ★TURN [ET] A listener asks what Brad's handwriting looked like. Ruby cannot answer. Nobody can. Nobody alive ever saw Brad write cursive, and the letter is in cursive, and it is kind to him. The letter is probably not hers. **Wall; +20 energy** | R3a (D311), 30 | Everything aired Day 311, the widest brief before the close: the second letter may not be Violet's and its kindness points at Brad. Never: the swap's mechanics, the panel's meaning, that Brad is alive |
| 08 | R5, D3 | 20 | 85 | (t)[AL] Gerald's booth: a letter that passes examination is not proved hers, love; it is just not proved anyone else's. Method, not facts; canon register | canon (Gerald) | Gerald says nothing the police could not; method only |
| 09 | A5, F4, R4, D3 | 36 | 155 | (t)[ET] Ep 7 assembled: the gap, the slip, the kindness, the question nobody can answer, built into one broadcast | 55, 62, R3a, 30 | The broadcast as built; nothing beyond p07's licence |
| 10 | A3, F2, D2, A1 | 9 | 45 | (t)[AL] Ep 7 publishes and the mailbag opens: more post than any week before, and two letters in it that matter (unread). A quick win before the heavy week | A5 D311 | The volume; never the two letters' contents |

Parallel pair: p01 ∥ p02 (both from chapter 6's close; p03 requires both). p07 airs on Day 311 exactly.

## Chapter 8 · The First Train · Days 311 to 318 · 10 pkgs · 25 cards · 220 T1eq · 945 CC

Alive becomes thinkable and is never said. The first five-card packages (p02, p09) with single-card quickies beside them (p04, p10).

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | A5, R4 | 24 | 100 | ★TURN [ET] The skipper's letter: the deckhand who drank at Brad the signwriter's went missing three years before Violet, and was never found. To Del, never aired. Brad acquires a shadow. **Season notch candidate sits here (F1)** | 10c, R6a (D311) | The letter verbatim; that it goes to Del and is never aired; never what it means |
| 02 | R5, R4, A4, D3, F3 | 40 | 170 | (t)[ET] The harbour years mapped, five cards across the town: a man who lived aboard, kept a dry bag at a mate's, drank ashore, missed a sailing, and had a first name and a trade nobody ever asked for. **First five-carder** | 10c, C8 via the skipper | The deckhand as the skipper knew him; never where the bag went, never the flat |
| 03 | R4, D3 | 12 | 50 | (t)[AL] The harbour master's ledger of leavers: itinerant crews, men who go between tides, files closed as drifted. Men the sea is allowed to keep | C8 texture, NEW route | The class of men, lawfully; never this man's file contents |
| 04 | A3 | 4 | 20 | (t)[AL] Ally files it: she cannot see what the skipper's letter means, and says so, and keeps it. A drawer with a weight in it. Quick win | R6a | Her not-knowing, honestly; nothing else |
| 05 | A5, F4 | 24 | 100 | ★TURN [ET] The platform woman writes: the first train out, the Saturday Brad's car was found, a man with wet boots paying cash. Someone walked out of Kestrel Head at dawn. To Del, not aired | 60 (D318) | The letter, the boots, the cash single; never who was on the train |
| 06 | F5, R4, A3 | 28 | 120 | (t)[ET] The timetable against the search: last train in Friday 17:10, first out 05:47, and a ridge path between them that a fit man could walk in a night. Built on the board, held, never aired | 60, public timetables | The reconstruction as Ally's private working; never aired, never confirmed |
| 07 | D4, R3 | 12 | 50 | (t)[CF] The clerk and the change: he complained about breaking a note at 05:40 and remembered the boots because of it. Small memory outlives tape | 60, R10 via the letter, NEW compression | The texture of the sighting; never the tape (it expired) |
| 08 | A5, A3 | 20 | 85 | (t)[AL] On air: if you were on that train, write to me. The letter itself goes to Del; the ask is the broadcast | 60, A5 D318 | The ask verbatim; never what Ally hopes |
| 09 | F5, A5, R4, D4, F3 | 52 | 230 | (t)[AL] The re-string, five cards: every card on the wall re-read in one sitting against a possibility Ally will not say on air. Her private note: I have stopped saying "was". **Chapter wall** | all held rows | Her private note exactly; never the word alive |
| 10 | A3 | 4 | 20 | (t)[AL] Ep 8 publishes. Quick win; the week empties out | A5 D318 | Nothing new |

Parallel pair: p03 ∥ p04 (both from p02; p05 requires both). p05 onward sits on Day 318's broadcast week; Del revives the deckhand's file on Day 312 off-page and the player never sees it (C7).

## Chapter 9 · One of a Kind · Days 319 to 325 · 10 pkgs · 26 cards · 220 T1eq · 945 CC

Four descriptions become one object; describing it out loud is a decision with a cost. Decision D4 in p08 (climax wall, +2 Platinum Ingots). No asset in this chapter may depict the panel itself: the board shows the witnesses' composite, text and sketch fragments, never the object (F6).

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | A4, D4, D3 | 20 | 85 | (t)[ET] The four describers gathered: the café, the cinema owner, the two signwriters, four memories of one thing nobody has seen since she died | 56 | The witnesses' recollections; never where the thing is |
| 02 | F4, D3×2 | 16 | 70 | (t)[ET] Cross-check one: black ground, gold capitals, an ampersand. The letters agree | 56 | The converging details; nothing else |
| 03 | F4, D3 | 12 | 50 | (t)[ET] Cross-check two: a crack across one corner. Three of four remember it; nobody knows how it got there | 56 | The crack as described; never its cause |
| 04 | F5, A4 | 24 | 100 | (t)[ET] Cross-check three: a small mark bottom right that was not her name and not his. The strangest fact in the case | 56 | The mark as described; never whose it is |
| 05 | D4, A3 | 12 | 50 | (t)[CF] She ranked handwriting out loud: the woman who judged every hand in town could not place this one, and said so for a month. Ally holds, privately, what the trade of one dead friend was, and does not air it | v1.2, 56 | Violet's expertise; Ally's private juxtaposition exists but is never aired and never confirmed; never the word Brad |
| 06 | R3 | 4 | 20 | (t)[CF] The brush: sable on a handle cut from a broken oar, green tape at the ferrule, hers beyond mistake from across a road. What her hand looked like in the world. Quick win | v1.2 | The brush as texture (Fable's seed, kept as texture per the spine); never a clue |
| 07 | A4, F3, R3 | 16 | 70 | (t)[ET] Del asks: hold one detail back, so claims can be tested. She does not say why she asks | C7, v1.0 D4 | Del's request and her silence about her reasons; never her private hypothesis |
| 08 | R5, A5, F5, F4 | 56 | 240 | ★TURN [ET] The description assembled: one of a kind, and about to be spoken into every kitchen in the country, including one Ally cannot know about. Airing it is the only way to find it, and airing it warns whoever holds it. **D4 resolves here. Climax wall; +2 Platinum Ingots** | 56 | The full description as assembled; Del's request; that Ally airs knowing the holder will hear; never whose mark, what it proves, who hears |
| 09 | A5, D4, R4 | 32 | 135 | (t)[AL] Ep 9 airs the description (per D4). The player leaves the app having said it out loud | A5 D325 | What aired per branch; never the burn (the player never sees it; Ally never learns it) |
| 10 | A4, R4, F3×2, D3 | 28 | 125 | (t)[AL] The night after: nothing left to do but wait; the tip line light burning in a dark studio | A5 | Ally's side only; nothing that happens 400 miles away exists in this game's render |

Parallel pairs: p02 ∥ p03 (both from p01; p04 requires both) and p05 ∥ p06 (both from p04; p07 requires both).

## Chapter 10 · The Man on the Wall · Days 326 to 333 · 10 pkgs · 32 cards · 225 T1eq · 975 CC

The letter, the wait, the arrest, the accounting. Strict chain: this week runs on rails and the fiction says so. Five-carders (p04, p08, p10) with quickies beside them (p03, p06).

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | A5, F4 | 24 | 100 | ★TURN [ET] A private message, Day 326: a landlady four hundred miles away; a quiet lodger under another name whose habits she has heard from strangers on air for six weeks; a wall with a pale gap where a black and gold thing hung until Tuesday. An address. Never aired | 63 (D326) | Written last with the close (v1.0 rule kept): the final briefs are drafted only after every other package's prose is locked |
| 02 | A4, F3, D2 | 14 | 60 | (t)[ET] 07:00, Del: everything handed over whole, the download map beside it as corroboration. Del moves inside the day | 63, 61 | As p01 |
| 03 | D2 | 2 | 10 | (t)[AL] The wait, day one: Ally holds the heaviest thing she has ever held and says nothing to anyone. Quick win | A5 | As p01 |
| 04 | A5, A4, F4, R4, D3 | 44 | 185 | (t)[ET] Ep 10 built, five cards: an episode that must be indistinguishable from episode nine, because he listens. The player assembles a broadcast whose whole job is to be boring, and knows why | A5 D332 | As p01 |
| 05 | D4, A3, R3, D1×3 | 19 | 80 | (t)[CF] What gave him away was never evidence: keys moved twice, thank you like it cost him, up at five. Six weeks of habits, told by people who loved him, heard by a woman who dusts his room | 63, 55 | As p01 |
| 06 | A3 | 4 | 20 | (t)[AL] Ep 10 airs, Day 332. Quick win; silence holds | A5 D332 | As p01 |
| 07 | A5, R4, F3 | 28 | 120 | ★TURN [ET] The news, Day 333: arrested four hundred miles away, under a name that sat for three years in a missing-person file in Del's own precinct. Brad Collins, alive | public record D333 | As p01 |
| 08 | A4, F4, R4, D4, F3 | 36 | 150 | (t)[ET] The accounting, five cards: every held tip laid in its true place at last: the Wednesday visitor, the first train, the skipper's deckhand. What Ally knew, when, and why she never said it | 57, 60, R6a, within consent | As p01 |
| 09 | A4, D3, F3 | 16 | 70 | (t)[CF] Margo read it right first and paid first: she said it once, to one person, and her words are in the record now. The letter was wrong, and she was not | 59, A5 | As p01 |
| 10 | A5, R4, D4, F3, D2 | 38 | 180 | (t)[AL] The close: he is alive and what that un-writes; the survivors' war ends where the accusation was; the landlady is never named, even now; and the honest limit: the arrest is for the identity offences, and Violet's staircase is not Ally's to promise. Sign-off. **+3 Platinum Ingots, +20 energy.** Sets `fk.ep01.complete`; spawns `cold_case_a`, `ep2_teaser` (shape is Stephen's, F2) | 63 held, public record | As p01; the last brief ends the blindness, written only after everything else is locked |

---

# C. THE DECISIONS

The four decisions of v1.0, re-placed at package boundaries. Their class analysis stands unchanged and is not restated in full here (v1.0 Part C is the reference); what follows is the placement, the axis, and the standing caveat.

**The known limitation, named, not hidden:** the spine's clock and arrest are fixed; they survived three adversarial rounds by being inevitable. No decision moves who is arrested or when. Decisions redistribute who is protected, exposed and believed along the way, and what the public record ends up holding. The ending-changing axis is unavailable by construction, this document says so plainly, and the question goes to the attack again (the attack prompt names it in scope as a fairness question, with the spine itself out of scope).

| D | Package · Day | The choice | Class | Branch flags |
|---|---|---|---|---|
| D1 | `fk_p02_03` · ~272 | Ruby's letter read whole on air, or only what Ally can verify (the garbled leak lands by chapter 6 on the held branch) | B | `aq.fk.d1.aired` / `aq.fk.d1.held` |
| D2 | `fk_p04_09` · 290 | The tip to Del raw, or Bay 3 on tape first then both handed over (Liam goes quiet to the show until Day 311 on the market branch) | A | `aq.fk.d2.direct` / `aq.fk.d2.market` |
| D3 | `fk_p06_06` · ~302 | Margo's words verbatim anonymised, or paraphrased with the channel shielded (one signwriter instead of two answers; the Ep 9 assembly drops the crack; Tessa's close line differs) | B | `aq.fk.d3.verbatim` / `aq.fk.d3.paraphrase` |
| D4 | `fk_p09_08` · 325 | The mark aired, or held at Del's request (the sift versus the clean identification at the close) | A | `aq.fk.d4.mark_aired` / `aq.fk.d4.mark_held` |

Placement checks: one live decision at a time; every branch rejoins within the following chapter (D1 by ch3, D2 by ch7 p01 at the latest via Liam's return on Day 311, D3's evidence-quality tail is perceivable at ch9 and closes at ch10, D4 within the close); no flag gates a mainline package, so no reachable state starves the chain; every named cost surfaces in a scene or on the board before the episode ends. Clock check unchanged from v1.0: no B6 day moves on any branch; the burn, the landlady, Del's 07:00 and the arrest hold in all sixteen reachable flag states.

D3's cross-chapter consequences in package terms: `aq.fk.d3.paraphrase` changes p07_01 (one letter, not two), p09_03 (the crack drops from the assembly, two describers not three) and one Tessa variant line at p10_10. Variant dialogue nodes and board annotations only; identical package graph.

---

# D. CHAPTER 1, IN SECONDS

The cold open v0.4 (~470 words, ~3:20 at 140 wpm) spreads across chapter 1's ten packages. v1.0's three-segment cut is superseded as a proposal by this finer one; both remain proposals until Stephen rules the boundaries (F4). Every word is v0.4 as adopted; nothing is rewritten.

**The proposed cut** (VO plays across each package's opening and completion nodes; the merge sits between):

| Pkg | v0.4 stretch | ~Words | ~Seconds |
|---|---|---|---|
| p01_01 | "This is Echoes of Havenbay" to "…bottom of her stairs." | 28 | 12 |
| p01_02 | "Violet painted signs…" to "…side of a bus once." | 47 | 20 |
| p01_03 | "Her death was ruled an accident…" to "…nothing more." | 24 | 10 |
| p01_04 | "But two weeks later…" to "…written in Violet's handwriting." | 28 | 12 |
| p01_05 | "Their names were…" to "…leave behind four keys." | 59 | 25 |
| p01_06 | "She also told her friends…" to "…all four of them could be there together." | 68 | 29 |
| p01_07 | "And when they finally opened it…" to "…what that object was." | 51 | 22 |
| p01_08 | "Then the group began to disappear." to "…That left Liam and Ruby." | 71 | 30 |
| p01_09 | "They both went to the police…" to "…At least, not yet." + the Del scene dialogue | 90 | 39 + scene |
| p01_10 | "I don't know what was behind that locked door…" to the ruled sign-off | 92 | 39 |

**First interaction:** the seeded pair pulses under p01_01's VO; the board accepts the tap when that segment ends, at **~0:12 to 0:15**. This is earlier than v1.0's 38 to 39 seconds because the finer package grain lets the open keep rolling between merges instead of front-loading it. I6's 35-to-40-second band is still an open ruling: if Stephen holds the band, the board simply stays locked until the end of p01_03 (~0:42) with no structural change. One number to rule (F3).

**Timeline (draft):** 0:00 ident and the death line · ~0:13 first tap (one A-T1: the §5.7 choreographed deterministic merge) · ~0:45 the ruling pinned · ~1:10 the keys land (p01_04, the first turn) · ~2:20 the pact and the second letter · ~3:20 the disappearances · ~4:00 Del on the steps (p01_09, the second turn, the chapter's heaviest card at A-T4) · ~5:30 sign-off, Ep 1 publishes, chapter 1 ends. Chapter 1 net energy ≈ 23: comfortably inside the FTUE tank with the whole open played.

Mechanical duties (systems pass, not this document): p01_01 carries the FTUE entitlements and the lab grant exactly as `e1_tip` does; the junk-drawer generator arrives via p01_10's overflow; the diner is granted at chapter 1's close (first D card is p02_01); `aq.loc.rusty_anchor.active` is set at chapter 2's open (first R card is p02_06). Wired and verified in the editor, not asserted here.

---

# E. ECONOMY CONFORMANCE

### E1. The envelope, met exactly

| Ch | Pkgs | Cards | T1eq | v1.1 envelope | Net energy (flat 1.55) | CC | CC at 4.3 |
|---|---|---|---|---|---|---|---|
| 1 | 10 | 10 | 35 | 35 | ≈23 | 150 | 151 |
| 2 | 10 | 15 | 70 | 70 | ≈45 | 300 | 301 |
| 3 | 10 | 20 | 105 | 105 | ≈68 | 450 | 452 |
| 4 | 10 | 21 | 135 | 135 | ≈87 | 580 | 581 |
| 5 | 10 | 24 | 170 | 170 | ≈110 | 730 | 731 |
| 6 | 10 | 23 | 205 | 205 | ≈132 | 880 | 882 |
| 7 | 10 | 27 | 215 | 215 | ≈139 | 925 | 925 |
| 8 | 10 | 25 | 220 | 220 | ≈142 | 945 | 946 |
| 9 | 10 | 26 | 220 | 220 | ≈142 | 945 | 946 |
| 10 | 10 | 32 | 225 | 225 | ≈145 | 975 | 968 |
| **Total** | **100** | **223** | **1,600** | **1,600** | **≈1,032** | **6,880** | **6,880** |

No chapter needed a budget different from the envelope: the variety the beats wanted fit inside it, so nothing was taken. Card count lands at 223 against the model's derived working figure of 250 (average 2.23 cards per package, 7.2 T1eq per card against the model's 2.5 and 6.4). The ruled numbers (100 packages, 1,600 T1eq) are met exactly; the card figure was always derived, and the difference is that this design prefers a slightly heavier single card over two trivial ones where the beat is one object (the letter, the slip, the screenshot). If the tuning pass wants more board actions per package, the mechanical conversion is T4 → T3×2 and T5 → T4×2 at identical T1eq; it changes nothing in this document but the Cards column.

Rewards riders (draft): +20 energy at p05_10, p07_07, p10_10 (total +60); +2 Platinum Ingots at p09_08, +3 at p10_10 (total 5, matching the shipped episode). Toasts per §5.7 on every package of 3+ cards.

### E2. The variety evidence (the ruling's test)

Package sizes across the episode: **28 one-card, 36 two-card, 26 three-card, 5 four-card, 5 five-card.** Within chapters, against a mechanical ramp:

- **Chapter 2 carries the ruled early spike:** p02_06 (the boxes, 16 T1eq, three cards) sits beside 2 and 4 T1eq singles.
- **Chapters 7 to 10 keep quick wins beside the heavy builds:** p07_10 (9 T1eq across four small cards), p08_04 and p08_10 (single A-T3 quickies beside a 52 T1eq five-carder), p09_06 (single R-T3 beside the 56 T1eq climax), p10_03 and p10_06 (2 and 4 T1eq singles in the finale week).
- **No chapter is monotonic:** chapter 8 runs 24, 40, 12, 4, 24, 28, 12, 20, 52, 4; chapter 10 runs 24, 14, 2, 44, 19, 4, 28, 36, 16, 38.
- **Tier spread inside chapters:** chapter 10 spans T1 to T5; chapter 5 spans T2 to T5; the five-carders mix three or four families each.
- **The heaviest single package** (p09_08, 56 T1eq, three T5s and a T4) is the climax and is far below The Listener's 160/185-class walls: the mass lives in the spread, which is the model's whole point.

### E3. A finding for the tuning pass: the family mix is lab-heavy, and the flat 1.55 yield hides it

By family: **A 630 · F 377 · R 291 · D 302** (after four diegetic swaps that moved the boxes and the ladder work to the junk drawer). A podcast-and-documents episode is honestly A and F shaped: the tape, the letters, the spreads. But the lab's tuned yield (1.3355 T1eq per tap) is the worst of the three generators, so the refined tap count at this mix is **≈1,210 taps** against the flat model's ≈1,032, which is closer to 12 free session-tanks than 10.3. Named, not solved here, because the levers are all tuning-pass property: shift more texture mass to R and D where the diegesis allows, retune the lab drop table, or accept ~12 sittings (the v1.1 ruling explicitly made the 1,600 tunable once the beats existed; this is that trade arriving on schedule). Chapter-sitting shape at either yield: chapters 1 to 4 clear a single tank free; chapters 5 to 10 run 1.1 to 1.5 tanks, which is one generous sitting or two short ones, with the +20 grants and ads narrowing it. Inside the 8-to-10-session guide at the flat yield, 10 to 12 at the refined one.

### E4. The beat census

| Payoff class | Count | Of which ★TURN |
|---|---|---|
| Evidence turn (ET) | 49 | 18 (17 full, 1 minor: p03_09) |
| Character fact (CF) | 20 | 0 |
| Ally line (AL) | 21 | 0 |
| Art with caption (AC) | 10 | 0 |
| **Total** | **100** | **18** |

Texture packages: 82, every one marked (t) in Part B. Every chapter carries at least one full turn; chapters 1, 2, 3, 4, 6, 8 and 10 carry two. Character facts: 20, of which 14 are mined straight from the five v1.2 (the tenor voice, the piano, the county swimming, the 2014 kitchen, the 4 a.m. call, the tonic, the habits, the flask, the cheque, the receipts bag, ten minutes early, the sharpness, the brush, ranked handwriting) and the rest are Tessa, the deckhand and the clerk from the spine's own texture. New facts and routes: none touch the graph; all are batched in F5.

### E5. The three weakest beats, named honestly

1. **`fk_p08_04` (Ally files it).** The beat is the absence of understanding: she holds the skipper's letter and cannot read it. Inherited from v1.0's lead 14, which was the thinnest full lead; as a package it is one card and twenty seconds, which is the right size for it, but a player who does not feel the weight will read it as filler. Kept because the close's identification is unbuyable without the player having held this letter as unexplained cargo, exactly as Ally does.
2. **`fk_p05_05` (the Sundays).** The board work maps a silence: withdrawals, sharpened edges, Sundays spoken for. The belief change is real but negative-space shaped, and it is the largest texture package in its chapter (24 T1eq), which risks toll-booth reading on a beat about nothing visible. Mitigation: the payoff line is the chapter's thesis (a silence with a shape) and the very next package is the chapter's turn.
3. **`fk_p10_03` (the wait, day one).** One card, one line, pure pacing. It exists to put air between the handover and the built broadcast, because the fiction needs the week to feel held. If any package in the episode is cuttable, it is this one; it is also two T1eq, so it costs the player forty seconds. Named so the cut, if it comes, is deliberate.

Also watched, defended: p01_01 (orientation, not a turn; the FTUE's price, capped at one merge) and p10_04 (a package whose deliverable is a deliberately boring broadcast; the point of the week, and the riskiest ask of the player's trust in the set).

### E6. Spine-day audit

Checked package by package against B6 and A5: p02_08 uses row 62 in its arrival week (Day 276, the tight joint v1.0 also carried) · p03_09 sits on Day 283 exactly · p04_09 on Day 290 · p05_06 on Day 297 (R15) · p06_01 on Day 300 (row 59) · p07_07 airs R3a on Day 311 · p08_01 reads the skipper's Day 311 letter after Ep 7 · p08_05 on Day 318 (row 60) · chapter 9 assembles sources dated 283 to 311 and airs Day 325 · p10_01 on Day 326 (row 63). Nothing aired precedes its A5 episode; everything held is shown held; nothing reaches the player that Ally never learns; the burn (Day 325 night) is never rendered and never known to Ally or the player. **Pass.**

Two private-inference items, watched and deliberately inside the line: p08_06 (the timetable reconstruction) and p09_05 (the trade juxtaposition) are Ally reasoning privately from lawful material, never aired and never confirmed by the game. Both are the class of thing v1.0's fk_bay3 already established (the player suspects and cannot say what of); neither surfaces a B6 fact early. Flagged for the attack rather than hidden.

---

# F. THE QUESTIONS FOR STEPHEN

Each in the ruled form: the question · what the structure needs · what changes otherwise. Stop there.

### F1. The season notch (carried from v1.0, still open)

§3.3 still lists The Listener as Ep 1 with the coin on Dot's cradle. The candidate stands as v1.0 put it: the whisper lands in `fk_p08_01`. The skipper's letter carries one line Ally does not follow: the deckhand had once been paid to row a Thursday run out of the harbour, quit it, and lived aboard broke ever after. One sentence, unexplained, never mentioned again. It brushes the street-layer/apex separation and §3.4 says a finger connects to the head only by explicit season decision, which is what this asks for. **The cost candidate:** Ally's description is what erased the panel; she knows by the close that the wall was cleared the day after she aired it. What the podcast touches, it changes. **Needs:** one dialogue node in p08_01 and one in p10_10. **Otherwise:** the episode ships arc-silent and fails §3.4 at review.

### F2. The close's shape, and the teaser (carried, extended)

The v1.0 candidate stands (the accounting after the arrest is public; Margo's words in the record; the landlady never named; the honest limit about the staircase; final image, Liam and Ruby in the same harbour crowd). **New half of the question, from the v1.1 economy ruling:** does an Ep 2 teaser beat live inside `fk_p10_10`, as a story beat with no extra play (the shipped `ep2_teaser` spawn is story-neutral tail either way)? **Needs:** the close's beats confirmed, and the teaser yes/no. **Otherwise:** the close package and the final Fable brief cannot be written, and chapter 10 ships as a stub.

### F3. The first-interaction number (carried, changed by this structure)

The finer package grain lets the first required tap land at ~0:12 to 0:15, under VO that keeps rolling; v1.0 proposed 38 to 39 seconds against I6's open 35-to-40 band. Both are now live options with no structural difference: rule one number (accept the early tap, or hold the board to ~0:42 at the end of p01_03). **Needs:** one number. **Otherwise:** the FTUE build cannot lock.

### F4. The cold-open cut boundaries (carried, changed by this structure)

Part D's ten-way cut supersedes v1.0's three-way proposal. Every Ally line is Stephen-ruled and a cut is an edit: the ten boundaries need ruling (or a coarser regrouping onto fewer of chapter 1's packages, which the structure tolerates without renumbering). **Needs:** the boundary ruling. **Otherwise:** chapter 1's dialogue assets cannot be assembled.

### F5. The new-character-facts batch (new, one ruling for the lot)

All invented texture in one place, per the kickoff rule. Facts marked CANON are v1.2 or spine texture and only the **route** to Ally is new; nothing below touches the graph, and each is safe at every future reveal because it explains nothing and forecloses nothing.

| # | Package | The new thing | Fact status |
|---|---|---|---|
| 1 | p02_09 | Ruby's piano surfaces via sheet music on her stand during the kitchen sessions | Fact CANON (v1.2); route NEW |
| 2 | p04_02 | A yard hand mentions Liam's tenor, cab door shut | Fact CANON (v1.2); route NEW |
| 3 | p04_07 | Liam gives his half of August 2014, flat and unwilling | Fact CANON (v1.2); route NEW; the beat states only his half and no adjudication |
| 4 | p05_01 | An office neighbour describes Margo's shut office | Fact trivial; route NEW |
| 5 | p05_02 | A former county teammate writes to the show about Margo's swimming | Fact CANON (v1.2); route NEW |
| 6 | p03_01 | The café's menu boards and archive photos of Violet's public work, via Arthur Finch's Gazette morgue (canon role) | Facts CANON; route NEW (the morgue holds sign photos) |
| 7 | p08_03 | The harbour master talks generally about itinerant crews and files closed as drifted | Fact class CANON (C8); route NEW; he never opens this man's file to her |
| 8 | p08_07 | The clerk's complaint about the change rides inside the platform woman's letter (she was at the window) | Facts CANON (R9, R10); compression NEW |

### F6. Art-with-caption beats that are load-bearing (new, for the art bill)

Decorative AC beats are not listed; these carry story weight and their art fails the beat if it fails: the wake photograph (p01_05) · the key letter as object (p02_02) · the Regent marquee with the chained ladder (p03_04) · the market corridor at 05:40 (p04_08) · the coast road drive (p06_02) · the side-by-side letter spread (p07_02, reused p07_09) · the witness-composite of the panel (p09_08 and p09_09: **text and sketch fragments only; no asset may depict the panel itself, in this episode or any marketing**) · the pale gap on the lodger's wall, if the close ever shows it (p10_01: recommend narration only, keeping the landlady's world unrendered). Ten dedicated AC payoffs exist in total; the count is reported, not rationed, per the ruling.

---

# G. PACKAGE-CONTAINER ASSUMPTIONS (for the costing doc)

There is no package container in code (verified this session). This design assumes, and the costing doc prices exactly:

1. **A package is a data container above lead cards:** either a `PackageData` ScriptableObject holding ordered member card ids, or a `packageId` field on `LeadData` plus a manifest; the costing doc chooses. Cards are ordinary `LeadData` assets and flow through `LeadsRepository`, `LeadOutcomeMB` and the save path unchanged.
2. **A card carries exactly one requirement slot** (one family+tier, quantity 1 to 3, inside the shipped `[Range(1,3)]`). The schema supports up to 3 slots; v2.0 uses one per card so the bar reads one item per chip. Every Cards cell in Part B is a list of such single-slot cards.
3. **Package fulfilment = all member cards Activated.** The beat presentation (dialogue, art+caption surface, character-fact card) fires once, on the completing card's activation, via the package container; member cards before the last play at most a toast (§5.7 pacing on 3+ card packages).
4. **CC, energy, ingot and special rewards are package-level** (paid by the completing card or the container); member cards pay 0. Part B's CC column is the package payout. Banding stays on the shipped `SoftCurrency` field of the completing card if no container reward field exists.
5. **Gating is package-to-package**, implemented with the shipped primitives: every card of package N+1 carries `RequiredLeadIds` = the member card ids of package N (AND-gate semantics already in `CheckAndUnlockBlockedLeads`), or the container resolves it; either way no new gate machinery is strictly required. Parallel pairs gate both packages on the same predecessor.
6. **All cards of an Available package are simultaneously Available** (player picks order within the package); at most 2 packages Available at once, so the bar holds at most ~7 chips in the worst five-carder pair (UI grouping by package is the leads-bar work item in the systems bill).
7. **Save schema: no new aggregate.** Package state is derivable from activated card ids (the shipped `_activatedLeadIds` set persisted by the existing save path); the container never persists its own state. If the costing doc finds it must, the save aggregate rule applies (fold into `BoardSaveSystem`, crash-boundary tests mandatory).
8. **Ids and assets** as ruled in the header: `fk_p<ch>_<n>` packages, `fk_p<ch>_<n><letter>` cards, `Lead_FK_*` assets, `Assets/Content/FourKeys/`, database-decides-membership, slot ep01, no replay (R6), decision flags `aq.fk.d1..d4` never gate mainline packages.
9. **Family availability:** lab (A, F) from p01_01; junk-drawer generator via p01_10 overflow; diner granted at chapter 1 close; `aq.loc.rusty_anchor.active` set at chapter 2 open. Mirrors the shipped `e1_tip` duties; wired in a systems pass with editor verification.
10. **Analytics:** package_complete events with chapter, package id, T1eq, session index; the §7.7 climax watch attaches to p09_08 and p10_04.

The systems bill named in the economy model v1.1 (leads-bar grouping, beat presentation surface, package gating, FTUE teaching, analytics) sits on top of `feature/multi-episode-audit`, which is still unmerged and not play-verified; nothing here changes that sequencing.
