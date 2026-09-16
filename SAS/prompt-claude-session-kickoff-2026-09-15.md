<!-- pdf-title: Session kickoff 2026-09-15 -->

# SESSION KICKOFF · 2026-09-15

*Paste this to open the next session. Written 2026-09-14. Replaces the 09-12 kickoff. The adversary loop is closed; the work is now production: the story into the game, phrasing refined in play.*

## Where we are

**Spine v2.0.7 is the spine of the episode** (`SAS/episode-1-spine-four-keys-v2.0.md`, ruled by Stephen 2026-09-14). Premise v8.1 and spine v1.4 are superseded. Six full attacks (Fable four rounds, GPT two), device never broken, every machinery finding answered from the world or paid as a row. Two character locks are flagged as the things the reader tests must pass and that no adversary can settle: Violet will not watch (C1), and Margo keeps a stateable hypothesis for ten days (C5). Stephen: "We now need to get the story into the game. Only then can you tell if it really works. We can refine things as we go along as I think a lot of the story telling will be in the phrasing."

Also ruled 2026-09-13: the agency gate met on reasoning (structure doc); the jeopardy position (danger misdirected, not absent; four binding conditions; cold-open gate tested by the Part H cold read).

## The production path, in order

1. **Cold open v0.5.** `SAS/four-keys-cold-open-v0.5.md` is the proposed recut of the middle under v2.0.7, changed lines marked ★. Stephen rules the lines (his words). Then apply to `Assets/Content/FourKeys/Dialogue/Resolve_FK_P01_04/05/06.asset` and later packages that carry the three changed lines, and to `SAS/four-keys-prose/ch01.md`. Beat placement (which completion the "found a letter" reveal lands on) is settled in step 2.
2. **Structure v2.3.** Update `SAS/episode-1-structure-v2-four-keys.md` rows for chapter 1 packages 4 to 6, chapter 2 packages 2, 3, 8 and 10 (the room, the letter under glass, the reading's mechanism), chapter 3 (the sister on the ashes; the café), chapter 6 and 7 (the box number as a suspect turn; the panel from the trade; the kind line), chapter 9 (the panel described); retire every row that says "I think / I know", the board, the tin claim, the ladder. Keep the census arithmetic honest (finding 2 of the structure attack).
3. **The two letters as prose.** The bare key note (four identical copies; Ally puts Ruby's under glass in chapter 2) and the forged store letter (what the four read; Ruby reads it on tape in chapter 2). Brief a fresh Fable agent blind for the forgery from D3's claims table and the voice reference; the true letter is never read by anyone alive and needs no prose. Attach `SAS/ally-voice-reference-v1.0.md` for Ally; Violet's voice from the five.
4. **Chapter 2 prose revision** (`SAS/four-keys-prose/ch02.md`): the letter, the room reconstructed under v2.0.7 (Day 14 relief, Day 15 the three sentences, the lists, the kind line read aloud), Ruby's habits list including "agreed with whoever spoke last".
5. **Chapter 3 brief and prose**: the café and the cinema; the sister, bitterly, on the ashes; the download map.
6. **Into Unity**: graph assets, packages, leads for chapters 2 and 3 on the fk01 catalog; playtest chapter 1 with the recut open; the headless EditMode run when the editor is closed.
7. **Reader tests, when the words exist**: the Part H brief cold to three to five of the twelve testers ("do you need to hear the rest"); the forged letter and Violet's refusal to the three readers with the two questions: do you believe she would not look; do you believe they would turn on Liam.

## Standing rules

No em dashes anywhere in game-facing text. Ally's register: short lines, one fact per line, plain over crafted, never state the obvious. Copy rules: no endearments toward the player; directive lines of eight words or fewer; AU English (the biscuit barrel in player copy; "workshop" not "unit"). Never name a real chemical for the tin. Fable prose agents get the standing base and the redacted brief only, never Parts A to G. The Brad restraint rule from GPT's assessment: the reader meets each piece of his construction as a fact of the world before the next, never as a list.

## Parked (not blocking production)

The two DRAFT banner lines and the Episode Closed summary; the economy note (T1eq 65 to 70 per chapter); the TestFlight before 21 September (schedule stays out of story sessions).

## STATUS UPDATE 2026-09-14 (end of session)

Chapters 2 to 10 are in the game (commit eb39952). 95 packages, 238 cards, 95 beat graphs, four decisions, ch7 and ch10 branch packages, episode completes on `fk.ch10.complete`. Generator and validator live in the session scratchpad (`gen_assets.py`, `validate.py`, `guids.json`); the writers' JSON is in `scratchpad/dialogue/ch02..ch10.json`. Prose records: `SAS/four-keys-prose/ch02..ch10.md`.

Next session starts with Stephen's playtest of chapter 2 onward. Known gaps to expect in play: no portraits for Ruby, Liam, Tessa, the sister or the trade witnesses (image hides); ch7 packages 01 and 02 run in series, not parallel; p10_05 has five cards not six; the QA/dev toggle may need the chapter 1 slice completed first (entry is fk_p01_01a, chapter 2 opens on fk_p01_10b).

## TODO (added 2026-09-15 evening)

- **Fast Enter Play Mode experiment.** Project Settings, Editor: Enter Play Mode Options is already ticked but with neither reload disabled, so it gives nothing. Try "Reload Domain" OFF on a quiet day. Risk is editor-only: statics (HintService, LockerScreen gate, OverflowBucket, event subscriptions) survive between play sessions; several are guarded ("domain-reload-off safety"), several are not. Switch it straight back off if editor play differs from the phone. Builds are unaffected.
- **Unity patch 6000.3.14f1 to 6000.3.24f1** after b8 is out (same LTS stream; Firebase Unity SDK 13.13 to 13.16 can ride along). Uninstall the dead 6000.4.4f1 from the Hub.
- **Xcode 27 / iOS 27 SDK** required for all uploads from April 2027 (Mac + Xcode upgrade, not Unity).
