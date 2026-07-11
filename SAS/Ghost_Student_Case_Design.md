# The Ghost Student — Case Design Document
**Status:** In progress (design discussion 2026-05-03)
**Episode:** Ep01 — the vertical slice demo case

---

## Narrative

### Premise
Maya Chen is a scholarship student at Havenbay University. Three weeks ago she stopped coming to class. But according to the university, she was in a lecture this morning.

Before she vanished, Maya sent one encrypted message to the only lecturer she trusted: *"They're selling us."* The message was intercepted. Maya disappeared the next day.

What she'd discovered: the university has been running a ghost student service for years. Wealthy families pay to keep dropout children "enrolled" on paper — their academic activity cycled through real scholarship students who are paid or quietly coerced to carry the ghost. When Maya threatened to expose it, she became the latest person to go quiet.

She's alive. Barely. And she left a trail — if Ally can read it before the people who erased her notice someone's looking.

### The Twist
The ghost scheme isn't operated by one rogue administrator. It's institutional — protected by layers of the university's own bureaucracy. The money flows through a shell company that Ally eventually recognises from her father's old notes. Havenbay University is already inside the Ferryman's orbit. Maya's case is the first thread that leads Ally back toward Thomas Quinn's unfinished investigation.

### Phases

**Phase 1 — What happened to Maya?**
Ally establishes that Maya existed, discovered something, and vanished deliberately — not randomly. Closes with: *"Someone is running Maya's life remotely. They have institutional access. And they know I'm looking."*

**Phase 2 — Who's running the ghost, and where is Maya?**
Ally traces the financial structure of the scheme, confronts those with the access to run it, and locates Maya before the people operating the ghost realise the investigation is closing in.

### Resolution
Maya is found and brought to safety. The ghost student scheme is exposed on Ally's podcast. The university administrator responsible resigns. The shell company connection is aired — obliquely — as a season hook. Maya's voice plays at the end of the podcast episode: her original encrypted message, finally heard in full.

### Characters
| Character | Role |
|---|---|
| Ally Quinn | Protagonist / podcaster |
| Maya Chen | Victim — scholarship student, working-class Havenbay local, stubborn and principled |
| Marcus (Maya's brother) | The tip source — contacts Ally when official channels fail him |
| Dr. [TBD] — The Trusted Lecturer | Received Maya's message, scared, knows more than they're saying |
| The Roommate | Saw Maya change in the weeks before she vanished |
| Administrator [TBD] | Runs the ghost scheme — protected by the institution |
| The Other Ghost | A scholarship student still trapped in the scheme — key witness, branch point |
| Gerald Quinn | Ally's grandfather — recognises something in the money trail from his own old case files |

---

## Lead Types

### Full type list (post-MVP candidates included)

| Lead Type    | Player Fantasy                                      | Typical Req Count |
|--------------|-----------------------------------------------------|-------------------|
| Evidence     | "I found something physical."                       | 1–3               |
| Interview    | "I'm confronting someone."                          | 2–3               |
| Location     | "I'm searching somewhere new."                      | 2–3               |
| Timeline     | "I'm proving what happened."                        | 3                 |
| Data         | "I'm decoding hidden information."                  | 2–3               |
| Alibi        | "I'm checking someone's story."                     | 3                 |
| Money Trail  | "I'm following corruption."                         | 2–3               |
| Stakeout     | "I'm watching and waiting."                         | 2–3               |
| Lab          | "I'm turning clues into proof."                     | 2–3               |
| Podcast      | "I'm turning the case into an episode."             | 2–3               |
| Discuss      | "Talking things over with a confidant."             | 1–3               |

**Notes:**
- Discuss = personal lead with a confidant (Gerald, Helen). Witnesses and sources are always Interview type.
- Podcast = phase-closing or case-closing lead. Marks serious narrative progress.
- All types support the full reward difficulty range (Easy / Standard / Hard / Very Hard).
- Visual differentiation: unique icon per type. Colours TBD.

### MVP subset for Ghost Student (Ep01)

| Lead Type   | Used in Ghost Student |
|-------------|----------------------|
| Evidence    | Yes — Phase 1 opener, Thomas connection |
| Interview   | Yes — Roommate, Lecturer, Administrator, Other Ghost |
| Data        | Yes — Academic records |
| Location    | Yes — Find Maya |
| Money Trail | Yes — Ghost scheme payments |
| Podcast     | Yes — Phase 1 close + Case close |
| Discuss     | Yes — Gerald (money trail recognition) |

**7 types for MVP.** Timeline, Alibi, Stakeout, Lab deferred to post-MVP.

---

## Lead Tree — Ghost Student

### Phase 1: What Happened to Maya?

| # | Lead ID | Title | Type | Reqs | Reward | Spawns |
|---|---------|-------|------|------|--------|--------|
| 1 | `gs_tip` | The Encrypted Message | Evidence | 1 | Easy | `gs_roommate`, `gs_lecturer` |
| 2 | `gs_roommate` | Maya's Roommate | Interview | 2 | Standard | sets flag → `gs_records` |
| 3 | `gs_lecturer` | The Trusted Lecturer | Interview | 2 | Standard | sets flag → `gs_records` |
| 4 | `gs_records` | Pull Maya's Academic Records | Data | 2 | Standard | — |
| 5 | `gs_phase1_pod` | Episode Update (Pt.1) | Podcast | 3 | Hard | Phase 2 leads |

**Phase 1 gate:** `gs_phase1_pod` requires `RequiredLeadIds`: gs_roommate, gs_lecturer, gs_records (all three activated).

**Note on gs_records:** Unlocks when either gs_roommate or gs_lecturer has been activated (whichever comes second). Both interviews are needed to give Ally the context to know what she's looking for in the records.

---

### Phase 2: Who's Running the Ghost?

| # | Lead ID | Title | Type | Reqs | Reward | Spawns / Branch |
|---|---------|-------|------|------|--------|-----------------|
| 6 | `gs_admin` | The University Administrator | Interview | 3 | Standard | — |
| 7 | `gs_money` | Follow the Ghost Payments | Money Trail | 2 | Standard | `gs_gerald` |
| 8 | `gs_other_ghost` | Get the Other Ghost to Talk | Interview | 2 | Standard | **Branch** (see below) |
| 9 | `gs_gerald` | Talk It Through With Gerald | Discuss | 1 | Easy | `gs_thomas_connection` |
| 10 | `gs_thomas_connection` | Thomas's Old Notes | Evidence | 2 | Standard | `gs_find_maya` |
| 11 | `gs_find_maya` | Find Maya | Location | 3 | Hard | — |
| 12 | `gs_case_close` | "They're Selling Us" | Podcast | 3 | Very Hard | — |

**Phase 2 gate:** `gs_case_close` requires `RequiredLeadIds`: gs_admin, gs_other_ghost, gs_thomas_connection, gs_find_maya.

**Branching lead — `gs_other_ghost`:**
> "The other ghost student is terrified. How do you approach it?"
> - **Offer anonymity** → spawns `gs_ghost_anon` — slower build, safer for the witness, full corroboration
> - **Go on record** → spawns `gs_ghost_public` — faster path, but the witness is exposed and the admin is alerted sooner

Both branches converge to the same NarrativeFlag. The branch affects dialogue flavour, reward size, and the difficulty of `gs_admin` (going public tips off the administrator, making the confrontation harder).

---

### Unlock Flow (summary)

```
gs_tip
  ├── gs_roommate ──┐
  └── gs_lecturer ──┴── gs_records
                              ↓
                       gs_phase1_pod  ← all 3 Phase 1 leads done
                              ↓
           ┌──────────────────┼──────────────────┐
      gs_admin          gs_money           gs_other_ghost
                            ↓                  (branch)
                        gs_gerald
                            ↓
                  gs_thomas_connection
                            ↓
                       gs_find_maya
                              ↓
                       gs_case_close  ← leads 6, 8, 10, 11 done
```

---

## FTUE Cold Open

The game opens in medias res. Ally is already on Havenbay University campus at night, having followed the "They're selling us" fragment that arrived in her podcast tip line.

The merge tutorial is diegetic: Ally assembles tools to access a locked building (merge chain = access tools). She finds Maya's room — staged to look recently occupied, but the tells are wrong. The pillow is too neat. The coffee cup too placed. Someone was here hours ago. The ghost is active and tended.

The jeopardy: Ally realises she's being watched. She has to choose: grab the physical evidence now (risky — triggers alarm) or photograph everything and get out (safer — evidence might be moved).

This is the first agency moment. It's low stakes in narrative terms (both options advance) but establishes the tone: Ally's choices have weight, and the people she's investigating are already one step ahead.

The podcast VO closes the FTUE: *"Three weeks. That's how long Maya Chen has been a ghost. Her assignments are perfect. Her attendance is flawless. And she hasn't been to class since October 14th. Someone is running her life. And tonight, I found out they're doing it in real time. This is Echoes of Havenbay — and we need to find her before they find me."*

---

## Lead Lifecycle

### States (maps to existing LeadState enum)
- **Blocked** — not yet visible to player (gated by RequiredLeadIds or prior leads)
- **Available** — visible, requirements not yet met
- **In Progress** — at least one requirement satisfied on board
- **Ready** — all requirements satisfied, player can activate
- **(Activated)** — consumed, moves to Cork Board

### On Activation
1. Required board items consumed
2. Lead card moves to **Cork Board** (does NOT disappear — see Cork Board section)
3. Rewards granted (see Rewards section)
4. Dialogue / cutscene / minigame fires (depending on lead type)
5. NarrativeFlags set
6. Follow-up leads spawned (including branching choices — see Branching section)

---

## Cork Board

A dedicated Case Investigation view — cork board aesthetic with red strings connecting leads.

- **Access:** Available as a screen/overlay during play
- **Content:** All activated leads displayed as pinned cards connected by strings
- **Replay:** Player can tap any pinned lead to replay its dialogue, cutscene, or minigame
- **Purpose:** Lets player review evidence, revisit story beats, feel case progress accumulating
- **Scope:** TBD — separate full screen, or overlay panel?

---

## Rewards

On lead activation, rewards can include:
- **Soft Currency** (Case Cash) — existing field `SoftCurrency` on LeadData
- **Energy Grant** — existing field `EnergyGrant` on LeadData
- **Specific Items** — place item(s) directly onto the board (new — needs data + delivery system)
- **Generators** — add a new generator to the board (new — needs data + delivery system)
- **Loot Boxes** — random reward bundle drawn from a table (new — needs loot table system)

---

## Branching / Player Agency

**`gs_other_ghost` branch — "How do you approach the witness?"**
> - **Offer anonymity** → `gs_ghost_anon`: slower corroboration, witness is protected, full evidence
> - **Go on record** → `gs_ghost_public`: faster path, administrator alerted, `gs_admin` difficulty increases

Both paths converge to the same NarrativeFlag. The branch changes dialogue, reward size, and downstream difficulty of the administrator confrontation.

**FTUE agency moment** — grab evidence vs. photograph and leave. Both advance, but reward differs. Establishes agency tone early.

**Data shape needed:** `BranchOutcome[]` on LeadData, each branch with a label + `SpawnLeadIds[]`. Replaces/extends the current flat `SpawnLeadIds[]`.

**Open:** Is the branch choice surface inside the dialogue graph or a separate post-dialogue UI?

---

## Data Requirements (new fields needed on LeadData)

| Field | Type | Purpose |
|---|---|---|
| `RequiredLeadIds` | `string[]` | Declarative phase gates — lead only becomes Available when all listed leads are Activated |
| `BranchOutcome[]` | Struct with label + SpawnLeadIds[] | Replaces flat SpawnLeadIds[] for branching leads |

---

## Decisions Log

| Date       | Decision                                                                 |
|------------|--------------------------------------------------------------------------|
| 2026-05-03 | Lead types defined — 11 types total, MVP subset TBD                     |
| 2026-05-03 | Podcast = phase/case closing lead                                        |
| 2026-05-03 | All lead types support full Easy/Standard/Hard/Very Hard reward range    |
| 2026-05-03 | Unique icons per lead type; colours TBD                                  |
| 2026-05-03 | New type: Discuss (confidant conversations, 1–3 items)                   |
| 2026-05-03 | Cork Board confirmed: activated leads persist here, replayable           |
| 2026-05-03 | Rewards expanded: soft currency, energy, items, generators, loot boxes   |
| 2026-05-03 | Branching confirmed: player choices gate which follow-up leads spawn     |
| 2026-05-03 | Ghost Student Ep01 confirmed (not Kestrel Point — that becomes Ep02)     |
| 2026-05-03 | Ghost Student narrative confirmed: Maya Chen, ghost student scheme       |
| 2026-05-03 | Ghost Student lead tree: 12 leads across 2 phases                       |
| 2026-05-03 | MVP lead types: Evidence, Interview, Data, Location, Money Trail, Podcast, Discuss (7 types) |
| 2026-05-03 | Discuss leads use Ally's own confidants (Gerald, Helen) — witnesses/sources are Interview type |
| 2026-05-03 | Declarative phase gates: RequiredLeadIds[] on LeadData (not NarrativeFlag checks) |
| 2026-05-03 | Gerald Quinn confirmed as primary confidant for Discuss leads            |
| 2026-05-03 | Branch in gs_other_ghost affects downstream difficulty of gs_admin (going public tips off admin) |
| 2026-05-03 | FTUE cold open: campus infiltration at night, agency moment on evidence grab vs photograph |
| 2026-05-03 | Season hook via gs_thomas_connection: shell company links to Thomas Quinn's old notes |

---

## Open Questions

- Cork Board: separate screen or overlay? How is it accessed from the main game?
- Branch choice surface: inside dialogue graph or separate post-dialogue UI?
- Item/generator rewards: delivered directly to board, or to an inbox?
- Loot box: deferred to post-MVP, or needed for Ep01 demo?
- Character names: Dr. [TBD] trusted lecturer, Administrator [TBD]
- The three existing demo leads (StakeoutDiner, GetBethTalking, LateNightMeet) are pipeline test assets — confirm whether to retire or repurpose them as Ghost Student flavour content
