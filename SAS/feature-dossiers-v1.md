# Character Dossiers ("Case Files") — design v1 + Ep1 fact sets — **RULED & BUILT 2026-08-11**

> **Stephen's rulings (2026-08-11), all applied:**
> 1. "Appears in N scenes" REMOVED from the profile modal — meta copy breaks the fiction.
> 2. NO fixed fact count — characters may have many facts or few; the UI only ever
>    shows the NEXT available locked fact, never a full locked list.
> 3. Prices: escalating 50/100/150/200 stands, any price CAPPED AT 300 CC for now.
> 4. All nine [INVENTION] lines approved as written.
> 5. Rewards ARE visible on the locked fact.
> 6. Final-fact gating on episode resolution confirmed.
> 7. No ingots as rewards confirmed.
> 8. Completion cassette slots stay reserved (Dot's ships now with her existing clip).
> 9. Mrs. Vale: no dossier for now.
> 10. **2026-08-12 addendum:** Vera's dossier CUT (Stephen-ruled) — her fact set
>     below stays as authored copy in case she ever earns a file. Live Ep1 set =
>     five dossiers, 2,500 CC total.
>
> **Build (same day):** `DossierCatalog` / `DossierService` / `DossierPopup` +
> `AllyDossierEntry` in `Assets/Scripts/UI/Dossiers/`; CASE FILE button on the
> profile modal; Ally's file opens from the HUD bust; QA menu under AQ/QA/Dossiers.

Stephen's pitch: a bio per character, progressively unlocked with CaseCash, each
reveal carrying bible facts plus a reward; completing every dossier is the
long-tail side quest. Diegetic frame: these are ALLY'S CASE FILES on the people
of Havenbay — an investigator keeps dossiers; the player is reading hers.

## Design

- **Entry points:** the character profile modal (evidence board polaroids) gains
  a CASE FILE button opening a dedicated dossier popup. Ally's own dossier opens
  from her HUD bust (previously tap-dead).
- **Structure per character:** one free intro line + a VARIABLE number of locked
  facts (Ep1 ships 4 each, Vera 3 — deliberately a thinner file, and future
  characters may run longer or shorter). The popup shows every unlocked entry
  plus ONLY the next locked fact. Escalating prices **50 CC per step, capped at
  300 CC** (Ep1: 50 / 100 / 150 / 200; 500 CC per full dossier).
- **Rewards:** items and Case Kit specials ONLY, themed to the character.
  **No energy ever** (CC→energy ban) and **no ingots in v1** (a CC→premium
  conversion would leak against IAP). Calibration rule: reward shop-value ≤
  half the fact price — the player pays for the lore; the reward is a thank-you.
- **Spoiler gating:** each character's final fact needs `e1.phase1.complete`+
  episode resolution (`aq.lead.e1_close.seen`) in addition to CC — keeps
  case-outcome facts safe and gives dossiers life after the credits. Season-arc
  material (Ferryman, the truth about Thomas) NEVER appears in Ep1 dossiers.
- **Completion reward:** finishing a dossier grants that character's
  **Tip-Line Cassette** — their voice as a keepsake. Dot's exists today (her
  goodnight); Mo/Del/Gerald cassettes ride the VO recording queue; until a
  clip exists the completion grants the listed special and reserves the
  cassette slot ("tape pending" state).
- **Economy shape:** full Ep1 set = 2,800 CC. Deliberately MORE than one
  episode's income after shop and locker spending — completing the wall is a
  long-tail goal, not a day-one checkbox.
- **Persistence:** PlayerPrefs JSON (precedent: specials, shop); flagged for
  the eventual save-aggregate consolidation pass alongside them.
- **Analytics:** `dossier_fact_unlocked` (character, index, price, balance).

**Writing rules for facts:** 1–3 sentences, noir register, no em dashes, no
exclamation marks, Ep1-spoiler-safe. Every line below is bible-sourced;
anything that adds connective tissue beyond the bible is flagged
**[INVENTION — approve/edit/cut]**.

---

## Ep1 fact sets (the actual content, for approval)

### ALLY QUINN — free intro
*Host of Echoes of Havenbay. Licensed investigator. Tells the city what it won't tell itself.*

| # | CC | Fact | Reward |
|---|---|---|---|
| 1 | 50 | Born March 30th. An Aries, which Gerald says explains everything and excuses nothing. **[INVENTION: the Gerald quip]** | Coffee and Donut (food T3) |
| 2 | 100 | The silver locket she never takes off holds a photo of her father. Thomas Quinn wrote for the Gazette. He died when she was thirteen. | Recorder & Headphones (audio T3) |
| 3 | 150 | Her favourite film is Spotlight. Her favourite food is pancakes at a late-night diner. She maintains these are professional influences. **[INVENTION: last line]** | Box Knife (special) |
| 4 | 200 · gated | The PI licence is real. She just knows that people talk to a podcaster long before they talk to an investigator. | Search Warrant (special) |
| ✔ | — | Completion: Carbon Copy + cassette slot reserved (Ally cassette TBD — her voice is the show itself; maybe the pilot episode stinger?) | |

### GERALD QUINN — free intro
*Retired Brookford PD detective. Ally's grandfather, first reader, and favourite bad influence.*

| # | CC | Fact | Reward |
|---|---|---|---|
| 1 | 50 | Thirty years a detective. He taught Ally what to write down, when to shut up, and how to spot a lie. | Beer Bottle (bar T4) |
| 2 | 100 | The corner booth at the Rusty Anchor is held for him without asking. The older dockhands remember what kind of cop he was, and that credit extends to Ally. | Wine Glass Red (bar T5) |
| 3 | 150 | The teal suit and the magenta shirt are not a phase. He says a detective should be memorable to friends and forgettable to suspects, and refuses to explain further. **[INVENTION: the sayings]** | Bolt Cutters (special) |
| 4 | 200 · gated | He checks Ally's ribs when he hugs her. Old habit. He has been inspecting her for damage since she was thirteen. **[INVENTION: extends the L4 rib-check canon into a habit]** | Carbon Copy (special) |
| ✔ | — | Completion: cassette slot reserved (Gerald VO pending) + Skeleton Key | |

### MO CALLAHAN — free intro
*Owner of the Rusty Anchor. Keeper of the neutral ground and everything said across it.*

| # | CC | Fact | Reward |
|---|---|---|---|
| 1 | 50 | Third generation behind the bar. Patrick Callahan opened the doors in 1924; her father Seamus salvaged a shipwreck anchor, bolted it by the door, and renamed the place around it. | Tall Glass Orange (bar T3) |
| 2 | 100 | House rule older than she is: violence goes outside, whoever you are. Everyone trusts the rule. That is why Mo hears everything. | Champagne Flute (bar T6) |
| 3 | 150 | She calls exactly one person "my lovely" at a time and rations it like the good whiskey. **[INVENTION: the rationing line — bible says the endearment is hers, exclusivity per person is new]** | Box Knife (special) |
| 4 | 200 · gated | Upstairs, behind a locked door: ledgers, old matchbooks, and a shoebox of favours owed. Mo collects debts patiently, the way the tide does. **[INVENTION: the simile]** | Evidence Tag (special) |
| ✔ | — | Completion: Mo cassette (VO pending) + Search Warrant | |

### DEL CRUZ — free intro
*Sergeant, Harbor Ward precinct. Calls Ally "Quinny". Nobody else is allowed.*

| # | CC | Fact | Reward |
|---|---|---|---|
| 1 | 50 | The kestrel on her right wrist points home to Kestrel Point. The Saint Michael medallion hangs on its own chain, never the one with the badge. | Evidence Bag (forensic T2) |
| 2 | 100 | Her table outside the Kestrel Corner Diner has the best sightlines on the street. Back to the wall, eyes on the crossing and everyone's hands. Coffee makes you a friend at that table. | Hot Coffee Cup (food T2) |
| 3 | 150 | At twenty-three, a rookie constable, she stood on the Quinn doorstep the night Thomas died. Ally was thirteen. Del has been quietly watching out for her ever since. | Search Warrant (special) |
| 4 | 200 · gated | She bends rules the way a locksmith bends wire: precisely, quietly, and only for doors that should never have been locked. **[INVENTION: the whole line — a character thesis, please rule]** | Skeleton Key (special) |
| ✔ | — | Completion: Del cassette (VO pending) + Bolt Cutters | |

### DOT ELLIS — free intro
*Retired school cleaner, Rivermouth. For three years she said goodnight to the whole city, one voicemail at a time.*

| # | CC | Fact | Reward |
|---|---|---|---|
| 1 | 50 | Thirty years cleaning the Chandler Road school. She knows every floorboard in Rivermouth and precisely which gulls are troublemakers. **[INVENTION: the gull census]** | Paper Cup (food T1) |
| 2 | 100 | Tuesday quiz night at the Rusty Anchor, table six. She has never once missed it. | Tall Glass Orange (bar T3) |
| 3 | 150 | The garden behind the blue gate is the most loved thing on Chandler Road. Hollyhocks, roses, and a fence you could walk with your eyes shut. | Coffee and Donut (food T3) |
| 4 | 200 · gated | She went out through the allotments and caught the first bus of the morning, and told nobody, to keep her sister out of it. Brave, dry, and allergic to fuss. | Evidence Tag (special) |
| ✔ | — | Completion: **Dot's cassette — SHIPS TODAY** (her goodnight voicemail already exists as a clip) | |

### VERA — free intro (3 facts; a deliberately thin file)
*Dot's sister. Larkhill, the hill cottage. Says hello, which for Vera is a parade.*

| # | CC | Fact | Reward |
|---|---|---|---|
| 1 | 50 | The hill cottage above Larkhill: one kettle, two chairs, and a view worth the climb. **[INVENTION: the inventory]** | Hot Coffee Cup (food T2) |
| 2 | 100 | She and Dot speak in a shorthand fifty years deep. Half their sentences do not need finishing. **[INVENTION: whole line, though it dramatizes established sisterhood]** | Wine Glass Red (bar T5) |
| 3 | 150 · gated | When Dot needed to disappear, Vera asked no questions. That is the entire family style. | Bolt Cutters (special) |
| ✔ | — | Completion: cassette slot reserved (Vera VO pending) + Carbon Copy | |

---

## Rulings — CLOSED 2026-08-11

All six open rulings answered by Stephen; see the banner at the top of this
document. Nothing here remains open. Future work notes: Ally's completion
cassette concept still wants an idea (pilot stinger? Thomas's old voicemail,
someday); Mo/Del/Gerald/Vera cassettes ride the VO recording queue.
