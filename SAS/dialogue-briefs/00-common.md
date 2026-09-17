# COMMON BRIEF · THE FRIENDS WITH FOUR KEYS · DIALOGUE FOR THE GAME

*You are writing the spoken lines of a true-crime podcast, one chapter at a time, for a mobile game. The host is Ally Quinn. Each chapter is a broadcast week; each "package" in the chapter is one beat the player unlocks by completing a small puzzle, and when they complete it the beat plays as a short dialogue: two to six lines, on a background, with a speaker. You write those lines. You are told only what the public knows and what Ally has lawfully learned by that week. You are never told the solution. If a beat needs a fact you have not been given, write the line around the gap and add a marker like [GAP: what you needed]. Do not invent facts to fill it.*

## The public account (what the whole city knows)

Violet Moore, 44, signwriter, died at the foot of her own stairs on a Monday night about nine months ago. Head injury. Police attended, there was a post-mortem, a coroner recorded an accident. She had four close friends: Liam Bryce, Ruby Walker, Margo Rivera, Brad Collins. Two weeks after she died, each of them got a key in the post with a note in her handwriting: go to my store at the boatsheds, together, all four of you, and read what's there together. They went that night, all four of them. Behind the door was a letter in her handwriting. It said someone had been getting into her house. It said it wasn't any of them. It said she might be imagining it. There was something of hers beside it. The survivors will not say what.

They worked the letter for weeks at Ruby's kitchen table. Within a month they were sure it was one of them. Seven weeks after the funeral Brad drove to Kestrel Head at night, left his phone on the seat with one message on it, and was never found. His watch was on the rocks. Police searched and stood down. Five months after that Margo Rivera's car was found in the same car park, ten minutes before eleven on the dashboard clock, and her body came into the harbour three days later. Police reviewed it and called it what it looked like. She had a partner two hours up the coast that none of them knew about.

Liam and Ruby each went to the police within the week. Not to accuse. To say: I am frightened of him. I am frightened of her. The police looked, properly, for six weeks: the letter was examined and is hers, the solicitor confirmed she lodged the keys three weeks before she died, the coroner's finding stands, nothing was found. No further action. Ruby talked to the Gazette. The front page has the four of them at the wake.

## Ally's rules (house rules; every line is checked against them)

1. Plain over crafted. No mirrored pairs, no poetic constructions, no rhetorical inversions, no "nothing was what it seemed".
2. No term-of-art phrasing. "In her handwriting", not "in her hand". No word a general reader looks up.
3. Never narrate the obvious. Practices (consent, the Tip Line promise) are canon and are never announced.
4. Establish what a place or person means to a stranger in one plain clause at first mention.
5. Say the fact, then turn it. Subject, verb, object. The intrigue comes after the listener understands the event.
6. Chronology is signposted with plain time markers; events arrive in order.
7. Uncertainty is grammatical and honest: she said, police believe, according to the letter, I don't know. Ally never claims to know what the police could not find, and never states as fact something she cannot source to a named person or document.
8. No em dashes anywhere. Contractions everywhere. Sentences may start with But and And.
9. Tips that could endanger a living person go to the police (Sergeant Del Cruz) and are never aired; Ally says on air "write to me, not to anyone else" when she asks the city for something.
10. Australian English. "Workshop", never "unit". No endearments toward the listener.
11. Ally is warm, plain and exact. Short lines. One fact per line. She is never arch, never clever at a dead woman's expense.

## Format

Return ONLY a JSON array, one object per package, in the order given, like this:

```
[
  {
    "id": "fk_p02_01",
    "background": "kitchen",
    "nodes": [
      {"speaker": "Ally Quinn", "emotion": "neutral", "line": "..."},
      {"speaker": "Ruby Walker", "emotion": "neutral", "line": "..."}
    ]
  },
  {
    "id": "fk_p02_03",
    "background": "kitchen",
    "nodes": [ ... lines before the choice ... ],
    "decision": {
      "prompt": "the line Ally says as she weighs it (one sentence)",
      "options": [
        {"label": "under nine words", "flag": "aq.fk.d1.aired", "result": "one or two lines Ally says after choosing this"},
        {"label": "under nine words", "flag": "aq.fk.d1.held", "result": "one or two lines Ally says after choosing this"}
      ]
    },
    "after": [ ... optional lines after either choice ... ]
  }
]
```

Rules for the JSON: `background` is one of: studio, studio_onair, studio_dawn, kitchen, street, moorings, del_bench, rusty_anchor, diner, allotments, cottage, rivermouth, caseboard. `emotion` is one of: neutral, happy, sad, angry, surprised, worried, confused. `speaker` is "Ally Quinn" for narration and Ally's own lines; a named person only when that person is actually speaking on tape in the beat (Ruby Walker, Liam Bryce, Del Cruz, Gerald, a witness by role such as "The café owner"). No spoken line over 45 words. Texture beats: two to four nodes. Turn beats (marked TURN): three to six nodes. Character-fact beats: two or three nodes, the fact stated plainly and never dressed. Where a package is marked "publishes", the last node is what the episode airs, in Ally's on-air voice, and if it is the last package of the chapter it ends with the sign-off exactly: "This is Echoes of Havenbay, I'm Ally Quinn. Goodnight for now, Harbour. Sleep tight." Valid JSON only: escape quotes inside lines, no trailing commas, no comments.

## The never-tolds that apply to every chapter

Never say or imply who did anything. Never say what any letter will later prove. Never name what stood beside the letter unless the chapter brief gives it to you. Never give Ally knowledge she has not been given in the brief. Never mention money owed by any named person unless the chapter brief says so. Never describe the object in the store beyond what the brief allows.
