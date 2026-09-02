# Four Keys, Chapter 1 Slice: Playtest Verdict and Disposition (2026-09-02)

Stephen played the chapter 1 vertical slice in the editor (Main Merge, AQ menu toggle, QA Reset + Play). This file records his verdicts verbatim and how each was disposed. Rulings are his; dispositions marked PROVISIONAL are Claude's mechanical choices awaiting his word.

## Verbatim

Round 1 (bugs, 12:46 to 12:53):

> Missing dialogue data and missing Ally portrait and the background should be her "On Air Studio". No generator available for fingerprint brush. Progress stalled as I cannot play on.

Round 2 (bugs, 13:06 to 13:09):

> Please ensure to revert the editor to the MainMerge scene for testing so that I don't need to fix it everytime. No portrait above card. Same issue as before - Missing dialogue data and missing Ally portrait and the background should be her "On Air Studio".

Round 3 (feel verdict, 13:42):

> The beat works now, on to the feel verdict. The dialogue needs a little work but it is pretty good. Segments 9 & 10 are too big so we may need to to go to 12 segments as these both split nicely after the 3rd line on each. I also noticed that nothing found its way to the evidence board. I also notice on occasion that the required package was the same item over 2 cards, this looks contrived and they should be different. Also, the FTUE tuturial has been completely lost, this really needs to be reinstated to get the full FTUE feel, flow and timings.

## Disposition

| # | Finding | Kind | Disposition | Where |
|---|---|---|---|---|
| 1 | "Missing dialog data" on every card; no portrait; harbour backdrop | Bug | FIXED. Root cause: the slice runtime was constructed before its catalog was assigned (Awake on AddComponent), so it held zero packages and no beat ever fired; the Listener lead bridge then booted its per-lead fallback. Bootstrap builds the object inactive; runtime rebuilds lazily. Member cards now skip the bridge's fallback; all ten beat graphs carry the On Air studio backdrop. | 5847f72, 125dea3 |
| 2 | No generator for the brush (forensic family unreachable) | Bug | FIXED. Structure v2.2 puts the lab grant at p01_01; the card lacked it. p01_01a grants gen_investigation_lab via the Stash, as the Listener's Bridge lead does. | 5847f72 |
| 3 | Editor left on an Untitled scene | Process | FIXED. Caused by running the test suite through the editor bridge. Rule: never run tests through the bridge while Stephen is testing; reload Main Merge after any bridge action that can change the scene. | this session |
| 4 | No portrait above the card | Bug | FIXED. The 12 cards carried no actorPortrait; all now use Ally's badge (the Listener convention). | 125dea3 |
| 5 | Dialogue "needs a little work but pretty good" | Ruling queue | OPEN. Line rulings in context are agenda item 2; the file is `four-keys-prose/ch01.md`. | |
| 6 | Segments 9 and 10 too big; split after the third line of each; chapter goes to 12 segments | Ruling (given) | DONE. Package 09 keeps N1 to N3 (police found nothing; "tell me straight"; "We looked. Properly."); new 09b carries N4 to N9 (Del's interpretation and "be careful"). Package 10 keeps N1 to N3 (the Gazette war, Ruby yes Liam not yet, the I-don't-knows); new 10b carries N4 to N8 (four friends, four keys; sign-off). Chain is strict: 09a, 09b, 10a, 10b. Chapter 1 is now 12 packages over 14 cards. PROVISIONAL: titles ("It Lives in People" for 09b; "Havenbay Takes Sides" for 10; "Ep 1 Publishes" moves to 10b), the two new cards ask Forensic T2 each (+4 T1eq, chapter total 39), rewards split 15/10 and 10/10 CC. This is an F4 re-seam of the Part D cut; the words did not change. | this session |
| 7 | Nothing reached the evidence board | Gap | OPEN, sized. The board populates from leads with `aq.lead.<id>.seen` flags and reads cast and location from each lead's resolutionDialogue. Package cards have neither: the beat lives on the package (beat_seen flag, beatDialogue). Plan: teach the board to pin packages (title pin when beat_seen; cast from the beat graph's speakers; location from its stage backdrop; replay boots the beat graph). Roughly half a day. Mirrors the Listener at package granularity; not a redesign of the board. | |
| 8 | Same item over two cards in one package looks contrived | Ruling (given) | DONE for p01_06 (06b now asks Audio T2; 06a keeps Forensic T2; equal T1eq). Standing rule recorded for authoring: the cards of one package never ask the same item. p01_04 already differed (Audio T2 + Forensic T2). Note: the structure table's "F1x2" style entries are one card asking quantity 2, not two cards; those stand. | this session |
| 9 | FTUE tutorial completely lost; must be reinstated for feel, flow and timings | Ruling needed | OPEN. The first-merge choreography is hard-wired to the Listener (lead e1_tip, intro nodes E1_L1_N1 to N3, payoff at N4, seed pair Audio T1, target Audio T2) and the slice gates it off. Reinstating it under Four Keys means making the choreography data-driven per episode. Options and recommendation are in the session message to Stephen; his ruling decides the build. | |

## Rulings queued for Stephen (one at a time)

1. FTUE reinstatement approach (the build-shaping one; asked first).
2. Evidence board: confirm package-as-pin.
3. Provisional titles and card asks for the split (item 6).
4. Segment 9 backdrop: Del bench (current) or the studio.
5. Chapter 1 line rulings in context (agenda item 2).

## Structure impact to fold into v2.2 when Stephen confirms

Chapter 1: 12 packages, 14 cards, 39 T1eq (was 10 / 12 / 35). Authored census 107 (was 105). Timeline draft's ~3:30 and ~4:30 marks now each span two beats.
