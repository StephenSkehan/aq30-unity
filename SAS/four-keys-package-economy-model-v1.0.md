<!-- pdf-title: The lead-package economy model v1.0 -->

# THE LEAD-PACKAGE ECONOMY MODEL, v1.0

*2026-08-31. First arithmetic pass for Stephen's lead-package concept, against the canonical economy (Schedule B anchors, 2026-07-17 tuned drop tables, regen 150s, cap 100). This re-models the episode's requirement mass; it does not touch energy, regen, drop tables, CC sinks or SKUs. Drafted for ruling; every number below the anchors is a tuning draft.*

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

## The headline arithmetic

- **100 packages ≈ a story beat every 2.5 to 3 minutes** at the 4-to-5-hour guide. Genre-correct cadence.
- **250 cards** at the working ramp (average 2.5 cards per package).
- **Requirement mass ≈ 1,400 T1eq** (4.2x Schedule B's 330). Per card that is an average of ~5.6 T1eq, i.e. mostly single T2 to T4 items in small quantities: no board-straining walls.
- **Net energy ≈ 900** (1,400 ÷ 1.55), which is **about 9 free session-tanks**: the episode genuinely occupies 8 to 10 sessions without tank surgery, ladder spend, or ads.
- **Days to complete:** ~9 days at one session a day, ~5 at two. **This resolves the July finding**: Schedule B's ~2-day episode needed ~700+ T1eq to reach genre-typical length, and the package model carries 1,400 comfortably because the mass is spread across 250 small cards instead of 12 walls.
- **CC:** at the 4.3 yardstick the episode pays **≈6,000 CC** (was 1,410). Sinks are absolute-priced (locker, Mo's, Case Kit), so this expands the player's sink headroom rather than breaking anything; band the rewards per package at tuning.

## The ramp (working draft, 10 packages per chapter)

| Ch | Packages | Avg cards/pkg | Cards | T1eq budget | Net energy | Feel |
|---|---|---|---|---|---|---|
| 1 | 10 | 1.0 | 10 | 30 | ≈19 | FTUE. Single cards, T1 to T2, instant wins; the cold open v0.4 spread across the first packages |
| 2 | 10 | 1.5 | 15 | 60 | ≈39 | Second family unlocks |
| 3 | 10 | 2.0 | 20 | 90 | ≈58 | First two-card, two-family packages |
| 4 | 10 | 2.0 | 20 | 120 | ≈77 | Tier climb begins |
| 5 | 10 | 2.5 | 25 | 150 | ≈97 | Mid-episode; Mo's and locker doing work |
| 6 | 10 | 2.5 | 25 | 180 | ≈116 | |
| 7 | 10 | 3.0 | 30 | 190 | ≈123 | Three-family packages standard |
| 8 | 10 | 3.0 | 30 | 190 | ≈123 | First five-card package |
| 9 | 10 | 3.5 | 35 | 190 | ≈123 | |
| 10 | 10 | 4.0 | 40 | 200 | ≈129 | Finale; five-card multi-family peaks |
| **Total** | **100** | **2.5** | **250** | **1,400** | **≈903** | |

Per-chapter play at ~1.1 taps per net energy point lands each chapter at roughly one session (chapter 1 lighter by design). Package-size peak of 5 lives inside chapters 8 to 10; averages above conceal singles and fives in every late chapter, which is where the dial gets tuned.

## What this supersedes and what it does not

- **Supersedes:** Schedule B's 330 T1eq / 12-lead requirement plan as the production-episode model, and the 16-lead structure premise of `episode-1-lead-structure-four-keys-v1.0.md` (rework to v2.0 as chapters × packages; its 16 leads survive as the turn-beat skeleton and its Part F questions stand).
- **Does not touch:** the slice as shipped, drop tables, energy system, CC sinks, SKUs, FTUE grants.

## Open tuning items (drafted here, ruled later)

1. Per-package card-size and tier mix inside each chapter (the averages above are the envelope).
2. CC reward banding per package against the ≈6,000 total; whether energy/ingot rewards scale with it.
3. Chapter 1's exact FTUE budget in seconds and packages (waits on the I6 first-interaction ruling and the v0.4 cut).
4. Whether optional packages exist outside the 100 (cold cases, Ep2 teaser) and their pricing.
5. Beat payoff type per package (art+caption / character fact / Ally line / evidence turn): drafted in the structure rework, budgeted later per Stephen's ruling.

## The systems bill, named and not costed here

A package container above lead cards: data (PackageData or an extension of the existing lead/requirement shape), leads-bar UI grouping, package-complete beat presentation (art+caption surface), gating by package rather than card, save schema addition, FTUE teaching, analytics events. Sits on top of `feature/multi-episode-audit`, which is still unmerged and not play-verified. Cost it the way the multi-episode bill was costed, before build.
