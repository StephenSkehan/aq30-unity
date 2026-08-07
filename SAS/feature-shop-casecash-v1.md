# The CaseCash Shop — design v1 · **RULED + BUILT 2026-08-07**

> **Stephen's rulings (same day):** ① Venue = **Mo's Back Room** ② items IN for
> v1 ③ **20 CC per T1eq** (up from the sketch's 12; T2=40, T3=80, T4=160) +
> 400 CC generators ④ unlock at L5 confirmed ⑤ generator offer = ANY owned type
> including the Field Kit. Shipped same day: MoShopService / MoShopPopup /
> ShowMoShopMB on the cash pill's + (self-hides pre-L5), QA menus
> (Open Mo Shop / Unlock Flag / Restock). Text below = original sketch.

Goal: a second CaseCash sink with player agency ("buy items and generators with
earned cash" — Stephen, 2026-08-06). Everything below prices against the shipped
Schedule B economy; nothing here touches ingots, energy, or IAP.

## The economic frame (why this is safe to build)

- CC income: **1,410/episode** + 500 FTUE. Existing sink: locker slots
  (200/400/800/1600 = 3,000 for all four). A typical player buys 1–2 slots and
  finishes Ep1 with **~700–1,100 CC idle**. That surplus is the shop's budget.
- Canon guardrail: **no CC→energy path, ever.** A bought ITEM is frozen energy
  (1 T1eq ≈ 0.6–0.7 energy at tuned drop rates), so item pricing must sit far
  above the earn yardstick. A bought GENERATOR is NOT — taps still cost energy —
  so generators are the safe headline product.
- Earn yardstick: 1,410 CC / 330 required T1eq ≈ **4.3 CC per T1eq earned**.

## What's for sale

| Slot | Contents | Price | Limit |
|---|---|---|---|
| 3 item offers | Random DISCOVERED tiers (T2–T4 band) from families whose generator the player owns | Tier-priced (below) | Each offer buyable once |
| 1 generator offer | Duplicate T1 of an owned generator type | Flat 400 CC | 1/day |

- Stock refreshes at **local midnight** (same reset infra as the energy ladder;
  same backwards-clock clamp).
- **Discovered tiers only** — the shop reads ItemDiscoveryService, so it can
  never spoil a silhouette in the family view. The shop sells convenience, not
  discovery.
- T1 excluded (trivial, 1 tap); T5+ excluded (aspirational tiers stay earned).
- Delivery: purchases go to **the Stash** with the generator-arc flight — the
  systems shipped this week do all the work.

## Pricing rule (items)

**T1eq price = 12 CC** (~2.8× the earn yardstick — a visible convenience tax,
steep enough that grinding energy is always the rational bulk path):

| Tier | T1eq | Price |
|---|---|---|
| T2 | 2 | 25 CC |
| T3 | 4 | 50 CC |
| T4 | 8 | 95 CC |

Sanity check vs the wall leads: L8 needs 56 T1eq; buying it outright at shop
prices ≈ 670 CC ≈ half an episode's total CC on one lead — possible once as a
splurge, never sustainable. The energy loop keeps primacy. Generator at 400 CC
competes directly with locker slot 10 (400) — a real decision, which is the
point of a sink.

## Where it lives

- **Entry point: the cash pill's "+" button** — currently dead/greyed. It sits
  on the CC counter, it's already styled, and it gives the shop a home without
  a fourth corner button. (Energy "+" keeps opening the energy store.)
- **Fiction (needs Stephen's ruling):**
  - (a) **Mo's Back Room** — the Rusty Anchor's neutral-ground trading post.
    Zero new canon (Mo exists, speaks, the bar is "cops and crooks coexist"),
    maximum charm. Recommended.
  - (b) **Harbour Market** — new minor location, generic-safe but invents canon.
  - (c) Fiction-light "MARKET" label, name it later.

## Gating and telemetry

- Unlocked at **L5 (e1_pod1 resolved)** — post-FTUE, first wall approaching,
  the moment surplus CC starts existing. Story flag gate, same idiom as family
  drops. Hidden entirely before then (dead + stays dead).
- Feature flag (FeatureFlagsRuntime) so it can ship dark if tuning slips.
- Analytics: offer impressions, purchases (slot, itemId/tier, price), balance
  at purchase — enough to see whether it cannibalizes locker slots.

## v1 scope cuts

No ingot pricing, no energy, no cosmetics, no restock-early-for-ingots (classic
monetization lever — defer, it complicates the no-CC→energy story), no bundle
offers, no shopkeeper dialogue (a Mo line on first open would be lovely — one
line, banked for the script pass).

## Open rulings for Stephen

1. Venue/name: Mo's Back Room / Harbour Market / plain MARKET?
2. Items at all in v1, or generators-only MVP (the economically bulletproof
   version — items can follow after live data)?
3. Price constants: 12 CC per T1eq and 400 CC generators feel right?
4. Unlock at L5 confirmed?
5. Generator offer: any type owned, or exclude the Field Kit (starting gen
   duplicates are the strongest board-acceleration item)?
