<!-- pdf-title: The Friends with Four Keys, Structure v2.2 -->

# THE FRIENDS WITH FOUR KEYS
## THE STRUCTURE, v2.2: CHAPTERS AND LEAD PACKAGES

**v2.2, 2026-09-01, same day: F1 RULED.** The season notch, Stephen's own design after a three-source blind batch (39 candidates, `four-keys-notch-candidates-v1.md`; his verdict on the batch's whole class: reaction is weak, action is strong). **The whisper:** the skipper's Day 311 letter mentions, in one unremarkable line, that he kept a small box of the deckhand's things left aboard, in case anyone ever came asking. Nobody asks until the arrest. The box opens in the accounting: a few photographs, bland, pinned where the player can study them and see nothing. At the finale playback Gerald goes still at one of them, asks Ally for it, asks her to keep it off the show: not yet, love, a few things to check first. Ally, plainly: he hasn't kept anything from a case since he retired, and he retired the year Dad died. **The cost:** the landlady's letter already told Ally the wall gap appeared the day after she aired the description: what she described, she erased, and the close says so. The connection lives in Gerald's head and one deliberately ambiguous frame; nothing settled moves; what he saw is committed at season planning, never in Ep1.

**v2.1, 2026-09-01.** The ChatGPT structure attack (five fronts, REWORK, `four-keys-structure-attack-chatgpt-2026-09-01.md`) and the overnight critical review are folded in under Stephen's two rulings of 2026-09-01: ① **the agency forks are adopted**: D3 forks play in chapter 7 and D4 forks the close, mutually exclusive flag-gated branch packages of equal mass, rejoining on the settled arrest, per the attack's findings 7 and 8; D1 and D2 stay lower-order with their promised consequences now mapped to named rows ② **chapter 1 is recut**: first tap at 12 to 15 seconds (F3 thereby ruled), the accident ruling and the keys inside the second package cycle (keys by ~0:45), the accusation landing exactly on p01_04's completion, and p01_09 board-first so Del interprets rather than repeats. Also applied: the census relabelled around the turns actually delivered (findings 1 and 5), the tap arithmetic done in one unit system (finding 2, with a correction: the 1,210 was gross taps including the tuned ×1.10 overhead, which the attack's ceiling omitted; the unit mixing it caught was real), chapter 1's size mix and chapter 7's quick win repaired (findings 3 and 4), and the CC column labelled as rounded. Played packages remain 100; authored packages are now 105 (five branch pairs).

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
| 1 | The Front Page | 266 to 269 | **Ep 1 (269)** | The accusation lands (p01_04) · she knew, not thought (p01_06) · the police door closes honestly, board-first (p01_09) | 10 | 35 |
| 2 | The Kitchen Table | 270 to 276 | **Ep 2 (276)** | The reading landed on Liam because the letter leans (p02_03; the slip is provenance texture, not a turn) | 10 | 70 |
| 3 | Whose Hand | 277 to 283 | **Ep 3 (283)** | Money moved in her last months · what she found had a shape (the download map is texture: attention, not belief) | 10 | 105 |
| 4 | The Wednesday Visitor | 284 to 290 | **Ep 4 (290)** | The letter reads built to fit Liam · Brad was inside the arrangement before it opened | 10 | 135 |
| 5 | Did She Say Anything | 291 to 297 | **Ep 5 (297)** | The lane man was the wrong build for Liam; Margo is the missing voice | 10 | 170 |
| 6 | Two Hours Up the Coast | 298 to 304 | **Ep 6 (304)** | Margo doubted the letter's meaning · if the thing was wrong there was a real thing, and it is missing | 10 | 205 |
| 7 | I Think Against I Know | 305 to 311 | **Ep 7 (311)** | The second letter is probably not hers, and nobody alive ever saw Brad write | 10 | 215 |
| 8 | The First Train | 311 to 318 | **Ep 8 (318)** | A man vanished out of Brad's life three years early · someone walked out of Kestrel Head at dawn · Ally stops saying "was" (p08_09) | 10 | 220 |
| 9 | One of a Kind | 319 to 325 | **Ep 9 (325)** | The town's one judge of hands could not place this one, and the only signwriter in the room is dead (p09_05; the broadcast assembly p09_08 is the climax act, not a turn) | 10 | 220 |
| 10 | The Man on the Wall | 326 to 333 | *(Ep 10, Day 332, airs inside the chapter)* | The landlady's letter (p10_01) · Margo was right first (p10_09; the arrest confirms, it does not turn) | 10 | 225 |

**How v1.0's 16 turn leads map in** (the skeleton, relabelled per the attack): fk_frontpage → p01_01 (orientation, still not a turn) · fk_fourkeys → p01_04 (the accusation, real after the recut) · fk_sergeant → p01_09 (board-first after the recut) · fk_kitchen → p02_03 · fk_boxes → p02_06 + p02_08 (texture: provenance, not belief) · fk_calling_in → p03_02 · fk_gold_panel → p03_06 (the map at p03_09 is texture) · fk_liam → p04_06 · fk_bay3 → p04_09 · fk_asking → p05_06 + p05_08 · fk_tessa → p06_03 + p06_06 · fk_wrong_thing → p06_08 · fk_ithink → p07_03/04/05 building to p07_07 · fk_skipper → p08_01 · fk_platform → p08_05, and the re-string p08_09 is the unclaimed turn the census missed · fk_one_of_a_kind → p09_05 (the turn) + p09_08 (the act) · fk_close → p10_01 + p10_09 (Margo righted) with p10_07 as confirmation.

**The 18 turns of v2.1:** p01_04, p01_06, p01_09 · p02_03 · p03_02, p03_06 · p04_06, p04_09 · p05_06 · p06_06, p06_08 · p07_07 · p08_01, p08_05, p08_09 · p09_05 · p10_01, p10_09. Every chapter carries at least one; chapters 2, 5, 7 and 9 carry exactly one.

**Evidence board phases** (`boardPhase`): Phase 1 = chapters 1 to 3 (the public case) · Phase 2 = chapters 4 to 7 (the doubt) · Phase 3 = chapters 8 to 10 (the man).

**Broadcast map** (unchanged from v1.0): Eps 1 to 9 air at the ends of chapters 1 to 9 on Days 269, 276, 283, 290, 297, 304, 311, 318, 325; Ep 10 (Day 332) airs inside chapter 10 and is indistinguishable from Ep 9 by design; the arrest is Day 333.

**Locations** (new builds flagged, carried from v1.0): Ally's studio (exists), precinct steps (exists), Ruby's kitchen ⚠, harbour café ⚠, Regent cinema frontage ⚠, boatyard ⚠, slip boatsheds at dawn ⚠ (was the fish market corridor; Stephen-ruled 2026-09-03), Mariner's Row lane ⚠, Tessa's kitchen ⚠, harbour master's office ⚠, inland halt platform ⚠. Kestrel Head stays off the art bill: photographs and narration only, never playable.

---

# B. THE BEAT AND PACKAGE TABLES

Format per row: **#** (package id is `fk_p<ch>_<n>`) · **Cards** (family letter + tier, ×q for quantity; A = Audio Investigation, lab, T1 to T6 · F = Forensic Tools, lab, T1 to T5 · D = Kestrel Corner Diner food, T1 to T12 · R = Rusty Anchor, junk drawer, T1 to T10; T1eq of tier n = 2^(n−1) per item) · **T1eq** package total · **CC** on package completion (cards pay 0; Part G assumption 4) · **Beat**: ★TURN or (t) texture, payoff class in brackets (ET evidence turn · CF character fact · AL Ally line · AC art with caption), then the beat in one line · **Source** (B6 row numbers from spine v1.4; "v1.2" = the five; NEW = Part F5 batch) · **Fable** (the redaction delta: the standing base is spine Part G plus all earlier packages' briefs; each line states only what is added, and what is never told).

Diegetic family reads (v1.0, kept): A = the tape and the show (tip line, letters read aloud, the broadcast itself) · F = documents and scene work · R = the harbour (yard, slip, market, the pub that changes its name) · D = door-knocking (cafés, kitchens, a witness's coffee).

Gating: within a chapter, packages chain (each requires the previous) except the parallel pairs named under each table; the last package of a chapter spawns the first of the next. Maximum simultaneous Available packages is 2, the bar's shipped comfort. Decisions (Part C) resolve inside the named package's completion dialogue. **Flag rule, amended at v2.1:** no flag ever gates a *shared* mainline package; the D3 and D4 branch packages are flag-gated in mutually exclusive pairs (`requiresFlag`/`forbidsFlag`, the shipped primitives) whose union is the mainline, so exactly one version is reachable in every state and nothing can starve.

## Chapter 1 · The Front Page · Days 266 to 269 · 12 pkgs · 14 cards · 39 T1eq · 150 CC

**Playtest amendment 2026-09-02 (Stephen-ruled, record: `four-keys-ch1-playtest-verdict-2026-09-02.md`):** packages 09 and 10 were too big in play and split after their third line each (09 / 09b, 10 / 10b; words unchanged, F4 re-seam only); the cards of one package never ask the same item (06 is now F2 + A2); chapter census 12 packages, 14 cards, 39 T1eq; authored total 107. Rows below reflect the amendment.

FTUE, recut 2026-09-01 per Stephen's ruling (attack findings 3, 5 and 6): the accident ruling and the keys arrive in the second package cycle, the accusation lands exactly on p01_04's completion, p01_09 is board-first, and two packages carry two cards so the chapter has size mix. Lab families only (A and F flow from the first generator grant). The cold open v0.4 is spread across packages 1 to 10; the cut is Part D; the boundary sign-off is Stephen's (F4). First deterministic merge choreographed per §5.7.

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | A1 | 1 | 10 | (t)[AL] Ident and the death line: a signwriter, her stairs, nine months ago. First tap at ~0:13. Orientation, not a turn; it must never grow | 54 | Nothing new; VO is v0.4 as ruled |
| 02 | F1×2 | 2 | 10 | (t)[ET] Her work in one breath, then the ruling pinned: accident, a fall, nothing more. The accepted account is on the board by ~0:45 | 54, v1.2 | Violet's public work only; never the panel |
| 03 | A2 | 2 | 10 | (t)[ET] Four keys and a letter in her handwriting arrive in the post: the dead woman arranged something. The keys are in play by ~0:50 | 54 | Nothing new |
| 04 | A2 + F2 (two cards) | 4 | 20 | ★TURN [ET] Her closest friends named, and the letter's sentence lands on this package's completion: she thought one of them was planning to kill her. Accident is now one of two readings | 54 | Nothing new beyond Part G |
| 05 | A2×2 | 4 | 15 | (t)[ET] Not the police, and a locked door: her instruction, and the reason she gave | 54 | Nothing new |
| 06 | F2 + A2 (two cards; was F2 + F2, amended 2026-09-02: one package never asks the same item twice) | 4 | 20 | ★TURN [ET] The pact, the Saturday, and behind the door a second letter: this one didn't say she thought. It said she knew. A thing of hers beside it that two survivors will not name | 54 | Never what the object was |
| 07 | A3 | 4 | 15 | (t)[ET] Then the group began to disappear: Brad's car, Margo's car, the same car park | 54 | Nothing new |
| 08 | A2×2 | 4 | 15 | (t)[ET] The survivors walk into the police station separately and say the same thing: I'm afraid of the other one | 54 | Nothing new |
| 09 | A4 | 8 | 15 | ★TURN [ET] "Del on the Steps". Board-first, then Del on the steps delivers the result the narration has held back: they looked, properly, the letters are really hers, and they found nothing (segment lines 1 to 3) | 64 | Del says only what Part G gives her; never anything the file holds |
| 09b | F2 | 2 | 10 | (t)[ET] "It Lives in People" (split 2026-09-02). Del interprets: closed means procedure found nothing it could hold; paper is all she may act on; whatever is wrong lives in people. "Which did you believe?" declined; "be careful" | 64 | As 09 |
| 10 | A2 | 2 | 10 | (t)[AL] "Havenbay Takes Sides". The Gazette war, Ruby yes and Liam not yet, the I-don't-knows | 54 | Nothing new |
| 10b | F2 | 2 | 10 | (t)[AL] "Ep 1 Publishes" (split 2026-09-02). Four friends and four keys, two dead, two afraid; where we begin; sign-off as ruled. Ep 1 publishes; sets `fk.ch1.complete` | 54 | Nothing new |

Chain is strict (FTUE). Chapter notes: mechanical duties mirror `e1_tip` (entitlements, lab grant at p01, junk-drawer generator granted via p10's overflow, diner granted at p10 for chapter 2; `aq.loc.rusty_anchor.active` set by chapter 2 open). Three turns after the recut; 14 cards after the 2026-09-02 split (chain 09a, 09b, 10a, 10b strict); the wake photograph moves into p01_04's beat presentation (AC) rather than its own package.

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
| 08 | F4, F2 | 10 | 45 | (t)[ET] In Ruby's envelope, the solicitor's covering slip: the letters were lodged three weeks before she died. Provenance, not a new belief (the player already holds her fear as prior); it becomes load-bearing at p07_04 | 62 (D276) | The slip and its date; Ally notes it without knowing what it proves; never what it will prove |
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
| 09 | A5 | 16 | 75 | (t)[ET] Ally's own download map: one town, four hundred miles away, listening every week. Held, never aired, never explained. Attention, not belief (the attack demoted it, correctly); it pays off at p10_02 | 61 (D283) | The map shows one persistent far town and Ally files it as odd; never what the town means |
| 10 | A4, F2, F2 | 12 | 50 | (t)[AL] Ep 3 publishes: the money and the panel go on air. **D1 held branch: the garbled leak lands here: a rival column runs a mangled version of the letter, and Ruby's on-air cost (you could have read it right) is this package's variant close** | 56; D1 map | What aired per branch; nothing else |

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
| 08 | R4, D3, F2 | 14 | 60 | (t)[AC] The slip boatsheds at 05:40: the lockers along the back wall, the slip road where the vans park, no camera on any of it, anyone with a key and a reason | 57 setting, 27 | The geography as Ally lawfully observes it; never who used it |
| 09 | R5, F4, A3 | 28 | 120 | ★TURN [ET] The boat owner's tip: Brad was at her store on the Wednesday, three days before the Saturday. The dead man went early. To Del the same day, never aired. **D2 resolves here** | 57 (D290) | The tip verbatim; that it goes to Del within the day and is never aired; Ally's private vertigo, unexplained; never what the visit was for, or that anything was swapped |
| 10 | A4, D3, D1 | 13 | 60 | (t)[AL] Ep 4 publishes: Liam's side airs; the tip is not in it; Ally sits with what she is holding. **D2 market branch: this broadcast lacks Liam's consented follow-up, and his silence to the show starts here** | 58; 57 held | What aired and what is held; never the held thing's meaning |

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
| 07 | F5, A4 | 24 | 100 | (t)[ET] The deduction built: the store letter's claims laid against everything the room ever verified, with Margo's line pinned across them | 59, 30 | The layout as Ally builds it; never that the deduction is correct |
| 08 | F5, A5, D4 | 40 | 170 | ★TURN [ET] If the thing was wrong there was a real thing, and it is not in the store, not in the boxes, not in the estate. Something is missing from the world. The chapter's wall | 59, 56 | The deduction exactly as it will air; never what the real thing is |
| 09 | F4, A4, D3 | 20 | 85 | (t)[ET] Close reading of "that": what did Margo think the thing could not mean? The board holds the question open; nobody alive can answer it | 42, 59 | The question as a question; never its answer |
| 10 | A5, D3×2 | 24 | 110 | (t)[AL] Ep 6 publishes: the words air anonymised (per D3), the deduction airs, and the mailbox is watched | 59 | What aired per branch; never who is listening |

Parallel pair: p03 ∥ p04 (both from p02; p05 requires both). All of chapter 6 sits inside Days 300 to 304.

## Chapter 7 · I Think Against I Know · Days 305 to 311 · 10 pkgs · 27 cards · 215 T1eq · 925 CC

The intellectual climax: the letter stops being Violet's. The turn is p07; p03 to p05 build it in the open. Chapter wall at p07 (+20 energy). **D3's fork lives here (ruled 2026-09-01, attack finding 7): p01 and p02 exist in two mutually exclusive branch versions, flag-gated, equal mass, rejoining at p03.**

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01v | F4, A3, D3 | 16 | 70 | (t)[ET] **Verbatim branch** (`aq.fk.d3.verbatim`): two signwriters answer Ep 6's exact words with independent descriptions of the gilded panel. The rumour becomes double testimony | 56 | Their letters; never the mark's meaning |
| 01p | F4, A3, D3 | 16 | 70 | (t)[ET] **Paraphrase branch** (`aq.fk.d3.paraphrase`): one signwriter answers the softened ask, and Ally works the second description out of the café owner by legwork instead. Single-source testimony, harder won | 56 | One letter and a coaxed interview; never the mark's meaning |
| 02v | F5, F3 | 20 | 85 | (t)[AC] **Verbatim branch:** the side-by-side spread built from both letters, line against line, the crack corroborated twice. The episode's defining image at full strength | 55, 30 | The spread as an object; never its conclusion yet |
| 02p | F5, F3 | 20 | 85 | (t)[AC] **Paraphrase branch:** the spread built from one letter plus the shielding work: a separate board task scrubs every detail that could triangulate Tessa's channel before anything is pinned. The crack is uncorroborated and stays off the spread | 55, 30 | The thinner spread and the shielding as deliberate cost; never its conclusion yet |
| 03 | A5, F4 | 24 | 100 | (t)[ET] The certainty gap: the first letter says I think, and I may be wrong; the second says I know. Dead women do not usually get surer. **Rejoin: both D3 branches land here. D2 market branch: Liam's first words to the show since Day 290 arrive this week (Day 311), a two-line return that names his silence** | 55, 30, 62; D2 map | The gap as Ally reads it aloud; never its cause |
| 04 | F4, A4 | 16 | 70 | (t)[ET] The slip dates the first: lodged three weeks before she died, calm, provisioned, sealed. Fear with a filing system | 62 | The date doing its work; nothing else |
| 05 | A5, F3 | 20 | 85 | (t)[ET] The second letter is kind to exactly one man: every line leans away from the one it mentions kindly, once, as the one she sent up the ladder | 30 | The kindness observed; never its author |
| 06 | R4, D4, A2 | 18 | 75 | (t)[CF] The wedding that was a marathon: Ruby saw the results page in spring and said nothing. He lied about where he went, once, provably, about nothing that mattered. Or so it seemed | 8 via 55 | The fact as Ruby's held pebble; never what the lie was for |
| 07 | A5, F5, F3 | 36 | 155 | ★TURN [ET] A listener asks what Brad's handwriting looked like. Ruby cannot answer. Nobody can. Nobody alive ever saw Brad write cursive, and the letter is in cursive, and it is kind to him. The letter is probably not hers. **Wall; +20 energy** | R3a (D311), 30 | Everything aired Day 311, the widest brief before the close: the second letter may not be Violet's and its kindness points at Brad. Never: the swap's mechanics, the panel's meaning, that Brad is alive |
| 08 | R5, D3 | 20 | 85 | (t)[AL] Gerald's booth: a letter that passes examination is not proved hers, love; it is just not proved anyone else's. Method, not facts; canon register | canon (Gerald) | Gerald says nothing the police could not; method only |
| 09 | A5, F4, R4, D3, A1 | 37 | 160 | (t)[ET] Ep 7 assembled: the gap, the slip, the kindness, the question nobody can answer, built into one broadcast | 55, 62, R3a, 30 | The broadcast as built; nothing beyond p07's licence |
| 10 | A4 | 8 | 40 | (t)[AL] Ep 7 publishes and the mailbag opens: more post than any week before, and two letters in it that matter (unread). One card, one tap-run: a real quick win before the heavy week (attack finding 4's repair) | A5 D311 | The volume; never the two letters' contents |

Parallel pair: p01 ∥ p02 (both from chapter 6's close; p03 requires both). p07 airs on Day 311 exactly.

## Chapter 8 · The First Train · Days 311 to 318 · 10 pkgs · 25 cards · 220 T1eq · 945 CC

Alive becomes thinkable and is never said. The first five-card packages (p02, p09) with single-card quickies beside them (p04, p10).

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | A5, R4 | 24 | 100 | ★TURN [ET] The skipper's letter: the deckhand who drank at Brad the signwriter's went missing three years before Violet, and was never found. One passing line: he still keeps a small box of the man's things left aboard, in case anyone ever comes asking. To Del, never aired. Brad acquires a shadow | 10c, R6a (D311); F1 ruled | The letter verbatim including the box line, unweighted; that it goes to Del and is never aired; never what it means |
| 02 | R5, R4, A4, D3, F3 | 40 | 170 | (t)[ET] The harbour years mapped, five cards across the town: a man who lived aboard, kept a dry bag at a mate's, drank ashore, missed a sailing, and had a first name and a trade nobody ever asked for. **First five-carder** | 10c, C8 via the skipper | The deckhand as the skipper knew him; never where the bag went, never the flat |
| 03 | R4, D3 | 12 | 50 | (t)[AL] The harbour master's ledger of leavers: itinerant crews, men who go between tides, files closed as drifted. Men the sea is allowed to keep | C8 texture, NEW route | The class of men, lawfully; never this man's file contents |
| 04 | A3 | 4 | 20 | (t)[AL] Ally files it: she cannot see what the skipper's letter means, and says so, and keeps it. A drawer with a weight in it. Quick win | R6a | Her not-knowing, honestly; nothing else |
| 05 | A5, F4 | 24 | 100 | ★TURN [ET] The platform woman writes: the first train out, the Saturday Brad's car was found, a man with wet boots paying cash. Someone walked out of Kestrel Head at dawn. To Del, not aired | 60 (D318) | The letter, the boots, the cash single; never who was on the train |
| 06 | F5, R4, A3 | 28 | 120 | (t)[ET] The timetable against the search: last train in Friday 17:10, first out 05:47, and a ridge path between them that a fit man could walk in a night. Built on the board, held, never aired | 60, public timetables | The reconstruction as Ally's private working; never aired, never confirmed |
| 07 | D4, R3 | 12 | 50 | (t)[CF] The clerk and the change: he complained about breaking a note at 05:40 and remembered the boots because of it. Small memory outlives tape | 60, R10 via the letter, NEW compression | The texture of the sighting; never the tape (it expired) |
| 08 | A5, A3 | 20 | 85 | (t)[AL] On air: if you were on that train, write to me. The letter itself goes to Del; the ask is the broadcast | 60, A5 D318 | The ask verbatim; never what Ally hopes |
| 09 | F5, A5, R4, D4, F3 | 52 | 230 | ★TURN [ET] The re-string, five cards: every card on the wall re-read in one sitting against a possibility Ally will not say on air. Her private note: I have stopped saying "was". Brad's survival becomes her operative belief, and the player's. **Chapter wall** (promoted by the attack: the census had this as texture and it is the chapter's third turn) | all held rows | Her private note exactly; never the word alive |
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
| 05 | D4, A3 | 12 | 50 | ★TURN [ET] She ranked handwriting out loud: the woman who judged every hand in town could not place this one, and said so for a month. And the only other signwriter in her life is dead. Ally holds the juxtaposition privately and does not air it; the player holds it too. The chapter's turn (promoted by the attack: the panel starts pointing at its maker here) | v1.2, 56 | Violet's expertise; Ally's private juxtaposition exists but is never aired and never confirmed; never the word Brad |
| 06 | R3 | 4 | 20 | (t)[CF] The brush: sable on a handle cut from a broken oar, green tape at the ferrule, hers beyond mistake from across a road. What her hand looked like in the world. Quick win | v1.2 | The brush as texture (Fable's seed, kept as texture per the spine); never a clue |
| 07 | A4, F3, R3 | 16 | 70 | (t)[ET] Del asks: hold one detail back, so claims can be tested. She does not say why she asks | C7, v1.0 D4 | Del's request and her silence about her reasons; never her private hypothesis |
| 08 | R5, A5, F5, F4 | 56 | 240 | (t)[ET] The description assembled: one of a kind, and about to be spoken into every kitchen in the country, including one Ally cannot know about. Airing it is the only way to find it, and airing it warns whoever holds it. Consequential action, not a belief turn (the attack demoted it, correctly); the chapter's belief work was done at p05. **D4 resolves here. Climax wall; +2 Platinum Ingots** | 56 | The full description as assembled; Del's request; that Ally airs knowing the holder will hear; never whose mark, what it proves, who hears |
| 09 | A5, D4, R4 | 32 | 135 | (t)[AL] Ep 9 airs the description (per D4). The player leaves the app having said it out loud | A5 D325 | What aired per branch; never the burn (the player never sees it; Ally never learns it) |
| 10 | A4, R4, F3×2, D3 | 28 | 125 | (t)[AL] The night after: nothing left to do but wait; the tip line light burning in a dark studio | A5 | Ally's side only; nothing that happens 400 miles away exists in this game's render |

Parallel pairs: p02 ∥ p03 (both from p01; p04 requires both) and p05 ∥ p06 (both from p04; p07 requires both).

## Chapter 10 · The Man on the Wall · Days 326 to 333 · 10 pkgs · 32 cards · 225 T1eq · 975 CC

The letter, the wait, the arrest, the accounting. Strict chain: this week runs on rails and the fiction says so. Five-carders (p04, p08, p10) with quickies beside them (p03, p06). **D4's fork lives here (ruled 2026-09-01, attack findings 7 and 8): p08 to p10 exist in two mutually exclusive branch versions, flag-gated, equal mass, converging on the settled arrest and never on the record. The close's story shape inside each branch remains Stephen's F2 ruling.**

| # | Cards | T1eq | CC | Beat | Source | Fable |
|---|---|---|---|---|---|---|
| 01 | A5, F4 | 24 | 100 | ★TURN [ET] A private message, Day 326: a landlady four hundred miles away; a quiet lodger under another name whose habits she has heard from strangers on air for six weeks; a wall with a pale gap where a black and gold thing hung until Tuesday. An address. Never aired | 63 (D326) | Written last with the close (v1.0 rule kept): the final briefs are drafted only after every other package's prose is locked |
| 02 | A4, F3, D2 | 14 | 60 | (t)[ET] 07:00, Del: everything handed over whole, the download map beside it as corroboration. Del moves inside the day | 63, 61 | As p01 |
| 03 | D2 | 2 | 10 | (t)[AL] The wait, day one: Ally holds the heaviest thing she has ever held and says nothing to anyone. Quick win | A5 | As p01 |
| 04 | A5, A4, F4, R4, D3 | 44 | 185 | (t)[ET] Ep 10 built, five cards: an episode that must be indistinguishable from episode nine, because he listens. The player assembles a broadcast whose whole job is to be boring, and knows why | A5 D332 | As p01 |
| 05 | D4, A3, R3, D1×3 | 19 | 80 | (t)[CF] What gave him away was never evidence: keys moved twice, thank you like it cost him, up at five. Six weeks of habits, told by people who loved him, heard by a woman who dusts his room | 63, 55 | As p01 |
| 06 | A3 | 4 | 20 | (t)[AL] Ep 10 airs, Day 332. Quick win; silence holds | A5 D332 | As p01 |
| 07 | A5, R4, F3 | 28 | 120 | (t)[ET] The news, Day 333: arrested four hundred miles away, under a name that sat for three years in a missing-person file in Del's own precinct. Brad Collins, alive. Confirmation of p10_01, not a new belief (the attack demoted it, correctly); the week's dread pays off here without turning anything | public record D333 | As p01 |
| 08a | A4, F4, R4, D4, F3 | 36 | 150 | (t)[ET] **Aired branch** (`aq.fk.d4.mark_aired`): the sift, five cards. The mark went out on Ep 9 against Del's request, so every claim that followed is untestable; the accounting is done through a flood of false sightings, and the mark sits in the public record forever, where the next man who burns a life will read what gave this one away. The lasting exposure cost is on the board and in the close. And the skipper's box, kept three years in case anyone came asking, is finally asked for: a few photographs of the deckhand's, bland, pinned where anyone can study them (F1) | 57, 60, R6a, within consent | As p01 |
| 08h | F5, A4, R4, D3, F3 | 36 | 150 | (t)[ET] **Held branch** (`aq.fk.d4.mark_held`): the clean identification, five cards. The held detail let Del test the landlady's message against something nobody else could know; the accounting lays every tip in its true place through one verified chain, and the mark never enters the public record. And the skipper's box, kept three years in case anyone came asking, is finally asked for: a few photographs of the deckhand's, bland, pinned where anyone can study them (F1) | 57, 60, R6a, within consent | As p01 |
| 09a/09h | A4, D3, F3 | 16 | 70 | ★TURN [CF] Margo's re-reading, branch-flavoured (crowded record against clean record): the frightened dissenter becomes the one friend who interpreted the substitution correctly, and the player re-ranks everything she did through the middle chapters. The turn the census missed (promoted by the attack) | 59, A5 |; **and the notch lands (F1): playing the finale material for Gerald, he goes still at one of the deckhand's photographs, asks for it, asks for silence: not yet, love, a few things to check first. Ally: he hasn't kept anything from a case since he retired, and he retired the year Dad died** | 59, A5; F1 | As p01 |
| 10a | A5, R4, D4, F3, D2 | 38 | 180 | (t)[AL] **Aired branch close:** he is alive and what that un-writes; the survivors' war ends; the landlady is never named, and on this branch that anonymity is the one thing Ally managed to keep. The honest limit as ever. The notch's cost said plainly: the wall gap appeared the day after she aired the description; what she described, she erased (F1). Sign-off. **+3 Platinum Ingots, +20 energy.** Sets `fk.ep01.complete`; **no tail spawns (F2 ruled: spawns struck per the no-optionals ruling; the notch is the teaser)** | 63 held, public record | As p01 |
| 10h | A5, R4, D4, F3, D2 | 38 | 180 | (t)[AL] **Held branch close:** the same arrest, the same limit, and a record that holds only what was true; Tessa's close line differs per D3, and the mark dies with the case file. The notch's cost said plainly here too: what she described, she erased (F1). Sign-off, grants and flag identical to 10a; no tail spawns (F2) | 63 held, public record | As p01 |

---

# C. THE DECISIONS

The four decisions of v1.0, re-placed at package boundaries. Their class analysis stands unchanged and is not restated in full here (v1.0 Part C is the reference); what follows is the placement, the axis, and the standing caveat.

**The limitation, re-drawn at v2.1 (attack findings 7 and 8, Stephen-ruled 2026-09-01):** the spine's clock and arrest are fixed and no decision moves who is arrested or when. v2.0's four decisions all shared one package graph, which failed the agency gate: zero decisions changed play. v2.1's repair, adopted as GPT specified: **D3 and D4 are mechanically divergent** (mutually exclusive branch packages of equal mass: p07_01/02 and p10_08/09/10, different requirements, different beats, different record left behind), and **D1 and D2 are lower-order** with their promised consequences now mapped to named rows (p03_10's garbled leak; p04_10's silence and p07_03's return). Both forks converge on the settled arrest; what never converges is the player's route and the human record left behind.

| D | Package · Day | The choice | Class | Branch flags |
|---|---|---|---|---|
| D1 | `fk_p02_03` · ~272 | Ruby's letter read whole on air, or only what Ally can verify | B, lower-order | `aq.fk.d1.aired` / `aq.fk.d1.held` · held branch: the garbled leak and Ruby's cost land at **p03_10** (mapped row) |
| D2 | `fk_p04_09` · 290 | The tip to Del raw, or the sheds on tape first then both handed over | A, lower-order | `aq.fk.d2.direct` / `aq.fk.d2.market` · market branch: Liam's silence starts at **p04_10** and his two-line return lands at **p07_03**, Day 311 (mapped rows) |
| D3 | `fk_p06_06` · ~302 | Margo's words verbatim anonymised, or paraphrased with the channel shielded | **A, mechanically divergent** | `aq.fk.d3.verbatim` / `aq.fk.d3.paraphrase` gate **p07_01v/01p and p07_02v/02p** (different testimony, different spread, the shielding task; crack corroborated only on verbatim); tails at p09_03 and p10_10 stand |
| D4 | `fk_p09_08` · 325 | The mark aired, or held at Del's request | **A, mechanically divergent** | `aq.fk.d4.mark_aired` / `aq.fk.d4.mark_held` gate **p10_08a/08h, 09a/09h, 10a/10h** (the sift against the clean identification; different requirements and payoff art; the record differs forever) |

Placement checks, v2.1: one live decision at a time; D1's consequence lands at p03_10 and D2's at p04_10/p07_03 (mapped rows, no longer unplaced); D3's fork resolves inside chapter 7 (rejoin at p07_03) with perceivable tails at p09_03 and p10_10; D4's fork runs to the close and converges on the arrest. Branch pairs are mutually exclusive by `requiresFlag`/`forbidsFlag` and their union is the mainline, so exactly one version of each forked package is reachable in every state and no state starves. Branch masses are equal (16/16, 20/20, 36/36, 16/16, 38/38 T1eq), so the 1,600 envelope holds on every path. Clock check: no B6 day moves on any branch; the burn, the landlady, Del's 07:00 and the arrest hold in all sixteen reachable flag states. Authoring note: **100 packages played, 105 authored** (five branch pairs); the Fable brief and art bills count the authored number.

---

# D. CHAPTER 1, IN SECONDS

The cold open v0.4 (~470 words, ~3:20 at 140 wpm) spreads across chapter 1's ten packages. **Recut 2026-09-01 per Stephen's ruling and the attack's findings 5 and 6:** no words are rewritten and no sentences reordered; only the boundaries move. The accident ruling rides with the biography so no package is career texture alone; the keys arrive in the second cycle; the accusation is the payoff of p01_04's completion; p01_09's police result is spoken only after its board work.

**The ruled shape, boundaries pending Stephen's line-level sign-off (F4):**

| Pkg | v0.4 stretch | ~Words | ~Seconds | Note |
|---|---|---|---|---|
| p01_01 | "This is Echoes of Havenbay" to "…bottom of her stairs." | 28 | 12 | First tap at the segment's end |
| p01_02 | "Violet painted signs…" to "…nothing more." | 71 | 30 | Biography and ruling in one breath: the accepted account, never career texture alone |
| p01_03 | "But two weeks later…" to "…written in Violet's handwriting." | 28 | 12 | The keys are in play by ~0:50 |
| p01_04 | "Their names were…" to "…leave behind four keys." | 59 | 25 | The accusation lands on this completion: the turn |
| p01_05 | "She also told her friends…" to "…was the reason she had become afraid." | 33 | 14 | |
| p01_06 | "The four friends called each other…" to "…what that object was." | 91 | 39 | The pact, the Saturday, she knew: the second turn |
| p01_07 | "Then the group began to disappear." to "…three days after that." | 60 | 26 | |
| p01_08 | "That left Liam and Ruby." to "…I'm afraid of the other one." | 30 | 13 | |
| p01_09 | Board work first, then "A sergeant I trust…" to "…they could use." as the Del scene | 33 | 14 + scene | The result is heard only after fulfilment; Del interprets |
| p01_10 | "Now Liam and Ruby are accusing each other…" to the ruled sign-off | 100 | 43 | |

**First interaction: ruled 2026-09-01 at ~0:12 to 0:15** (the seeded pair pulses under p01_01's VO and the board accepts the tap at the segment's end). F3 is closed; I6's FTUE doc should be updated to record the ruling when the FTUE build locks.

**Timeline (draft):** 0:00 ident and the death line · ~0:13 first tap (one A-T1: the §5.7 choreographed deterministic merge) · ~0:45 the accepted account pinned · ~0:50 the keys land · ~1:20 the accusation (p01_04, the first turn) · ~2:10 she knew (p01_06, the second turn) · ~2:50 the disappearances and the walk-ins · ~3:30 Del on the steps after the chapter's heaviest board work (p01_09, the third turn, A-T4) · ~4:30 sign-off, Ep 1 publishes, chapter 1 ends. Chapter 1 net energy ≈ 23: comfortably inside the FTUE tank with the whole open played.

Mechanical duties (systems pass, not this document): p01_01 carries the FTUE entitlements and the lab grant exactly as `e1_tip` does; the junk-drawer generator arrives via p01_10's overflow; the diner is granted at chapter 1's close (first D card is p02_01); `aq.loc.rusty_anchor.active` is set at chapter 2's open (first R card is p02_06). Wired and verified in the editor, not asserted here.

---

# E. ECONOMY CONFORMANCE

### E1. The envelope, met exactly

| Ch | Pkgs | Cards | T1eq | v1.1 envelope | Net energy (flat 1.55) | CC | CC at 4.3 |
|---|---|---|---|---|---|---|---|
| 1 | 10 | 12 | 35 | 35 | ≈23 | 150 | 151 |
| 2 | 10 | 15 | 70 | 70 | ≈45 | 300 | 301 |
| 3 | 10 | 20 | 105 | 105 | ≈68 | 450 | 452 |
| 4 | 10 | 21 | 135 | 135 | ≈87 | 580 | 581 |
| 5 | 10 | 24 | 170 | 170 | ≈110 | 730 | 731 |
| 6 | 10 | 23 | 205 | 205 | ≈132 | 880 | 882 |
| 7 | 10 | 25 | 215 | 215 | ≈139 | 925 | 925 |
| 8 | 10 | 25 | 220 | 220 | ≈142 | 945 | 946 |
| 9 | 10 | 26 | 220 | 220 | ≈142 | 945 | 946 |
| 10 | 10 | 32 | 225 | 225 | ≈145 | 975 | 968 |
| **Total** | **100** | **223** | **1,600** | **1,600** | **≈1,032** | **6,880** | **6,880** |

Column notes (v2.1): the **CC at 4.3** column is **rounded per chapter and indicative**; the ruled totals reconcile exactly at 6,880 (the attack's 6,883 sum is the rounding artefact, now labelled). The **net energy (flat 1.55)** column is an indicative flat-yield figure superseded by E3's refined computation; it is kept only because the envelope was ruled against it. Cards are **played** cards; the D3/D4 branch pairs keep card counts equal per branch, so every path plays 223.

No chapter needed a budget different from the envelope: the variety the beats wanted fit inside it, so nothing was taken. Card count lands at 223 against the model's derived working figure of 250 (average 2.23 cards per package, 7.2 T1eq per card against the model's 2.5 and 6.4). The ruled numbers (100 packages, 1,600 T1eq) are met exactly; the card figure was always derived, and the difference is that this design prefers a slightly heavier single card over two trivial ones where the beat is one object (the letter, the slip, the screenshot). The attack confirmed no wall-class package exists (heaviest 56 T1eq against The Listener's 160-class) and named fk_p06_08 and fk_p09_08 as the concentrated-build tuning risks; both stay watched. If the tuning pass wants more board actions per package, the mechanical conversion is T4 → T3×2 and T5 → T4×2 at identical T1eq; it changes nothing in this document but the Cards column.

Rewards riders (draft): +20 energy at p05_10, p07_07, p10_10 (total +60); +2 Platinum Ingots at p09_08, +3 at p10_10 (total 5, matching the shipped episode). Toasts per §5.7 on every package of 3+ cards.

### E2. The variety evidence (the ruling's test)

Package sizes across the episode (played, v2.1): **27 one-card, 38 two-card, 26 three-card, 3 four-card, 6 five-card.** Within chapters, against a mechanical ramp:

- **Chapter 1 now mixes sizes** (attack finding 3's repair): two two-card packages (p01_04, p01_06) among the FTUE singles.
- **Chapter 2 carries the ruled early spike:** p02_06 (the boxes, 16 T1eq, three cards) sits beside 2 and 4 T1eq singles.
- **Chapters 7 to 10 keep quick wins beside the heavy builds:** p07_10 (one A-T4 card, a real quickie after finding 4's repair), p08_04 and p08_10 (single A-T3 quickies beside a 52 T1eq five-carder), p09_06 (single R-T3 beside the 56 T1eq climax), p10_03 and p10_06 (2 and 4 T1eq singles in the finale week).
- **No chapter is monotonic:** chapter 8 runs 24, 40, 12, 4, 24, 28, 12, 20, 52, 4; chapter 10 runs 24, 14, 2, 44, 19, 4, 28, 36, 16, 38.
- **Tier spread inside chapters:** chapter 10 spans T1 to T5; chapter 5 spans T2 to T5; the five-carders mix three or four families each.
- **The heaviest single package** (p09_08, 56 T1eq, three T5s and a T4) is the climax and is far below The Listener's 160/185-class walls: the mass lives in the spread, which is the model's whole point.

### E3. The tap and session arithmetic, done in one unit system (rebuilt at v2.1 per the attack's finding 2)

By family: **A 630 · F 377 · R 291 · D 302** (after four diegetic swaps that moved the boxes and the ladder work to the junk drawer). A podcast-and-documents episode is honestly A and F shaped, and the lab's tuned yield is the worst of the three generators.

**Gross taps, per family at the tuned yields, then overhead:** A+F 1,007 ÷ 1.3355 = 754 · R 291 ÷ 1.7427 = 167 · D 302 ÷ 1.6975 = 178 · subtotal 1,099 · ×1.10 waste overhead (the Schedule B baseline's factor) = **≈1,209 gross taps**. (The attack called v2.0's 1,210 arithmetically impossible against a 1,198 ceiling; the ceiling omitted the overhead factor, so the figure itself was right. What the attack correctly caught was the unit mixing below, which v2.0 did commit.)

**One unit system, as the baseline defines it:** a tap costs one energy, so gross taps = gross energy demand; net external energy = gross taps minus in-episode energy grants (the baseline's 233 taps and 213 net differ by the L5 +20 grant, the same rule). Grants here are +60 (E1's riders), so **net external energy ≈ 1,209 − 60 = 1,149 ≈ 11.5 free session-tanks** at the 100-energy tank.

**Sessions, stated honestly:** ~11 to 12 free sittings at this family mix, against the 8-to-10 guide; ads (+20 ×5/day) and the ladder narrow it for players who use them. Chapter shape at the refined figure (episode average ≈0.756 taps per T1eq): chapters 1 to 3 clear inside a tank; chapters 4 to 7 run 0.9 to 1.2 tanks; chapters 8 to 10 run 1.6 to 1.7, which is two sittings each in practice. **The levers, for the tuning pass, in preference order:** shift 100 to 150 T1eq of texture mass from A/F to R and D where the diegesis allows (the boxes, the harbour legwork, the door-knocking can all carry more), which brings net energy near 1,080 and the count to ~10.5; or retune the lab drop table; or accept 11 to 12 (the v1.1 ruling made 1,600 tunable once the beats existed, and this is that trade arriving with real numbers attached).

### E4. The beat census (relabelled at v2.1 per the attack's finding 1; v2.0's census was materially wrong)

| Payoff class | Count | Of which ★TURN |
|---|---|---|
| Evidence turn (ET) | 51 | 17 |
| Character fact (CF) | 20 | 1 (p10_09: Margo righted) |
| Ally line (AL) | 21 | 0 |
| Art with caption (AC) | 8 | 0 |
| **Total** | **100** | **18** |

The relabels, on the record: demoted to texture: p02_08 (provenance), p03_09 (attention), p09_08 (consequential action), p10_07 (confirmation); promoted to turns: p01_06 (she knew), p08_09 (stopped saying "was"), p09_05 (the trade juxtaposition), p10_09 (Margo righted); made real by the recut rather than relabelled: p01_04 (the accusation now lands on its completion) and p01_09 (board-first). Net count is still 18, which says v2.0 had the right amount of turn and the wrong addresses. Texture packages: 82, every one marked (t) in Part B. Every chapter carries at least one turn; chapters 2, 5, 7 and 9 carry exactly one. Character facts: 20, of which 14 are mined straight from the five v1.2 (the tenor voice, the piano, the county swimming, the 2014 kitchen, the 4 a.m. call, the tonic, the habits, the flask, the cheque, the receipts bag, ten minutes early, the sharpness, the brush, ranked handwriting) and the rest are Tessa, the deckhand and the clerk from the spine's own texture. New facts and routes: none touch the graph; all are batched in F5.

### E5. The three weakest beats, named honestly

1. **`fk_p08_04` (Ally files it).** The beat is the absence of understanding: she holds the skipper's letter and cannot read it. Inherited from v1.0's lead 14, which was the thinnest full lead; as a package it is one card and twenty seconds, which is the right size for it, but a player who does not feel the weight will read it as filler. Kept because the close's identification is unbuyable without the player having held this letter as unexplained cargo, exactly as Ally does.
2. **`fk_p05_05` (the Sundays).** The board work maps a silence: withdrawals, sharpened edges, Sundays spoken for. The belief change is real but negative-space shaped, and it is the largest texture package in its chapter (24 T1eq), which risks toll-booth reading on a beat about nothing visible. Mitigation: the payoff line is the chapter's thesis (a silence with a shape) and the very next package is the chapter's turn.
3. **`fk_p10_03` (the wait, day one).** One card, one line, pure pacing. It exists to put air between the handover and the built broadcast, because the fiction needs the week to feel held. If any package in the episode is cuttable, it is this one; it is also two T1eq, so it costs the player forty seconds. Named so the cut, if it comes, is deliberate.

Also watched, defended: p01_01 (orientation, not a turn; the FTUE's price, capped at one merge) and p10_04 (a package whose deliverable is a deliberately boring broadcast; the point of the week, and the riskiest ask of the player's trust in the set).

**Postscript at v2.1:** the attack judged this section's self-diagnosis "not honest enough": the three named weaknesses were expendable texture while the census itself misaddressed a third of its turns. Accepted; the census above is the attack-corrected one, and the standing lesson is that a census is a claim to verify, not a property of having written one.

### E6. Spine-day audit

Checked package by package against B6 and A5: p02_08 uses row 62 in its arrival week (Day 276, the tight joint v1.0 also carried) · p03_09 sits on Day 283 exactly · p04_09 on Day 290 · p05_06 on Day 297 (R15) · p06_01 on Day 300 (row 59) · p07_07 airs R3a on Day 311 · p08_01 reads the skipper's Day 311 letter after Ep 7 · p08_05 on Day 318 (row 60) · chapter 9 assembles sources dated 283 to 311 and airs Day 325 · p10_01 on Day 326 (row 63). Nothing aired precedes its A5 episode; everything held is shown held; nothing reaches the player that Ally never learns; the burn (Day 325 night) is never rendered and never known to Ally or the player. **Pass.**

Two private-inference items, watched and deliberately inside the line: p08_06 (the timetable reconstruction) and p09_05 (the trade juxtaposition) are Ally reasoning privately from lawful material, never aired and never confirmed by the game. Both are the class of thing v1.0's fk_bay3 already established (the player suspects and cannot say what of); neither surfaces a B6 fact early. Flagged for the attack rather than hidden.

---

# F. THE QUESTIONS FOR STEPHEN

All six ruled as of 2026-09-01: F1 the notch (the photographs and Gerald) · F2 the close (beats, image, notch-as-teaser, spawns struck) · F3 the first tap (12 to 15s) · F4 the cut (approved, provisional on implementation feel) · F5 the facts batch (all approved) · F6 the art list (closed with two constraints). Part F holds no open questions; the per-package briefs are unblocked.

### F1. The season notch: **RULED, Stephen, 2026-09-01**

Reached through a three-source blind batch (39 one-liners, `four-keys-notch-candidates-v1.md`) that Stephen rejected in favour of his own design, then sharpened together. **The whisper:** the deckhand's photographs, kept by the skipper in a box "in case anyone came asking", opened at the accounting when someone finally asks; Gerald goes still at one of them at the finale playback, asks for it and for silence: not yet, a few things to check first. Ally's anchor line dates his retirement to her father's death. Reaction became **action**: the player carries three questions out of the episode (what did he see, what will he do, why couldn't he say), and learns without being told that Gerald's own investigation never ended. **The cost:** what Ally described, she erased; the close says it plainly. **The retrospective payload** (the Thursday-run line, and whatever the frame holds) stays invisible in Ep1 and is committed at season planning, per §3.4's explicit-decision rule. Landed in p08_01 (the box line), p10_08a/h (the box opens), p10_09a/h (Gerald), p10_10a/h (the cost). ⚠ Standing follow-on for season planning: rebase the §3.3 ladder off The Listener, and decide what the photograph holds before Ep2's spine.

### F2. The close's shape, and the teaser: **RULED, Stephen, 2026-09-01**

**The beats:** the honest limit (the arrest is for the identity offences; Violet's staircase is unpromised, per evidential-language canon; caught is not answered for) · the survivors' release, not reconciliation: the war ends where the accusation was · Margo righted (p10_09's turn) · the landlady never named · the D4 branch cost and the F1 notch and cost woven through as already placed. **The final image: Liam and Ruby in the same harbour crowd.** Not together, not speaking; no longer standing apart. **The teaser: the notch IS the teaser.** Gerald leaving with a photograph and "a few things to check first" carries the next-episode pull; no constructed teaser beat exists. **The tail spawns are struck:** `cold_case_a` and `ep2_teaser` do not spawn, consistent with the v1.1 no-optionals ruling; nothing plays between the close and Ep 2. Chapter 10's briefs can now be written (still last, per the blindness rule).

### F3. The first-interaction number: **RULED, Stephen, 2026-09-01**

~0:12 to 0:15, the early tap, as part of the chapter 1 recut ruling. I6's band is superseded for this episode; the FTUE doc records it when the build locks. Closed.

### F4. The cold-open cut boundaries: **RULED, Stephen, 2026-09-01, provisional on implementation**

Part D's ten-way cut approved as tabled. Stephen's caveat, kept on the ruling: "we will not really know until it is implemented": the seams get a play-feel verification pass in the editor when the FTUE builds, and the cut may be re-seamed there without a new paper ruling (words still never rewritten without one). Chapter 1's dialogue assets assemble against these boundaries now.

### F5. The new-character-facts batch: **RULED, Stephen, 2026-09-01: all eight approved** (item 9 ruled earlier with F1). The briefs may cite every route below.

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
| 9 | p08_01, p10_08/09 | The skipper kept a small box of the deckhand's things left aboard, photographs among them, in case anyone came asking | Fact NEW; **RULED 2026-09-01 as part of F1** (spine ledger gains row 10f) |

### F6. Art-with-caption beats that are load-bearing: **RULED, Stephen, 2026-09-01: acknowledged and closed.** Nine pieces on the art bill as load-bearing, briefed against the render-language standard at production, with two hard constraints standing: the panel is never depicted anywhere including marketing, and one deckhand photograph is authored deliberately ambiguous.

Decorative AC beats are not listed; these carry story weight and their art fails the beat if it fails: the wake photograph (now inside p01_04's beat presentation, per the recut) · the key letter as object (p02_02) · the Regent marquee with the chained ladder (p03_04) · the slip boatsheds at 05:40 (p04_08) · the coast road drive (p06_02) · the side-by-side letter spread (p07_02, reused p07_09) · the witness-composite of the panel (p09_08 and p09_09: **text and sketch fragments only; no asset may depict the panel itself, in this episode or any marketing**) · the pale gap on the lodger's wall, if the close ever shows it (p10_01: recommend narration only, keeping the landlady's world unrendered) · **the deckhand's photographs (p10_08/09, F1, load-bearing): a few bland frames the player can study and see nothing; the art brief must author at least one frame with an unremarkable background element that can later become significant without repainting; what Gerald saw is committed at season planning, never in Ep1's assets; unlike the panel, these may be shown freely, their job is to give nothing.** Ten dedicated AC payoffs exist in total plus the photographs; the count is reported, not rationed, per the ruling.

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
8. **Ids and assets** as ruled in the header: `fk_p<ch>_<n>` packages, `fk_p<ch>_<n><letter>` cards, `Lead_FK_*` assets, `Assets/Content/FourKeys/`, database-decides-membership, slot ep01, no replay (R6), decision flags `aq.fk.d1..d4` never gate a shared mainline package; the D3/D4 branch pairs are flag-gated mutually exclusive package versions (`requiresFlag`/`forbidsFlag`, shipped primitives; v2.1) whose union is the mainline, so the container needs no new gate machinery for them either.
9. **Family availability:** lab (A, F) from p01_01; junk-drawer generator via p01_10 overflow; diner granted at chapter 1 close; `aq.loc.rusty_anchor.active` set at chapter 2 open. Mirrors the shipped `e1_tip` duties; wired in a systems pass with editor verification.
10. **Analytics:** package_complete events with chapter, package id, T1eq, session index; the §7.7 climax watch attaches to p09_08 and p10_04.

The systems bill named in the economy model v1.1 (leads-bar grouping, beat presentation surface, package gating, FTUE teaching, analytics) sits on top of `feature/multi-episode-audit`, which is still unmerged and not play-verified; nothing here changes that sequencing.
