<!-- pdf-title: The lead-package economy model v1.1 -->

# THE LEAD-PACKAGE ECONOMY MODEL, v1.1 (RULED)

*2026-08-31. Arithmetic model for Stephen's lead-package concept, against the canonical economy (Schedule B anchors, 2026-07-17 tuned drop tables, regen 150s, cap 100). It re-models the episode's requirement mass; it does not touch energy, regen, drop tables, CC sinks or SKUs.*

**v1.1: the four open numbers ruled by Stephen, 2026-08-31.**

1. **Total mass: 1,600 T1eq**, tunable once the 100 story beats exist.
2. **The ramp envelope stands as a guide, with variety mandated:** never a mechanical progression. Late chapters keep quick single-card wins beside the five-carders; early chapters get the occasional spike; sizes and tiers mix within every chapter.
3. **CC holds the 4.3 earn rate: ≈6,880 CC across the episode, banded per package.**
4. **No optional packages in Ep1: the 100 are everything.** No separate cold-case or teaser play. If an Ep2 teaser is wanted, it is a story beat inside the close package, not extra play; that is a structure question, not an economy one.

## The concept, as ruled (Stephen, 2026-08-31)

- **A lead package is the unit of story progress.** Fulfilling a package shows the next story beat; individual card fulfilment no longer drives story.
- A package contains **1 to 5 lead cards**, drawing items from different characters and different item families.
- Difficulty ramps by package size and item tier: single low-tier cards early, five-card multi-family packages late.
- **Every package fulfilment pays off with something:** art with a caption, a previously unknown character fact, a line from Ally, or an evidence turn. Art and VO budgets are decided later and do not constrain this design.
- Tentative episode total: **about 100 packages**. About **10 chapters**; a chapter ends when Ally publishes.
- Constraints re-ruled the same day: 4 to 5 hours and 8 to 10 sessions are **guides, not hard constraints**; session boundary = publish beat is a **nice-to-have** where natural; "the gap is Brad listening" framing dropped; no-replay and ep01 slot ids stand.

## Fixed anchors (not touched by this model)

| Anchor | Value |
|---|---|
| Energy | 1 per generator tap · regen 150s/point · free cap 100 · ladder 10/20/40/80 ingots · ads +20, 5/day |
| Drop tables | 2026-07-17 spawn-low/merge-up curves; effective yield ≈ **1.55 T1eq per net energy** (Schedule B measured: 330 T1eq from ≈213 net energy, ≈233 taps) |
| Session tank | ≈100 free energy per well-spaced session (cap refill dominates daily income; the July structural finding) |
| CC earn yardstick | ≈4.3 CC per T1eq |
| Card quantity cap | `LeadRequirement.quantity` is `[Range(1,3)]`: no card asks for more than 3 of an item |

## The headline arithmetic (at the ruled 1,600 T1eq)

- **100 packages ≈ a story beat every 2.5 to 3 minutes** of play. Genre-correct cadence.
- **250 cards** at the working ramp (average 2.5 cards per package); average **6.4 T1eq per card**: mostly single T2 to T4 items in small quantities, no board-straining walls, quantity cap 3 respected.
- **Requirement mass 1,600 T1eq** (4.8x Schedule B's 330).
- **Net energy ≈ 1,030** (1,600 ÷ 1.55) ≈ **10.3 free session-tanks**: the episode genuinely occupies about ten sittings without tank surgery, ladder spend, or ads; the ladder and ads shorten it for players who choose to.
- **Days to complete:** ~10 at one session a day, ~5 to 6 at two. **This resolves the July finding**: Schedule B's ~2-day episode needed ~700+ T1eq to reach genre-typical length, and the package model carries 1,600 comfortably because the mass is spread across 250 small cards instead of 12 walls.
- **CC: ≈6,880 across the episode at the ruled 4.3 rate** (was 1,410), banded per package. Sinks are absolute-priced (locker, Mo's, Case Kit), so this expands sink headroom; deeper locker slots and shop buys come into Ep1 reach, which the doubling curves were built for.

## The ramp (guide, 10 packages per chapter, variety mandated)

| Ch | Packages | Avg cards/pkg | Cards | T1eq budget | Net energy | Feel |
|---|---|---|---|---|---|---|
| 1 | 10 | 1.0 | 10 | 35 | ≈23 | FTUE. Single cards, T1 to T2, instant wins; the cold open v0.4 spread across the first packages |
| 2 | 10 | 1.5 | 15 | 70 | ≈45 | Second family unlocks; one spike package |
| 3 | 10 | 2.0 | 20 | 105 | ≈68 | First two-card, two-family packages |
| 4 | 10 | 2.0 | 20 | 135 | ≈87 | Tier climb begins |
| 5 | 10 | 2.5 | 25 | 170 | ≈110 | Mid-episode; Mo's and locker doing work |
| 6 | 10 | 2.5 | 25 | 205 | ≈132 | |
| 7 | 10 | 3.0 | 30 | 215 | ≈139 | Three-family packages standard |
| 8 | 10 | 3.0 | 30 | 220 | ≈142 | First five-card package |
| 9 | 10 | 3.5 | 35 | 220 | ≈142 | |
| 10 | 10 | 4.0 | 40 | 225 | ≈145 | Finale; five-card multi-family peaks, with quick wins kept beside them |
| **Total** | **100** | **2.5** | **250** | **1,600** | **≈1,032** | |

**The averages are an envelope, not a pattern (ruled).** Every chapter mixes sizes and tiers: a one-card quickie sits beside a four-carder in chapter 9, and chapter 2 carries one deliberate spike. Monotonic size-by-size progression is exactly what Stephen rejected. Per-chapter play at ~1.1 taps per net energy point lands each chapter at roughly one sitting (chapter 1 lighter by design).

## What this supersedes and what it does not

- **Supersedes:** Schedule B's 330 T1eq / 12-lead requirement plan as the production-episode model, and the 16-lead structure premise of `episode-1-lead-structure-four-keys-v1.0.md` (rework to v2.0 as chapters × packages; its 16 leads survive as the turn-beat skeleton and its Part F questions stand).
- **Does not touch:** the slice as shipped, drop tables, energy system, CC sinks, SKUs, FTUE grants.

## Open tuning items (drafted here, ruled later)

1. Per-package card-size and tier mix inside each chapter (the envelope above; variety per the ruling).
2. CC reward bands per package against the ≈6,880 total; whether energy/ingot rewards scale with it.
3. Chapter 1's exact FTUE budget in seconds and packages (waits on the I6 first-interaction ruling and the v0.4 cut).
4. Beat payoff type per package (art+caption / character fact / Ally line / evidence turn): drafted in the structure rework, budgeted later per Stephen's ruling.
5. The whole mass re-tuned once the 100 story beats exist, per the ruling on question 1.

## The systems bill, named and not costed here

A package container above lead cards: data (PackageData or an extension of the existing lead/requirement shape), leads-bar UI grouping, package-complete beat presentation (art+caption surface), gating by package rather than card, save schema addition, FTUE teaching, analytics events. Sits on top of `feature/multi-episode-audit`, which is still unmerged and not play-verified. Cost it the way the multi-episode bill was costed, before build.
