# Episode 2 — The Ferryman

*Echoes of Havenbay · v5 (branch added) 2026-07-12 · conforms to episode-writers-prompt.md v2 + pilot-spec relaxations*

## Format canon & the clock

The tip line keeps voices, timestamps and nothing else — that's the promise, and this episode's opening honors it: Rosa's message is a voicemail in which she *chooses* to identify herself. The absolute clock: her message is timestamped **10:52 p.m., Sunday**; the episode opens Monday. Datelines: **D1 Mon** (L1) · **D2 Tue** (L2, 6 a.m.; L3, afternoon) · **D3 Wed** (L4) · **D4 Thu** (L5 milestone — that morning's crossing passes unobserved; nobody knows the schedule yet) · **D6 Sat** (L6) · **D7 Sun** (L7) · **D9 Tue** (L8) · **D10 Wed** (L9) · **D11 Thu, 4:40 a.m.** (L10 — the next Thursday crossing; the investigation has to *wait for the tide*) · **D12 Fri** (L11) · **D13 Sat** (L12).

**2006 anchors (locked):** Danny's last genuine logbook entry — Sunday, 8 October 2006; he vanishes that night. His launch is found circling under Kestrel Point on the morning of the 9th, motor run dry. The Gazette's unbylined "thief" brief runs on the 10th. The meet with the journalist was set for Thursday, 12 October. The file closes on the 14th — six days.

**The branch (L11):** the episode's one choice — protect Eddie first vs. force the match into daylight — surfaces after Del states Eddie's own wish. Flags: `aq.e2.eddie_protected` / `aq.e2.merrick_published`; case-summary stamps **WITNESS SHIELDED** / **TRUTH IN DAYLIGHT** (do not reuse Ep1's PUBLIC/PROTECTED labels). Paths converge at L11n6; one variant line at L12n1a/1b.

**Evidential language rule:** "staged" and "probable homicide" until Del's official intake (L11) earns the word; L12 may say "homicide investigation" because by then it is one. The vessel is a **harbour launch** — oars *and* an auxiliary outboard with a tiller; Danny rowed the crossings because quiet was the job; Merrick used the motor to stage it. Episode 1's quiet boats and this episode's route are **never claimed as one operation** — resonance only, connection reserved for an explicit season decision.

## Logline

In 2006, the night ferryman Danny Reyes rowed Havenbay harbour's last crossing. He never came back. The boat did. The file said accident, the town said thief, and his daughter spent twenty years knowing both were lies. Ally proves the accident was manufactured — and discovers the money route Danny died carrying still runs on the same tide, watched by someone who has been collecting her family for twenty years.

**The victim:** Danny Reyes — and Rosa Reyes, 39, who runs the Harbor Ward fuel dock and has kept her father's logbook since she was nineteen.

**The secret:** Danny was the harbour's bagman — "the ferryman" was a job before it was a name. Every Thursday for eleven years he took unbanked cash from Pier 13 across the water in a small harbour launch, rowing the reach even with a motor aboard, because quiet was the job. He carried the money; he never took it. In his final month he began keeping a parallel ledger because he wanted out — the same month dock foreman "Sailor" Merrick offered nineteen-year-old Rosa a job on the dock accounts. Danny was building a case and a door out for his daughter, and he was going to trade the ledger to a journalist. The journalist was Thomas Quinn. Danny disappeared on the Sunday, four days before the Thursday meet. Merrick (d. 2011) motored the launch out, staged it under Kestrel Point, and wrote Danny's final logbook entry himself — and a coin was left taped to the tiller. The route never stopped. Its current operator — the Ferryman — knows exactly who Ally is.

**The reveal ladder:**
1. The Gazette itself carried the "thief" smear — and its morgue holds a 2006 records-request packet (requester's name long redacted) containing the effects list the official file lost: *one coin, taped to the tiller* — the same worn brass as the coin from a Rivermouth phone cradle. *(L3)*
2. Priya's tide work proves the launch arrived under power and was left performing an accident — and the final logbook entry isn't Danny's hand. Staged. *(L4)*
3. Danny wasn't robbing the till — he *was* the till: the second ledger, cut from a fused cashbox behind the fuel-dock till, shows a fortune crossing every Thursday. In his last month he starts underlining — the month Merrick reached for Rosa. *(L7)*
4. The redacted requester, and the journalist Danny was building the case for: Thomas Quinn. The meet never happened. *(L8→L9)*
5. The Thursday crossings never stopped — and Eddie's own gate book puts Merrick's hand on Danny's forged goodbye. The Pier 13 drop is live *this Thursday*. *(L6, confirmed L10)*
6. The Ferryman leaves Ally an envelope: his coin, and her father's **original** 2006 press badge — kept for twenty years. THE ROUTE OUTLIVES ITS RIDERS. *(L10)*

---

## Lead table

| # | id | Type | Title | Day | Gates | Requirement theme | Band |
|---|---|---|---|---|---|---|---|
| 1 | f2_tip | Podcast | The Boat Came Back | D1 Mon | (episode start) | forensic tools — setting up the interview rig | Easy |
| 2 | f2_rosa | Interview | The Fuel Dock | D2 Tue, 6 a.m. | spawned by L1 | diner food — 6 a.m. coffee and pastries for Rosa | Easy |
| 3 | f2_gazette | Evidence | What the Gazette Printed | D2 Tue | spawned by L1 | press items — Arthur's morgue files | Standard |
| 4 | f2_tides | Data | Run the Tides | D3 Wed | L2 + L3 | forensic tools — Priya's charts and light table | Standard |
| 5 | f2_pod1 | Podcast | Episode Update: The Man Who Rowed | D4 Thu | L4 | press items — cutting the mid-season episode | Standard (milestone: +200 CaseCash, +20 energy) |
| 6 | f2_watchman | Interview | The Man Who Locked the Gates | D6 Sat | spawned by L5 | diner food — a bus-stop meeting, two coffees | Standard |
| 7 | f2_ledger | Money Trail | The Second Ledger | D7 Sun | spawned by L5 | garage tools — cutting a fused cashbox open at Malone's | Hard |
| 8 | f2_gerald | Discuss | Gerald's Booth | D9 Tue | spawned by L5 | harbour/bar items — the Rusty Anchor | Standard |
| 9 | f2_badge | Evidence | The Gate Pass | D10 Wed | L7 + L8 | fingerprint evidence — Thomas's lockbox papers | Hard |
| 10 | f2_pier13 | Location | Pier 13 | D11 Thu, 4:40 a.m. | L9 + L6 | harbour items — staking out the pylons | Very Hard (+2 ingots) |
| 11 | f2_delcruz | Interview · **BRANCH** | Make It Official | D12 Fri | L10 | forensic tools — the intake packet, indexed and bagged | Standard |
| 12 | f2_close | Podcast | The Ferryman, Part One | D13 Sat | L11 | ceremonial: one low-tier press item — "press Publish" | Requirement: Ceremonial · Reward band: Very Hard milestone (+500 CaseCash, +20 energy, +3 ingots) |

Card subtitles:
1. *A voicemail at 10:52 on a Sunday night. "You say goodnight to the harbour. It kept my father in 2006."*
2. *Rosa Reyes kept her father's logbook through the smear and the silence. She's ready to open it.*
3. *The Reyes file closed in six days, and the Gazette helped bury him. Arthur Finch owes this one.*
4. *Water moves the way it moves, whoever signs the paperwork. Priya Shah checks the tides against the story.*
5. *Four days on the water. Time to tell Havenbay what the logbook says.*
6. *Eddie Okafor locked the harbour gates for thirty years. He'll talk — two suburbs from the water.*
7. *A corroded cashbox behind the fuel-dock till, Danny's initials scratched underneath. He kept a second book.*
8. *Gerald's booth at the Anchor. He remembers the Reyes closure — and whose diary mentions a Thursday meet.*
9. *Dad's lockbox holds a Pier 13 gate pass for Thursday, 12 October 2006. It was never used.*
10. *Thursday, 4:40 a.m., Pier 13. If the route still runs, it runs tonight.*
11. *A probable homicide, a falsified file, a live operation. Interview room three — and one call that's Ally's to make.*
12. *Danny Reyes was not a thief. Say it on the record.*

**Portraits:** Ally, Gerald, Del Cruz, **Rosa Reyes (new — 1 of the 4-per-episode budget; voicemail + fuel dock)**. Arthur, Priya, Eddie and Mo are quoted in Ally's narration.

---

## Dialogue scripts

### L1 — f2_tip · "The Boat Came Back" (4 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Rosa (voicemail) | neutral | [steady — rehearsed for twenty years] My name is Rosa Reyes. You say goodnight to the harbour every week, like it's a friend. It kept my father. Danny Reyes rowed the last crossing for eleven years. One night he didn't come back. The boat did. |
| 2 | Ally | neutral | The tip line keeps voices, timestamps and nothing else — that's the promise. At 10:52 on Sunday night, Rosa Reyes chose to leave her name, the fuel dock, and one reason to come find her: the last entry in her father's logbook isn't his handwriting. |
| 3 | Ally | worried | Accident, the file said — closed in six days, with a rumour pinned to a dead man's coat: that he'd been skimming the till. The harbour believed it for twenty years. His daughter never did. |
| 4 | Ally | neutral | I've read a lot of last words. These were written by someone still breathing. This is Echoes of Havenbay. Episode two starts at the water. *(sets aq.lead.f2_tip.seen)* |

### L2 — f2_rosa · "The Fuel Dock" (4 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | neutral | The Harbor Ward fuel dock at six a.m. smells like diesel and old rain. Rosa Reyes has run it since she was nineteen — since the week the harbour decided her father was a thief who fell in the dark. |
| 2 | Rosa | neutral | He hated the water at night. Twenty years on the crossing and he never once swam. He kept oars on a boat with a motor, because quiet was the job. Ferrymen don't fall, Miss Quinn. They get put over the side. |
| 3 | Ally | worried | She gave me the logbook. Eleven years of crossings, one line each — weather, cargo, initials. The last line has no weather and no initials, and the ink skips like the pen was moving too fast. |
| 4 | Ally | neutral | Rosa kept this book through the smear, the silence, the sympathy casseroles. Twenty years is a long time to hold a door open. I intend to walk through it. |

### L3 — f2_gazette · "What the Gazette Printed" (4 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | neutral | Arthur Finch keeps the Gazette's memory in a basement he calls the morgue. He pulled the Reyes clippings before I finished asking. He'd been waiting for someone to ask for twenty years. |
| 2 | Ally | surprised | "We ran the thief story," he said, laying down the 2006 brief — four sentences, no byline, no source. "It arrived written. I was junior desk that week; I filed my objection, and they filed me. So I kept everything." |
| 3 | Ally | surprised | Everything includes a 2006 records-request packet — requester's name long since redacted — holding what the official file lost: the effects list from the launch. Rope, thermos, tackle box. And one item nobody explains: a single coin, taped to the tiller. |
| 4 | Ally | worried | Old brass, worn smooth — the same kind Del lifted from a phone cradle in Rivermouth. Files this clean don't happen by accident, and smears don't write themselves. Somebody built this forgetting — and twenty years on, somebody is still leaving the same coin. |

### L4 — f2_tides · "Run the Tides" (3 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | neutral | Dr. Priya Shah spread the 2006 tide tables across her light table like an accusation. Here's what a tide can't do: lie. Water moves the way it moves, whoever signs the paperwork. |
| 2 | Ally | surprised | "A drifting boat on that night's ebb ends up two miles south," she said. "Danny's launch was found circling under Kestrel Point the next morning, motor run dry. It didn't drift, Ally. It arrived under power — and was left performing an accident." |
| 3 | Ally | neutral | And the last line? Eleven years of Danny's open fours and flat sevens — the goodbye has neither, plus an abbreviation he never used once. Somebody wrote it for him. For now I'm writing "staged" where "murder" wants to go. |

### L5 — f2_pod1 · "Episode Update: The Man Who Rowed" (4 nodes; node 3 varies on the Episode 1 flag)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | neutral | [host register, steadier than she feels] This is Echoes of Havenbay. Four days on the water. Here's what we know. |
| 2 | Ally | neutral | Danny Reyes rowed the harbour's last crossing for eleven years, and one night in 2006 his boat came home without him. The file says accident. The tides say the boat was driven. The logbook says his last words belong to someone else's hand. |
| 3a | Ally | worried | (requires aq.e1.truth_public) Last case, I gave this city the whole sound and took the noise that came with it. This time, some things stay off the feed until the people attached to them are safe. A listener wrote that rule. I'm just reading it out. |
| 3b | Ally | worried | (requires aq.e1.truth_protected) Last case, I held the audio and let quiet do the work. I'm holding again: names, places, and everything a frightened witness owns stay off this feed until they're safe. A listener wrote that rule. I'm just reading it out. |
| 4 | Ally | neutral | Danny Reyes didn't fall. And the man who wrote his goodbye may have written more than one. Stay with me. *(narrative flag: f2.phase1.complete — fires on Proceed)* |

### L6 — f2_watchman · "The Man Who Locked the Gates" (5 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | neutral | Eddie Okafor locked the harbour gates every night for thirty years. He agreed to meet me at a bus stop two suburbs from the water. His choice. His route. |
| 2 | Ally | surprised | "Thursday nights the Pier 13 gate stayed open," he said. "Merrick's orders. Special crossing. No manifest, no lights — and you didn't look at the boat. You looked at your shoes." |
| 3 | Ally | worried | Sailor Merrick. Dock foreman, 2001 to 2011. Died with a marina berth he couldn't afford and a funeral nobody cried at. Eddie says Merrick took a boat out the night Danny vanished. He's been afraid for twenty years — not of Merrick. Of whoever Merrick answered to. |
| 4 | Ally | surprised | Eddie kept his own gate book — thirty years of Thursdays in his careful hand. Merrick's countersignatures are all closed fours and hooked sevens. Priya has both books now. The hand that signed the gates open wrote Danny's goodbye. |
| 5 | Ally | neutral | "The Thursday crossings didn't stop when Danny died," he told me — still whispering, in daylight. "They never stopped at all." Dot Ellis counted quiet boats from her window in Rivermouth. Different water, same discipline. This city has a habit. |

### L7 — f2_ledger · "The Second Ledger" (4 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | neutral | Rosa found it the week this show sent her back to the old dock office: a corroded cashbox behind the till housing, her father's initials scratched underneath, the lock fused to rust. We cut it open at Malone's. Inside: the second book. |
| 2 | Ally | surprised | Same eleven years, different truth. Amounts, dates, and five names that aren't names — Pilot, Chalk, Anchor, Ledger, and one that's just F. Cash out from Pier 13 on Thursdays. Cash back at dawn, lighter. |
| 3 | Ally | neutral | Run the totals and this harbour moved more money in a year than the port authority declared in five. None of it ever touched a bank. All of it crossed dark water in a rowing boat. |
| 4 | Ally | worried | Danny wasn't stealing from the till. Danny *was* the till. In his final month the entries change — he starts underlining. The month Merrick offered Rosa a job on the dock accounts. He wasn't just building a case. He was building Rosa a way out. |

### L8 — f2_gerald · "Gerald's Booth" (4 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Gerald | worried | Mo kept the booth clear and the coffee coming — she remembers Danny, you know. I remember the closure. Six days. The detective who signed it retired eighteen months later, boat and berth, the whole postcard. Nobody asks where a pension like that comes from. That's the point of it. |
| 2 | Ally | surprised | Gerald — Dad's 2006 diary. October. "D.R., last crossing, Thursday. He keeps a book." The meet never happened, did it. Danny disappeared on the Sunday, four days short. And the redacted name on Arthur's records request — that was Dad. |
| 3 | Gerald | sad | Your father carried that one to the end, love. He decided his phone was the leak, and he never wrote a source's real name again. All those initials in his files — Danny Reyes taught him that. Posthumously. |
| 4 | Ally | neutral | Danny was a ferryman; the Ferryman is whoever sent the boat. Twenty years, two Quinns, the same route. Whoever they are, they've been part of this family longer than I have. Time they met me properly. |

### L9 — f2_badge · "The Gate Pass" (3 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | neutral | Dad's lockbox again. I used to think it was a shrine. It's a filing cabinet with a guilt complex. |
| 2 | Ally | surprised | October 2006: a Pier 13 gate pass, visitor class, made out to T. Quinn for Thursday the twelfth. Never used. Under it, a note in his shorthand: "If D. doesn't show, the book goes to the water. Don't chase it. They count the ones who chase." |
| 3 | Ally | worried | He stopped chasing — for two years, until another story wouldn't let him sleep. That story waits. Tonight is Danny's. The gate pass names the drop, and the drop matches a line in the second ledger. If the Thursday crossings never stopped, neither did Pier 13. |

### L10 — f2_pier13 · "Pier 13" (5 nodes)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | worried | Thursday, 4:40 a.m. Pier 13, north shed — where a gate pass says my father should have stood. Del had a car three minutes out; Gerald had my live location and a deadline. Rosa's blessing rode shotgun. I have never felt less alone in an empty place. |
| 2 | Ally | surprised | At 5:02 a launch crossed with no lights. One figure, one crate, gone in ninety seconds. Twenty years after Danny Reyes went into the water, his route still runs on his tide. |
| 3 | Ally | surprised | The crate wasn't cargo. It was left facing my hide, lid loose, one envelope inside. My name on it — not "A. Quinn." *Ally.* The way a friend would write it. |
| 4 | Ally | worried | I photographed it where it sat, then opened it — it was addressed to me, and evidence was never its point. Inside: a coin like the one on Danny's tiller. My father's 2006 press badge — not a copy, the badge. And five words in block capitals: THE ROUTE OUTLIVES ITS RIDERS. |
| 5 | Ally | angry | [cold precision, not volume] You want me scared. Noted. But you kept his badge for twenty years — a souvenir, Ferryman, which makes this a collection. Collections get found. I'm coming for the rest of it. |

### L11 — f2_delcruz · "Make It Official" (Interview — THE BRANCH · 6 node positions)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1 | Ally | neutral | For once, Del Cruz didn't meet me on a bench. Harbor Ward precinct, interview room three, recorder running, door closed. Her ward. Her water. Her case number, by the end of it. |
| 2 | Del Cruz | neutral | "You're handing me a probable homicide with a falsified file, a dead facilitator, and a live operation," she said. "You know what that does to a career?" I said I didn't. She said, "Wakes it up. Mine's been asleep for years." |
| 3 | Ally | worried | Everything went in under its own name — Rosa's logbook, the second ledger, the tide report, the gate book. The live operation she can't touch without a taskforce, and taskforces leak. Then Del slid the last call across the table, and it wasn't hers to make alone. |
| 4 | Del Cruz | neutral | Eddie says use his name. I say move him first — once the match airs, anyone who remembers the gates knows where it came from. Publish now, and it can't be buried. Wait, and he can't be found. Your show. Your call. |
| — | CHOICE | — | ① **Protect Eddie first** — Del moves him before anything airs   ② **Force it into daylight** — publish Merrick's match now |
| 5a | Ally | neutral | (sets aq.e2.eddie_protected) We moved him first. By nightfall Eddie was somewhere the harbour couldn't guess, the gate book sealed under a case number. The match holds without spending his name — and it now lives inside the system Del says leaks. We chose the lock we don't trust. |
| 5b | Ally | neutral | (sets aq.e2.merrick_published) We published by evening: Merrick's hand on Danny's goodbye, letter shapes side by side. By nine the reopening was too loud to lose. The cost rode along — anyone who remembers the gates can guess my source, and now the Ferryman knows exactly what we hold. |
| 6 | Ally | neutral | (converge) Either way, Rosa gets a phone call this week that's twenty years late. It won't say "murder" yet. It will say "reopened." Some words weigh more in an ear that's waited that long. |

### L12 — f2_close · "The Ferryman, Part One" (5 node positions; node 1 varies on the L11 branch)

| # | Speaker | Emotion | Line |
|---|---|---|---|
| 1a | Ally | neutral | (requires aq.e2.eddie_protected) This is Echoes of Havenbay — and as of nine o'clock this morning, the Reyes crossing is open again. A homicide investigation. The gate-book match stays under seal until its witness is safe. The case reopened without spending his name. |
| 1b | Ally | neutral | (requires aq.e2.merrick_published) This is Echoes of Havenbay. We published Merrick's match before the file could disappear twice. By noon the city knew who forged Danny's goodbye — and as of nine this morning, the Reyes crossing is open again. A homicide investigation, too loud to lose. |
| 2 | Ally | neutral | Danny Reyes was not a thief. Say it at the fuel dock. Say it at Mo's. He carried criminal money for eleven years — carried it, never took it — and when he tried to put it down, the water was told to keep him. |
| 3 | Ally | sad | Rosa listened to the whole episode standing up, holding the logbook. When it ended, she wrote one line beneath her father's last one — in her own hand, with today's date. Ledgers should end honestly. Now this one does. |
| 4 | Ally | worried | The man who ordered that crossing is still out there, still counting. He kept my father's badge for twenty years, and gave it back to tell me he'd had it. He believes the route outlives its riders. |
| 5 | Ally | neutral | Maybe it does. But hear this, Havenbay: routes need water, and the tide is turning. I'm Ally Quinn. Episode three is already recording. *(narrative flag: f2.ep02.complete — fires on Proceed)* |

---

## Season-arc notch

This episode converts the Ferryman from a name into a present, aware antagonist — and makes it generational: he watched Thomas, now he watches Ally, and he wants her to know. New season facts: the money route is older than any corporate name the season will meet (the Voss Bio thread lands ~Episode 5, unnamed on air until then) and moves physically by water from Pier 13 on a fixed Thursday tide; the operation uses code names (Pilot, Chalk, Anchor, Ledger, F); the coin calling-card goes back at least to 2006 and is the same *type* as the Rivermouth coin (an evidentiary link between calling cards, not between operations); the Ferryman kept Thomas Quinn's original press badge for twenty years — he collects; Thomas lost a source in 2006 and never wrote a source's real name again. The L11 branch leaves season flags: `aq.e2.eddie_protected` (the evidence sleeps inside a system Del distrusts — a fuse for the Ep3 leak thread) or `aq.e2.merrick_published` (the Ferryman knows what Ally holds; Eddie is inferable). Deliberately unanswered: whether "F" is the Ferryman himself or his operator (a broker like Silas Vex fits the ledger's handwriting-free discipline); what the crossings carry now; where the route lands; whether the Rivermouth quiet boats and the Thursday crossings share water or masters — resonance planted at L6n5, the connection reserved for an explicit season decision; and — pointed at Episode 3 — why Del Cruz believes a taskforce would leak from inside Havenbay PD.

## The resolution beat

Rosa. Twenty years of holding a door open, resolved not by an arrest (her father's killer is dead) but by the truth said out loud and the case reopened — sealed by the image of her writing the logbook's final line herself, in her own hand, with the date. The ledger ends honestly. That's the episode's thesis in one physical action, and Ally never explains it.

## Canon usage note

**Canon used:** Pier 13 (the drop — its coin lore is the episode's spine), the Ferryman's coin (planted in the 2006 effects list, connected by type to Episode 1's Rivermouth coin — which never aired, so the link stays inside the investigation on either branch), Arthur Finch/the Gazette morgue (the smear ran in his own paper; he was junior desk, objected, was overruled, and kept the redacted records-request packet), Dr. Priya Shah (tides + questioned-document comparison against Danny's own entries and Eddie's gate book), Kestrel Point (where the launch was staged), Malone's garage (cutting the fused cashbox — the garage requirement made diegetic), the Rusty Anchor + Mo Callahan (Gerald's booth), Sgt. Del Cruz/Harbor Ward (formal intake, interview room three), Thomas Quinn's lockbox/shorthand/source discipline, the ~Ep5 Voss Bio thread (production note only — L9n3 keeps it unnamed on air), Dot Ellis (one resonance line, L6n5), Silas Vex (season hypothesis only), Episode 1's PUBLIC/PROTECTED flag (L5n3a/3b variant).
**Invented (episode-level only):** Danny and Rosa Reyes (victim and keeper — no canon role exists; Rosa takes one of the episode's four portrait slots), Eddie Okafor (witness — his gate book is the forgery match), Sailor Merrick (dead hand of the crime). Each fills a role canon deliberately leaves open per episode.

---

## v3 changes (2026-07-12 — season-order re-points, per bible Ch 6.2)

| Item | Resolution |
|---|---|
| 1 · L1 caller opener | "You found the ghost student" replaced with The Listener's goodnight-harbour ritual — the caller trusts the show because it listens, not because of a named prior case. |
| 2 · Voss Bio | Unnamed in dialogue (generalized); survives as a production note; the thread lands ~Ep5 (Ghost Student). |
| 3 · Aired-so-far audit | No other on-air references to prior episodes found. |
| 4 · Rivermouth resonance | One light connective line (quiet boats); villain identity deliberately NOT connected. |

## v4 changes (2026-07-12 — logic-and-structure pass; external review triaged against canon)

**Accepted and applied:**

| Item | Resolution |
|---|---|
| 1 · Tip-line canon | Live "line two" opening replaced with Rosa's voicemail (she chooses to identify herself); L1 retitled "The Boat Came Back"; timestamp 10:52 p.m. Sunday. |
| 2 · L5 leak | Second-ledger tease and attic-location leak removed; replaced with the branch-echo editorial-practice node (3a/3b on `aq.e1.truth_public/protected`). |
| 3 · Ledger discovery | Taped tin box → corroded cashbox behind the fuel-dock till, initials scratched beneath, lock fused; found because the investigation sent Rosa back; cut open at Malone's (garage requirement now diegetic). |
| 4 · Handwriting | Handedness inference replaced with feature comparison (open fours, flat sevens, an unused abbreviation) vs Danny's eleven years — and matched to Merrick via Eddie's gate book (new node, L6n4). |
| 5 · Vessel | Locked as a harbour launch: oars + auxiliary outboard + tiller; Danny rowed (quiet was the job), Merrick motored it to Kestrel Point; "motor run dry," "left performing an accident." |
| 6 · Absolute clock | Full dateline table (D1–D13) + locked 2006 anchors (last entry Sun 8 Oct; meet Thu 12 Oct; file closed 14 Oct — six days). The investigation now visibly *waits for Thursday's tide*. |
| 7 · Pier 13 competence | Del's car three minutes out, Gerald holding live location + deadline; photograph-first, and the evidence cost of opening acknowledged ("evidence was never its point"). |
| 8 · The badge | Photocopy → Thomas's **original** 2006 press badge; the "collection" deduction now valid; L10n5 and L12n4 rewritten around possession. |
| 9 · Coin bridge | Ep1 coin explicitly connected *by type* at L3n4 ("the same kind Del lifted from a phone cradle in Rivermouth") — private, so it works on either Ep1 branch; L10's coin is "a coin like the one on Danny's tiller," never "the same coin." |
| 10 · Official intake | L11 moved into Harbor Ward precinct, interview room three, recorder running; evidence entered under its own names; Eddie as protected witness; "officially anonymous, unofficially airtight" cut. |
| 11 · L12 economy | Requirement made ceremonial (one low-tier press item — "press Publish"), mirroring Episode 1; milestone rewards unchanged. |
| 12 · Rosa presence | Rosa speaks directly with portrait: L1 voicemail + L2n2 (one new portrait character, within the 4-per-episode budget). |
| 13 · Evidential language | "Staged"/"probable homicide" until L11's intake; L12's "homicide investigation" is now procedurally earned. |
| 14 · Title lands | "Danny was a ferryman; the Ferryman is whoever sent the boat" (L8n4). |
| 15 · Danny's motive & complexity | The underlining month = the month Merrick offered Rosa a dock-accounts job (L7n4); L12n2 keeps moral precision ("carried it, never took it"). |
| 16 · Arthur's guilt | Junior desk, filed his objection, was overruled, kept everything — including the redacted records-request packet (which quietly plants the L8 Thomas reveal). |
| 17 · Line notes | Logline gains "He never came back. The boat did."; "Season talk" → "That story waits"; "two Quinns in the same ledger" → "Twenty years, two Quinns, the same route"; L6 Dot line sharpened. |

**Rejected or modified (with reasons):**

| Suggestion | Ruling |
|---|---|
| "Dot's and Eddie's numbers agree" | Rejected as written — it evidentially merges the two operations, which canon reserves for an explicit season decision. Kept as resonance: "Different water, same discipline." |
| Rosa post-episode voicemail | Skipped — it would clone Episode 1's Dot-voicemail close into formula. Rosa's logbook line remains the ending image. |
| L12 should not be Very Hard | Half-adopted: the *requirement* is now ceremonial (Episode 1's own pattern); the Very Hard *band* stays because it labels the milestone reward tier, not the grind. |
| L4 as murder proof | Adopted in spirit: L4 now closes on "staged," and the word "homicide" is earned procedurally through L11's intake. |

## v5 changes (2026-07-12 — the branch; all three open decisions resolved, Stephen-approved)

| Item | Resolution |
|---|---|
| 1 · THE BRANCH (L11) | Candidate (b) adopted with the reviewer's refinement: the choice is about timing and pressure, and Eddie has agency ("Eddie says use his name. I say move him first"). ① Protect Eddie first (`aq.e2.eddie_protected`, stamp WITNESS SHIELDED) ② Force it into daylight (`aq.e2.merrick_published`, stamp TRUTH IN DAYLIGHT). Converges at L11n6; one variant line at L12n1a/1b. Balanced costs: shielded = the evidence sleeps in the system Del distrusts; daylight = Eddie inferable, the Ferryman learns what Ally holds. Ep1 labels deliberately not reused. |
| 2 · Rosa's portrait | APPROVED — canonical reusable character; prompt block added to portrait-prompts-canon-cast.md; expression priority for Ep2: neutral → sad (grief held for years) → angry (at the smear, restrained) → happy (restrained vindication); full 7-sheet later. |
| 3 · Art schedule | Press = P0 (gates L3/L5/L12 and recurs all season; family needs design + generation). Garage = P1 (tier art already exists in SAS/Item Icons — import + gap-fill only; no second family). Caveat logged: if Ep2 ships in the beta build these are pre-beta dependencies, not post-launch. |
| 4 · Metadata split | L12's table row now separates Requirement: Ceremonial from Reward band: Very Hard milestone; data schema should carry both fields (`RequirementDifficulty` / `RewardBand`) so nobody balances a grind onto the finale. |
| 5 · Production note | Two new case-summary stamp sprites needed: WITNESS SHIELDED / TRUTH IN DAYLIGHT (pattern: ui_stamp_public/protected in the art kit). Both L11 branch variants and both L12n1 variants are VO-recorded, per the Ep1 discipline. |

Remaining before lock: table read (performance/timing veto) → runtime pacing pass → numbered-revision passes → VO LOCK; cue sheet and actor sides generate from asset data at production.
