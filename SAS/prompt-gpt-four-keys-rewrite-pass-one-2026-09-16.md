# PROMPT FOR GPT · THE FRIENDS WITH FOUR KEYS · REWRITE PASS ONE · 2026-09-16

You reviewed the full dialogue script of this episode yesterday and your review has been read and ruled on. This is the first rewrite pass. It is deliberately narrow: chapter 2 in full, plus the specific scenes changed by today's rulings, plus a list of exact line fixes. Chapters 3 to 10 are NOT to be rewritten in this pass; the player who stopped reading did so halfway through chapter 2, and she reads chapter 2 again before anything else moves.

Return your work as JSON in the exact schema at the end, and nothing else outside the three JSON blocks. It is consumed by a generator, so the schema is not negotiable.

## Rulings in force (from the author, today)

1. **Scene cap.** No scene over about 120 words or 5 spoken lines. The Letter (currently 825 words, 26 lines, one scene) is split across chapter 2's ten scenes. The letter itself may run longer than the cap only in the one scene that reads it, and even there it is trimmed: the player must never face more than eight lines in one scene.
2. **Brad in chapter 4 is degraded, not moved.** The boat owner saw a young man in the corridor early with something flat under his arm, at a door he cannot name and would not swear to. Enough to file, not enough to convict. He does not say "Brad", does not say "Violet's door", does not describe a parcel carried from her door to another. Ally files it with the police as a sighting of a man, unnamed.
3. **Decision 2 rebalanced: direct costs access, sheds costs trust.** Ring Del now: the tip goes in clean, but Del closes the sheds to Ally, so she never gets the corridor again or the boat owner a second time. A day at the sheds first: she gains a second witness and the layout of the corridor for her board, Del gets it a day late, and Liam stops answering her. Both options pay and both hurt. Write the prompt, both option labels (under nine words each) and both results so that a player can honestly want either.
4. **Chapter 7 branch re-keyed to Tessa's exposure.** The two versions of chapter 7 scenes 1 and 2 are chosen by decision 3 (chapter 6: air Margo's exact words, or paraphrase to protect Tessa). Verbatim (`aq.fk.d3.verbatim`): the exact words spread, a stranger up the coast recognises the phrasing of a private message, Tessa is frightened and goes quiet on Ally. Paraphrase (`aq.fk.d3.paraphrase`): Tessa stays safe and keeps talking. The panel witnesses come in the same either way; they are no longer what the branch is about. Scene 3 of chapter 7 follows both and must read correctly after either.
5. **The café is the Kestrel Corner Diner.** Wherever the script says "the harbour café" or "the café owner", it is the Kestrel Corner Diner and the diner's owner. Chapter 2 does not visit it; the rule applies to the line fixes.
6. **Canon corrections.** Gerald Quinn is Ally's grandfather and a retired police detective, never a newsman. Del Cruz is a woman: she, her. Echoes of Havenbay is a podcast, never radio; people listen in kitchens, cars and on headphones, not "with the radio on". The false sightings after the mark aired ran for eight days, not a fortnight (the panel airs on day 325, the arrest is day 333). Episode ten airs on day 332 as the quiet episode; after the day 333 arrest Ally releases a short unscheduled update the same night and says so on air: the closing scenes of chapter 10 are that update, not a second episode.
7. **Kestrel Head** is the headland and its car park; **Kestrel Point** is its lighthouse. Both names stand.
8. **Ruby is mid thirties**, not thirty-nine. Fix the age line.

## Craft rules (house rules, unchanged)

- Ally is the dominant voice, but not the only person allowed a complete thought. In chapter 2, let Ruby collide with Ally at least twice, and let the kitchen table be experienced through Ruby's telling in exchanges, not in one Ally summary.
- Ally's restraint stays but its phrasing varies. Across chapter 2 use "I don't know" at most three times and "that's all of it" once. The ask "write to me, not to anyone else" appears only when that week's episode actually asks the city for something. The publish scene ends with the sign-off: "This is Echoes of Havenbay, I'm Ally Quinn. Goodnight for now, Harbour. Sleep tight."
- Ruby's "anyway" is a retreat from feeling: at most three in the chapter, each at a moment she is backing away from something.
- The four habits of Brad (keys moved twice on the table, thank you like it cost him, up at five to run, agreed with whoever spoke last) matter enormously later. Give them a scene of their own where they land as character, told by Ruby with affection, not as a list.
- Protect these lines wherever they occur; do not paraphrase them: "You eat under her work and don't know it." "Her letters, but not her day." "A silence, but it has a shape." "A sentence can be an address." "There are men the sea is allowed to keep, and nobody argues." "I have stopped saying 'was'." "What I described, I erased."
- Facts do not change. Every fact in the current text is canon unless a ruling above changes it; you may move facts between scenes within a chapter, cut repetition and add exchange, but you may not add a new fact about the case. Where a scene needs a fact that does not exist, leave the gap.
- Never-tolds stand: nothing that reveals Brad is alive, forged the letter or moved the panel before the chapters that reveal it; the panel is described, never depicted; no real chemical is ever named for the tin.
- Australian English. No em dashes anywhere; use commas, full stops or colons. No endearments from Ally to the player. Directive lines to the player under eight words.

## Chapter 2 scene frame (ids, order and what each scene is for)

Each scene is a package the player completes on the merge board; the scene plays when the package is done. Ten scenes, in this order, with these ids and backgrounds. Keep the ids exactly.

| id | working title | background | job of the scene |
|---|---|---|---|
| fk_p02_01 | Ruby's Kitchen | kitchen | Arrive. The table, the heater, Ruby filling the room. Short. |
| fk_p02_02 | The Note Under Glass | caseboard | The bare note, read once, exactly. Art beat, so very short. |
| fk_p02_03 | The Letter | kitchen | Ruby reads the letter, her hand over the lines about Violet's mother. Only the letter. The chapter's decision D1 sits at the end of this scene (prompt, two options, results; schema below). |
| fk_p02_04 | Ruby Walker | kitchen | Who Ruby is: nurse, talks when frightened, sober, mid thirties. Character, in exchange. |
| fk_p02_05 | Four in the Morning | kitchen | Ruby's one private memory of Violet. Short. |
| fk_p02_06 | The Room | kitchen | The night they opened the locker and the next evening at the table, in Ruby's telling with Ally pushing back: the relief, the shame of it, the key arithmetic, the alibis. |
| fk_p02_07 | Four Habits | kitchen | Brad as Ruby had him. His boxes at hers, nothing odd in them, and the four habits as affection. |
| fk_p02_08 | The Covering Slip | kitchen | The solicitor's slip in the envelope: lodged three weeks before, posted after the funeral, four addressees. Short, factual. |
| fk_p02_09 | Saturdays | kitchen | The six meetings: how the table turned on Liam week by week, Brad arguing his side and losing, Margo going quiet at the sixth, Brad gone a week later. Ruby and Ally in exchange. |
| fk_p02_10 | Episode Two | studio_onair | Ally publishes. Recap only what the city needs; the D1 variant lines at the end; sign-off. |

Speakers available in chapter 2: "Ally Quinn", "Ruby Walker". Emotions: neutral, happy, sad, angry, surprised, worried, confused.

Decision D1 (in fk_p02_03): option A flag `aq.fk.d1.aired`, option B flag `aq.fk.d1.held`. Variant nodes elsewhere use `"variant": "aq.fk.d1.aired"` or `"variant": "aq.fk.d1.held"`.

## Current chapter 2 (source text; rewrite this)

```json
[
 {
  "id": "fk_p02_01",
  "background": "kitchen",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Ruby Walker's kitchen. A pine table under the window, and a heater beside it. This is the table the four of them sat at with Violet's letter, week after week."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Before any of that it was Tuesdays. Violet came on Tuesday nights, for years, and Ruby had the heater on before she knocked."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "happy",
    "line": "Heater on, her at the door, then the same argument we'd had for years. Every Tuesday. That was the friendship. Anyway. Tea?"
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The argument was about whether Ruby's husband should be told. Told what, Ruby didn't say, and I don't know."
   }
  ]
 },
 {
  "id": "fk_p02_02",
  "background": "caseboard",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "This is Ruby's copy of the note. Four went out, all the same, two weeks after Violet died. I photographed it at the kitchen table. It's under glass on my board now. Violet's handwriting."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "sad",
    "line": "If you've got this, I'm dead, and I'm sorry about the timing. Go to my locker at the boatsheds by the slip. The padlock's new and these four keys are the only ones. Go together, all four of you, and read what's there together. Violet."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "That's all of it."
   }
  ]
 },
 {
  "id": "fk_p02_03",
  "background": "kitchen",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The letter behind the door. Violet's handwriting, dated three weeks before she died. Police examined it and gave it back. Ruby kept it. She read it to me at the table."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "There are lines in it about Violet's mother. Ruby won't read those. She put her hand over them. I haven't guessed at what's under it."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "It isn't any of you four, and I'd know. Someone with my key has been in my house, or I've started imagining things. I can't tell which, and both frighten me."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "The first thing. The yard gate. I bolt it every night. One morning in October it was unbolted, and I couldn't swear I'd bolted it."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "I changed the front door lock in October and told only you four where the new spare was. It happened again after that. So I took the key in, told nobody, and it stopped."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "I don't think I'm going to die. I think I'm going to be sixty and embarrassed. But if I'm wrong nobody needs to know, and if I'm right you'll want to know what I noticed."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "sad",
    "line": "If I'd told you now you'd have sat in my kitchen with a torch and I'd never have heard the end of it."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Here Ruby's hand goes over the page. The lines about Violet's mother. She picks up again below her hand."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "Somebody got the place of my spare from one of you without your knowing, or watched one of you use it. So write down everything odd you've noticed this year. About me, and about who's been near each of you."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "Read the four lists together at one table. Since I'm asking you to, here's mine. Liam has owned a third of the yard for eight years and lets the town think he's staff."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Her hand goes down again. One line. She won't read that one either."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "Ruby's been on nights since August and won't say why. Margo hasn't been to the slip since the summer. And Brad paid me back the van money this autumn, every penny. I put it here because it's the one thing this year that went right."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "sad",
    "line": "Not the police. I've nothing they'd act on and I won't have them in your lives over a gate and a feeling. You four can do what they won't. Read this together, and be careful. Write legibly. Violet."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "worried",
    "line": "That bit about the police. That never sounded like Vi to me. Liam said that night why she was like that about police. Something from the night her mum died. I don't know what. I let it go. I shouldn't have. Anyway."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "That's the letter. Then Ruby told me about the room, in order. The night they opened the locker, the first thing was relief. Then being ashamed of the relief. Nobody said the rest out loud that night."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The next evening, at this table, they said it. She changed her lock and told the four of them. It happened again. She took the key in, told nobody, and it stopped."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "sad",
    "line": "Liam found her. Ruby says the spare wasn't in its pot, so he broke a pane to get in, and the back door was latched."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "They looked for anyone else in Violet's life. No cleaner, no lodger, no tradesman. A sister she hadn't spoken to in years. Nobody."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "worried",
    "line": "We knew a wall could be climbed. We chose not to think it. Anyway. Saturdays after that. Every Saturday, here."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The lists Violet asked for, who'd been near each of them: nobody. Where were they on the Monday night: Ruby on shift, Margo at home alone, Liam alone at the yard, Brad at home alone. Ruby's words: nobody had an alibi, and Liam had the least."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "sad",
    "line": "She thought she'd know. And she's dead. I said that every week. I'm saying it to you now. Anyway."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Meeting by meeting, Ruby says, the letter's lines about Liam were read against him. The yard he'd kept quiet. The lines about her mother, which Ruby won't repeat. And that he'd fixed a window catch in Violet's back bedroom in spring, and the back door."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Brad argued Liam's side every week. Reasonably, Ruby says. He lost every week. By the fifth week the table had settled on Liam. That's Ruby's account. Nobody else's yet."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "worried",
    "line": "At the sixth meeting Margo stopped arguing and stopped looking at anyone. Ruby doesn't know why and won't guess. A week later Brad drove to Kestrel Head."
   }
  ],
  "decision": {
   "prompt": "Next week I have to decide what this city hears of Violet's letter.",
   "options": [
    {
     "label": "Read it whole, as Ruby read it",
     "flag": "aq.fk.d1.aired",
     "result": "I'll read it all, every line Ruby read me. They'll go out in Ruby's voice as much as Violet's, and Ruby is the one who'll carry them round this town."
    },
    {
     "label": "Air only what I can check",
     "flag": "aq.fk.d1.held",
     "result": "I'll read the parts I can stand behind from someone other than Ruby. The city hears less. Ruby keeps the rest at her own table."
    }
   ]
  },
  "after": [
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "You do what you think. I'll put the kettle on. Anyway."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The lines under her hand stay there either way."
   }
  ]
 },
 {
  "id": "fk_p02_04",
  "background": "kitchen",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Ruby Walker is thirty-nine, a practice nurse at the surgery on Wharf Street. When she's frightened, she talks. She fills the room, gives you a job, makes tea nobody asked for, and ends her sentences with anyway."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The glass in her hand is tonic. She's been sober six years. She said so early, so nobody had to ask."
   }
  ]
 },
 {
  "id": "fk_p02_05",
  "background": "kitchen",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Ruby told me this one without being asked."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "sad",
    "line": "Three years ago she rang me at four in the morning. I drove over. Neither of us ever said what it was, not that night and not after. Anyway."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "I don't know what it was. Ruby didn't say, and I'm not going to guess."
   }
  ]
 },
 {
  "id": "fk_p02_06",
  "background": "kitchen",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "When Brad Collins's rent stopped, his landlord boxed up his flat and sent the boxes to Ruby. They're at hers now."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Police had been through the flat before it was boxed. Passport, bank cards, tools, laptop. All there. All still there."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "sad",
    "line": "Ruby and I went through every box at the table, over days. Clothes, tools, paperwork. A whole flat. There's nothing odd in any of it. Days of careful nothing."
   }
  ]
 },
 {
  "id": "fk_p02_07",
  "background": "kitchen",
  "nodes": [
   {
    "speaker": "Ruby Walker",
    "emotion": "neutral",
    "line": "He'd put his keys on the table and then move them. Twice, always twice. He said thank you like it cost him. Up at five to run, every day."
   },
   {
    "speaker": "Ruby Walker",
    "emotion": "happy",
    "line": "And the one everybody knew. He agreed with whoever had spoken last. Seven years he was the pet of the room. Anyway."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "That's Brad Collins as Ruby had him. Four habits. I've written them down as she said them."
   }
  ]
 },
 {
  "id": "fk_p02_08",
  "background": "kitchen",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Ruby kept the envelope her note came in. Inside it, behind the note, there's a covering slip from Violet's solicitor."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "It says the four envelopes were lodged with the firm three weeks before Violet died, and posted after the funeral. It lists four addressees. Liam, Ruby, Margo, Brad."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "It tells me where the note came from and when. That's what it's for. It's on the board, under the note."
   }
  ]
 },
 {
  "id": "fk_p02_09",
  "background": "kitchen",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "There's a piano in Ruby's front room. The sheet music on the stand is a long way past carols."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "happy",
    "line": "She got her grade eight at fourteen. The surgery thinks she plays a bit at Christmas. She lets them."
   }
  ]
 },
 {
  "id": "fk_p02_10",
  "background": "studio_onair",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "This week I sat at Ruby Walker's kitchen table. It's pine. It's the table the four of them worked Violet's letter at, Saturday after Saturday, after they opened the locker."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Ruby read me the letter from behind the door. All of it but the lines about Violet's mother. Those she covered with her hand, and I haven't guessed at them."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "sad",
    "line": "She told me how the table went, meeting by meeting. According to Ruby, Brad argued Liam's side every week and lost every week. At the sixth meeting Margo stopped arguing. A week later Brad drove to Kestrel Head."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "When Brad's rent stopped, his landlord sent his flat to Ruby in boxes. Passport, cards, tools, laptop. Police had seen them all before. I've been through every box. There's nothing odd in any of it."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Four things about Brad Collins, from Ruby. He moved his keys on the table, twice. He said thank you like it cost him. He ran at five. And he agreed with whoever had spoken last."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "worried",
    "line": "Everything you heard tonight came from Ruby Walker, at her own table, under her own name. In a town this size, that costs something."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "variant": "aq.fk.d1.aired",
    "line": "Next week I'll read you Violet's letter whole, every line Ruby read me. They go out in Ruby's voice, and she'll carry them round this town. She knows that. This is Echoes of Havenbay, I'm Ally Quinn. Goodnight for now, Harbour. Sleep tight."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "variant": "aq.fk.d1.held",
    "line": "Next week you'll hear the parts of Violet's letter I can check with someone other than Ruby. You'll hear less. The rest stays at her table. This is Echoes of Havenbay, I'm Ally Quinn. Goodnight for now, Harbour. Sleep tight."
   }
  ]
 }
]
```

## Chapter 4 scenes to rewrite under rulings 2 and 3 (keep ids; fk_p04_08 is an art beat and may stay as is)

```json
[
 {
  "id": "fk_p04_08",
  "background": "moorings",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The slip boatsheds, twenty to six in the morning. A row of brick boat lockers along the back wall, each with its own steel door. The slip road runs past the front, where the vans park."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "There's no camera on any of it. The boat owners are here from first light. Anyone with a key and a reason can walk this corridor and nobody looks twice."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "sad",
    "line": "Violet's locker is a brick bay with a steel door. Her sister had the padlock cut. It's been empty since."
   }
  ]
 },
 {
  "id": "fk_p04_09",
  "background": "studio",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Late in the week a man rang the Tip Line. He owns a boat and works the slip before the tide. He knew Violet by sight because she lettered his boat, and he knew Brad because Brad painted his tender."
   },
   {
    "speaker": "The boat owner",
    "emotion": "neutral",
    "line": "The Monday the notes came, I was down there at twenty past seven. The young one, Brad, was at Violet's locker door. He carried a flat wrapped parcel from her door to another door in the same corridor. I said \"early for you.\" I thought her pupil was collecting her kit."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "worried",
    "line": "That was hours before the four of them came at night. He knows what he saw, and he knows it's about a man who can't answer it. He's agreed to let me pass it to the police. It's not going on air."
   }
  ],
  "decision": {
   "prompt": "It goes to Del today whichever way I do this; the question is whether I go to the sheds with a microphone first.",
   "options": [
    {
     "label": "Ring Del now, as it stands",
     "flag": "aq.fk.d2.direct",
     "result": "I rang Sergeant Del Cruz from the studio and read it out the way he gave it to me. Del has it clean, and the show has nothing it can use, which is how it should be."
    },
    {
     "label": "A day at the sheds on tape first",
     "flag": "aq.fk.d2.market",
     "result": "I spent a day at the sheds with the recorder, and Del got the tape and the tip together, a day late. Liam heard I'd been down there with a microphone, and he's stopped answering me."
    }
   ]
  },
  "after": [
   {
    "speaker": "Ally Quinn",
    "emotion": "worried",
    "line": "Either way, it's not going out. What I'm holding is this: a man nobody can ask went to her locker early that morning, and I don't know why."
   }
  ]
 },
 {
  "id": "fk_p04_10",
  "background": "studio_onair",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "This week, Liam Bryce. He's known Violet Moore since the school bus, thirty-five years. He drives the crane at the boatyard, and he owns a third of it, which he's never said out loud until now."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The letter had lines about him. He answered every one of them at his own kitchen table, with the tape running."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "He asked, twice, to take her mother's ashes out on his boat. He fixed a window catch in her back bedroom. He rehung her back door. He fitted the gate at the back of her yard."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Every one of those things happened. He told me so himself. What they add up to, I can't tell you. I don't know, and the police looked for six weeks and took no further action."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "worried",
    "line": "There's more in my notebook this week than in this episode. Some of it I don't understand yet, and I won't say it until I do."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "sad",
    "variant": "aq.fk.d2.market",
    "line": "I asked Liam to come back in before this went out. He didn't answer. So tonight is one afternoon at his table, and I'll leave it there. This is Echoes of Havenbay, I'm Ally Quinn. Goodnight for now, Harbour. Sleep tight."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "variant": "aq.fk.d2.direct",
    "line": "I'll be back at the yard next week, if Liam will have me. This is Echoes of Havenbay, I'm Ally Quinn. Goodnight for now, Harbour. Sleep tight."
   }
  ]
 }
]
```

Decision D2 lives in fk_p04_09: option A flag `aq.fk.d2.direct`, option B flag `aq.fk.d2.market`. fk_p04_10's variant lines must match the new consequences (direct: the sheds are closed to Ally; sheds: Liam has stopped answering).

## Chapter 7 scenes to rewrite under ruling 4 (keep ids)

```json
[
 {
  "id": "fk_p07_01v",
  "background": "studio",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Last week I read out the exact words I'd been given for the panel and asked anyone who'd seen it to write. Two signwriters did. Neither knew the other had."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Both describe the same thing. A black ground. Gold capitals and an ampersand. Her own alphabet, done better than she'd ever done it."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "surprised",
    "line": "Both put a crack across one corner. Both put a small mark bottom right, and both say it wasn't her name."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Two people who've never met, describing one object from memory, and they agree on the corner. I've pinned both letters side by side."
   }
  ]
 },
 {
  "id": "fk_p07_01p",
  "background": "diner",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Last week I asked, in my own words, for anyone who'd seen the panel. One signwriter wrote back."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Black ground. Gold capitals and an ampersand. Her own alphabet, done better than she'd ever done it. A small mark bottom right that wasn't her name. And a crack across one corner."
   },
   {
    "speaker": "The café owner",
    "emotion": "neutral",
    "line": "She had it on the chair beside her, wrapped up. I saw a corner of it when she moved it. Black, with gold letters. I couldn't tell you about any crack."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "worried",
    "line": "That's one letter and one memory. It took three mornings at that counter to get the second. It's thinner than I wanted, and I'm using what I've got."
   }
  ]
 },
 {
  "id": "fk_p07_02v",
  "background": "caseboard",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Both letters, pinned side by side, with the matching lines joined in red. Black ground to black ground. Gold capitals to gold capitals. The ampersand. The crack across one corner, in both."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Two people, two memories, one object. I've written under it what it is: a description, twice over, of a thing nobody has seen since she died."
   }
  ]
 },
 {
  "id": "fk_p07_02p",
  "background": "caseboard",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "One letter, pinned, with the café owner's words on a card beside it. Black ground. Gold capitals and the ampersand. The mark bottom right that wasn't her name."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The crack stays off the board. One person mentioned it and nobody else did, and one isn't enough to pin. I've written under it what it is: one description, and half of another."
   }
  ]
 },
 {
  "id": "fk_p07_03",
  "background": "studio",
  "nodes": [
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "Ruby's envelope had more in it than the key and the note. Folded behind them was the solicitor's covering slip, the paper a solicitor sends out with anything they've been asked to hold."
   },
   {
    "speaker": "Liam Bryce",
    "emotion": "sad",
    "variant": "aq.fk.d2.market",
    "line": "I haven't said a word to you since the sheds. That was mine to carry, not yours. Ruby says you've got the slip. I'll tell you what I know about that night, if you want it."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "variant": "aq.fk.d2.direct",
    "line": "Ruby slid it across the table and said take it, I've read it enough. I've read it a dozen times since."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "It lists four addressees. Three are street addresses. One is a post office box number."
   },
   {
    "speaker": "Ally Quinn",
    "emotion": "neutral",
    "line": "The envelopes went in the post on the Friday evening after the funeral, after the last collection. They carry the Saturday postmark. The solicitor has already confirmed she lodged the keys three weeks before she died."
   }
  ]
 }
]
```

## Part C: exact line fixes elsewhere (return as old/new pairs; touch nothing else in those chapters)

Find and fix every instance of: Gerald described as a newsman (chapter 7, Gerald's Method); "He" for Del (chapter 10, Del's Bench and One Chain); "radio" or "with the radio on" (chapter 9, One of a Kind and Episode Nine); "a fortnight" (chapter 10, The Flood); the closing scenes of chapter 10 (both versions) reframed as the short update released the night of the arrest; "the harbour café" and "the café owner" wherever they occur (chapters 3, 7, 9) to the Kestrel Corner Diner and its owner; "What gave him away was never evidence" softened so it is true (the habits found him; the evidence came after). Keep every other word of those lines.

## Output schema

Return exactly three fenced JSON blocks, labelled PART A, PART B, PART C.

PART A: a JSON array of the ten chapter 2 packages, in order, each shaped like this:

```
{"id": "fk_p02_NN", "background": "<key from the table>",
 "nodes": [{"speaker": "...", "emotion": "...", "line": "..."}],
 "decision": {"prompt": "...", "options": [{"label": "...", "flag": "...", "result": "..."}, {"label": "...", "flag": "...", "result": "..."}]},
 "after": [{"speaker": "...", "emotion": "...", "line": "..."}]}
```

`decision` and `after` appear only on fk_p02_03. Variant lines carry `"variant": "<flag>"` on the node.

PART B: a JSON array of the rewritten chapter 4 and chapter 7 packages listed above, same shape (`decision` and `after` on fk_p04_09 only).

PART C: a JSON array of `{"chapter": N, "id": "fk_pNN_MM", "old": "<exact current line>", "new": "<replacement line>"}`.

No commentary, no scores, no alternatives. One polished version.
