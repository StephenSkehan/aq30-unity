# Episode 1 — The Listener · VO Recording Manifest

*v1 · 2026-07-12 · the one-stop session sheet. Reconciled against `episode-1-the-listener-VO-LOCK-v1.0` (master) and the shipped engine dialogue assets (`Assets/Content/TheListener/Dialogue/`). Every spoken line below is verbatim from the locked master.*

## Session summary

| Session | Cast | Voiced cues | Est. runtime | Notes |
|---|---|---|---|---|
| A | **Ally Quinn** | 13 | ~4–4.5 min unique VO | First-person podcast narration; close mic, one listener |
| B | **Dot Ellis** (78) | 7 (one sitting) | ~1 min unique VO | Warm, dry, unhurried; never feeble. Voicemail register |
| — | **SFX / sound design** (not voice actors) | 4 | — | E1-02, E1-17, E1-18, E1-19 — see §4 |

Unique spoken VO ≈ 515 words. Both branch variants (E1-11 **and** E1-12) are recorded in Ally's session. Dot's six in-cottage scenes are **unvoiced** — do not record this session (v1.x "fully voiced" pickup candidates only).

**Engine target:** each `vo_*.wav` drops into the matching dialogue node's `voiceClip` field (currently `{fileID: 0}`) in the named asset. The filename convention `vo_{ep}_{lead}n{node}_{speaker}` maps 1:1 — e.g. `vo_e1_l1n3_ally` → `Resolve_E1_Tip` node 3. ARC clips load into the archive lead's diegetic playback list.

---

## 1. Session A — Ally Quinn (13 cues)

| Cue | Engine slot | Direction | Est | Filename | Branch |
|---|---|---|---|---|---|
| E1-03 | Tip · n3 | polished host voice, fatigue underneath | 0:18 | `vo_e1_l1n3_ally` | — |
| E1-04 | Tip · n4 | remorse under restraint — guilt real, control professional | 0:19 | `vo_e1_l1n4_ally` | — |
| E1-05 | Tip · n5 | resolve; returning to host register | 0:14 | `vo_e1_l1n5_ally` | — |
| E1-06 | Pod1 · n1 | host register, steadier than she feels | 0:15 | `vo_e1_l5n1_ally` | — |
| E1-07 | Pod1 · n2 | level, factual | 0:13 | `vo_e1_l5n2_ally` | — |
| E1-08 | Pod1 · n3 | direct appeal | 0:12 | `vo_e1_l5n3_ally` | — |
| E1-09 | Pod1 · n4 | quiet steel; to one particular listener | 0:13 | `vo_e1_l5n4_ally` | — |
| E1-10 | Close · n1 | host register, warmer than the opening | 0:19 | `vo_e1_l12n1_ally` | — |
| E1-11 | Close · n2a | owning the cost of the loud road | 0:17 | `vo_e1_l12n2a_ally` | **PUBLIC** |
| E1-12 | Close · n2b | owning the cost of the quiet road | 0:15 | `vo_e1_l12n2b_ally` | **PROTECTED** |
| E1-13 | Close · n3 | plain; a rule being made, not a speech | 0:11 | `vo_e1_l12n3_ally` | — |
| E1-14 | Close · n4 | intimate and controlled — not a costume change | 0:14 | `vo_e1_l12n4_ally` | — |
| E1-16 | Close · n6 | four words, then the promise. Let the pause do the work | 0:07 | `vo_e1_l12n6_ally` | — |

**Lines (verbatim, locked master):**

- **E1-03** — My show ends the same way every week: I read out the tip-line number and say, tell me what this city won't. The line keeps voices, timestamps and nothing else. That's the promise. For three years, Dot left the last message after every episode.
- **E1-04** — Three nights ago — 3:04 on Friday morning — she left the message you just heard a piece of. Forty seconds. No words. She opened the line and let the house speak for her. It waited three days in my queue. I was busy making a show about listening.
- **E1-05** — Dot, if you can hear this: goodnight, the harbour misses you. Everyone else — this is Echoes of Havenbay. The tip line is open. This week, it's mine to answer.
- **E1-06** — This is Echoes of Havenbay, with a case alert. Her name is Dot Ellis, from Rivermouth. At 3:04 on Friday morning, someone was inside her house. She hasn't been heard from since.
- **E1-07** — Police logged a welfare check. No forced entry, no report from family, and an adult is allowed to leave home without explaining herself. They won't call it an abduction. I'm not asking them to — yet.
- **E1-08** — So we do what this show does. We knock on Rivermouth's doors ourselves — and you'll notice I'm not saying her street on this feed. If you knew Dot Ellis, the tip line is open, and it keeps no names.
- **E1-09** — One more thing, for whoever needs to hear it. She left us your footsteps. Eight of them — eight steps too slow to stay invisible. That was Friday. We're faster now.
- **E1-10** — This is Echoes of Havenbay. Dot Ellis is safe. She was found through the life she left in plain sight — a school, a parish, a bus route, and three years of messages nobody had thought to treat as evidence. She declined an interview. Her words: "I'm not content, love. I'm a person."
- **E1-11 (PUBLIC)** — On the record: the van traces to Harbourline Security — dissolved in 2011, still driving. You heard the full track when we did, and it worked, and it cost: two hundred ghosts in the queue, and the van gone before police reached it. Loud truth. I'd choose it again. I think.
- **E1-12 (PROTECTED)** — On the record: the van traces to a dissolved port-security company. Del has asked me to keep its name off this feed — and this time, I'm listening. Quiet truth cost an afternoon of silence I don't want to spend again. I'd choose it again. I think.
- **E1-13** — And one thing changes at this show, starting tonight: a voice can tell us where to look without me telling everyone else where it lives. That's the new rule. Dot wrote it; I'm just reading it out.
- **E1-14** — And now — not as a reporter, but to the man with the careful walk: this show is built on listening. This case taught me who else does. She's safe. The evidence is safe. You're the only one still hiding.
- **E1-16** — Goodnight, Dot. Goodnight, harbour. — The tip line stays open.

---

## 2. Session B — Dot Ellis (7 cues, one sitting)

*Register: warm, amused, unhurried; a woman who enjoys leaving these messages. 78 years old, sharp not feeble.*

| Cue | Engine slot | Direction | Est | Filename |
|---|---|---|---|---|
| E1-01 | Tip · n1 | warm, amused, unhurried — enjoys leaving this message | 0:14 | `vo_e1_l1n1_dot` |
| E1-15 | Close · n5 | warm; the ritual, reclaimed — teasing, means every word | 0:16 | `vo_e1_l12n5_dot` |
| ARC-1 | archive clip | everyday; mid-thought | 0:05 | `vo_e1_arc1_dot` |
| ARC-2 | archive clip | fond | 0:07 | `vo_e1_arc2_dot` |
| ARC-3 | archive clip | observational, matter-of-fact | 0:04 | `vo_e1_arc3_dot` |
| ARC-4 | archive clip | mock-offended; a joke she's made before | 0:05 | `vo_e1_arc4_dot` |
| ARC-5 | archive clip | warm; her sign-off with a flourish | 0:06 | `vo_e1_arc5_dot` |

**Lines (verbatim, locked master):**

- **E1-01** — It's Dot from Rivermouth, love. Nothing worth troubling you with — the gulls held a parliament on my roof, and the quiet boats went by twice. Say goodnight to the harbour for me, Ally.
- **E1-15** — It's Dot. Not from Rivermouth tonight — imagine that. Vera says hello, which for Vera is a parade. You found an old woman who didn't want a fuss and you made one anyway. Good girl. Say goodnight to the harbour for me, Ally.
- **ARC-1** — The gulls again, love. A full parliament, right on my roof.
- **ARC-2** — School let out early — bell went at noon and the little ones sang all the way down the road.
- **ARC-3** — Bridge sang twice tonight. Boats running late.
- **ARC-4** — The quiet boats went by again. No lights. Rude, I call it.
- **ARC-5** — Say goodnight to the harbour for me. And the lighthouse, while you're at it. It gets left out.

---

## 3. Recording spec (both sessions)

- **Format:** WAV, 48 kHz / 24-bit, **mono**, uncompressed. One file per cue, named **exactly** as the filename column (lowercase, underscores, no extension in the name field — `vo_e1_l1n3_ally.wav`).
- **Mic:** close, intimate podcast distance; pop filter; dry room, minimal reflection. **Deliver dry** — no reverb/EQ/compression baked in; the game mixer adds space.
- **Slate:** speak the cue ID before each take ("E1-03, take one"), then ~1 second of silence before the line.
- **Takes:** 3–5 per cue. For the branch pair (E1-11/E1-12) and the intimate lines (E1-09, E1-14, E1-16), keep at least two contrasting reads.
- **Room tone:** capture 30 seconds of silence at the top of each session (noise cleanup + gap fill).
- **Levels:** peak ≈ −6 dBFS, never clipping. Watch mouth clicks and breath on the close, quiet lines.
- **Both branch variants are mandatory** and recorded in the same Ally session — the game plays one based on the player's Episode 1 choice.
- **Delivery:** all raw takes, plus one chosen take per cue in a `/selects` folder using the exact filenames above. Selects are what get imported.

**Performance red-lines (from the lock):** never announcer-voiced, never campy. E1-14 is "intimate and controlled — not a costume change" (Ally doesn't switch to a villain-address voice; she stays herself, lower). E1-16 is four words then the promise — the pause carries it. Dot's E1-15 is teasing warmth, not frailty.

---

## 4. SFX cues — sound design, NOT the voice actors

| Cue | Engine slot | Spec | Est | Filename |
|---|---|---|---|---|
| E1-02 | Tip · n2 | the 40-seconds excerpt mix, 8–12 s, abrupt cut | 0:11 | `sfx_e1_forty_excerpt` |
| E1-17 | Close · n7 | stinger: this episode playing → a click → silence → a diesel starts | 0:08 | `sfx_e1_stinger` |
| E1-18 | Forty (L2) asset | full 40-second mix **+ isolated stems** (floorboard, strained spring, diesel idle, 8 footsteps) | 0:40+ | `sfx_e1_forty_full` / `sfx_e1_forty_stem_*` |
| E1-19 | Bridge (L4) asset | swing-bridge counterweight note, **22-second loop cycle** | 0:22 | `sfx_e1_bridge_loop` |

The forty-seconds stems (E1-18) are load-bearing for the L2 evidence scene (Ally isolates each sound). The bridge loop (E1-19) must cycle cleanly at 22 seconds — Gerald counts it on-screen.

---

## 5. Reconciliation & import notes

**Actor-sides corrected today (2026-07-12) to conform to the locked master — these are lock-conformance fixes, not pickups:**
1. Dot's age **72 → 78** (canon: roster + portrait + VO-LOCK all say 78).
2. **E1-03** reworded to match the master verbatim ("For three years, Dot left the last message after every episode" — the sides had drifted to "Dot has left… for three years"). *If you actually prefer the sides phrasing, that is a formal pickup you'd log against the master — say so and I'll invert it.*
3. Ally cue-count header **14 → 13** (13 is the true count; E1-15 is Dot's).

All 20 spoken cues now verify **verbatim** against VO-LOCK v1.0.

**Open flag (your call):** the actor-sides call Dot "a 78-year-old **widow**." Canon (roster, scripts) never establishes a late husband — "widow" is an unsupported addition to the actor context note. Harmless, but remove it if unintended; it doesn't affect any line.

**Import:** the naming convention maps each selected take straight into its `voiceClip` slot; the importer sets `waitForAudio` per node once clips land (per the portrait-doc engineering note, that's my side). No dialogue-node text changes are needed — the engine `line:` fields already match the lock.
