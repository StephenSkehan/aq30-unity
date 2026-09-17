# SESSION KICKOFF · 2026-09-17 · The Friends with Four Keys

*Written at close of 2026-09-16. Supersedes the 09-16 kickoff. Read `four-keys-art-prompts-2026-09-15.md` (portrait style block re-ruled: eyes about one fifth of face height, Ally 4B is the anchor; transparent PNG preferred) and `four-keys-dialogue-json/` (generator sources and scripts now live in the repo).*

## Where things stand

- **Build 8** on TestFlight and both phones. No feedback yet on b8 itself.
- **Rewrite pass one is in the game** (1296c44): chapter 2 split to ten short scenes (1,150 words, only The Letter over the cap, in short nodes), Brad degraded in ch4, D2 rebalanced (direct costs the sheds, sheds costs Liam), ch7 branch keyed to Tessa's exposure, ch9 crack check un-branched, ch10 close is a short update the night of the arrest, all canon fixes (Gerald retired detective, Del she, podcast not radio, eight days, café = Kestrel Corner Diner, "What found him was never evidence. That came after."). Chapters 3 to 10 still carry the longer scenes GPT flagged; they wait for Trish's verdict on chapter 2.
- **Trish has NOT yet reread chapter 2.** That is the gate before any more dialogue work. Editor is immediate; phone needs b9.
- **Art pipeline is live and working.** Method that works: generate fresh in the house style with Ally 4B + Del attached as style anchors; edit toward the references, never edit a realistic base toward stylised; one fix per turn; start fresh after two failed edits.

## Cast status

| Character | State | Files |
|---|---|---|
| Ally | 4B approved, eyes one fifth, transparent, 7 frames in game (second frames deleted) | `Assets/Art/Characters/Ally/` |
| Ruby | v2 approved: late thirties, round face, freckles, hazel, ash-brown bob; 7 frames in game; neutral's brows slightly anxious (leave unless it jars) | `Assets/Art/Characters/Ruby/` |
| Del | Stephen has resized the eyes in chat; NOT exported or sent; needs transparent PNG + 7 frames | pending |
| Liam | neutral approved pending edit: remove ear defenders, add small black two-way radio at collar, eyes to camera, nudge toward Ally style; then 7-frame prompt (Claude to write) | pending |
| Gerald, Mo, Dot, Tip Line | still on the old eye size; same edit prompt shape as Del's | pending |
| Violet, Margo, Brad, Tessa, sister, diner owner, cinema owner, 2 signwriters, yard hand, boat owner | not started; blocks in the kit | pending |

## Backgrounds

- **Ruby's kitchen**: approved composition (third image of the 18:34 set: long table, four chairs on legs, pot-belly stove on the floor with flue and log basket, pendant, harbour + lighthouse, piano through the doorway). Awaiting the 9:19.5 extend export (1284x2778). On import: add location entry `ruby_kitchen`, re-point kitchen scenes (ch2, 4, 5, 7), and change Ruby's line "had the heater on before she knocked" to "had the stove lit before she knocked" (fk_p02_01).
- Remaining 8 backgrounds not started.

## Open items

- b9 build after Trish's chapter 2 read (or before, if she prefers the phone).
- Ruby case file copy, Liam case file copy (writing, not art).
- TODO from 09-15: Fast Enter Play Mode experiment; Unity patch to 6000.3.24f1 + Firebase 13.16; Xcode 27 by April 2027; `SAS/ipm.jpg` untracked, delete or keep.
