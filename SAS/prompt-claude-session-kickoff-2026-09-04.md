# Session kickoff, 2026-09-04 (written at the close of 2026-09-03)

Read `MEMORY.md` and the Four Keys block of `project_state.md` first, then this. Stephen's rulings in memory are never reopened by you; he reopened one himself yesterday and that is recorded below.

## Where 2026-09-03 left things

**Chapter 1 plays clean on fk01.** Two full playthroughs, eight verdict rounds, all disposed in `SAS/four-keys-ch1-playtest-verdict-2026-09-02.md` (rounds 4 to 8): speaker name in a bordered pill under the portrait, first names only; guided-loop banner placed from the subject's real rect and parked 14 px off the green card (via `LeadsBarView.ReadyCardRoot`); Stash reward hidden until its reveal flight lands and the loop waits for it; Episode Closed after the last tap; merge slide shows the consumed item; consumed evidence flies to the evidence board button; "N of M discovered" raised; Gerald's portrait back on the banners (portrait lookup scans package graphs and falls back to `Resources/App/Characters`). Stephen: "everything looks good."

**Chapter 1 line rulings applied:** "Only two of the friends"; the Del scene's four lines including "specialists'" and the new closing line ("I can't officially tell you that there is something not right here. But I can tell you if you do start digging then be careful, very careful."). Economy note deferred: chapter 1's T1eq feels low (Stephen's instinct 65 to 70 against 39); do not touch the FTUE chapter until chapters 2 and 3 have been played.

**Chapter 2 prose exists** (`SAS/four-keys-prose/ch02.md`, blind Fable, checks PASS). The **key letter is RULED** (Stephen's police paragraph with the double negative fixed; "locker at the boatsheds by the slip"). Two canon changes came out of ruling it:

1. **The store is a locker at the boatsheds by the slip**, not a fish-market cage; the Day 16 dawn witness is a boat owner (Violet lettered his boat, Brad did his tender), never "the fishmonger at Bay 3". Folded through spine (v1.5 amendment note in the v1.4 file), structure, synopsis, digest, ch02.
2. **The Regent ladder sabotage is STRUCK** as contrived. The prior fall on her record is an innocent, witnessed stumble on the weedy slip while walking backwards talking (bruised, swam anyway). The inciting threat must be staged inside her house or aimed at a habit only the four know. Twenty candidates were generated (`four-keys-threat-scenarios-fable-2026-09-03.md`, `...-gpt-2026-09-03.md`) and disposed with gates and one forced ranking in `four-keys-threat-scenarios-disposition-2026-09-03.md`. Recommendation: **F6, the car in the yard** (exhaust into the cabin during her twenty warm minutes after the swim; yard padlock key on the unit hook, known to all four), with F1 (back door screwed, key in the tin) available as a second sign of entry. **Stephen rules this first thing.**

**Blocked on that ruling:** Violet's genuine locker letter (a strong draft exists from a separate Fable agent; its ladder paragraph is void; keep "I know it's not Brad's work. I taught him everything he knows and I'm not that good a teacher." and "Be careful. Write legibly."); Brad's forgery (must be rebuilt from the genuine letter: keep her shape, cut the panel and the line clearing him, keep the four in play with a "one of you" litany and no gendered pronoun, add an explicit refusal to name without proof, replace the removed clue with a lean at Liam such as "one of you has a key to my front door"); the **"I know" certainty ruling** (Stephen: hold until the genuine letter is settled; keeping it preserves the premise ruling, the cold open line and chapter 7); Ruby's "week four, the ladder" line in ch02 p02_03; spine A1 −46/−45, B1 13/14/22, C1; structure p03_04 (Regent marquee stays as the unfinished-job image only).

## First moves

1. `git pull`; confirm main is at b8c01d5 or later.
2. Put the threat disposition in front of Stephen and take his ruling (one question).
3. Fold the ruled threat through the spine and structure; then Violet's genuine letter (one more blind revision on the ruled threat), then the certainty ruling, then Brad's forgery, then Ruby's line. Each letter comes back to him for line ruling.
4. Then the rest of the chapter 2 line rulings, chapter 3 prose on his say-so, the two DRAFT banner lines and the DRAFT closing summary.

## Process notes

- Never run the test suite through the mcp-unity bridge while the editor is open. Headless EditMode baseline is green (146).
- Fable stays blind to spine Parts A to F. The genuine-letter agent is a separate one-off and holds graph facts; never merge its context with the chapter agent's.
- Every attack or generation result is saved verbatim beside its prompt before it is judged.
