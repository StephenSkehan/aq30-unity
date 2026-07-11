# Episode Writer's Prompt — Ally Quinn: True Crime Merge
*v2 — full canon bible embedded. Copy everything below this line into the chatbot.*

---

You are writing an episode for **Ally Quinn: True Crime Merge**, a mobile narrative merge game. The story is the product: players merge items to unlock "leads" (investigation steps), and each lead pays off with a short scripted scene. Your job is to write one complete, compelling true-crime episode inside the delivery constraints below. Violating a constraint makes the episode unshippable, so treat them as hard.

## Canon-first rule (governs everything below)

Use established canon wherever it fits. Invent ONLY at the episode level: the case's victim, its one-off witnesses, its dead men. Every functional role that canon already staffs must be cast from canon — archives research goes to Arthur Finch, forensic analysis to Dr. Priya Shah, autopsy questions to Dr. Cortez, police access to Sgt. Del Cruz, digital work to Alex Vega, bar scenes to Mo Callahan's Rusty Anchor. Locations likewise: prefer named canon landmarks over invented ones. Signature motifs (the Ferryman's coin, Thomas's missing laptop, Ally's locket, Del's kestrel tattoo, Gerald's booth) are sacred — use them precisely or not at all.

## The world (canon — do not contradict)

**Setting:** Havenbay, a fictional mid-size coastal city. Grounded and contemporary — no supernatural elements. Six districts:
1. **Old Docks & Harbor Ward** — warehouses, fish markets, union halls. Landmarks: **Pier 13** (Ferryman lore — his coins are found near the pylons), **Kestrel Point Lighthouse** (fog, rendezvous), **The Rusty Anchor** (dockside bar, est. 1924, Mo Callahan's neutral ground — cops and crooks coexist; Gerald's regular booth).
2. **Downtown Civic Row** — City Hall, courts. Landmarks: Hart Civic Center, the **Havenbay Gazette** building (Arthur Finch's domain; "Havenbay's conscience").
3. **Stonebridge Viaduct** — elevated rail, under-bridge camps, Nova Skye's coded murals.
4. **Highcliff Heights** — old money, private security. Voss Group HQ.
5. **Rivermouth Industrial** — foundries, scrapyards, chop shops.
6. **Havenbay University & Arts Quarter** — labs, student radio, idealists and whistleblowers.

**Protagonist — Ally Quinn** (26): licensed PI and host of the true-crime podcast *Echoes of Havenbay*. Auburn wavy hair, teal trench coat, silver locket holding her father's photo, retro headphones, satchel of podcast gear. Sharp, dry, self-aware; compassionate but never sentimental. She narrates every scene — the whole game is her podcast episodes and field recordings.

**Core cast:**
- **Gerald Quinn** (74) — grandfather, retired Brookford PD detective. Primary confidant; Rusty Anchor regular; teal suit, magenta shirt, wire-rim glasses. Voice: short sentences, regret without self-pity.
- **Helen Quinn** (46) — mother, school principal, moral anchor. Still wears Thomas's wedding ring. Fears Ally repeating her father's fate.
- **Thomas Quinn** (43, deceased) — father, investigative journalist at the Gazette. Died in a suspicious car crash; his laptop was never found. Assassinated by the Ferryman network. His files and notes are a recurring evidence source.
- **Arthur Finch** (62) — semi-retired Gazette journalist/archivist. Knowledge hub for historical cases and buried scandals. Lives above a used bookstore. Knew Thomas.
- **Alex Vega** (29) — freelance hacker, Harbor Ward origin. Won't touch jobs involving kids or trafficking.
- **Sgt. Delaney "Del" Cruz** (36) — Havenbay PD, Harbor Ward precinct. Kestrel tattoo, Saint Michael medallion. Bends rules for justice, off the record.
- **Mo Callahan** — owner of the Rusty Anchor. Keeps confidences; Ally's informal protector.
- **Dr. Priya Shah** — forensic scientist. **Dr. Lionel Cortez** — medical examiner.

**Antagonists (season arc — never fully caught before the finale):**
- **The Ferryman** — shadow villain. Makes problems disappear. **Calling card: a coin.** Identity unknown until the season finale.
- **Councillor Evelyn Hart** — the public face of "progress"; redevelopment as graft. Connected to the Ferryman.
- **Dante Voss** — ex-Special Forces, runs Voss Group private security.
- **Silas Vex** — information broker, the Ferryman's operator. Smooth, always a step ahead.
- **Nova Skye** — graffiti artist; encodes warnings in murals. Ambiguous ally.

**Season arc:** who killed Thomas Quinn, and what conspiracy was he uncovering? Each episode = a self-contained case + exactly one new clue advancing the Ferryman/Hart thread — one new fact, one new cost, never the full answer. The season finale (not yours to write) unmasks the Ferryman and Hart live on the podcast.

**Aired so far:**
- **Episode 1 — "The Ghost Student":** university admins sold student biometric identities to Voss Bio; Maya Chen found out and hid; Ally found her and exposed it. Gerald revealed Thomas investigated Voss Bio in the 2000s.
- **Episode 2 — "The Ferryman":** the 2006 murder of harbour bagman Danny Reyes; established that the money route still runs, and the Ferryman left Ally his coin plus a photocopy of her father's press badge — he's watched the Quinns for twenty years. Number your episode 3 or later; do not contradict either.

## Tone rules

- Noir-lite: grounded, melancholy, wry. **Never campy, never gory, never lectures.**
- Rating boundary (12+): missing persons, fraud, corruption, restrained murder — yes. Graphic violence, sexual content, on-page harm to children — no.
- Ally's voice: first-person podcast narration; concrete sensory anchors over abstractions; she never states the theme out loud.
- The victim is a person, not a puzzle. Every episode needs one living character who has carried the case emotionally and gets an on-page resolution beat.
- **Anti-patterns (instant rejection):** villain monologues; Ally naming her feelings; exposition dumps; coincidence solving the case; the season villains caught early; police doing Ally's work; characters acting stupid for plot; puns in titles; withholding the episode's own resolution; lines over ~45 words.

## Delivery format (hard engine constraints)

**Structure — exactly 12 leads in two phases:**

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
Exactly 12 leads, exactly 2 podcast milestones (L5, L12), parallel threads in both phases, one climax lead.

**Each lead needs:** `lead_id` (snake_case); **type** — Podcast, Interview, Evidence, Data, Money Trail, Location, or Discuss (*Discuss* = Ally with Gerald or Helen ONLY; external witnesses are always *Interview*; every type appears at least once, Discuss at most twice); **title** (≤ 32 chars, no puns); **card subtitle** (≤ 140 chars, written like a case-file label); **requirement theme** (one line: what items the player gathers — forensic tools, fingerprint evidence, harbour/bar items, diner food, garage tools, press items — must make diegetic sense); **reward band** (Easy / Standard / Hard / Very Hard; climax and finale are Very Hard).

**Dialogue script per lead:**
- **3–5 numbered nodes**, ~44 nodes per episode total. Each node: `speaker`, `emotion`, `line`.
- Portrait speakers are **Ally Quinn and Gerald Quinn ONLY.** All other characters speak as quotes inside Ally's narration ("'He hated the water at night,' she told me."). Art-budget constraint — the podcast frame makes it natural.
- Emotions: `neutral, happy, sad, angry, surprised, worried, confused`. `angry` at most once per episode, and it must be earned. `happy` almost never.
- **Strictly linear.** No choices, branches, or conditional nodes.
- Lines 1–3 sentences, ≤ 45 words. Mobile, tap-to-advance.
- Each phase ENDS on a recontextualizing reveal.

## Required output format

1. **Logline** (2 sentences), **the victim**, **the secret**, **the reveal ladder** (5–6 reveals in play order).
2. **Lead table** — all 12 rows: id, type, title, subtitle, gates, requirement theme, band.
3. **Full dialogue scripts** for all 12 leads.
4. **Season-arc notch** — what the episode adds to the Ferryman/Hart/Thomas thread; what it deliberately leaves unanswered.
5. **The resolution beat** — who got closure and the on-page moment it landed.
6. **Canon usage note** — which canon characters/locations/motifs you used and why each invented element couldn't be served by canon.

Quality bar: the mid-episode reveal should make a listener physically stop; the finale should resolve the case with emotional honesty while making the Ferryman feel closer and more personal. If it works as a synopsis but not as 44 spoken lines, rewrite the lines — the lines are the game.
