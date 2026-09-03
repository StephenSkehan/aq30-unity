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
| 7 | Nothing reached the evidence board | Gap | DONE. The board pinned only leads with `aq.lead.<id>.seen` and a resolutionDialogue; package cards have neither. The board now works over a `BoardScene` (title, graph, portrait): resolved Listener leads and completed packages (beat_seen, beatDialogue) both map onto it, so the cast row, location pins and replay work per package. Del appears once package 9 is seen; locations come from the beat backdrops (studio, Del bench). | this session |
| 8 | Same item over two cards in one package looks contrived | Ruling (given) | DONE for p01_06 (06b now asks Audio T2; 06a keeps Forensic T2; equal T1eq). Standing rule recorded for authoring: the cards of one package never ask the same item. p01_04 already differed (Audio T2 + Forensic T2). Note: the structure table's "F1x2" style entries are one card asking quantity 2, not two cards; those stand. | this session |
| 9 | FTUE tutorial completely lost; must be reinstated for feel, flow and timings | Ruling | RULED and BUILT (Stephen chose the proper slice entry, 2026-09-02 afternoon, after first picking the data-driven route). Four Keys chapter 1 is EpisodeCatalog entry `fk01` with its own database, package catalog, FTUE config, steps and completion flag; the database swap and polling driver are deleted; boot goes through the normal episode path via an editor-only boot override (AQ > Dev Boot Episode). The first-card choreography reads `FtueChoreographyConfig` from the entry (null = the Listener's shipped constants, pinned by tests). Four Keys: package 1's beat plays up front, guided first generator tap (arrow + pulse, deterministic Audio T1), auto-proceed, package 1 pays without repeating; the guided case loop follows. Play-verified by Stephen ("seems to be running OK"). | 750fed6 |

## Rulings taken (asked one at a time, 2026-09-02 evening)

| # | Question | Ruling | Applied |
|---|---|---|---|
| R1 | Where Four Keys sits in the season list | **First in order, id stays fk01.** Stephen: "Four Keys is episode 1." Consequences accepted: fresh saves and builds boot Four Keys; The Listener locks behind it until fk.ch1.complete; the ep01 rename happens when Four Keys ships. | Catalog reordered fk01, ep01..ep04; boot priority is save pointer, then the catalog's first playable, then the scene's legacy id. |
| R2 | Split titles and card asks | **Stand.** "Del on the Steps" / "It Lives in People"; "Havenbay Takes Sides" / "Ep 1 Publishes"; 9b and 10b ask Forensic T2. | Structure v2.2 chapter 1 table updated. |
| R3 | Segment 9 backdrop | **Del bench for 9 and 9b.** | Already so; no change. |
| R4 | Guided loop generator choice | **Loop prefers the feeding generator**; if it is still in the Stash, the Stash is the pointer. | GuidedCaseLoopMB: FeedingFamilies from the workable cards' requirements; pulse only feeding generators; Stash pointer with DRAFT banner copy "Place the Lab from the Stash." and generic "Tap the Lab. Every item helps." (both copy lines need Stephen's ruling; the kit line is the ruled 2026-08-21 copy). |
| R5 | Chapter 1 line rulings | **After the next full playthrough**, one line at a time. | Queued. |

Still open for Stephen: the two DRAFT banner lines under R4; the DRAFT closing summary on the fk01 entry; the OPEN working-note lines deferred since the slice was built.

## Round 4 · 2026-09-03 · full chapter 1 playthrough on the fk01 entry

**Verbatim (with screenshots):**

> Finished play through. Move the character name to a pill just under the portrait and use first name only. This should allow dialogue to lift by a line. Prompt appears over the stash. Prompt is not close enough to the lead card, it needs to be almost touching to avoid confusion. Also, note the right hand border of the prompt is missing in both examples. Episode closed appears before the last dialogue has been read. It should appear after a tap after the last dialogue line. change to "...Violet. Only two of the friends..." Change to "Tell me straight Del, I want the city to know the truth. What's suspicious in those files?" change to "...nothing. That's the truth, Quinny. It's taken six weeks of good specialist's time but there's nothing to chase." change to "So, it's case closed?" change to "...paper is clean. Whatever has been going on with those five friends, it doesn't live on the paper. It lives in the people."

**Console during the run:** no errors, no new warnings. Intro closed and marked package 1 pre-played; first tap 4 s later spawned Audio T1; package 1 paid without re-presenting; auto-proceed; lab placed from the Stash 34 s after the first tap; packages 2 to 10 completed in order, paid then seen.

| # | Finding | Kind | Disposition |
|---|---|---|---|
| 4.1 | Speaker name to a pill under the portrait, first name only; body lifts a line | Ruling (UI) | DONE. `DialogueController.ApplyStageLayout` builds a `_SpeakerPill` (house blue, rounded) anchored under the portrait holder at the strip top and reparents the speaker label into it; body top raised from 330 to 365 of 1920. `DialogueRunner.DisplaySpeakerName` shows the first name ("Del Cruz" → "Del", "Ally - Podcasting..." → "Ally", "Dot Ellis (voicemail)" → "Dot"); names beginning "The " or with a lowercase continuation ("Tip line") stay whole. Applies to every episode. |
| 4.2 | Guided-loop prompt appears over the Stash | Bug | FIXED. The banner was placed from the subject's pivot with a fixed clearance; the Stash root is tall, so the banner landed over its icons. Placement now uses the subject's real rect bounds in screen space (camera-aware) and parks above or below the edge. |
| 4.3 | Prompt not close enough to the lead card; must be almost touching | Ruling (UI) | DONE. Gap is now 14 px from the subject's edge (was 95 px from its centre). |
| 4.4 | Right-hand border of the prompt missing | Bug | FIXED. The 1040-wide banner overflowed the narrower scaled canvas on tall phones and an inverted clamp range parked it off the right edge. The banner now fits the canvas width minus a margin before clamping. |
| 4.5 | Episode Closed appears before the last dialogue line is read; should follow a tap after the last line | Bug | FIXED. `CaseResolutionService` now publishes the resolved event only once no dialogue runner is active (one frame after the closing card's activation, then wait for close). Edit-mode tests keep the synchronous path. Applies to the Listener's close as well. |
| 4.6 | p01_06: "Only two of the friends are still alive" | Line ruling | APPLIED to the graph, ch01.md, and cold open v0.4 (noted in its header as the one in-play word change). |
| 4.7 | p01_09 Ally: "Tell me straight Del, I want the city to know the truth. What's suspicious in those files?" | Line ruling | APPLIED to graph and ch01.md. |
| 4.8 | p01_09 Del: "We looked. Properly. There's nothing. That's the truth, Quinny. It's taken six weeks of good specialist's time but there's nothing to chase." | Line ruling | APPLIED as written. Query for Stephen: "specialist's" (one) or "specialists'" (many)? Left as written until he says. |
| 4.9 | p01_09b Ally: "So, it's case closed?" | Line ruling | APPLIED. |
| 4.10 | p01_09b Del: "...paper is clean. Whatever has been going on with those five friends, it doesn't live on the paper. It lives in the people." | Line ruling | APPLIED to graph, ch01.md and the digest's ch1 entry (paraphrase). |

Still unverified in this round (Stephen did not mention them): the evidence board's package pins; the Stash pointer moving to the lab once placed (the log shows the placement, not the banner).

## Structure impact to fold into v2.2 when Stephen confirms

Chapter 1: 12 packages, 14 cards, 39 T1eq (was 10 / 12 / 35). Authored census 107 (was 105). Timeline draft's ~3:30 and ~4:30 marks now each span two beats.
