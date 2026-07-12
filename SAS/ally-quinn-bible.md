# Ally Quinn: True Crime Merge — Master Bible

*v2.0 · 2026-07-11 · the connected whole: world, season, cast, craft, systems-as-story, and production law.*
*Supersedes `Bible/Ally Quinn Bible.docx` (March scaffold). Built per `SAS/bible-source-dossier.md` (CHECKPOINT 1, all rulings incorporated).*

**How to use this document.** This bible is the narrative program's home truth — the one place where the world, the season arc, the cast, the episode craft, and the production rules connect. It does not duplicate its masters: dialogue truth lives in locked scripts, roster truth in `canon-character-roster.md`, appearance truth in `portrait-prompts-canon-cast.md`, systems truth in the SAS architecture docs and the economy sheet. Where this bible and a master disagree, the master wins and this bible gets corrected. Precedence for any conflict: **locked scripts → roster → this bible → prompts/kits → SAS systems docs → legacy sources** (full archaeology in Appendix 9.4).

**Contents**

1. Vision & Positioning
2. Havenbay
3. The Season Arc
4. Character Compendium
5. Episode Format & Writing Craft
6. Episode Guide
7. Game Systems as Narrative
8. Production Canon Rules
9. Appendices — glossary · flag registry · asset naming · source archaeology · Episode Seed Vault · go-to-market canon · Parked Ideas Vault

---

# Chapter 1 — Vision & Positioning

## 1.1 The high concept

**Merge clues. Solve cases. Expose the truth.**

*Ally Quinn: True Crime Merge* is a narrative-driven merge puzzle game for mobile. Step into the shoes of Ally Quinn, a true-crime podcaster and licensed private investigator. In the rain-soaked streets of Havenbay, every merge uncovers evidence, every clue unlocks a story, and every case brings you closer to exposing the conspiracy that took her father's life. Stylish noir visuals, addictive merge puzzles, and a binge-worthy mystery combine to deliver a detective game you can't put down.

The shorthand that sells it in a sentence: **"Merge Mansion meets True Detective."** The design thesis underneath it, unchanged since the earliest GDD: "By merging clue items, the player feels like they are actively conducting the investigation — uncovering evidence piece by piece… making merges feel like meaningful detective work rather than abstract puzzles." The game replaces the merge genre's renovation-and-restoration meta with **Merge to Solve**: every interaction on the board is a diegetic investigative action, and the story is not a wrapper around the mechanics — the story is the product.

And the frame that makes it singular: the whole game *is* Ally's podcast. "The game isn't just like a TV series; it **is** the production of a podcast series" — the player co-produces *Echoes of Havenbay* one lead at a time, and each solved case pays off as a finished episode.

## 1.2 The four pillars

Every design decision answers to these:

1. **Narrative Integrity.** The story is never decoration. Every item, requirement, and generator must be diegetically consistent with the current investigation — the player should never experience "narrative drift," where board actions feel disconnected from the stakes. Requirements read as investigation ("gathering forensic tools = processing a scene"), never as toll booths.
2. **Investigation Depth.** Depth is measured by deductive engagement, not merge count. Leads gate on other leads the way conclusions gate on premises; each phase ends on a recontextualizing reveal; the evidence board accumulates a case the player can *see* they built.
3. **Satisfying Tactility.** "The act of merging must feel intellectually and physically rewarding" — synchronized visual, audio, and haptic feedback, scaled to significance: muted clicks for routine merges, a thematic swell when a clue lands. Grounding "the satisfying click of a merge in the intellectual satisfaction of unmasking a killer."
4. **Emotional Honesty.** The genre pillar the project added to the merge formula: victims are people, not puzzles; every case has a keeper who carries it and gets an on-page resolution; truth is delivered even where justice can't be. This is what "out-writing everyone in the subgenre" means in practice.

## 1.3 Tone charter: noir-lite

Five tone commitments, quotable and binding:

- **Investigative, not brutal.** "The darkness is intellectual intrigue, not bloodstains."
- **Atmospheric noir-lite.** Foggy docks, whispered secrets, unsolved puzzles — "stylised to stay accessible and never oppressive."
- **Conversationally human.** Ally is "witty, sharp, and empathetic… a true crime podcaster who knows how to balance tension with humanity."
- **Empowering curiosity.** "Each combination is a reveal, a step deeper into the case… progress and discovery rather than grind."
- **Respectfully playful.** "The humour comes from human behaviour, not parody." A clue can hide in a grocery list or a city permit.

The canonical tone statement, verbatim: *"Ally Quinn: True Crime Merge delivers the intrigue and atmosphere of a noir-inspired true crime podcast — mysteries that feel authentic and worth solving — wrapped in a playful, bingeable merge puzzle experience. The tone is sharp, immersive, and human: mysterious enough to engage true crime fans, yet light and stylish enough to keep merge players comfortable and coming back."*

The register is grounded and contemporary — modern forensic vocabulary (digital footprints, cell-tower triangulation, chain of custody) over hard-boiled pastiche. No supernatural elements, ever; the ghosts of Havenbay are metaphorical and historical. Chapter 5 carries the full writing-craft law (rating boundary, anti-patterns, evidential language).

## 1.4 The audience

**The Investigative Casual.** Merge's player base runs 70–75% female, core ages 25–54, sessions of 15–25 minutes; true-crime podcast audiences run ~73% female and highly engaged — 34% of Americans 18–49 listen regularly, and they "crave a sense of participation." The overlap is this game's audience: players with a high need for cognition who prioritize intellectual closure and deductive satisfaction, who find the genre's cozy default (family dramas, farm restorations) "shallow or repetitive" and want grit, mystery, and stakes — without gore or camp. Roughly a quarter of merge MAU are "dolphins" — occasional $2–$10 spenders *if they feel respected*.

**Danielle H.**, the anchor persona: 34, digital marketing professional, Sydney, time-poor and media-savvy, earbuds always in. Crime Junkie and Casefile subscriber; watches *Only Murders in the Building*; plays Merge Mansion and Lily's Garden but finds them too cosy. She "plays to feel like she's part of a true-crime investigation." What she needs: 20–30 minute sessions without punishing downtime; storytelling more mature than the cozy default; progression without aggressive monetisation walls. What loses her: energy hard-gates that stall story for days; merge-as-housekeeping ("she feels clever for merging, not just tidy"); story behind spend — "she would feel betrayed if the story was locked behind aggressive spend." What she keeps: solved cases as trophies — the produced podcast episode she plays on her commute — and a suspect gallery worth screenshotting to her true-crime Facebook group. *(Source note: her age appears as 29 in the oldest GDD and 34 in later documents; 34 is adopted.)*

Her ideal journey, end to end: install off the "unlock the podcast episode" hook → a first five minutes where the tutorial *is* detective work → every merge chain produces something meaningful that visibly updates the case → the first solved case unlocks a produced episode → "play merges → influence investigation → unlock podcast → share & discuss → await next case."

## 1.5 The USPs

1. **The diegetic podcast.** Ally narrates everything; the UI is framed as her production dashboard; case rewards are finished episodes; the tip line is an in-world institution. No competitor has audio-native narrative as the actual product surface — and Ally's VO doubles, at zero extra cost, as ready-made short-form marketing (the podcast clips *are* the TikTok pipeline).
2. **The evidence web.** A persistent case board that fills as the player investigates — the proto-meta that grows (per the v1.2 roadmap) into pins-as-collectibles and board-completion rewards, always inside the fiction: it is Ally's wall, not a menu.
3. **Merge-as-investigation.** Requirement themes, item families, and generators are all places and practices in Havenbay (Chapter 7): the mechanics echo the fantasy.
4. **A strong female lead with multigenerational mentorship** — Ally and Gerald's relationship is the emotional spine no genre competitor carries.
5. **The season conspiracy.** A self-contained case per episode plus exactly one arc clue: the "Netflix drip" that converts casual players into an audience.

## 1.6 Competitive frame

- **vs. Merge Mansion (Metacore):** it pioneered the secretive-family-history hook, but its play is tethered to domestic renovation. AQ swaps "cleaning and fixing" for an active forensic loop — the motivation is unmasking a conspiracy, not restoring an estate.
- **vs. Gossip Harbor (Microfun):** the genre's biggest recent success ($115M+ monthly IAP by late 2025) and proof that *narrative-driven* merge is the growth story. Against its cozy soap-opera register, AQ positions psychological-thriller: desaturated palette, high-contrast light, a shadow-broker antagonist. We cannot out-liveops Microfun; we can out-write everyone in the subgenre.
- **vs. June's Journey (Wooga):** the cautionary comp — a mechanical disconnect between hidden-object play and narrative. AQ's answer is total integration: the merge is the investigation.
- **Positioning phrase:** the "advanced evolution" of the merge genre — the mature-investigative segment between Merge Dragons' fantasy abstraction and Merge Mansion's domesticity, currently underserved and validated by market data (merge revenue roughly quadrupled since 2022 on the strength of narrative-driven titles).

The honest strategic bottom line (market research, July 2026): v1 is **a quality-first niche entry, not a Gossip Harbor competitor at launch** — the correct strategy for a solo studio with no UA budget. Launch is validation and audience seeding: measure D1 against the ~25% genre benchmark, treat reviews and story-completion rate as the leading indicators, and let episodes be our "events" until real liveops exist. The differentiator is the writing; everything in this bible exists to protect it.

# Chapter 2 — Havenbay

## 2.1 The city

*"A rain-soaked coastal metropolis where ship horns cut through fog and neon. Havenbay looks like a refuge, but its bay shelters old money, new schemes, and secrets that wash ashore when the tide turns."*

Havenbay is a fictional, contemporary, mid-size coastal city — population ~480,000, "big enough to feel like a true city, small enough that 'everyone knows someone.'" It is grounded and modern: no supernatural elements, no period trappings. The mood is contemporary noir — warm lamplight against cold rain, hope under pressure. Weather does half the cinematography: frequent drizzle and sea fog, with sudden dramatic clears at golden hour. The palette: muted blues and greys, oxidized copper, wet asphalt, accented by sodium-amber, teal neon, and police blue. The soundscape: foghorns, gulls, trolley brakes, basement-bar jazz, scanner chatter.

The city's thesis is **vertical corruption**: wealth and power sit uphill (literally, in Highcliff), and the money that feeds them moves through the docks below. Working Havenbay — dockworkers, teachers, cops, reporters — is the city's heartbeat; above it, old shipping-and-real-estate money coexists uneasily with new players like Councillor Hart's redevelopment machine and the Voss Group's private-security empire; beneath it runs an underground of hustlers, brokers, and organized crime "tolerated so long as they don't interfere with 'respectable' interests." Public sentiment is cynical about official truth — which is why *Echoes of Havenbay* has cult appeal: it "says the things the papers won't."

**History in four beats:** (1) *Harbor origins* — a fishing town grown into shipyards on an immigrant backbone; (2) *the war boom* — the docks expand, the unions gain leverage and enemies; (3) *bust and "renewal"* — deindustrialization, blight, and aggressive redevelopment whose paperwork is where half the bodies are buried; (4) *the quiet scandal* — thirteen years ago, journalist Thomas Quinn dies in a "rainy accident," his laptop missing; files scatter, whispers persist (Chapter 3).

## 2.2 The six districts

Each district is a playable hub with its own texture, landmarks, and case flavor.

1. **Old Docks & Harbor Ward** — warehouses, fish markets, union halls; grit, cash-in-hand deals, multigenerational loyalty. Landmarks: **Pier 13** (Ferryman lore — his coins are found near the pylons), **Kestrel Point Lighthouse** (fog, rendezvous), **The Rusty Anchor** (§2.4). Del Cruz's precinct; Alex Vega's home turf. The older dockhands know Gerald from thirty years of Rusty Anchor nights and know what kind of cop he was, and that street credit extends to Ally.
2. **Downtown Civic Row** — City Hall, the courts, "suits, donors, 'official statements.'" Landmarks: the **Hart Civic Center** (the Councillor's name on the skyline), **Havenbay City Hall**, and the **Havenbay Gazette** building — Arthur Finch's domain, "Havenbay's conscience," with the morgue (archive) in its basement. Benji Park grew up here.
3. **Stonebridge Viaduct** — the elevated **Bayline** rail, under-bridge camps, transit clang and whispered trades. Nova Skye's coded murals live on these pillars; Bayline Central Station sits beneath the arches, its walls a message board for those who can read paint.
4. **Highcliff Heights** — old money, gated terraces, private security; "quiet streets, loud lawyers." Landmark: **Voss Group HQ**, a mirror-glass fortress with private cameras watching the public street. Elite prep academies; galleries that launder more than reputations.
5. **Rivermouth Industrial** — foundries, scrapyards, chop shops, freight spurs, decommissioned silos; "sparks at midnight, deals at dawn." Landmark: **Malone's garage** (Frankie's kingdom — part workshop, part safehouse). Episode 1 made Rivermouth's residential edge canon: **Chandler Road** (the school Dot cleaned for thirty years, and her cottage at №11 with the blue gate), the **swing-bridge** on the Rivermouth reach whose counterweight sings on a twenty-two-second cycle when boats run late, the allotments behind the cottages, and the one morning bus route out — through **Larkhill**, a quiet hill town (**Chapel Lane**, the church, **St. Brigid's Pharmacy**) where Dot's sister Vera keeps her cottage. Captain Drake was raised in Rivermouth.
6. **Havenbay University & Arts Quarter** — labs, student radio, community theater, indie press; "idealists, whistleblowers, late-night threads." The university is known for journalism and forensic sciences (Priya Shah's lab consults from here); its bureaucracy is the setting of the Ghost Student case (~Ep5).

**Street-name bank** (canon texture, use freely): Bayview Ave, Lantern Street, Mariner's Row, Foundry Lane, Orchard Steps, Harbor View Court, Wickham Pier, Kestrel Road, Stonebridge Way, Dock 9 Diner, The Blue Lantern Motel.

## 2.3 Institutions

- **Havenbay City Police Department (HCPD).** Captain Dana Drake runs the detective division as a strict institutional gatekeeper — careful, not corrupt, and aware that some of her colleagues haven't been. Harbor Ward precinct is Del Cruz's; Benji Park walks its beat. The department's texture: an old-boys' undercurrent, evidence that occasionally "goes under maintenance," and — pointed at Episode 3 — Del's stated belief that a taskforce would leak from inside. Priya Shah consults for the lab; Dr. Cortez keeps the morgue. Gerald served his forty years at **Brookford PD** — his old force in another city, the reason his HCPD influence is moral rather than institutional.
- **The Havenbay Gazette.** The paper of record, diminished but not dead. Thomas Quinn's employer; Arthur Finch's archive. Its conscience is real and so are its compromises — the 2006 Reyes "thief" smear ran in its own pages, unbylined, "arrived written" (Episode 2).
- **City Hall.** Councillor Evelyn Hart's stage. Redevelopment as public progress, graft as its private engine; records can go missing at the right word. The Founder's-Day-gala class of civic ritual masks the arrangements underneath.
- **The Voss Group.** Private security and "risk management" headquartered in Highcliff — corporate muscle with a philanthropy façade, run by Dante Voss with Damien Kroll as his field lieutenant. Where Hart launders intent into paperwork, Voss converts it into force.
- **Harbourline Security.** A dissolved (2011) port-security company whose assets refuse to die: its vans — governor retrofit from the '09 recall, plates hiding behind a shell contractor — are still driving in 2026 (Episode 1). Harbourline is the season's connective tissue at the street level: defunct on paper, operational in fog.
- **Havenbay University.** Journalism and forensic sciences; scholarship kids and legacy donors; the institutional-bureaucracy antagonist of the Ghost Student case.
- **Transport.** The Bayline elevated rail and streetcars; the Harbor Ferry Terminal (lockers, pylons, handoffs); Havenbay International Airport (private jets, quiet cargo); cab drivers who hear everything.

## 2.4 The Rusty Anchor

The bar is as much a character as any person in Ally's world. Founded **1924** by **Patrick Callahan** as *Callahan's Dockside* (a "soda shop" front through Prohibition); renamed **The Rusty Anchor** in the 1970s by his son **Seamus**, after the salvaged shipwreck anchor now bolted by the door under a battered green neon sign; inherited in the 2000s by Seamus's daughter **Moira "Mo" Callahan**, who keeps it what it has always been: **neutral ground**. Cops and crooks drink under the same roof; city officials slip in for a whisky away from prying eyes; violence goes outside, whoever you are — and because everyone trusts the rule, Mo hears everything.

Geography of the room: ~60 seats — twenty stools, eight booths, six tables — plus a rear courtyard with an acoustic stage. **Gerald's booth** (the corner booth, held for him without asking) is the series' warmest set and an unofficial confessional; Mo's locked upstairs office holds "ledgers, old matchbooks, and a shoebox of favors owed." House fare: shepherd's pie, fish and chips in newsprint, the chowder of the day, Callahan's Stout, Dockhand's Whiskey, and the signature **Anchor Drop** (dark rum, ginger beer, lime, bitters). Thomas Quinn met sources here; Gerald calls it his second home; the Rusty Anchor item family (Chapter 7) makes the player buy their rounds like everyone else.

## 2.5 Episode 1 geography (locked)

The Listener made a corridor of the city canon: **Rivermouth** (Chandler Road, the school bell at 8:30 and noon, the swing-bridge's 22-second counterweight note, the blue-gated cottage, Mrs. Vale's doorbell camera next door) → the river reach where the **quiet boats** run dark (low in the water going out, riding high coming back; cash mooring fees, no manifests) → the **allotments** and the first morning bus → **Larkhill** (Chapel Lane, the church, the hill fog, Vera's cottage, St. Brigid's Pharmacy). Scripts set on this ground must respect these facts; the swing-bridge note and the school bells are now diegetic timekeepers any future Rivermouth scene can reuse.

## 2.6 Culture & texture

Food: chowders, pier dumplings, 2 a.m. diners, lumpia stands, Irish pubs — and the Corner Diner, whose takeout boxes open Rivermouth's doors (Episode 1). Fashion: dock coats and flannel below, tailored trench-coat money above; noir silhouettes echo the genre on purpose. Sports: the **Havenbay Mariners** (baseball), the **Bay City Sharks** (hockey — games often end in bar fights), and amateur boxing under Frankie Malone's promotion. Art: "rebellion and code" — Nova's murals on the Viaduct, Highcliff galleries fronting quiet money. Tech: "a city caught between analog grit and digital intrusion" — half-working fingerprint scanners and sabotaged CCTV two streets from gleaming corporate data centers. Faith: churches as community anchors rather than political powers; a parish newsletter can still find a missing woman (Episode 1).

Havenbay's contrasts are the story engine: wealth against poverty, tradition against progress, public virtue against private vice. Its warmth is real — and so are the shadows it casts.

# Chapter 3 — The Season Arc

The season question: **who killed Thomas Quinn, and what conspiracy was he uncovering?** Every episode is a self-contained case plus exactly one new arc clue — one new fact, one new cost, never the full answer — until the finale unmasks the Ferryman and Councillor Hart live on the podcast. "The personal quest to solve her father's murder is the key that unlocks the city's deepest conspiracy."

## 3.1 The Thomas Quinn incident

Thirteen years ago — Ally was thirteen — investigative journalist Thomas Quinn, 43, died when his car left the road on a rainy night. Official ruling: accident. The hallmarks of a cover-up were all present and all deniable: a traffic camera "under maintenance"; witnesses gone unreliable or silent; a report filed with questionable timing; an early witness statement — describing **two vehicles stopped roadside near the crash site** — later omitted from the file; and his ever-present **laptop missing from the wreck** ("lost in the marsh," said the police). The Gazette ran a brief obituary and never printed his final story.

What he was chasing: a corruption web tying old-money redevelopment, missing public funds, and police involvement — the machine this bible calls the Ferryman network. Hours before he died he told a colleague he'd found something "explosive." He named no names. He'd learned not to.

**The season's timeline (canon, assembled from locked and draft scripts):**

- **2006 — the Reyes crossing.** Harbour bagman Danny Reyes, wanting out of the Thursday cash route he'd rowed from Pier 13 for eleven years, starts a second ledger and reaches out to a journalist: Thomas Quinn. Danny disappears four days before the meet; a coin is taped to his tiller; the Gazette prints an unbylined smear that "arrived written." Thomas keeps the unused Pier 13 gate pass and a note in his own shorthand: *"If D. doesn't show, the book goes to the water. Don't chase it. They count the ones who chase."* He never writes a source's real name again. (Episode 2, draft.)
- **~2008 — Voss Bio.** After two years of not chasing, Thomas investigates Voss Bio — the biometric-data thread that resurfaces in the Ghost Student case (~Ep5). (Pilot spec; Ghost Student design.)
- **2011 — housecleaning.** Harbourline Security is dissolved; dock foreman "Sailor" Merrick — the hand that faked Danny's last logbook entry — dies with a marina berth he couldn't afford. Paper dies; operations don't.
- **2013 — Day Zero.** Thomas is killed. The laptop vanishes; the witness statement evaporates; the file closes.
- **2013–2021 — the Erasure.** The systematic removal of Thomas's footprint. Gerald — who never accepted the ruling and ran his own quiet investigation — is pressured into retirement, told to "let it go," by an Internal Affairs process that was less about his conduct than his questions.
- **~2023 — the podcast.** Ally, now a licensed PI, starts *Echoes of Havenbay*. Three years of episodes build the audience — and the tip-line ritual — that Episode 1 inherits.
- **2026 — the Resurgence.** The Listener airs. A coin appears on a phone cradle in Rivermouth. The season begins.

Thomas persists in the story as evidence and absence: his files and notes (initials only, post-2006), the lockbox ("a filing cabinet with a guilt complex"), his fountain pen, the sepia photo in Ally's locket, the photocopied 2006 press badge the Ferryman returns to her like a taunt. The rule from the pilot spec stands: episodes touch Thomas lightly — his absence, his files existing, not their contents — until the season deliberately opens them.

## 3.2 The Ferryman network

The hierarchy, as the season gradually exposes it:

- **The Ferryman** (apex, identity locked until the finale). The shadow broker who makes problems disappear — "not just disappear physically, but erased from records, bank accounts, memory." Calling card: a coin. City folklore says the coins are tokens from Havenbay's defunct 1920s ferry line; the two seen on-page so far (Dot's phone cradle; Danny's tiller) are old brass, worn smooth, *placed*. The darkest version of the legend: a betrayer found near the harbour with two coins on their eyes — the ferryman's fare. Competing rumors about his origin, both alive in-world and neither confirmed: a 1980s dock enforcer who outlived his rivals; or a man who "once worked within the system" — a former cop or official turned rogue. What Episode 2 makes canon: he is generational (he watched Thomas, now he watches Ally, and he wants her to know), he is present-tense (the route still runs on its tide), and he is personal (*THE ROUTE OUTLIVES ITS RIDERS*). What remains deliberately open: whether the ledger's "F" is the Ferryman himself or his operator.
- **Councillor Evelyn Hart** (the public face). Reformer, redeveloper, patron of civic progress — and the network's political laundering layer: city funds redirected, legal troubles smoothed, zoning that coincidentally enriches her donors. The season's cruelest irony, held for the late game: **Hart was a patron of Thomas Quinn's early career — she funded his exposés.** His death was not her direct order; she looked the other way while the Ferryman handled it. Her menace is velvet: "We wouldn't want you to end up like dear Thomas, would we?"
- **Dante Voss** (the muscle made corporate). Ex-Special Forces; the Voss Group sells "risk management" to the city's respectable class and enforcement to its hidden one. Voss is not the puppet master — he is the face of Havenbay's *new* corruption, violence with a business plan. His exposure arc runs through Voss Bio (~Ep5).
- **Damien Kroll** (the field lieutenant). Voss's deniable operator — an ex-special-forces contractor who ran his own outfit and has worked both legitimate and shady clients. Kroll is the antagonist the episodes can *defeat without spending Voss*: when a case needs a professional with a face, a warrant-proof employer, and a personal code that stops short of children, it needs Kroll. (Ruling 2026-07-11: revived to the principal roster precisely for this function.)
- **Silas Vex** (the market). The information broker who serves the network without belonging to it — "pawn, partner, or just clever enough to survive near the fire." The standing season hypothesis (Episode 2's notch) is that the ledger's handwriting-free discipline fits a broker like Vex; unproven by design.
- **The operational layer.** Code names from Danny's second ledger — Pilot, Chalk, Anchor, Ledger, F — a fixed Thursday tide from Pier 13, cash that never touches banks. Alongside it, the season carries street-level threads that *look* like the network's fingers but are canonically unconnected to it: Harbourline's undead vans and Episode 1's man with the careful walk remain **unnamed, unseen, and distinct from the Ferryman unless a script explicitly connects them** (Chapter 3.4).

Nova Skye orbits the network from the other side — an ambiguous ally whose murals code warnings about it; Sabine Rourke drafts its contracts by day and leaks to Arthur Finch by night, loyalties deliberately unresolved.

## 3.3 The per-episode clue ladder

The delivery mechanism is fixed: each episode's case is solved honestly and completely, and the season advances by exactly one notch, usually in the final minutes, usually underplayed. The ladder so far:

1. **Ep1 — The Listener (locked):** the whisper. A coin, somewhere it shouldn't be — placed dead centre on Dot's phone cradle. Ally photographs it, Gerald goes silent ("Don't put that on the show, love"), and no one explains anything. Plus the street-level thread: a pale van tracing to dissolved Harbourline Security, and a stinger — somewhere, the episode playing; a click; a diesel starts. Someone now knows her name.
2. **Ep2 — The Ferryman (draft):** the detonation. The coin becomes a signature (2006, Danny's tiller), the route becomes live (Pier 13, Thursday, 4:40 a.m.), the antagonist becomes aware and generational (the envelope: his coin, Thomas's photocopied press badge, five block capitals). New facts: the route predates Voss Bio; code names; Thomas's 2006 source discipline. Left open: who "F" is; what the crossings carry now; why Del believes a taskforce would leak.
3. **Ep3 (open):** pointed by Ep2's notch — the leak inside Havenbay PD. (And the Ep2 stinger question: who else was listening?)
4. **~Ep5 — The Ghost Student (design doc):** the institutional turn. Voss Bio surfaces; the shell company in the scheme's money trail matches Thomas's old notes; Gerald's lockbox reveal — Thomas investigated Voss Bio in the 2000s. The conspiracy acquires a corporate name and a family connection.
5. **Mid-to-late season (shape):** the network's tiers come into focus — a red-herring antagonist is spent (the episode where Ally, raw and frustrated, tells her listeners she's "chasing a ghost"); Hart's philanthropy and Voss's contracts start sharing paperwork; the Kestrel-and-harbour geography accumulates weight.
6. **Finale (reserved):** Ally presents everything, live: the Ferryman unmasked, Hart exposed, Thomas vindicated — the podcast as courtroom. The Ferryman's face does not exist as an asset until this script locks (Chapter 8.5). The finale is never assigned to an external writer.

## 3.4 Rules for arc writing

- **One notch per episode.** One new fact, one new cost. An episode that advances the arc twice is spending the finale early; an episode that advances it zero times is filler. Both fail review.
- **The whisper lands unexplained.** Ally may notice; she may not understand. The player who continues is the one the whisper was for.
- **The season villains are never caught early**, never monologue, and never do the case-of-the-week's work. Hart and Voss appear in civic daylight or not at all; the Ferryman appears as effects, artifacts, and absences.
- **Costs compound.** Each notch should also cost Ally something irreversible — aired her own culpability (Ep1), accepted she is watched (Ep2), and onward. The arc is measured in what she can't take back.
- **Keep the street layer separate from the apex.** Harbourline vans, careful walkers, quiet boats — the network's fingers are episode material; its head is finale material. Scripts connect a finger to the head only by explicit season decision.
- **Thomas opens slowly.** His files are a well, not a faucet: one page, one initial, one artifact per withdrawal, each earned by the case at hand.

# Chapter 4 — Character Compendium

**The census: 20 principal/recurring + 3 Episode 1 characters = 23 narrative characters, plus 2 historical figures and 1 unnamed Episode 1 antagonist.** The roster (`canon-character-roster.md`) is the single source of truth for who exists; this chapter is the deep file on each of them — biography (legacy lore migrated to canon names), relationship to Ally, voice, and appearance. Appearance status: **★ CANON** means an approved image exists and is binding; *proposed* means a prompt block exists and the first approved image will become canon (Chapter 8.2).

Format note on voice: quoted lines in these entries are register samples from the lore and scripts — how the character *sounds* — not locked dialogue unless attributed to a locked script.

---

## 4.1 The Quinns

### 1. Allison "Ally" Quinn — 26 · protagonist · ★ CANON (model sheets + emotion set v2, 2026-07-11)

Licensed private investigator and host of *Echoes of Havenbay*. Ally grew up in Havenbay, raised by her widowed mother Helen and her grandfather Gerald after her father's death when she was thirteen. Gerald's half-told stories of unsolved cases — and the staged "mystery scenarios" he built around the house for her — made an investigator of her before she knew the word; journalism school and a teenage true-crime blog made her a storyteller. Her breakout was real: a podcast season on a decade-old missing-person case produced a crowdsourced lead — an overlooked witness — that helped police make an arrest. She licensed as a PI under Gerald's coaching and built the show on a promise: the tip line "keeps voices, timestamps and nothing else."

Her ethics are the show's spine: she vets everything before airing it, holds back what could burn a witness, funnels crucial tips to police rather than broadcasting them — and, since Episode 1, follows the rule Dot wrote: a voice can tell her where to look without her telling everyone else where it lives. Her mantra, inherited from Gerald: *"Do the right thing, even if it's the hard thing."* Her flaw, named in the old design docs and dramatized in Episode 1: investigative obsession — tunnel vision that can put the story ahead of the people in it, which is exactly the mistake the fish-hook broadcast made and the season won't let her forget.

**Voice.** First-person podcast narration: sharp, dry, self-aware; compassionate but never sentimental; concrete sensory anchors over abstraction; she never states the theme out loud. Her anger is cold precision, never volume (one `angry` per episode, earned). "I've read a lot of last words. These were written by someone still breathing."

**Appearance (locked by model sheets).** Auburn/copper shoulder-length wavy hair swept right-to-left; large green-blue (teal) eyes; teal trench coat with wide lapels over a black collared shirt; the **silver locket** on a fine chain holding Thomas's photo; retro silver-and-black headphones worn around the neck; brown leather cross-body satchel of podcast gear. Signature props: vintage chrome broadcast microphone, handheld cassette recorder. The blue in noir shots is lighting, never dye (Chapter 8.3). The locket is her purpose-built vulnerability beat — the private crack in her competence, spent at most once per episode.

### 2. Thomas Quinn — 43, deceased · Ally's father · seed: the sepia photo in her locket

Investigative journalist at the Havenbay Gazette, later independent — union corruption, dockside smuggling, redevelopment graft. "Both wound and beacon": the reason Ally holds a microphone at the dark, and the season's central mystery (Chapter 3.1). Killed thirteen years ago in a staged accident; his laptop was never found. What survives of him is method and artifact: files annotated in initials only (a discipline Danny Reyes taught him posthumously), a battered messenger bag, the fountain pen Helen kept and Ally now carries, a shorthand only she and Gerald can read, the lockbox. Episodes touch him lightly — his absence, his files existing, not their contents — until the season deliberately opens the well. On-page he exists as the photo in the locket, "dark-haired, kind-eyed, 40s; indistinct but human."

### 3. Helen Quinn — 46 · Ally's mother · school principal · *proposed* (prompt block exists)

Principal of a Havenbay public high school and the family's moral anchor — the fight for underfunded schools is hers, daily and unglamorous. Widowed at thirty-three; she "prefers not to dwell on the painful past, yet quietly encourages Ally to pursue the truth in her own time." Her terror is specific: she has already lost one investigator she loved, and she watches her daughter walk the same road with the same stubbornness. She wears Thomas's wedding ring beside her own on a chain at her neck. Discuss-lead confidant (with Gerald); her item family — Helen's Gifts, notes to lockets — is Ally tending the relationship in play. Desk mug: "Because I Said So — Principal."

**Voice.** Composed warmth with worry living just underneath; says less than she means; asks the question Ally is avoiding.

**Appearance (proposed).** Clear family resemblance — auburn greying at the temples, cut to the jaw; slate-blue blazer over cream; the two rings on the chain.

### 4. Gerald Quinn — 74 · Ally's grandfather · retired detective · ★ CANON (in-game portrait set)

Forty years a detective at **Brookford PD** — another city's force, which is why his authority in Havenbay is moral rather than institutional. A reputation for integrity and tenacity; some of his darkest cases still unsolved; a Columbo-esque warmth that hides the sharpest ears in any room. He never accepted the ruling on Thomas's death, ran his own quiet investigation, and was pressured into retirement for it — told, officially and otherwise, to let it go. He didn't. He carries guilt older than that: the missed evidence, the witness signals overlooked, the cold cases a younger man filed and an old man still reads. His mentorship of Ally has an atonement engine underneath.

He trained her with puzzles and staged mysteries, taught her "what to write down, when to shut up, and how to spot a lie that looks like paperwork" — and insists she earn her own name rather than trade on his. He fears exactly one thing: that the case that took his son will take his granddaughter, which is why he steered her away from it for years and why his silences (a full minute over the photo of a coin) say more than his sentences.

**Voice (locked register).** Short sentences, regret without self-pity. Endearment: "love." "Loop the low note for me, love. Just that. Nothing else in my ears." His episode-1 beat — counting a bridge's breathing under his breath, pencil ticking the table — is the character in miniature: old knowledge, quietly lethal.

**Appearance (locked).** Teal/green textured suit, magenta shirt, swirling tie, round gold wire-rim glasses, and a neat white moustache (canon retcon with portrait set v2 — the approved image rules, per Ch 8.2). His corner booth at the Rusty Anchor is held without asking; the Anchor is his second home. The old fedora-and-waistcoat look is legacy art, superseded.

---

## 4.2 The allies

### 5. Arthur Finch — 62 · Gazette journalist/archivist · ★ CANON (emotion sheet 2026-07-11; tousled hair is canon)

Semi-retired senior investigative journalist, now the Gazette's archivist and Havenbay's memory. Decades of scoops and forced resignations behind him; publishes the occasional piece under pseudonyms; lives above a used bookstore. He keeps the paper's morgue in the basement — and, folded into it (ruling 2026-07-11), the city's forgotten files: the cold-case archive function is his, an encyclopedic memory that produces documents thought lost by everyone else, including at least one restricted file touching Thomas Quinn's death. He mentored Thomas; he ran the 2006 pages that smeared Danny Reyes and has owed the dead man ever since ("I called it an obituary for a man's name"). His cynicism is real and so is what's under it: he'd been waiting twenty years for someone to ask for those clippings.

**Relationship to Ally.** Foil and partner — old newsroom cynicism meeting new-media nerve, with genuine respect underneath and Thomas between them always. He needles ("Cute piece you did on that cold case — not exactly Pulitzer material, but you have fans, I hear"), he delivers, and when it matters: "Don't screw this up, kid."

**Voice.** Velvet over gravel; bone-dry; institutional memory speaking in citations.

**Appearance (locked).** Lean, slightly stooped; silver hair **tousled** (recorded canon deviation), short silver beard; half-moon reading glasses low on the nose; brown cardigan over a checked shirt; press lanyard from another decade; pencil behind the right ear; ink-smudged fingers. His face anchors the press item family artwork. Warm archive lighting.

### 6. Alexander "Alex" Vega — 29 · freelance hacker/digital investigator · *proposed* (prompt block exists)

Harbor Ward born, Puerto Rican descent; the grey zone between legality and crime, worked with unmatched skill and a strict personal code: **no jobs involving kids or trafficking, no hurting innocents, no selling out friends.** Expunged teenage arrest (quietly arranged, the story goes, by a retired detective doing a favor); a past that makes the shadows comfortable and disposability a private fear — "deep anxiety about being disposable in a city where information is currency." Met Ally over the encrypted remains of her father's surviving files — the laptop is gone, but what Thomas left behind still needed hands like Vega's. Has pulled Ally out of more than one cyber trap since. Wears USB drives like dog tags.

**Voice.** Smirking intelligence, three steps ahead; gum-snap casual about things that would get anyone else arrested ("Old habits"). *Open canon point: Vega's pronouns are deliberately unset — no script has featured Vega yet; the first that does decides, on purpose. Recorded in the roster (2026-07-12).*

**Appearance (proposed).** Wiry, quick, androgynous style: undercut dark hair, silver ear cuffs, faded hoodie under a beaten bomber jacket with enamel pins, fingerless gloves, one earbud always in.

### 7. Moira "Mo" Callahan — late 50s · owner of The Rusty Anchor · ★ CANON (portrait 2026-07-11)

Third-generation keeper of the family bar (§2.4) and of the neutral ground it stands for. Has known Gerald since his working days; helped look after Ally in the year after Thomas died — the back room, a cola, a jigsaw puzzle — and treats her as family still. Behind the no-nonsense front is the sharpest people-reader on the waterfront and a locked office of ledgers, matchbooks, and favors owed. She remembers everything and everyone: it was Mo who could put a name to three seconds of Dot's voice — "Dorothy Ellis. Cleaned Chandler Road school for thirty years. Tuesday quiz, table six — tell her it misses her."

**Relationship to Ally.** Informal protector, supplier of street-level truth, keeper of her confidences — and the one adult who'll tell her when she's being reckless: "Baby, you're chasing a storm in a raincoat full of holes. Be careful."

**Voice.** Warm, unhurried, terminally unimpressed; endearments as punctuation ("hon," "sugar"); a bar towel's worth of wisdom per line. "I'll tell you, honey, that man's lying so bad I can see his nose growing from here."

**Appearance (locked).** Sturdy build, strong forearms; silver-streaked copper hair pinned up loosely (streak pattern exact per approved portrait); dark green henley, sleeves pushed up, bar towel over one shoulder; small gold claddagh ring. **Her signature warm amber light is an approved exception to the global noir key.**

### 8. Nova Skye — ~late 20s · street artist/activist · coded-message informant · *proposed*

Havenbay's margins talking back: sprawling murals under the Stonebridge Viaduct that double as messages for those who can read them — warnings about gang weather, faces of the missing, a small "Q" when the message is for a Quinn. Connected in the art world and the underground both; moves through subcultures the way other people change coats. Ambiguous by design: she is an ally of the truth, not of Ally, and the distinction surfaces exactly when it's least convenient. Her legal name is **Naomi Lachance** — canon fact, deliberately unrevealed in-story; save the reveal for an authored beat.

**Voice.** Guarded amusement; speaks in weather metaphors ("storm clouds gathering east side"); never on the record. "She paints the truth."

**Appearance (proposed).** Lean, dressed for climbing: paint-flecked bomber over a hoodie, respirator around the neck, rattle-can caps on the pack strap; dark messy crop with a single bleached streak; paint under the nails, dried color on one cheekbone. Warm sodium-lamp underpass lighting.

### 9. Francis "Frankie" Malone — 38 · mechanic, boxing promoter, fixer · *proposed*

Runs Malone's garage on the Rivermouth edge — anything with an engine fixed, and for clients who can pay, "specialty" modifications no invoice describes. Promotes amateur boxing, teaches street kids to jab, wears a St. Christopher, and owes Ally permanently: her podcast cleared his sister's name, a debt he repays in intel, safe exits through the garage's back door, and flat refusal of money (donuts accepted). His Ferryman-network adjacency is real and murky — "both resource and risk; deals always carry strings" — which makes him the ally most likely to know something he shouldn't and least likely to say where he heard it. His skills have an unspent season use: vehicle forensics, and one particular thirteen-year-old crash.

**Voice.** Easy charm with an angle behind it; calls Ally **"Q."** "What trouble are we getting into today?"

**Appearance (proposed).** Stocky ex-boxer build gone slightly soft; twice-broken nose, scar through one eyebrow; navy coveralls tied off at the waist, grease-marked knuckles, old boxing-club tattoo; toothpick at the corner of a grin. Warm workshop light.

### 10. Dr. Priya Shah — late 30s · forensic scientist · *proposed* (prompt block exists)

Runs her lab — Havenbay University crime lab, consulting for HCPD — with precision, authority, and no patience for sloppy work. DNA, trace, toxicology, tides if the case demands it (Episode 2's tide-table work is hers): the discipline of turning traces into truths, and a refusal to massage results for anyone's narrative, Ally's included. Rarely leaves the lab; always in the field in her head; delivers breakthroughs with a specialist's leap ("This pollen grows in exactly one part of town"). Her standing caution to Ally: evidence can illuminate, but it can also endanger.

**Voice.** Quick intelligence with dry amusement; enjoys being asked properly. "Here's what a tide can't do: lie."

**Appearance (proposed).** Neat black braid over one shoulder; white lab coat open over a mustard turtleneck; blue nitrile gloves in the breast pocket; thin rectangular glasses.

### 11. Dr. Lionel Cortez — mid-50s · chief medical examiner · *proposed* (prompt block exists)

Cuban-American, widowed; has seen the worst of two cities' harms up close and answered with gentleness. Talks softly to his cases, apologizes for the intrusion, keeps classical music in the morgue and a church or a bar for afterwards. The morgue under Cortez is "archive and sanctuary: the last voice for those silenced." He never speculates — "I can confirm the cause of death was poison; who administered it, I'll leave to you investigators" — which is exactly why his rare raised eyebrow moves a case. Works hand in glove with Shah: her objectivity, his empathy, one formidable science team.

**Voice.** Gentle gravity; chooses every word as if the dead are listening, because to him they are.

**Appearance (proposed).** Broad, calm, deliberate; salt-and-pepper hair, trimmed grey moustache; muted teal scrubs under an open lab coat; reading glasses at the collar; heavy careful hands.

---

## 4.3 The police

### 12. Sgt. Delaney "Del" Cruz — 36 · Harbor Ward sergeant · ★ CANON (portrait v2 + emotion sheet 2026-07-11)

Over a decade in HCPD, sergeant by grit and intelligence, Filipino heritage, Harbor Ward to the bone. Her father died in an "accidental" warehouse fire that everyone on the docks understood was nothing of the kind — the private engine behind a career of bending rules for justice and paying for it in sleep. Her partnership with Ally is unofficial and mutually essential: Del converts podcast-grade findings into legally usable cases ("officially anonymous, unofficially airtight"), pulls what can be pulled, stalls what can be stalled, and draws the line out loud when Ally reaches it. She is the season's proof that not all of Havenbay's police are compromised — and, as of Episode 2's notch, the one who believes a taskforce would leak from inside her own department.

**Voice (locked register).** Level, economical, honest about the cost of things: "You know what that does to a career?" — "Wakes it up. Mine's been asleep for years." Her signature mode: "I can't officially tell you to do this, but if you do, be careful."

**Appearance (locked, v2).** Athletic, composed; black hair in a short low bun with loose strands; plainclothes charcoal jacket over navy; badge on a neck chain; Saint Michael medallion at the collar; **kestrel tattoo (hovering falcon, v2-corrected) on the right wrist** — Kestrel Point ties. Cool blue ambient light. Her `happy` is a restrained professional half-smile; her `angry` is controlled command presence.

### 13. Captain Dana Drake — early 50s · HCPD captain · *proposed*

Rivermouth-raised; refused the bribes that found everyone eventually; carries a cheek scar from a Harbor Ward bust where she saved two rookies. Runs the detective division by the book because she has buried colleagues who didn't — "she is not corrupt, she is careful," and she is haunted by the graft she cannot root out alone. With Ally she is the gate, not the wall: hates being blindsided by outside investigations, sees the value in the results, oscillates between obstacle and quiet enabler depending on how defensible Ally's involvement is — and holds a career's worth of soft spot for the Quinn name that she would deny under oath.

**Voice.** Measured institutional authority; gives nothing away; her concession is a door left unlocked, never an invitation. "Let us handle this from here."

**Appearance (proposed).** Tall, squared posture; immaculate dark uniform, captain's insignia; steel-grey precise short cut; pale level eyes; reading glasses folded in the breast pocket. Cool blue-grey precinct light.

### 14. Officer Benjamin "Benji" Park — 24 · rookie patrol officer · *proposed*

Korean heritage, Civic Row childhood, fresh out of the academy with his uniform slightly too crisp and three pens in the breast pocket. Idealism intact, enthusiasm occasionally faster than procedure ("We got the lab results this morning… oops"); courageous in the clutch — hands shaking, aim steady. He looks up to Del and to Ally both, and functions as the precinct's conscience-in-training: the one who asks, out loud, whether justice is possible inside the system. Everyone quietly hopes the city doesn't fix him. Carries his mother's silver bracelet; comic relief whose jokes land a beat behind everyone else's, to his delight.

**Voice.** Earnest, eager, procedural to a fault: "Uh, Sergeant — is it okay that she's…?"

**Appearance (proposed).** Boyish open face; black hair slightly too neat; radio clipped high. Cool precinct lighting softened with a hint of warmth — the last idealist.

---

## 4.4 The antagonists & the ambiguous

### 15. Silas Vex — indeterminate middle age · black-market information broker · *proposed*

Thrives in the city's underbelly dealing the only commodity that never devalues. Real surname unknown; impeccably, deliberately unremarkable; "could sell sand in the desert and secrets to a spy." He rarely lies outright — he tells truths warped by omission — and his relationship with Ally is transactional with a thin, real seam of respect: she negotiates without fear, and he finds that interesting. Every trade goes in the ledger of favors ("Consider us square — for now"). Gerald's verdict is the character in one line: **"the devil you shake hands with when you're drowning."** Season standing: the Ferryman's operator by hypothesis (Episode 2's notch), never yet by proof — pawn, partner, or just clever enough to survive near the fire.

**Voice.** Patient, amused, appraising; opens with what you came to ask before you ask it. "I hear you're looking for a missing girl. I might know something about that."

**Appearance (proposed; this prompt supersedes all older looks).** Slim, precise, expensively unremarkable: charcoal overcoat with a high collar, dark scarf, thin leather gloves he does not remove, silver signet ring worn over the glove. Cold and underlit — light from below, face half in shadow.

### 16. Sabine Rourke — mid-40s · investigative attorney & fixer · *proposed*

The law, negotiable. Drafts Councillor Hart's contracts; bulletproofs Voss Group operations; and leaks files to Arthur Finch; once quietly covered for Gerald. Every loyalty deliberately unresolved — "equal parts confidante and potential betrayer," either your best ally or opposing counsel, and you will not find out today. Her warnings to Ally are the genre's most honest: "righteous crusades often end in shallow graves." (Ruled 2026-07-11: no relation to the legacy bar-owner surname in the migration table — the echo is coincidence, not lineage.)

**Voice.** Courtroom half-smile in prose form — sympathy or strategy, indistinguishable by design.

**Appearance (proposed).** Impeccable dark tailoring, silk blouse, discreet expensive watch; auburn-brown controlled low twist; slim leather folio. **Split lighting: warm key one side, cold fill the other — her ambiguity rendered literally.**

### 17. Councillor Evelyn Hart — 55 · public reformer, covert power broker · *proposed*

The public face of "progress": redevelopment ribbon-cuttings, anti-crime initiatives, a practiced luminous smile that never quite reaches eyes doing arithmetic. Privately, the network's political layer — city funds redirected, legal troubles smoothed, zoning that enriches her donors — with mayoral ambitions and a genius for never letting the word "corruption" touch her directly. Her history with the Quinns is the season's buried knife: she patronized Thomas's early career, funded his exposés — and looked the other way while the Ferryman handled him. She has tried charm on Ally (a podcast grant, "a subtle bribe to play nice") and, when that failed, velvet threat: "Surely, dear, you wouldn't want to drag someone's good name through the mud without evidence." And once, with the mask fully off for exactly one sentence: "We wouldn't want you to end up like dear Thomas, would we?"

**Voice.** Gracious public warmth over private calculation; doublespeak as native tongue; self-justification as civic virtue ("for the greater good").

**Appearance (proposed).** Polished campaign-poster presence: jewel-tone skirt suit, pearl earrings (her grandmother's), flawless silver-blonde jaw-length hair, civic lapel pin. **Split lighting: podium-warm key, cold rim behind.**

### 18. Dante Voss — 52 · head of Voss Group private security · *proposed*

Ex-Special Forces; built the Voss Group into the city's private army — security, "risk management," and services no brochure lists. A predator in a suit who "sells safety while sowing fear": philanthropy façade in Highcliff daylight, enforcement contracts in the dark. He despises exactly one thing — uncontrolled truth — which makes Ally's show a line item on a threat assessment he has already priced. Not the puppet master: the *face* of Havenbay's new corruption, muscle made corporate, and the season's mid-game mountain (Voss Bio, ~Ep5). Violence with a business plan.

**Voice.** Unhurried threat assessment; never raises his voice because he has never needed to.

**Appearance (proposed).** Broad, hard, disciplined; midnight-blue suit fitted like armor, collar open, no tie; grey-flecked military crop; thin old scar at the temple; tactical-grade watch the only ornament. Cold corporate light, hard shadows.

### 19. The Ferryman — identity unknown · shadow broker · **NO PORTRAIT until the finale reveal**

The season's apex antagonist: the man who makes problems cross over — "not just disappear physically, but erased from records, bank accounts, memory." Full arc treatment in Chapter 3.2; the production rule in Chapter 8.5. What canon permits on-page: the coin, the envelope, block capitals, silhouette, hands, effects and absences. What canon forbids: a face, a name, a voice with a body attached. He killed Thomas Quinn; he has watched the Quinns for twenty years; he wants Ally to know it. "Never seen in daylight."

### 20. Damien Kroll — Voss Group field lieutenant · *proposed* (revived 2026-07-11)

Ex-special-forces contractor who built his own high-end security firm and a reputation for getting the job done regardless of moral implications — hired over the years by corporations and syndicates alike, both legitimate and shady clients. Now runs field operations for the Voss Group: the deniable layer between Voss's contracts and their consequences. Disciplined, calculating, physically formidable — "you won't catch him monologuing; more likely to silently move in for the chokehold" — with a professional's code rather than a conscience: no children, no public carnage; "not a psychopath, just amoral about his contracts." Climbs mountains at high altitude for fun, which tells you everything about his relationship to risk. Designed as a dark mirror to Ally: the same discipline, competence, and persistence, pointed by a paycheck instead of a principle. His function in the season: the security antagonist an episode can *defeat* — burned, arrested, outmaneuvered — without spending Voss himself.

**Voice.** Flat, economical, final. "Drop this case. You're out of your depth." / "Moral compasses don't pay the bills."

**Appearance (proposed prompt to be written on first story use).** Shaved head, tactical gear or black suit by assignment, earpiece; reads as the professional in the doorway you noticed one beat too late. Antagonist lighting: cold, underlit.

---

## 4.5 Episode 1 additions

### Dorothy "Dot" Ellis — 78 · retired school cleaner, Rivermouth · ★ CANON (portrait + emotion sheet 2026-07-11)

The listener. Cleaned Chandler Road school for thirty years; Tuesday quiz at the Anchor, table six; insomniac scientist of her own river — decades of nights at the same window, greeting what belonged and noting what didn't. For three years she left the last message after every episode of *Echoes of Havenbay*; when someone came into her house at 3:04 a.m., she opened the line and let the house speak — "braver in forty seconds than most of us manage in a year" — then saved herself: out through the allotments, first bus, phone off, protecting her sister Vera. Declined the interview afterwards: **"I'm not content, love. I'm a person."** Canon's standing reminder that victims are people, keepers can be their own heroes, and the show's power cuts both ways.

**Voice (locked register).** Warm, amused, unhurried; wry understatement as a way of life. Her `angry` is cold — dignified, never shouting: "You turned my window into a broadcast, Ally Quinn."

**Appearance (locked).** Small, wiry, upright and unfussed — capable, not frail; silver-white practical set; sharp grey-green eyes, crow's feet, dry amusement; reading glasses on a chain; moss-green cardigan, cream floral blouse, small gold scallop-shell brooch. Warm amber key. No headphones (Ally's prop), no teal (Ally's color); her palette is moss, cream, silver.

### Vera — Dot's sister, Larkhill · surname TBD · no portrait needed yet

The hill cottage on Chapel Lane; "The hills are quiet. You'd hate it. — V." A presence defined by economy: "Vera says hello, which for Vera is a parade." Kettle-watcher during apologies. If a future script needs her surname, it gets minted deliberately at that moment.

### Mrs. Vale — Dot's neighbour · non-speaking · no portrait

Holder of the spare key under Dot's own arrangement (curtains shut + can't raise her = use it); her doorbell camera's eleven seconds are the episode's forensic pivot. Canon shape: welfare-check witness, quoted through Ally, never staged.

---

## 4.6 Historical figures

- **Patrick Callahan** — Mo's grandfather; founded the dockside tavern in 1924 (*Callahan's Dockside*; a "soda shop" through Prohibition).
- **Seamus Callahan** — Mo's father; salvaged the shipwreck anchor and renamed the bar **The Rusty Anchor** in the 1970s.

## 4.7 The unnamed

**The man with the careful walk** — Episode 1's active threat. Male (established only at L7, by footage); tall; every second step lands harder; face angled off-lens "like a man with practice"; a lanyard that did the work. Drives the pale van with the dead left-rear lamp that traces to dissolved Harbourline Security. Twenty minutes inside Dot's house in daylight; back at 3:04 a.m. eight days later; last seen pulling away without lights on Chapel Lane; still at large, and — per the stinger — still listening. **Unnamed, unseen, and kept distinct from the Ferryman unless a script explicitly connects them.** He is deliberately not a roster character: he is a pending question.

---

## 4.8 Name-migration table

Stale names that must NOT resurface outside this table. (Full authority: the roster.)

| Role / lore | Oldest (Deep Lore bible) | v1.2/v1.3 era | ★ CANON |
|---|---|---|---|
| The city | Brookford / "Havenford" | Havenford | **Havenbay** (Brookford survives only as Gerald's old PD) |
| Bar owner | Margaret "Mags" O'Rourke, 61 | Maxine "Max" Carver, 61 | **Moira "Mo" Callahan**, late 50s |
| Police sergeant | Tahlia "Tally" Reed, 34 | Naomi Reed, 34 | **Sgt. Delaney "Del" Cruz**, 36 |
| Police captain | Nora Kincaid, 48 | Naomi Drake | **Captain Dana Drake**, early 50s |
| Hacker | Rae Bishop, 29 | — | **Alexander "Alex" Vega**, 29 |
| Ally's mother | (unnamed widow) | Elaine Quinn | **Helen Quinn**, 46 |
| Cold-case archivist | Lila Marrow, 46 | — | **folded into Arthur Finch** (2026-07-11) |
| Security contractor | Damien Kroll, 42 | Damien Kroll | **Damien Kroll** — name survives; role sharpened to Voss Group field lieutenant |
| Police liaison (proto-GDD) | Det. Marcus Hale | — | superseded (functions split: Del Cruz / Dana Drake) |
| Forensics (proto-GDD) | Dr. Emily Zhao | — | superseded by **Dr. Priya Shah** |
| The newspaper | Brookford Gazette / Havenford Herald | Havenford Gazette | **Havenbay Gazette** |

Also ruled out of canon (2026-07-11): all middle names sourced from no document ("Thomas Edward," "Arthur Elias," "Dante Aurelius," "Dana Elise," etc. — minted deliberately when a beat needs one, not before); Danielle H. (player persona), Marlow & Lena (old FTUE prototypes), and the agent-squad names — production artifacts, not in-world people; unnamed functional walk-ons (the mayor, the bus driver, the retired Harbourline mechanic, diner staff) — not roster characters unless a script names them.

## 4.9 Roster rules

1. A new character in a script is added to the roster at approval, marked *proposed* until a portrait is approved.
2. The first approved portrait is the canon appearance, recorded in the roster and `portrait-prompts-canon-cast.md`; its incidental details become facts.
3. Portrait production order (by first likely story use): Mo → Del → Arthur → Helen → Priya → Cortez → Vega (Mo, Del, Arthur, Dot done as of 2026-07-11).
4. External summaries — including AI-generated rosters — are audited against the roster; anything untraceable is invention until deliberately adopted.
5. The lighting grammar (Chapter 8.4) rides with every new portrait: civilians warm, cops cool, antagonists cold and underlit, the ambiguous split-lit.

# Chapter 5 — Episode Format & Writing Craft

Every episode of *Ally Quinn: True Crime Merge* is an episode of *Echoes of Havenbay* first. The player is not "playing levels with story attached" — they are producing a true-crime podcast with Ally, one lead at a time. This chapter is the writers' room: the format, the doctrine, and the red lines. It consolidates the Episode Writer's Prompt v2, the Pilot Spec v1 (2026-07-10), and the craft rulings minted while locking Episode 1. Where this chapter and a locked script disagree, the locked script wins and this chapter gets a pickup.

## 5.1 The podcast frame

The whole game is diegetic audio. Ally narrates every scene as first-person podcast narration or field recording; witnesses and sources exist as quotes inside her narration ("'He hated the water at night,' she told me"). This frame is the game's signature USP and also its art-budget shield: a character can be fully present in an episode without ever needing a portrait.

Rules of the frame:

- Ally's voice is the camera. Concrete sensory anchors over abstractions. She never states the theme out loud — the episode's meaning arrives through what she notices, not what she declares.
- The tip line is an institution: it "keeps voices, timestamps and nothing else. That's the promise" (Episode 1, locked). Scripts may build on this promise; no script may break it casually — it is load-bearing for how cases arrive.
- Podcast-type leads are the tent poles: L1 (the cold open), L5 (mid-season milestone), L12 (case close). All Podcast leads are voiced by Ally — write those nodes for the ear: rhythm, breath, no visual crutches.
- Episode 1 added the *stinger* as a format tool: a final wordless node after the close (this episode, playing. A click. Silence. A diesel engine starts). Use sparingly; it is the format's equivalent of a post-credits scene.

## 5.2 The 12-lead structure (hard engine constraint)

Exactly 12 leads in two phases, exactly 2 podcast milestones (L5, L12), parallel threads in both phases, one climax lead:

```
PHASE 1 (5 leads)
  L1  Tip (Podcast) ──spawns──> L2 and L3 (parallel)
  L2  Investigation A ─┐
  L3  Investigation B ─┴─gates──> L4 (Data/synthesis)
  L4 ──gates──> L5 Podcast milestone (recap + hook)

PHASE 2 (7 leads)
  L5 ──spawns──> L6, L7, L8 (three parallel threads)
  two of L6–L8 ──gates──> L9 (connection reveal)
  L9 (+ another) ──gates──> L10 Climax (usually Location)
  L10 ──gates──> L11 (consequence/authority)
  L11 ──gates──> L12 Podcast case close
```

Each phase must END on a recontextualizing reveal. Lead types: Podcast, Interview, Evidence, Data, Money Trail, Location, Discuss — every type appears at least once per episode; Discuss (Ally with Gerald or Helen ONLY, at most twice); external witnesses are always Interview. Post-MVP types held in reserve: Timeline, Alibi, Stakeout, Lab (see the Parked Ideas Vault).

Per lead: `lead_id` (snake_case) · title ≤ 32 characters, no puns · card subtitle ≤ 140 characters written like a case-file label · requirement theme (one line that makes diegetic sense: what the player gathers and why) · reward band (Easy / Standard / Hard / Very Hard; climax and finale are Very Hard).

Dialogue: 3–5 numbered nodes per lead, ~44–48 nodes per episode (one minted exception: the branch lead runs to 6 node positions, counting its variant pair — Episode 1's L9). Each node: speaker, emotion, line. Lines are 1–3 sentences, ≤ 45 words — mobile, tap-to-advance. Emotions: neutral, happy, sad, angry, surprised, worried, confused. `angry` at most once per episode and it must be earned; `happy` almost never (Dot's voicemails earned it twice in Episode 1 — that's the bar).

## 5.3 The greenlit relaxations (Pilot Spec, 2026-07-10)

Three constraints from the Writer's Prompt v2 were deliberately relaxed for the shipped package. Write FOR these:

1. **Portraits.** Episode witnesses may get faces and hold their own dialogue nodes with the full 7-emotion set. Budget: up to 4 new portrait characters per episode (art is AI-generated per character sheet). Ally and Gerald remain the anchors. Episode 1's portrait cast: Ally, Gerald, Dot, Del Cruz.
2. **One branch point per episode** — exactly one choice moment, placed in Phase 2, two options that differ in *approach* (compassion vs. pressure, publish vs. protect). Both paths reach the same case resolution but change one scene's content and one line in the finale. No branching plot topology. Episode 1 minted the pattern: PUBLIC TRUTH vs. PROTECTED TRUTH (`aq.e1.truth_public` / `aq.e1.truth_protected`), surfaced as two buttons at L9, converging at L9n6, paying variant nodes at L10 and L12.
3. **VO.** All Podcast-type leads are voiced. Both branch variants get recorded. Episode 1's unique spoken VO ≈ 515 words ≈ 4–4.5 minutes at true-crime pace.

## 5.4 The Pilot Doctrine

Written for Episode 1 but now the standing quality bar for any episode that must work for a cold audience (which is every episode a new player starts with). Every script must engineer these, in roughly this order:

1. **Dramatic question in ≤ 2 nodes.** The cold open poses a question the player cannot leave.
2. **Ally's warmth beat** — a concrete act of kindness that costs her something, performed not narrated, inside L1–L2.
3. **Ally's competence beat** — she does something investigatively brilliant on-page, inside L1–L2.
4. **The grievable victim** — built from ONE concrete humanizing detail, not biography. Attachment is the episode's OUTPUT, never its input: no script may rely on pre-existing love for any character.
5. **Every recurring character gets an engineered entrance** — Gerald's first scene must itself create the attachment any later scene spends.
6. **Jeopardy reaches Ally personally by the midpoint.**
7. **Vulnerability beat** — one private crack in Ally's competence (the locket is purpose-built), placed before the climax.
8. **Guided tour** — the case's first three leads teach the series engine under the story.
9. **Effort engine** — the keeper visibly benefits from the player's per-lead work. Players love who they labor for.
10. **Whiff of change + the whisper** — Ally ends the episode irreversibly altered; the season clue lands unexplained.

Additional pilot duties (episode-scale): hook inside 60 seconds — the hook is *lines*, not premise; jeopardy is present-tense (someone in danger *now*, no purely historical harm, no purely institutional villains — the antagonist is a person with a face, or a terrifying absence where a face should be); the merge requirements must feel like *doing the investigation*, never like a toll booth; the case completes with emotional honesty — truth delivered even where justice can't be.

## 5.5 Evidential language (the Episode 1 craft ruling)

The show never claims what the evidence hasn't yet proven — and the scripts enforce this at the line level. Episode 1's implementation is the reference: the intruder stays ungendered until the doorbell footage genders him at L7 ("the house does when they cross it" / "whoever was in her hall"); the strained-spring door stays unidentified until Dot's own account settles the geography at L11. This is not pedantry — it is Ally's professional ethic rendered as prose discipline, and it doubles as mystery craft: language that outruns the evidence spoils reveals.

Corollaries: relative time ("three nights ago") is used at most once per episode, then weekday/timestamp thereafter — episodes run on an absolute clock with datelines per lead. Chronology must be auditable (Episode 1's VO-lock conditions included a full inspector-chronology audit).

## 5.6 The keeper

Every episode needs one living character who has carried the case emotionally and gets an on-page resolution beat. The victim is a person, not a puzzle. Episode 1: Dot carried her own case (and Mrs. Vale held the key); Episode 2: Rosa Reyes, twenty years of holding a door open, resolved by writing the logbook's final line in her own hand. The resolution beat is the episode's thesis in one physical action — and Ally never explains it.

## 5.7 Toast pacing & merge integration

Hard leads (2-item requirements, long merges) carry progress toasts: first at ~30% of requirement, second at ~70%, completion scene at 100%. Toasts are authored narrative micro-beats, not system messages ("Mail: days deep" · "The kitchen: packed in the dark") — they keep the story breathing while the player grinds the board. The first merge goal of an episode may be choreographed into the cold open itself (Episode 1: "Set up the rig," a deterministic single merge inside L1, nodes 4–5 firing on completion).

Requirement themes must make diegetic sense per family: forensic tools = processing a scene; fingerprint evidence = documentation; harbour/bar items = the Anchor, the water, the drive; diner food = door-knocking supplies, a witness's coffee. (Garage and press families exist but are art-gated — avoid as primary requirements until their art ships.)

## 5.8 VO lock & pickup discipline

A script advances through versions (v1.1 → v1.2 → v1.2.1 → v1.3) to **VO LOCK**: external review lock granted, table read validates performance and timing only, and thereafter *any text alteration is a formal PICKUP logged against the master*. The VO cue sheet (speakers, direction, estimated durations, filenames — `vo_e1_l1n3_ally` pattern) is regenerated from asset data before recording; actor sides are generated from the cue sheet, stripped of implementation annotation. Once lines are recorded, changes require explicit pickup decisions with budget attached. Episode 1 — The Listener locked 11 July 2026 as VO LOCK v1.0; it is the process reference.

## 5.9 Tone red-lines

Noir-lite: grounded, melancholy, wry. **Never campy, never gory, never lectures.** Rating boundary (12+): missing persons, fraud, corruption, restrained murder — yes; graphic violence, sexual content, on-page harm to children — no. No supernatural elements, ever.

**Anti-patterns (instant rejection):** villain monologues; Ally naming her feelings; exposition dumps; coincidence solving the case; the season villains caught early; police doing Ally's work; characters acting stupid for plot; puns in titles; withholding the episode's own resolution; lines over ~45 words.

Canon-first rule: invent ONLY at the episode level — the case's victim, its one-off witnesses, its dead men. Every functional role canon already staffs must be cast from canon: archives → Arthur Finch; forensics → Dr. Priya Shah; autopsy → Dr. Cortez; police access → Sgt. Del Cruz; digital work → Alex Vega; bar scenes → Mo Callahan's Rusty Anchor. Prefer named canon landmarks. Signature motifs (the Ferryman's coin, Thomas's missing laptop, Ally's locket, Del's kestrel tattoo, Gerald's booth) are sacred — use them precisely or not at all.

## 5.10 The season notch

Each episode = a self-contained case + exactly one new clue advancing the Ferryman/Hart/Thomas thread — one new fact, one new cost, never the full answer. Every script ends with a written **season-arc notch**: what the episode adds, and what it deliberately leaves unanswered (the unanswered list is the hook inventory for future episodes). The quality bar, verbatim from the writers' prompt: "the mid-episode reveal should make a listener physically stop; the finale should resolve the case with emotional honesty while making the Ferryman feel closer and more personal. If it works as a synopsis but not as 44 spoken lines, rewrite the lines — the lines are the game."

# Chapter 6 — Episode Guide

The season ladder as it stands at bible v2.0. One locked episode, one draft, one design document holding a mid-season slot, a repeatable cold-case loop, and a vault of renamed prototype cases ready to be pitched. Canon status is marked on every entry; only Episode 1 is canon in full.

## 6.1 Episode 1 — "The Listener" ★ LOCKED (VO LOCK v1.0, 11 July 2026)

*"Her last message to the show has no words in it. Forty seconds of someone else in her house."*

**The case.** For three years, Dorothy "Dot" Ellis, 78, retired Chandler Road school cleaner from Rivermouth, left the last message after every episode of *Echoes of Havenbay* — goodnight wishes, gull parliaments, the quiet boats. At 3:04 a.m. Friday she left forty seconds of silence: a floorboard, a strained spring, a diesel idling outside. She opened the line and let the house speak for her. The message waited three days in Ally's queue.

**The engine underneath.** Dot's insomniac window-watching had made her a scientist of her river — and three weeks earlier Ally had aired her voicemail about "the pretty one": a boat running dark, two-note horn, a fish-hook mark on the funnel. The quiet boats are a smuggling route (low in the water going out, riding high coming back; mooring fees paid in cash, quarterly, by a company with no phone number). Ten days after the fish-hook aired, a fake gas inspector with a careful walk mapped Dot's window. Dot fled through the allotments to her sister Vera's cottage on Chapel Lane, Larkhill, phone off, calling only the tip line — "I knew you'd hear it. Eventually."

**Structure (locked lead table).** 12 leads, 6 datelines (Mon–Sat), absolute clock anchored to the 3:04 a.m. Friday message:

| # | id | Type | Title | Day | Band |
|---|---|---|---|---|---|
| 1 | e1_tip | Podcast · VO | The Tip Line | Mon | Easy · 20 |
| 2 | e1_forty | Evidence | The Forty Seconds | Mon | Easy · 20 |
| 3 | e1_archive | Data | Three Years of Goodnights | Mon eve | Standard · 50 |
| 4 | e1_bridge | Discuss (Gerald) | The Volume-Up | Tue | Standard · 50 |
| 5 | e1_pod1 | Podcast · VO | Case Alert: Dot Ellis | Tue | Milestone · 200 + 20 energy |
| 6 | e1_cottage | Evidence | The Cold Kettle | Wed | Hard · 95 · toasts ×2 |
| 7 | e1_inspector | Interview | The Man Who Came at Noon | Wed | Standard · 50 |
| 8 | e1_boats | Money Trail | The Quiet Boats | Wed | Hard · 95 · toasts ×2 |
| 9 | e1_delcruz | Interview · BRANCH | Off the Record | Thu | Standard · 50 |
| 10 | e1_trail | Data | Where Dot Went | Thu eve | Standard · 50 |
| 11 | e1_hills | Location · CLIMAX | The Hill Cottage | Fri | Very Hard · 185 + 2 ingots · toasts ×2 |
| 12 | e1_close | Podcast · VO | Goodnight, Harbour | Sat | Milestone · 500 + 20 energy + 3 ingots |

Portraits: Ally, Gerald, Dot, Del Cruz (Mo participates, quoted in L4, without dialogue nodes). The branch at L9: release the full recording (**PUBLIC TRUTH**, `aq.e1.truth_public` — the crowd names the engine in hours; two hundred false sightings; the van gone by dusk) vs. publish a transcript only (**PROTECTED TRUTH**, `aq.e1.truth_protected` — Del traces plate and VIN cleanly; "he still doesn't know what we hold"). Branch variants pay off at L10 and in one line of the finale (L12n2a/2b).

**Canon this episode minted** (now binding everywhere):
- The tip line's promise: voices, timestamps, nothing else.
- Ally's new broadcast rule, stated in the close: "a voice can tell us where to look without me telling everyone else where it lives. That's the new rule. Dot wrote it; I'm just reading it out."
- Rivermouth geography: Chandler Road (school + Dot's house at №11, blue gate), the swing-bridge whose counterweight sings on a 22-second cycle, the allotments, the one morning bus through Larkhill, Chapel Lane, St. Brigid's Pharmacy.
- **Harbourline Security** — dissolved 2011, its vans still driving (governor retrofit after the '09 recall; plate registered to a shell contractor, VIN tracing back to Harbourline). The pale van with the dead left-rear lamp remains at large; the man with the careful walk (every second step lands harder) remains unnamed, unseen, and *distinct from the Ferryman* unless a script explicitly connects them.
- The coin: old brass, worn smooth, placed dead centre on Dot's phone cradle where the phone should sleep. Gerald's reaction — a full minute of silence, then "Where was it," then "Don't put that on the show, love" — is the season arc's first on-page tremor.
- The stinger: somewhere, this episode playing; a click; silence; a diesel engine starts. The threat heard the show.
- Dot's exit line ethic: "I'm not content, love. I'm a person."

**Why this episode leads the season** (from the pitch process): the hook is audio-native (the game's USP *is* the case), the keeper is the victim and she saves herself — Ally's job is to *listen well enough to catch up* — and the jeopardy indicts the show itself (Ally aired the window; the season's theme of listening-as-power cuts both ways from the first hour).

## 6.2 Episode 2 — "The Ferryman" · DRAFT v5 (branch added 2026-07-12, not yet locked)

*In 2006, night ferryman Danny Reyes rowed the harbour's last crossing and never came back; the file said accident, the town said thief, and his daughter spent twenty years knowing both were lies.*

Full 12-lead draft exists and conforms to the writers' prompt v2. The case: Danny Reyes was the harbour's bagman — "the ferryman" was a job before it was a name — rowing unbanked cash from Pier 13 every Thursday for eleven years. He kept a second ledger because he wanted out; the journalist he was building the case for was Thomas Quinn; he disappeared four days before the meet. Dock foreman "Sailor" Merrick (d. 2011) faked the final logbook entry; a coin was taped to the tiller. The route never stopped — and at Pier 13 its current operator leaves Ally an envelope: his coin, a photocopy of her father's 2006 press badge, and five words: THE ROUTE OUTLIVES ITS RIDERS.

Season facts the draft establishes: the money route predates Voss Bio and moves physically by water from Pier 13 on a fixed tide; the operation uses code names (Pilot, Chalk, Anchor, Ledger, and one that's just F); the coin calling-card goes back at least to 2006; Thomas lost a source that year and never wrote a source's real name again — Danny Reyes taught him that, posthumously. Keeper: Rosa Reyes, who ends the episode writing the logbook's final line in her own hand.

**Re-points applied in v3 (2026-07-12).** The draft was written when "The Ghost Student" was Episode 1; its callbacks now point at The Listener. What changed, and what was deliberately preserved:
- L1's caller now opens on The Listener's goodnight-harbour ritual ("You say goodnight to the harbour… It kept my father") — the caller trusts the show because it listens, not because of a named prior case.
- L9n3 generalizes Voss Bio to "another story wouldn't let him sleep" — the two-year fact survives, the name stays off-air until ~Ep5.
- The aired-so-far audit found no other on-air references to prior episodes; the writers-prompt's aired-so-far list is updated to the new season order.
- The optional resonance was taken: one line at L6 (Dot's quiet boats — "Different water, same discipline") sews the two cases into one harbour; villain identity deliberately unconnected, reserved for an explicit season decision.

**v4 (external review triaged against canon, 2026-07-12)** hardened the episode's logic: Rosa opens by voicemail (tip-line canon) and speaks with a portrait; the second ledger comes from a fused cashbox cut open at Malone's; the forged entry is matched to Merrick through Eddie's gate book; the vessel is locked as a launch; a full D1–D13 dateline clock (the investigation waits for Thursday's tide) and 2006 anchors are locked; the envelope now holds Thomas's *original* press badge; the Ep1 coin is connected by type at L3; Del's intake is formal (interview room three, evidence under real names, Eddie protected); L12's requirement is ceremonial; and L5 echoes the Ep1 PUBLIC/PROTECTED flag via variant nodes. **v5 adds the branch (Stephen-approved):** at L11, after Del voices Eddie's own wish ("Eddie says use his name. I say move him first"), the player chooses **WITNESS SHIELDED** (`aq.e2.eddie_protected` — Eddie moved, the match sealed inside the system Del distrusts) or **TRUTH IN DAYLIGHT** (`aq.e2.merrick_published` — the reopening too loud to bury; Eddie inferable, the Ferryman aware). Paths converge; one variant line at L12n1. Rosa's portrait is approved (prompt block in the portrait doc); press icons are P0 and garage import P1 in the art schedule. Remaining before lock: table read → runtime pacing pass → revision passes → VO LOCK.

## 6.3 "The Ghost Student" · DESIGN DOC, slotted ~Episode 5

The original vertical-slice case, retired from the pilot slot by the 2026-07-10 replacement decision and preserved as a mid-season episode. Premise: Maya Chen, scholarship student, discovers Havenbay University runs a "ghost student" service — wealthy families pay to keep dropout children enrolled on paper, their academic activity carried by scholarship students. She sends one message ("They're selling us"), is intercepted, and vanishes while the university's records insist she attended a lecture this morning.

Season duties (as re-assigned by the pilot spec): introduces **Voss Bio** and the Gerald lockbox reveal — the shell company in the scheme's money trail matches Thomas Quinn's old notes; Gerald reveals Thomas investigated Voss Bio in the 2000s. These are now *mid-season* escalations, landing after the Ferryman has become personal (Ep2) and before the back half.

Production status: full 12-lead tree, FTUE cold-open concept, and branch design (`gs_other_ghost`: offer anonymity vs. go on record — going public tips off the administrator and hardens the confrontation) exist in the design doc. Needs: renumbering to its season slot, a rewrite pass to current craft standard (Pilot Doctrine, evidential language, datelines), character naming for Dr. [TBD] and Administrator [TBD], and the standard pitch-and-lock pipeline.

## 6.4 Cold-case repeatables

The between-episodes loop, shipped with Episode 1: short repeatable lead chains framed as Ally reopening Havenbay's minor cold files. Launch chain: **Storage Unit 44 → The Second Set → Last Orders → cycles back.** They do not increment episode progress (the 12/12 label), do not appear on the evidence board, and survive save/reload mid-chain. Narratively they are the show between seasons — smaller truths, kept warm. The v1.1 roadmap builds the weekly Cold Case streak event on this scaffold (see Chapter 9 roadmap notes).

Writing bar: cold cases are miniatures, not episodes — but they obey the same tone rules and the canon-first rule. They may seed future episode material (a name, a place, an unexplained detail) but must resolve their own question honestly.

## 6.5 Episode Seed Vault

Three complete prototype cases from the earliest story development, preserved with casts renamed to current canon. **Non-canon until produced** — premises, beats, and structures are raw material for future pitches, not aired history. The full renamed sketches live in Appendix 9.5; one-line index:

1. **"A Shadow in the Park"** — a ten-year-old park murder reopened; the blamed boyfriend was innocent; the grieving best friend did it and kept the victim's locket for a decade.
2. **"Fatal Facade"** — a Highcliff socialite shot in a staged burglary; the charming boyfriend is a renamed serial predator; two red-herring thieves turn out to be a terrified witness and a loyal assistant.
3. **"Vanishing Point"** — a twenty-year-old "abandonment" that was a pregnant woman's escape, a hidden second life, and a double murder under a freshly poured patio.

Alongside them, the three unproduced Episode 1 pitches (2026-07-10) are preserved in the Seed Vault's annex (Appendix 9.5) — "The Overnight Girl," "The Confession," and "Dead Air" — each pilot-shaped, each with a coin whisper already designed, each explicitly legal to franken-pick for parts.

## 6.6 Season shape (working)

Locked points on the ladder: Ep1 The Listener (aired, in-game) → Ep2 The Ferryman (the coin detonates; the antagonist becomes aware and generational) → ~Ep5 The Ghost Student (Voss Bio surfaces; Thomas's investigation named). The finale unmasks the Ferryman and Hart live on the podcast — that episode is reserved (never assigned to an external writer), and the Ferryman's face stays ungenerated until its reveal. Between locked points, episodes are pitched against the Pilot Doctrine and judged by the standing rubric (cold-open strength, present-tense jeopardy, emotional keeper, one-sentence clarity, canon nativeness, whisper elegance, branch quality, teachability). Every episode: one new season fact, one new cost, never the full answer.

# Chapter 7 — Game Systems as Narrative

This chapter explains the shipped systems *as storytelling instruments* — why each exists narratively and what fiction it carries. It deliberately does not duplicate specs: the SAS architecture chapters (Shared Kernel/Economy, CaseFlow, Leads, Merge Board UI, SaveLoad, Analytics — SAS Chapters 1–11) and `ally_quinn_economy_v0_2.xlsx` are the masters for numbers and implementation. Where a figure appears here, it is quoted from those masters as of July 2026; if they move, they win.

## 7.1 The loop, as fiction

Merge → leads → dialogue → flags → resolution. The player merges items on the board to satisfy lead requirements; activating a lead consumes the items, pays rewards, plays the scene, sets narrative flags, and spawns the next leads. The founding design intent (GDD, Aug 2025) still governs: "By merging clue items, the player feels like they are actively conducting the investigation — uncovering evidence piece by piece… making merges feel like meaningful detective work rather than abstract puzzles." The pilot spec sharpened it into a red line: requirements "must feel like *doing the investigation*… never like a toll booth."

The lead lifecycle is the story's clock: Blocked → Available → In Progress → Ready → Activated. Activated leads persist on the evidence board (cork-board view, red string, replayable scenes) — the case visibly accumulates. Merging is untimed and unfailable; the challenge is resource management, the mood is concentration, not reflexes. Chapter cliffs and gated leads do the retention work story-side; the energy system does it economy-side.

## 7.2 Requirement bands & the episode economy

Every lead carries a reward band — Easy / Standard / Hard / Very Hard — with CaseCash rewards drawn from banded ranges (Ep1 midpoints: 20 / 50 / 95 / 185) and episode-level multipliers scaling later episodes (Ep2 ×1.1 … Ep6 ×1.55). Milestones (L5, L12) add energy and Platinum Ingots on top: Episode 1 pays 200 CC + 20 energy at the mid-season milestone and 500 CC + 20 energy + 3 ingots at the close, with the climax adding 2 ingots. Very Hard leads carry a 0.5–1.0% rare Fast Track drop.

The validated Episode 1 arc: ≈144 T1-equivalent items across 12 leads, paying 1,365 CaseCash + 40 energy + 5 ingots total (golden path corrected to its lead table's sum, 2026-07-12). The tuning intent: a full FTUE energy tank (100) carries roughly through Phase 1; the pinch lands mid-Phase-2, where the energy-out popup funnel takes over. Bands are narrative pacing as much as economy: Easy leads are story beats you walk into; Very Hard leads (climax, finale) are earned. One schema rule minted by the Ep2 pass: requirement difficulty and reward band are **separate axes** (`RequirementDifficulty` / `RewardBand`) — a milestone finale pays Very Hard rewards over a *ceremonial* requirement (Ep1 and Ep2 both do), and nobody should balance a grind onto a finale by reading its reward tier.

## 7.3 Item families & their character ties

Seven item families exist, plus the currency display chains; four families shipped live in Wave 1 (33 items + 3 generators), two are art-gated, one is relationship-driven. Every family is diegetic — it belongs to a place or a person in Havenbay, which is why requirements can read as investigation:

| Family | Tiers | Narrative home |
|---|---|---|
| Forensic Tools | 5 (Cotton Swab → Complete Forensic Kit) | Ally's scene-work; Priya Shah's discipline. "Processing a scene." |
| Fingerprint Evidence | 6 (Partial Dusted Print → Database Match) | Documentation and proof; the lab pipeline. |
| Rusty Anchor | 10 (Shot Glass → 50-Year Scotch) | Mo's bar — buying rounds, holding booths, loosening tongues. The family hero (the 50-year scotch, wax seal, faded label) is practically a character. |
| Food Gifts / Corner Diner | 12 (Paper Cup → Lobster Champagne Banquet) | Door-knocking supplies, witness coffees, kindness as method. |
| Garage | 10 (Socket Wrench → the pimped ride) | Frankie Malone's world. Art-gated — avoid as a primary requirement until art ships. |
| Press Items | — | Arthur Finch's Gazette morgue (his face anchors the family artwork). Art-gated. |
| Helen's Gifts | 10 (Note → Locket) | The relationship family — Ally tending her mother. Its top tier being a locket is not an accident. |
| Currency/Cash | 5 (single note → pile of bundles) | CaseCash made visible on the board. |

Generators are places, not machines: the **Investigation Lab** (desktop forensic station), the **Corner Diner** takeout window, and the **Junk drawer** (worn wooden evidence drawer — granted in Episode 1 via L1's overflow as Ally literally clears space to work). The tier progression rule doubles as a readability rule: each tier reads visibly "more" than the last, so a player can rank two icons at a glance.

## 7.4 The evidence board

The cork board is the case's memory — activated leads pinned, clustered by phase, strings drawn, scenes replayable. Cold cases and teasers deliberately stay off it: the board is *this case*. Market research classifies it as the game's proto-meta (a persistent space that visibly fills), with a v1.2 roadmap to make it active — pins as collectibles, board-completion rewards — without ever betraying its fiction: it is Ally's wall, not a menu.

## 7.5 Energy & monetisation

Philosophy first (GDD, unchanged since Aug 2025 and validated by the market research): "no paywalls in the story — all players can experience the full narrative"; monetise convenience, never plot; all ads opt-in, no interstitials; no dark patterns. The shipped v1 is deliberately light: one rewarded placement, four energy SKUs, an ingot ladder, two bundles.

The numbers (economy sheet v0.2, canonical anchors):
- **Energy**: 1 per generator spawn · regen 90s/point · free cap 100 · FTUE grant 100. Accelerators consume 2 or 4 energy per spawn and auto-merge output to T+1.
- **Rewarded video**: "Watch-for-Energy," +20 energy, 5/day cap, overflow allowed. The only placement in v1.
- **Refills**: 100/200/500/1000 energy at A$1/2/5/10 — A$1 per 100 energy is the canonical price anchor.
- **Platinum Ingots** (premium currency, silver-white stacked-ingot iconography): packs at A$1.99/20 · A$4.99/60 · A$9.99/150 · A$19.99/400 · A$49.99/1500. The FTUE premium grant equals the A$9.99 tier.
- **Bundles**: Starter Pack A$3.99 (+300 energy, 50 ingots, cosmetic badge) · Investigator's Kit A$14.99 (+1000 energy, 150 ingots, Fast Track, Knife, cosmetic frame).
- **Inventory slots**: 9–12 bought with CaseCash (200→1600, doubling), 13+ with ingots (10→400 ladder, with CaseCash riders on 17–20).

In-fiction rule for all of it: the store sells *time and space*, never truth. Ingots and energy accelerate the investigation; they cannot skip it, and no clue is ever behind a price.

## 7.6 Flags, choice, and consequence

Narrative flags are the season's long-term memory (registry in Appendix 9.2). The branch pattern (one per episode, PUBLIC/PROTECTED) writes exactly one flag per episode — cheap to store, potent to reference: any future script may quietly acknowledge how the player told the truth in Episode 1 (`aq.e1.truth_public` vs `aq.e1.truth_protected`). Flags also carry mechanical duties (Episode 1's L1 sets the rusty_anchor availability flag that gates L4's beer-bottle requirement). Discipline: flags are namespaced (`aq.e1.*`, `f2.*`), set on Proceed, and never retconned — a set flag is canon.

## 7.7 Telemetry as story QA

Analytics is tuned to narrative health, not just funnel health: Episode 1's climax lead (L11) ships with a beta-telemetry watch — time from L10 to first toast, toast gap, sessions-to-complete, abandonment %, energy exhaustion % — because a climax that stalls on the board is a story failure delivered by the economy. The genre benchmarks to read the beta against: D1 ≈ 25%, D7 ≈ 3–7%; story-completion rate and reviews are the leading indicators that the differentiator works.

# Chapter 8 — Production Canon Rules

The rules that keep a one-person studio's canon coherent across scripts, art, audio, and code. These are process law: when a rule here conflicts with convenience, the rule wins; when it conflicts with a locked script, the script wins and the rule gets amended deliberately.

## 8.1 Canon-first

Use established canon wherever it fits; invent only at the episode level (the case's victim, its one-off witnesses, its dead men). Every functional role canon already staffs is cast from canon. External summaries — including AI-generated rosters and synopses — are audited against the canon docs: anything not traceable to the roster or a script is treated as invention until deliberately adopted. Current canon always wins name/fact conflicts with older documents; the name-migration table (Chapter 4.8 / roster) exists so stale names can be recognized and never resurface.

## 8.2 First-approved-image

Where the bible is silent, appearances are PROPOSED — **the first approved image becomes canon** and gets written back into the roster and this bible. Consequences:

- Generate the neutral portrait first; approve it; only then generate emotion variants with the approved neutral attached as identity reference.
- An approved portrait's incidental details are now facts (Mo's exact hair-streak pattern; Del's v2 hovering-falcon kestrel; Arthur's tousled hair — a recorded canon deviation from his original prompt).
- Model sheets beat single images: once a character has approved sheets (Ally: master + palette), future generations attach the sheets, not the original portrait.
- If one cell of an emotion sheet fails acceptance, regenerate the whole sheet — cheaper than matching a single cell later.

## 8.3 Blue-is-lighting

The signature noir look uses cool blue rim light and deep navy ambient (#0A1220). **Blue tones are LIGHTING ONLY, never material color.** Ally's hair is warm auburn/copper in neutral light; the blue/violet in noir shots is rim-lighting, not dye. This ruling is embedded in every art prompt and inherited by every asset. (Decided once, deliberately, in the Ally model-sheet kit — inverting it would require regenerating the canon.)

## 8.4 The lighting grammar (temperature = allegiance)

- **Civilians read warm** (Mo's amber bar light is an approved exception to the global noir key; Dot's warm amber key).
- **Cops read cool** (Del's cool blue ambient; Drake's blue-grey precinct light; Benji's cool light "softened with a hint of warmth" — the last idealist).
- **Antagonists read cold and underlit** (Vex lit from below, face half in shadow; Voss in hard corporate shadows).
- **Morally ambiguous characters get a split key** — warm from one side, cold from the other (Sabine Rourke; Evelyn Hart's podium-warm key with cold rim). The lighting *is* the allegiance; grade accordingly and never spend a character's temperature accidentally.

## 8.5 The Ferryman: DO NOT GENERATE

The Ferryman remains faceless until the season finale reveal, by explicit canon decision. Any imagery is silhouette, coin, or hands only. No portrait prompt exists and none may be written before the finale episode is locked. The same discipline applies in prose: he is rendered through effects and artifacts (the coin, the envelope, the block capitals), never through described features.

## 8.6 Naming conventions

- Portraits: `char_{firstname}_{emotion}_f01.png`, filed in `Assets/Art/Characters/{Name}/` with pre-assigned GUIDs (the Gerald pattern).
- Items: `{family}_t{NN}_{descriptor}.png` (e.g. `rusty_anchor_t04_beer_bottle`), 1024×1024, transparent background, silhouette readable at 96 px.
- VO: `vo_{ep}_{lead}n{node}_{speaker}` (e.g. `vo_e1_l1n3_ally`); SFX: `sfx_{ep}_{descriptor}`.
- Leads: `{ep}_{slug}` snake_case (`e1_tip`, `f2_pier13`, `gs_records`).
- Narrative flags: namespaced dot-paths (`aq.e1.truth_public`, `f2.ep02.complete`); set on Proceed; never retconned.
- Characters: no middle names exist in canon (explicitly ruled out 2026-07-11); middle names are minted deliberately when a story beat needs one.

## 8.7 Script pipeline & lock discipline

Pitch (judged against the Pilot Spec rubric) → draft → numbered revisions with per-item correction checklists (the Episode 1 pattern: v1.2 micro-pass, v1.3 final corrections, each item logged with its resolution) → table read (performance/timing veto only) → **VO LOCK**. After lock, any text change is a formal pickup logged against the master. The VO cue sheet and actor sides are generated artifacts — regenerate from asset data, never hand-edit.

## 8.8 Document hierarchy

Single sources of truth, in precedence order for their domains:

1. **Locked scripts** (episode-1-the-listener-VO-LOCK-v1.0) — dialogue, episode facts, minted canon.
2. **canon-character-roster.md** — who exists, ages, roles, appearance status, the migration table. New characters enter here at script approval, marked *proposed* until a portrait is approved.
3. **This bible** (`SAS/ally-quinn-bible.md`) — the connected whole: world, arc, craft, rules. It cites the masters rather than duplicating them.
4. **portrait-prompts-canon-cast.md** + model-sheet kits — appearance canon and generation method.
5. **SAS architecture docs & economy sheet** — systems truth (the bible's Chapter 7 summarizes and points).
6. **Legacy sources** (Deep Lore backstory bible, Setting Bible v1.3, GDD v250825, March scaffold, storylines, Full AQ Download) — quarry, not canon. Mine them through the migration table; never quote a stale name into a new document. Appendix 9.4 records what superseded what.

The flow of truth: a script mints a fact → the roster/bible record it → prompts and systems consume it. Never the reverse: art or systems convenience does not create canon.

## 8.9 Rating & content boundaries

12+ across all content: missing persons, fraud, corruption, restrained murder in scripts; no graphic violence, no sexual content, no on-page harm to children; grounded and contemporary, no supernatural elements. Monetisation content rules ride along: no dark patterns, no story behind a paywall, ads opt-in only. These are store-rating commitments as well as taste — treat violations as shippability bugs, not notes.

# Chapter 9 — Appendices

## 9.1 Glossary

- **Lead** — one investigation step: a card with requirements, a reward band, and a 3–5 node scene. Twelve per episode, two phases.
- **Node** — one dialogue line: speaker + emotion + line (≤45 words). ~44–48 per episode.
- **Band** — reward tier of a lead: Easy / Standard / Hard / Very Hard (climax and finale are Very Hard).
- **Toast** — authored narrative micro-beat shown at ~30% / ~70% of a hard lead's requirement progress.
- **The whisper / the notch** — the episode's single season-arc clue, landed unexplained (whisper = the pilot's coin; notch = the general term).
- **Keeper** — the living character who has carried the case emotionally and gets the on-page resolution beat.
- **The branch** — the episode's one choice moment (Phase 2, two approaches, converging). Episode 1 minted the PUBLIC TRUTH / PROTECTED TRUTH pattern.
- **Stinger** — wordless final node after the case close (minted by Episode 1).
- **VO LOCK / pickup** — the script lock state; any post-lock text change is a formal pickup logged against the master.
- **Pilot Doctrine** — the ten engineered beats every cold-audience episode must hit (Chapter 5.4).
- **Evidential language** — the craft rule that narration never outruns proof (Chapter 5.5).
- **Evidence board / cork board** — the persistent per-case view of activated leads, phase-clustered, replayable.
- **Cold case** — repeatable between-episode lead chain (Storage Unit 44 → The Second Set → Last Orders → cycle).
- **Tip line** — the show's in-world intake: "voices, timestamps and nothing else."
- **CaseCash (CC)** — soft currency. **Platinum Ingots** — premium currency. **Energy** — the session pacer (90s regen, cap 100).
- **Generator** — board item producing family items (Investigation Lab, Corner Diner, Junk drawer).
- **Item family / tier** — themed merge chain (T1 base; each tier visibly "more").
- **FTUE** — first-time user experience; Episode 1's rig-merge cold open.
- **Golden path** — the QA walkthrough of an episode's lead/economy spine.
- **Binge window** — the 20–30 minute session the economy is tuned around.
- **Dolphins** — occasional $2–$10 spenders (~25% of merge MAU) who pay only if respected.
- **★ CANON / proposed** — appearance status: approved image binding / prompt awaiting first approval.
- **Migration table** — the register of stale names that must never resurface (Chapter 4.8).

## 9.2 Narrative-flag registry

Flags are namespaced dot-paths, set on Proceed, never retconned. Registry as of bible v2.0:

| Flag | Set by | Meaning / consumers |
|---|---|---|
| `aq.e1.truth_public` | Ep1 L9 choice ① | Full recording released; L10 + L12n2a variants; any future script may reference the loud-truth precedent |
| `aq.e1.truth_protected` | Ep1 L9 choice ② | Transcript only; Del holds the clean copy; L10 + L12n2b variants |
| Ep1 rusty-anchor flag | Ep1 L1 | Gates the Rusty Anchor requirement family for L4 (with the gen_junk overflow grant) |
| `aq.lead.f2_tip.seen` | Ep2 L1n4 (draft) | Episode 2 opened |
| `f2.phase1.complete` | Ep2 L5n4 (draft) | Ep2 mid-season milestone |
| `f2.ep02.complete` | Ep2 L12n5 (draft) | Episode 2 closed |
| `cold_case_a` (chain) | Ep1 completion | Cold-case loop live; teaser card "Episode 2: The Ferryman" present, non-proceedable |
| `aq.e2.eddie_protected` | Ep2 L11 choice ① (draft) | WITNESS SHIELDED — Eddie moved first, match sealed; the evidence sleeps inside the system Del distrusts (Ep3 leak-thread fuse); L12n1a variant |
| `aq.e2.merrick_published` | Ep2 L11 choice ② (draft) | TRUTH IN DAYLIGHT — reopening un-buryable; Eddie inferable; the Ferryman knows what Ally holds; L12n1b variant |
| Ghost Student branch flag | gs_other_ghost (design) | Anonymity vs. on-record; both converge; going public hardens `gs_admin` |

Rules: one branch flag per episode; flags are cheap to set and expensive to honor — every flag added is a promise that some future script will read it. The registry in this appendix must be updated at every episode lock.

## 9.3 Asset naming

Canonical patterns (full law in Chapter 8.6): portraits `char_{firstname}_{emotion}_f01.png`; items `{family}_t{NN}_{descriptor}.png`; VO `vo_{ep}_{lead}n{node}_{speaker}`; SFX `sfx_{ep}_{descriptor}`; leads `{ep}_{slug}`; flags `aq.{scope}.{name}`. Item art acceptance: 1024×1024, transparent background, silhouette readable at 96 px, family-consistent angle and lighting, no text, no baked shadow.

## 9.4 Source-document archaeology

What superseded what — so nobody re-mines stale documents unknowingly. Precedence within any conflict: **locked scripts → roster → this bible → prompts/kits → SAS systems docs → legacy sources.**

| Source (era) | Status | What it still owns | What replaced it |
|---|---|---|---|
| `AQCharBible_BackStory.docx` — "Deep Lore" (oldest) | quarry | richest character backstories & Relationship-to-Ally write-ups (migrated into Ch4) | names via migration table; city = Havenbay |
| `Ally Quinn Story lines.docx` (prototype) | quarry | the 3 seed cases (§9.5) | cast renamed; format converts to 12-lead |
| `Ally_Quinn_Character_Setting_Bible_v1.3` (mid) | quarry | themes/tone strands, background-mystery seeds, voice-line gold | "Havenford"/Southern-Gothic frame dropped; districts superseded by canon six |
| `Ally_Quinn_True_Crime_Merge_GDD_v250825` (Aug 2025) | quarry | audio direction (sole source), loop framing, art pillars, monetisation doctrine | all systems specs → shipped game + SAS docs |
| `Full AQ Download` (mixed era) | quarry | positioning/persona/marketing (Ch1, §9.6), episode skeleton & taxonomy (Ch5/§9.7), world detail (Ch2), origin of the current canon layer | Kestrel Point Ep1 + both prototype FTUEs superseded by The Listener; character detail defers to roster |
| March scaffold (`Bible/Ally Quinn Bible.docx`, Mar 2026) | quarry | vision prose (Ch1), 13-year timeline structure (Ch3), pillar framing | superseded in full by this bible |
| Ghost Student design doc (May 2026) | active design | the ~Ep5 case | its Ep1/vertical-slice framing (pilot duties moved to The Listener) |
| episode-writers-prompt v2 | active, partially relaxed | canon block, 12-lead law, tone rules | portrait/branch/VO constraints relaxed by pilot spec (Ch5.3); "aired so far" list updated by the season reorder |
| episode-1-pitch process (A/B/C + D) | closed | Pitch D became The Listener; A/B/C preserved (§9.5 annex) | — |
| episode-1-the-listener VO LOCK v1.0 (11 Jul 2026) | **LOCKED CANON** | everything it says | changes only by pickup |
| canon-character-roster.md v1.0 | **CANON** | who exists | — |
| portrait-prompts-canon-cast.md (rev 2026-07-11) | **CANON** | appearances & lighting grammar | — |
| `ally_quinn_economy_v0_2.xlsx` + SAS Ch1–11 | **CANON (systems)** | all numbers & implementation | — |
| Old `Bible/Ally Quinn Bible.docx` | to be archived at Phase 3 | — | **this document** |

## 9.5 Episode Seed Vault

Three complete prototype cases, casts renamed to canon, preserved as pitch raw material. **Non-canon until produced.** Conversion notes apply to all three: written as 14-day daily-beat structures with a merge-clue per day — production requires conversion to the 12-lead/two-phase format, Pilot-Doctrine pass, evidential-language pass, and a decision on each case's season slot and notch. All three predate the canon rule that Gerald's career was at *Brookford* PD — each "Gerald worked the original case" beat needs either relocation to a Brookford-era case or a recast of the original investigator (Drake's early career is the natural candidate).

### Vault 1 — "A Shadow in the Park" (cold case, 10 years old)

Ally's podcast reopens the decade-old murder of high-school senior **Mia Donovan**, found dead in a city park after midnight; her boyfriend **Mark Carter** was blamed but never charged, his life ruined by suspicion. The reinvestigation — old case file, re-run forensics, a locked diary, Mo's decade-old memory of a girl crying "I didn't mean for this to happen" in her parking lot — converges on the grieving best friend, **Chloe Martin**: secretly involved with Mark, she lured Mia to the park to confess, shoved her in the fight that followed (head striking a bench), then staged the scene to frame Mark. Key beats: the unidentified size-6 shoe prints honored ten years late; partial female DNA under Mia's nails (Vega acquires the comparison sample by means Del is careful not to ask about); Mia's missing heart-shaped locket dropped by Chloe at the memorial — inside, the photo torn to remove Mark, and a note: *"If I can't save you from him, I'll save you from yourself. Forgive me."* Confession under interrogation; Mark publicly exonerated; the original investigation's tunnel vision apologized for on the record ("We all saw what we expected to see"). Keeper: Mark — and the original investigator's conscience. Cast: Del Cruz (evidence box, warrant, interrogation), Mo (earwitness + kept footage), Priya (modern DNA on old evidence), Vega (the toothbrush), Drake (the gatekeeping), Gerald (the guilt — pending the Brookford repoint).

### Vault 2 — "Fatal Facade" (fresh murder, staged burglary)

Socialite **Vivian King, 34**, is found shot dead in her Highcliff home, the scene dressed as a burglary — "too neat," staged. Her charming boyfriend of three months, **Brandon Wells** ("finance"), has a camera-perfect alibi; the safe was opened at 11:55 p.m. with Vivian's own fingerprint; her signature diamond necklace is missing; and she was due at her lawyer's the next morning to change her will. Layered red herrings: the grieving assistant **Natalie Sloane** pawned the necklace — through Silas Vex's channels — but to fund a private investigator and her mother's medical debts; the housekeeper **Marisol** (renamed from the prototype's "Rosa" to avoid collision with Episode 2's Rosa Reyes) fled with the safe's cash — but as a terrified eyewitness who walked in on the killer and was threatened into silence. The truth: "Brandon Wells" is **Daniel Ward**, a serial predator of wealthy women who legally renamed himself two years ago after another convenient death; Ally baits his confession at his condo with a fake exposé deadline, wire in her pocket, Del's team in the stairwell. He lunges on tape. Keeper: Marisol (the witness who gets to stop being afraid) and Natalie (leniency earned). Cast: Del (the trap), Benji (crime-scene comic beat), Cortez (TOD), Priya (the vase-shard blood that is neither victim's nor killer's — it's the housekeeper's), Vega (the identity excavation), Vex (the fence who trades up), Mo (the back room), Drake (the witness relay). Rewrite notes: the Day-7 "male DNA" line contradicts the Rosa match — fix; Brandon's camera-alibi bypass is unexplained — close it or mine it (an accomplice raises the case's season utility); rename Daniel Ward or the Vault-3 infant (first-name collision).

### Vault 3 — "Vanishing Point" (20-year-old disappearance; the darkest)

**Catherine Thompson** vanished twenty years ago, her car abandoned on a rural route; her husband **Robert** always said she abandoned the family. Her daughter **Lily** — eight then, twenty-eight now — hires Ally after finding a sealed letter from her mother addressed to her eighteenth birthday. The excavation: a $5,000 cash withdrawal a week before the vanishing; a P.O. box under her maiden name; mail forwarded to a small town a hundred miles north where "**Catherine Hale**" rented a cabin for eight months — pregnant, fleeing Robert's death threats, writing unmailed letters to **Michael Bennett** ("Uncle Mike," Robert's best friend, the man who loved her and never acted on it): *"Our son is healthy. I named him Daniel — after you; you'll understand."* (The prototype line minted a middle name for Michael; reworded here per the no-middle-names ruling.) The lease ends abruptly. The original investigator's twenty-year-old memory — Robert poured a new concrete patio a month after his wife disappeared — becomes a warrant, ground-penetrating radar, and the worst find in the vault: adult female and infant remains. Robert's raging on-site confession; the blank lighthouse postcard Michael kept for nineteen years reframed as Catherine's proof-of-life signal that no one read. Keeper: Lily — and Michael, who learns he had a son. Coda: a survivors' charity in Catherine's name. Cast: Del (warrant, dig, arrest), Vega (financial archaeology, the road trip, the lock), Cortez (the announcement under the patio), Drake (quiet sanction), Gerald (the guilt engine — pending repoint). Rewrite notes: the lighthouse postcard maps naturally to **Kestrel Point**; the case's content (infant victim) sits at the 12+ boundary — resolution must stay off-page and restrained, per tone law.

**Also preserved as pilot-shaped seeds** (full pitches on file, `episode-1-pitches.md`): **"The Overnight Girl"** (Nina Vasquez, the 3:10 a.m. two-ring signal, the diner window, the ice store; keeper: her kid brother Marco), **"The Confession"** (Frank Doyle dies hours after confessing a 1989 foundry fire he didn't commit; Gerald's episode; keeper: Gerald and June Calloway), **"Dead Air"** (Mags Okonkwo's cut final broadcast, the condemned station, the demolition clock; keeper: Ray Delgado, the engineer who cut the feed). Each has a designed coin-whisper and a designed branch; franken-picking is legal.

## 9.6 Go-to-Market & LiveOps canon

The commercial playbook, preserved from the Full AQ Download and validated/adjusted by the July 2026 market research. Chapter 1 carries positioning; this appendix carries execution.

**Brand kit.** Icon: Ally silhouette + magnifying glass. Palette: noir pastel. Tagline stack: "Merge clues. Solve cases. Expose the truth." / "Play the mystery. Piece by piece." Store keywords: "true crime merge," "detective story game," "mystery merge," "merge game," "detective merge." Screenshots lead with story and portraits, not the board; the subtitle carries the podcast hook. Domain to register: AllyQuinn.com. Community: a "Case Breakers" Discord; TikTok (merge gameplay + crime facts), Instagram (character art), Reddit (r/truecrime adjacency).

**The marketing asymmetry (the plan's one big idea):** ads mimic crime-doc trailers, not gameplay ads — rain, evidence photos, red string, Ally's voice. The VO does double duty: 3–4 podcast nodes cut as captioned vertical clips *are* the TikTok pipeline at zero extra production cost, and real *Echoes of Havenbay* teaser episodes on Spotify/Apple blur game and reality. Micro true-crime podcasters (10–50k listeners) over big UA buys.

**Pre-registration blueprint (90 days, when used):** Tease (store pages, whispered-voicemail ads, 2 real teaser episodes) → Engage (weekly case-file drops; milestone rewards at 10k/25k/50k/100k pre-regs — energy pack → exclusive Ally cosmetic → limited "Founders Case" pack → podcast profile badge) → Convert (playable cliffhanger ads, minisodes, Apple featuring pitch, "7 Days to Solve the Case" countdown) → launch day: Ally "publishes" the launch episode; "Launch Mystery Night" livestream.

**Soft-launch frame:** Canada/Australia/UK, 6–12 weeks. Targets: CPI < $2.50; D1 > 35% aspirational (read against the ~25% genre benchmark), D7 > 12%, D30 > 5%; ARPDAU $0.20–0.40; day-90 breakeven ROAS. Solo-dev counsel adopted: organic devlogs and a superfan Discord first; paid UA "to scale a proven model, not to test a hypothesis."

**Post-launch roadmap (evidence-ordered, from the market research):**
1. **v1.1 (first month):** daily reward + weekly Cold Case streak event — cheapest proven retention/ARPDAU lift, scaffolded on the shipped cold-case loop.
2. **v1.2:** the evidence board becomes an active meta — pins as collectibles, per-episode board-completion rewards; consider a light "restore Ally's office" decoration loop.
3. **Content cadence:** Episode 2 within 6–8 weeks of launch (script exists); episodes are the "events" until real liveops exist.
4. **Rewarded depth:** second/third placements (cold-case reroll, board slot) after brand trust is established.
Named future SKUs/events held in the vault: "Ally's Case Notes Pass" (with Director's-Cut podcasts and Side Cases), "Podcast Sponsorship" ad-free IAP, "Detective's Stash," "Havenbay Holiday" events, "Case Breaker's Challenge."

**Benchmarks to hold ourselves to:** casual merge D1 ≈ 25%, D7 ≈ 3–7%; Gossip Harbor IAP ARPDAU ≈ $0.31 vs. puzzle baseline ≈ $0.08; ~50–70% of installs from organic store search — ASO is the primary acquisition channel, not a checkbox.

## 9.7 Parked Ideas Vault

**★ The Preservation Rule (Stephen, 2026-07-11): "DROP" never means delete.** Anything with plausible long-term merit lives here with a one-line description and its source. Only true duplicates and superseded numbers are dropped without preservation. Ideas graduate out of the vault by explicit decision, never by drift.

### Story & season material
1. **The Kestrel Point Disappearance** — the full pre-Listener Episode 1 (Lena Kovac, the deleted photos, Conrad Marlow, the Room 312 hook); a complete case skeleton awaiting a season slot and rename pass. *(Full AQ Download)*
2. **The pre-Listener season ladder** — burglary tutorial → Vex-linked murder → data-theft/City-Hall decrypt → red-herring mid-season confrontation; useful shape for the unwritten mid-season. *(Full AQ)*
3. **The 10 first-case elevator pitches** — Borrowed Life, Time Capsule, Replacement Child, Perfect Neighbor, Inheritance Scam, Vanishing Act, Wrong Room, Study Group, Memory Thief (Ghost Student graduated). *(Full AQ)*
4. **Ep1 pitches A/B/C** — The Overnight Girl / The Confession / Dead Air (§9.5). *(pitch doc)*
5. **"The Letter"** — ricin mailed to Arthur Finch; the 7-beat episode-fiction generator prompt it demonstrates ("NOT FOR USE YET"). Note the Calvin Rourke surname collision with Sabine before any use. *(Full AQ)*
6. **The Bay-side Strangler** — uncaught 1970s serial spree that shaped Gerald's rigor; season-arc-grade legacy cold case. *(Deep Lore / v1.3)*
7. **The Harbor Tunnel Heist (1990s)** — vanished armored truck; heist episode + possible Ferryman seed money. *(Deep Lore / v1.3)*
8. **"The Oracle"** — Thomas's encrypted source, never identified; a late-season reveal in waiting. *(scaffold)*
9. **The file tamperer** — the omitted witness statement is canon (Ch 3.1); parked: the *identity* of whoever pulled it — a living tamperer inside the file chain, unwritten. *(Deep Lore)*
10. **"Cargo manifests"** — the longshoreman's memory of Thomas's final line of inquiry; port-smuggling episode that feeds the metaplot. *(Deep Lore)*
11. **Hart × Gerald "shared past during a major unsolved case"** — dormant hook (fusable with #6). *(Deep Lore)*
12. **The mole in Ally's circle** — the Ferryman's implied informant close to home; high-risk, high-reward twist. *(v1.3)*
13. **Two coins on the eyes, staged on-page** — the legend is canon folklore (Ch 3.2); parked: the episode that opens on an actual Ferryman execution bearing the fare. *(v1.3)*
14. **Frankie's sister and the crash-forensics episode** — the debt and his skills are canon (Ch 4); parked: the sister herself, the favour's story, and the episode that puts his vehicle expertise on the 2013 crash. *(Deep Lore)*
15. **Gerald–Mo old flirtation** — warm elder-romance subplot in the bank. *(v1.3)*
16. **Ally-mentors-Benji** — mirror of Gerald-mentors-Ally. *(v1.3)*
17. **Human-trafficking thread + hidden federal agent** — the "Wrong Room" hook, explicitly built to develop across cases; handle with 12+ care. *(Full AQ)*
18. **Kestrel Point postcard signal** — the blank-postcard proof-of-life device (Vault 3); reusable iconography. *(storylines)*
19. **Havenbay urban-legend texture** — Ally "dedicated podcast episodes" to old legends; copycat-crime engine. *(v1.3)*
20. **The crime-type taxonomy beyond Ep1's kind** — the ranked top-10 (serial killers, cults, wrongful convictions, historical cold cases, comedic heists…) as a season-planning palette. *(Full AQ; summarized in Ch5 terms: each maps to a keeper shape and a jeopardy shape)*
21. **Southern Gothic tone strand** — the v1.3 "Nancy Drew meets True Detective, soul of Savannah" frame; dropped as *setting*, bankable as an episode flavor (an old-money estate case). *(v1.3)*

### Format & delivery
22. **The 4-part episode skeleton variants** — Cold Open→Investigation→Twist→Resolution with mid-episode podcast break; and the 7-beat fiction structure; both useful for marketing minisodes and future format experiments. *(Full AQ)*
23. **Pier 13 cinematic cold open** — the ledger-heist FTUE with Vex confrontation, coin-flipping muscle, Nova's graffiti escape; plus its short story, storyboard, and podcast VO script — ready-made trailer material. *(Full AQ)*
24. **The Kestrel FTUE spec** — the 5-minute "Ultimate First 5 Minutes" (scanner/laptop chains, CCTV scrub "brilliance moment," DMV rush, CITY HALL / QUINN CASE #001 twist) with full KPI/telemetry scaffold; superseded as content, exemplary as method. *(Full AQ)*
25. **Procedural Podcast Narrative** — merges trigger recorded Ally snippets; case-end compiles them into the "co-produced" episode. *(scaffold)*
26. **Danielle's "episodes she can own"** — Director's-Cut podcast episodes as season-pass content. *(Full AQ)*
27. **Dual-Voice Storytelling formalism** — real-time dialogue vs. reflective past-tense podcast voice; the reflective register is underused outside milestones. *(Full AQ)*
28. **Multiple endings / spare-or-condemn** — explicitly deferred; "re-evaluated post-launch." *(GDD / Full AQ)*

### Systems (post-MVP)
29. **Deferred lead types** — Timeline, Alibi, Stakeout, Lab (with their player fantasies). *(Ghost Student design)*
30. **Minigame catalog (17 concepts)** — Lie Detection, Evidence Web linking, Forensic Match, Scene Reconstruction, Audio Clue Decode, Safe Cracking, Surveillance Spot…; guidance: 30–90s, output = case progress, prototype 2–3 first. *(Full AQ)*
31. **World-systems suite** — District Boards; Time & Weather clue modifiers; **Public Pressure Meter** (the podcast as a lever that opens/closes NPC doors); Chain-of-Custody item tiering; **Favors Ledger** (social capital with Mo/Del/Finch/Nova). *(Full AQ)*
32. **Ten multi-type generator concepts** — Squad-Car Trunk, CCTV Hub, Evidence Intake, Informant Hotline, Pawn-Shop Counter… with tier-unlocked output pools. *(Full AQ)*
33. **The 15-tree Season-1 item pool** — Photography, Clue Analysis, DNA, Luxury Lifestyle, Tabloid Gossip, Blackmail, Alibi Evidence, Nightlife, Police Archive, Crime Scene Barriers… (four shipped; the rest are pre-designed families). *(Full AQ)*
34. **Cross-type fusion merges** (Wire + Battery → Taser) and special consumables (Smoke Bomb). *(GDD)*
35. **Upgradeable office / research board meta** — aligns with the v1.2 active-evidence-board roadmap; "Analysis Kit" style tool upgrades. *(GDD / Full AQ)*
36. **Red Herring Logic** — dead-end items "disproven" off the board with a Deduction Tool. *(scaffold)*
37. **Dead End mechanic / narrative gates** — throttled chains that force a pivot to context items. *(scaffold)*
38. **Evidence Web spatial puzzle** — Logic Link Connectors, soft-failure static, "Aha!" cinematic. *(scaffold)*
39. **Cliffhanger Protocol** — a teaser item spawned ~150s before predicted session end; ethically spicy, test with care. *(scaffold)*
40. **Sentiment Engine** — Ally's tone shifts with the player's empathy/aggression ratio; empathy-gated Confession Items. *(scaffold)*
41. **Havenbay Corruption Index** — corruption items invading the board, cleared by Legal Authority items. *(scaffold)*
42. **Sub-Surface Toggle** — the Ferryman's transit sub-level as a shadow board. *(scaffold)*
43. **Dynamic weather on the board** — tension downpours raising rare-clue spawns at higher energy cost. *(scaffold)*
44. **Time-Capsule Rewards** — Thomas audio logs unlocked at "chronological clarity" milestones. *(scaffold)*
45. **Adaptive/layered music** — instruments layering toward objective completion; the "Ally Quinn theme" leitmotif. *(GDD)*
46. **Character voice one-liners** via indie VA (beyond podcast leads). *(GDD)*
47. **Timed challenge mode with ad-powered second chance; endless mode post-story.** *(GDD)*
48. **Off-board storage stash; mystery crates; item-journal "???" teasers with recipes.** *(GDD)*
49. **AI-generated flavor text for mundane items; AR clue scanning; user-submitted clues in LTEs.** *(scaffold / Full AQ)*

### Monetisation & liveops (designed, unshipped)
50. **"Ally's Case Notes Pass"** — 28–30-day free/premium pass: outfits, office themes, Side Cases, Director's-Cut podcasts. *(Full AQ)*
51. **Seasonal events** — "Havenbay Holiday": Halloween Unsolved Cases / haunted-house tree; Christmas Stolen Presents tree; weekly Cold Case Files with leaderboards; Case Breaker's Challenge. *(Full AQ / GDD)*
52. **"Podcast Sponsorship"** ad-free IAP; **"Detective's Stash"** bundle; "Unlimited Energy 7 days"; "Detective's Wardrobe Pack." *(Full AQ / GDD)*
53. **"Founders Case" / "The Vanishing Violin"** limited case pack as a pre-reg/founder reward. *(Full AQ)*
54. **GH-style "order case" helper** (offers one of three useful items; pairs with ads); item collection album; animated portrait reactions; level/XP badge. *(ui-ux plan §J)*
55. **Narrative push notifications** ("Ally found a new clue…") — pending the retention-tooling pass. *(GDD)*

### Production & meta
56. **The 16-persona "Agent Squad" handbook** — internal AI-staff tooling doc; not in-world. *(Full AQ)*
57. **Kickstarter/Patreon option; "Founder" perks for beta testers.** *(GDD / Full AQ)*
58. **Empty marketing scaffolds** — positioning map, store listing draft, social calendar, launch-day campaign: planned deliverables, never drafted. *(Full AQ)*
59. **Old-bible lore fragments that didn't survive renaming** — Gerald-as-police-chief "golden era of community policing"; the Heritage Park district and its history; Boardwalk & Beachfront; Cypress Bible College and the televangelist network; the Founder's Day gala; the naval air station; the Historical Society's three families; the jazz bar where Arthur drafts articles; community center run by a former boxer; Nova's grandmother. Any of these can be re-naturalized into canon Havenbay by a deliberate script decision. *(Deep Lore / v1.3)*
60. **Character-lore offcuts pending a home** — Vex's old look (bald, groomed beard) if a "years ago" flashback ever needs him; Ally's celebrity-resemblance and favorites lists (Zodiac, All the President's Men — the Arthur echo); the press badge Ally flashes in Vault 2 (formalize via a Gazette credential from Arthur, or cut). *(Deep Lore / storylines)*
