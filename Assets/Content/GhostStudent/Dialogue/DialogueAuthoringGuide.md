# Ghost Student — Dialogue Authoring Guide

**READ THIS BEFORE AUTHORING ANY DIALOGUE ASSET.**

This guide is the authoritative reference for all Ghost Student CaseGraph dialogue in AQ30. Read it in full at the start of each authoring session. It covers: exact YAML format, portrait/emotion reference, character voice, lead type framing rules, anti-patterns, and a worked example.

---

## 1. CaseGraph Asset Format

Each lead's resolution dialogue is a `.asset` file serialised as a Unity MonoBehaviour.

### Full template with every field annotated

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 7e8140d05737a11468020c371c32ff33, type: 3}
  m_Name: Resolve_GS_<LeadName>          # Must match filename without extension
  m_EditorClassIdentifier: AQ.App::AQ.App.CaseGraph
  startId: GS_<CODE>_R1                  # ID of first node
  nodes:
  - id: GS_<CODE>_R1                     # Unique. Convention: GS_<CODE>_R<N>
    speaker: Ally Quinn                   # Exact display name — "Ally Quinn" or "Gerald Quinn"
    line: "Text here."                    # One beat per node. No line breaks.
    portrait: {fileID: 21300000, guid: <GUID>, type: 3}   # See Section 3
    emotion: 5                            # Int 0–6. See Section 3.
    voiceClip: {fileID: 0}               # Always {fileID: 0} — no VO recorded yet
    waitForAudio: 0                       # Always 0
    requiresFlag:                         # Leave empty unless node is conditional
    skipIfFlagMissing: 1                  # Always 1
    setsFlag:                             # Empty on all nodes EXCEPT the final node
    nextId: GS_<CODE>_R2                 # ID of next node. Empty string on final node.
    choices: []                           # Always [] — no branching in Ghost Student MVP
  - id: GS_<CODE>_R2
    # ... more nodes ...
  - id: GS_<CODE>_R<FINAL>
    speaker: Ally Quinn
    line: "Final beat — resonant, forward-looking."
    portrait: {fileID: 21300000, guid: 792b5f88f6dd2724d84b6954cc0b5fad, type: 3}
    emotion: 0
    voiceClip: {fileID: 0}
    waitForAudio: 0
    requiresFlag: 
    skipIfFlagMissing: 1
    setsFlag: aq.lead.gs_<leadId>.seen   # FINAL NODE ONLY. Always set this flag.
    nextId:                              # Empty — end of dialogue
    choices: []
```

### Rules

- `m_Script.guid` is always `7e8140d05737a11468020c371c32ff33` for every CaseGraph asset. Never change this.
- `startId` must exactly match the `id` of the first node in the list.
- `nextId` is an empty string (not a YAML null, not `~`) on the final node.
- `setsFlag` is empty on all nodes except the last. The last node always sets `aq.lead.gs_<leadId>.seen`.
- `requiresFlag` and `setsFlag` are trailing empty values (no quotes, no null). They look like: `setsFlag: ` with a space after the colon, or just `setsFlag:` — both work.
- `choices: []` always. No exceptions in the Ghost Student arc.
- `voiceClip: {fileID: 0}` always. No voice assets recorded yet.

---

## 2. Node ID Conventions

| Lead | Code | Node ID pattern |
|------|------|-----------------|
| gs_tip | TIP | GS_TIP_R1, GS_TIP_R2, … |
| gs_roommate | ROOM | GS_ROOM_R1, … |
| gs_lecturer | LEC | GS_LEC_R1, … |
| gs_records | REC | GS_REC_R1, … |
| gs_phase1_pod | P1P | GS_P1P_R1, … |
| gs_admin | ADM | GS_ADM_R1, … |
| gs_money | MON | GS_MON_R1, … |
| gs_other_ghost | OGH | GS_OGH_R1, … |
| gs_gerald | GER | GS_GER_R1, … |
| gs_thomas_connection | THO | GS_THO_R1, … |
| gs_find_maya | FMY | GS_FMY_R1, … |
| gs_case_close | CC | GS_CC_R1, … |

---

## 3. Portrait GUIDs and Emotion Reference

### Emotion enum (integer field)

| Value | Name | When to use |
|-------|------|-------------|
| 0 | neutral | Determined close. "I know what I have to do." Final nodes. |
| 1 | happy | Genuine breakthrough moment. Use sparingly. |
| 2 | sad | Grief. Loss. When something lands hard. |
| 3 | angry | Ally cornered or lied to. Use sparingly — she controls it. |
| 4 | surprised | Revelation. Information that reframes everything. |
| 5 | worried | Tension. Uneasy. Something's wrong and she knows it. |
| 6 | confused | Processing contradictory information. Piecing together. |

### Ally Quinn portrait GUIDs

All confirmed from existing working assets. Each GUID corresponds to a different pose/framing; the `emotion` field drives the expression on top of the pose.

| GUID | Pose | Canonical use |
|------|------|---------------|
| `e02460f0c092dde4891723eecbbdc368` | Podcast/narration — slight profile, recording stance | Opening nodes of Podcast-type and Interview-type leads. Emotion 5 (worried). |
| `ae0c3985c1991244d8ee72fa0756eb08` | Alternate expression — more direct gaze | Mid-dialogue revelation beat. Emotion 6 (confused) or 4 (surprised). |
| `7e03a4f198056f44ba2209764d9e8219` | Slightly different angle — caught off-guard feel | Reaction beats, especially emotion 4 (surprised). |
| `792b5f88f6dd2724d84b6954cc0b5fad` | Standard neutral — forward, composed | **Final node of every Ally lead.** Emotion 0 (neutral). |

**Default Ally authoring pattern (3-node lead):**
- Node 1: `e02460f0c092dde4891723eecbbdc368`, emotion 5
- Node 2: `ae0c3985c1991244d8ee72fa0756eb08` or `7e03a4f198056f44ba2209764d9e8219`, emotion 4 or 6
- Node 3: `792b5f88f6dd2724d84b6954cc0b5fad`, emotion 0

**For 4+ node leads** repeat the mid-dialogue portrait on additional middle nodes, vary between `ae0c3985...` and `7e03a4f1...` to create visual rhythm.

### Gerald Quinn portrait GUIDs

| GUID | Use |
|------|-----|
| `c3a4b5d6e7f8c3a4b5d6e7f8c3a4b5db` | Gerald speaking. All Gerald nodes. Confirmed in Resolve_GS_Gerald.asset. |

Gerald only appears in: gs_gerald, gs_thomas_connection (and potentially gs_money if needed). When Gerald speaks, use his GUID for his nodes; Ally's GUID for her response nodes.

---

## 4. Character Voice Rules

### Ally Quinn

**Core voice:** Sharp but never cruel. Empathetic but never sentimental. She trusts evidence over emotion — but the emotion leaks out anyway, which is the point.

**What she does:**
- Opens with observation, not explanation. She reports what she saw, what she felt, what she noticed. The significance follows from the detail.
- Speaks in past tense for podcast/narration leads (she's recounting to her audience).
- Speaks in present tense for Interview and Discuss leads (she's in the scene).
- Closes on a forward beat — a question, a new thread, a decision.
- Uses concrete nouns. People have names. Places have names. Times are specific.

**What she never does:**
- Explains things she already knows for the audience's benefit. ("Maya Chen, a ghost student, is someone who...") That's exposition. Cut it.
- Lectures. If she knows something, she says what she's going to do with it, not what it means.
- Gets camp. No wry quips, no genre-knowing winks. She's genuinely scared and genuinely determined.
- Repeats information from a previous lead in the same words. She builds on it.

**Sample voice checks:**

Good: *"He wouldn't say the name out loud. But when he walked me to the door he left a notepad face-down on the desk. I need to know whose name was on it."*

Bad: *"Professor Drake, who was Maya's trusted lecturer, revealed that someone in the faculty was selling biometric data. This is a major clue that will help me find Maya."*

---

### Gerald Quinn

**Core voice:** Soft-spoken. Measured. Every word chosen. He carries grief like a coat he never takes off — you don't hear it, you feel it. He was a detective. He respects evidence. He respects Ally because she is her father's daughter.

**What he does:**
- Speaks in short, precise sentences. No rambling.
- Addresses Ally directly — "you", "your father". Never generic.
- References Thomas obliquely. The loss is always present.
- Occasionally uses mild formal register — slightly archaic but not theatrical. ("I haven't shown anyone these." Not: "I've kept this hidden for years, waiting.")
- Sits in one emotional register per scene. He doesn't swing between fear and hope. He's grave. Or he's determined. Not both.

**What he never does:**
- Over-explains the conspiracy. He drops the thread; Ally pulls it.
- Acts surprised. He knew this was coming.
- Comforts Ally directly. He trusts her.

**Sample voice checks:**

Good: *"Your father looked into something like this. 2008. A private outfit funnelling money through Havenbay — Voss Bio, they were calling it then. Thomas thought it was a grant. Took him six months to work out it was a trade."*

Bad: *"Gerald looked at Ally with tired eyes. 'There are dark forces in this city, Ally. Your father discovered the truth and they silenced him. Now it's up to you to finish what he started.'"*

---

### Supporting characters (Roommate, Lecturer, Admin, Other Ghost, Maya)

These characters currently have no portrait assets. Until portraits are created, **do not add them as speakers**. All scenes involving these characters are filtered through Ally's narration — she reports what they said and did.

**Exception: Maya in gs_find_maya.** Maya gets one or two direct-speech lines at the emotional peak of the case, also filtered through Ally's reporting frame. Use speaker "Ally Quinn" for narration; for Maya's quoted words use em-dash inline in the line: *"She looked at me. Said: 'I thought no one was looking.'"*

---

## 5. Lead Type Framing Rules

### Podcast-type leads (ActionType: 5)
*gs_tip, gs_phase1_pod, gs_case_close*

Framing: Ally is recording her podcast. She's telling an audience. Past tense. Reflective. She draws conclusions aloud.

- Open with scene-setting. Where is she? What is she holding? What is she about to reveal?
- Mid-nodes: the revelation, building fact by fact.
- Close: the hook. What she still doesn't know. What she must do next.
- Voice-over feel. No other speakers. Ally only.
- gs_phase1_pod and gs_case_close are "chapter markers" — they sum up and launch what comes next.

### Interview-type leads (ActionType: 1)
*gs_roommate, gs_lecturer, gs_other_ghost*

Framing: Ally has just interviewed someone. The scene happened; she's reporting it. Past tense narration with present-tense quotes from the witness.

- Node 1: Setting the witness. Who is this person? One detail that reveals their fear or stakes.
- Node 2: The key thing they said or showed. Specific. One fact, not a summary.
- Node 3 (final): What Ally is going to do with that fact. Forward-looking.
- No other speakers. Even when quoting witnesses, the speaker is "Ally Quinn" and the quote is inside the line text.

### Evidence/Data-type leads (ActionType: 0 / 2)
*gs_tip (Evidence), gs_thomas_connection (Evidence), gs_records (Data)*

Framing: Ally has examined a piece of evidence or data. Analytical tone. What did she find? What does it mean? What's the gap?

- Node 1: What she was looking at. The object or dataset, described concretely.
- Node 2: The pattern or anomaly. "Too perfect." "Same name appearing three times." Show, don't explain.
- Node 3 (final): The next step or the question she can't yet answer.

### Money Trail leads (ActionType: 4)
*gs_money*

Framing: Financial investigation. Follow the flow of money. Technical but clear — Ally is smart, not jargon-heavy.

- She traces a path. Route → shell company → implication.
- One node per step in the trail.
- The trail leads to a person or institution she must confront.

### Location leads (ActionType: 3)
*gs_find_maya*

Framing: Ally is physically somewhere. Present-tense narration (she's doing this now or the memory is vivid). Sensory detail.

- Open with where she is. Concrete. Time of day, what she sees.
- The discovery moment. No build-up — get to it.
- The emotional weight lands on the person she finds, not on the setting.

### Discuss leads (ActionType: 6)
*gs_gerald (Evidence+Gerald), gs_thomas_connection (Evidence)*

Framing: Ally and Gerald together examining something. Can have Gerald as speaker. Multi-speaker scenes.

- Nodes alternate: Gerald says, Ally reacts, Ally decides.
- Gerald opens — he has the information.
- Ally closes — she has the decision.
- 3–4 nodes. Not more.

---

## 6. Flag Conventions

### Lead seen flag (setsFlag on final node)
Always: `aq.lead.gs_<leadId>.seen`

| Lead | setsFlag |
|------|----------|
| gs_records | `aq.lead.gs_records.seen` |
| gs_phase1_pod | `aq.lead.gs_phase1_pod.seen` + `gs.phase1.complete` |
| gs_admin | `aq.lead.gs_admin.seen` |
| gs_money | `aq.lead.gs_money.seen` |
| gs_other_ghost | `aq.lead.gs_other_ghost.seen` |
| gs_thomas_connection | `aq.lead.gs_thomas_connection.seen` + `aq.season.thomas_connection_made` |
| gs_find_maya | `aq.lead.gs_find_maya.seen` + `aq.gs.maya_found` |
| gs_case_close | `aq.lead.gs_case_close.seen` + `gs.ep01.complete` |

When a node sets multiple flags, use a single space-separated string: `setsFlag: gs.phase1.complete aq.lead.gs_phase1_pod.seen`

**Important:** `gs.ep01.complete` is the flag that triggers `CaseResolutionService`. It must be set exactly, no typos.

### NarrativeFlags in LeadData vs. dialogue setsFlag
These are two separate systems.
- `LeadData.NarrativeFlags[]` — set automatically by LeadsRepository when a lead is **activated** (player taps Resolve). Format: `gs.<leadId>.activated`.
- Dialogue `setsFlag` — set by the dialogue runner when a **dialogue node completes**. Format: `aq.lead.<leadId>.seen`.

Do not confuse them. A `requiresFlag` in a dialogue node must check a flag set by a dialogue node (via setsFlag), not a LeadData NarrativeFlag.

---

## 7. Anti-Patterns

**Do not do any of these:**

1. **Exposition dumps.** Ally doesn't explain what ghost students are to herself. She reacts to what she discovers.

2. **Repeating known information.** If Priti confirmed Maya had records (gs_roommate), the records lead doesn't recap that. It builds forward.

3. **Passive or vague lines.** "There seems to be something going on at the university." → Cut. Every line should assert something specific.

4. **Camp or genre-awareness.** "This is just like something out of a crime novel." → Never. The world is real to Ally.

5. **Emotional signposting.** "She felt a wave of sadness wash over her." → No. Use the emotion field and portrait. The line itself stays external and factual.

6. **Speaker: Ally Quinn saying things Ally wouldn't say.** Ally doesn't speak to camera. She doesn't explain her own investigation to herself. Every line should be speakable by a real person in a real situation.

7. **Mismatched lead type and voice.** A Podcast-type lead that reads like an in-person confrontation. An Interview-type lead without the witness-framing device. Match register to lead type.

8. **Breaking the tense.** Podcast and Interview leads are past tense. Location leads can be present tense. Don't mix within a scene.

9. **Null resolutionDialogue.** Every lead must have its dialogue wired before the sprint ends. The auditor (Days 13–14) will flag any `{fileID: 0}` as an error.

10. **Null portrait.** Every node must have a valid portrait GUID. Never `{fileID: 0}` on the portrait field.

---

## 8. Node Length Guide

| Nodes | When to use |
|-------|-------------|
| 3 | Standard. Most Interview and Evidence leads. |
| 4 | Emotionally important scenes with two speakers (Gerald scenes). Podcast chapter markers. |
| 5 | Case close only. The finale earns the space. |

Do not write more than 5 nodes. If a scene needs more, the dialogue is doing too much work — split the information across two leads, or trust the player to infer.

Each line: one beat. One revelation or reaction. If a line is making two points, it needs to be two nodes.

---

## 9. Worked Example — Good vs. Bad

### Scene: gs_gerald (The Gerald Scene)

This is the scene where Gerald reveals Thomas Quinn was investigating Voss Bio years earlier.

---

**GOOD — Resolve_GS_Gerald.asset (actual, authored)**

Node 1 (Gerald, emotion 5 — worried/grave):
> *"Your father looked into something like this. 2008. A private outfit funnelling money through Havenbay — Voss Bio, they were calling it then. Thomas thought it was a grant. Took him six months to work out it was a trade. Students for data. Bodies for numbers."*

Node 2 (Ally, emotion 4 — surprised):
> *"He never told me. Not a word. I've been through his case files — nothing about Havenbay, nothing about a Voss anything. Gerald, where are these files?"*

Node 3 (Ally, emotion 0 — neutral/determined):
> *"There's a lockbox. The one my father kept under the stairs — the one I was never supposed to open. I think I know where the key is. If Thomas found them before, I can find them again."*
> setsFlag: aq.lead.gs_gerald.seen

**Why it works:**
- Gerald drops specific facts (2008, "Voss Bio", "grant to trade") without over-explaining.
- Ally's reaction is her specific personal gap ("I've been through his files — nothing"). Not a generic surprised reaction.
- The close is forward-action: Ally has a new lead (the lockbox). She doesn't summarise; she decides.
- Three clean beats: reveal → personal landing → decision.

---

**BAD — Same scene done wrong**

Node 1 (Gerald, emotion 5):
> *"Ally, your father Thomas Quinn was a great detective. He once investigated a company called Voss Bio that was doing illegal things at Havenbay University. They were selling student data. This was a long time ago, back in 2008."*

Node 2 (Ally, emotion 4):
> *"Oh no, I had no idea! This is shocking. My father never told me about any of this. It makes me feel very sad and worried. What do I do now?"*

Node 3 (Ally, emotion 0):
> *"I need to look into this more. The ghost student case is clearly connected to something much bigger. I'll have to keep investigating to find the truth about Voss Bio and what happened to Maya."*
> setsFlag: aq.lead.gs_gerald.seen

**Why it fails:**
- Node 1: Gerald explains. He's briefing the audience, not talking to Ally. He uses Thomas's full name (he'd never say "Thomas Quinn" to his daughter). He uses "illegal things" — vague. He adds "This was a long time ago, back in 2008" — explaining the year after stating it.
- Node 2: Ally names and processes her own emotion out loud. The line "It makes me feel very sad and worried" is signposting. Real emotion shows in what she asks for, not what she names.
- Node 3: Generic forward beat. "Keep investigating to find the truth" — this is every detective show. Ally's actual close is specific: *a lockbox. Under the stairs. A key.*

---

## 10. Pre-Authoring Checklist

Before writing any dialogue asset, answer these:

- [ ] What type is this lead? (Podcast / Interview / Evidence / Data / Money / Location / Discuss)
- [ ] Who speaks? (Ally only, or Ally + Gerald?)
- [ ] What does the player learn in this scene that they didn't know before?
- [ ] What is the forward beat at the end — what does Ally do next?
- [ ] What portrait and emotion combination opens the scene?
- [ ] What is the setsFlag on the final node?
- [ ] Have I checked the node count is ≤ 5?
- [ ] Does each line contain exactly one beat?

---

## 11. Dialogue Nodes Per Lead (Sprint Plan Reference)

| Lead | Nodes | Key beats |
|------|-------|-----------|
| gs_records | 3 | Access enrollment system → attendance too perfect → ghost scheme is systematic |
| gs_phase1_pod | 4 | Recording an episode update → recap what roommate + lecturer + records revealed → "Someone is running Maya's life remotely" → Phase 2 hook |
| gs_admin | 4 | Request meeting → Hartley deflects → inconsistency spotted → "He's already making calls" |
| gs_money | 3 | Trace tuition → Voss Bio Holdings → "I need Gerald" |
| gs_other_ghost | 3 | Find the other ghost → Ally's approach → scheme is institutional |
| gs_thomas_connection | 4 | Gerald opens folder → Ally sees Voss Bio in Thomas's notes → Gerald's quiet vow → Ally's answer |
| gs_find_maya | 4 | Arrive at maintenance office → Maya found → "How did you find me?" → "I was looking" |
| gs_case_close | 5 | Recording → exposé → shadow/season hook → Maya's message → sign-off |

---

*Last updated: Sprint 3, Days 3–4 (2026-06-14)*
*Read this before starting any session where you author dialogue. Do not shortcut character voice.*
