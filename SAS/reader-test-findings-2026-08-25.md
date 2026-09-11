<!-- pdf-title: Reader Test Findings, twenty-one premises -->

# READER TEST, FINDINGS

**Subjects:** ChatGPT's nine (`AQ-NAR-MODEA-EXEC-2026-08-24-01`) and Claude's twelve
**Readers:** Stephen Skehan, Trish Szabo, independently · **Date:** 2026-08-25
**Verdict:** twenty-one premises, two survivors, no opener.

---

## 0. Housekeeping: my sealed prediction is dead

Predicted LEGS 5, 8, 9, 11 and REJECT 6, 7, 10, 12. Five hits, three misses. Death threshold was three. **The record-versus-people axis was not the operative variable and is withdrawn.** No retrofit.

---

## 1. What survived

| Premise | Stephen | Trish |
|---|---|---|
| **Claude 3**, the erased woman under the viaduct | "best of the bunch" | "Good. What happens next?" |
| **Claude 8**, two deaths one address, one doctor | "there may be something in this" | "Not bad at all. This might be interesting." |
| ChatGPT 4, the theatre understudy | "seed for a later episode" | "Easy to follow. I can picture it in my mind." |
| ChatGPT 9, the private club | setting wanted in canon | "you lost me at accountant" |

Two mechanics were praised independently of their premises, both by Trish: **crowd-sourced photographs and video assembling a picture no single image holds** (ChatGPT 1 and 3). She liked the reconstruction more than either crime.

---

## 2. The finding that answers Stephen's question

**The genre research exists and was banned from the step that generates premises.**

`prompt-kit-v2.1-FINAL.md` line 142, verbatim:

> `true-crime-genre-anatomy` is **excluded from Mode A premise generation and from the spoken-opening comparison.**

Both execution records state "Genre-anatomy reference used: No." Twenty-one premises were generated from thirty-four platinum cases that neither generator was permitted to look at.

**Why the ban existed:** to keep the clean Mode A comparison uncontaminated, since the three challengers had been written under the document's influence and would otherwise have had an unfair advantage. The reasoning was sound and the cost was the entire field.

---

## 3. Unbanning it is necessary and not sufficient

The document's own Part Four ranks the engines **by merge-board fit first**:

> Institutional failure and unreliable record are the two that best fit a merge board, because both are reconstructive.

Those are the two engines the readers just rejected wholesale. The ranking optimised for the mechanic, not for the audience, and it is my error.

Set against the readers' actual responses the ranking inverts. The two survivors sit on the two engines Part Four ranked lower and described accurately anyway:

- **Claude 3 is Engine 4, the invisible victim** — "carries the most contemporary licence"
- **Claude 8 is Engine 5, the ordinary door**, with Engine 1 asymmetry — "the strongest emotional engine for this specific audience"

**Both readers independently selected the two premises sitting on the engines the banned document named as strongest for this audience.** Neither premise was generated using it.

---

## 4. The two empirical patterns in the responses

### 4.1 Occupational jeopardy is the only jeopardy in the field

What the wronged person stands to lose, across ChatGPT's nine: an apprenticeship (three times), a union election, a place in a theatre network, a death benefit, a port licence, a professional bar, a guild apprenticeship. **Nine of nine are loss of occupational status.** Claude's twelve are broader but still economic and legal: freedom, a house, a benefit, a redundancy, a room.

**Across all twenty-one, not one premise has love, jealousy, sex, marriage, obsession, or a parent and child in conflict at its centre.**

Every case a general audience can name has exactly that: Serial, The Teacher's Pet, Dirty John, The Jinx, Making a Murderer. This is the largest single gap between our field and the genre, and it is traceable: **Gate 2 requires a named living person under a still-operating mechanism, and an institution is the easiest way to keep a mechanism running for thirty years.** The gate did not ask for workplaces. It made workplaces the cheapest compliant answer.

### 4.2 Trish hit a vocabulary wall four times in twenty-one

> "I don't know what a clipping box is" · "What is a fog dark loading room? What is a chandlery?" · "not sure what bypassing the lockout even means" · "I don't understand what crime she is serving 11 years for"

Three are trade terms. The fourth is worse: a premise where the crime itself was not legible. **No gate in v2.1 or CAI-1 tests whether a premise can be understood by someone who does not already work in a port.**

Stephen's rejections are the same wall from the other side, stated as boredom rather than confusion: "clipping box, handwritten slips, scanning batches of photos, settlement files. So so so boring" · "bookkeepers, accountants, ledgers, rosters, receipts" · "transcript geometry and invoices about delay circuits".

**The decisive evidence is an administrative document in nearly every one of the twenty-one.** Tally sheet, invoice, service card, payroll record, correction sheet, circulation card, cash notebook, reconciliation book, custody record, entry log, settlement file, stamped stubs, cue sheet. This is Kill Gate Zero working as designed: it rewards a decisive item that is lawfully reachable, and lawfully reachable items are paperwork.

---

## 5. What is not wrong

**Kill Gate Zero is not the problem and must not be weakened.** Every reader objection about believability was aimed at premises that *passed* it, and the failures were craft failures it does not test: "do brake pins have serial numbers", "cleaners would not notice", "a mother would never frame an admin form", "tills would just be synchronised". Gate Zero verifies that institutions behave correctly. It has no opinion on whether a human being would plausibly do the small thing the premise needs them to do.

**Gate 6 is satisfiable by fabrication.** Claude 2's framed manifest was invented to satisfy the grievable-detail requirement and Trish identified it instantly as false behaviour. No auditor in CAI-1 can detect this.

---

## 6. Recommended next steps, in priority order

1. **Stop the CAI-1 reciprocal audit.** Both fields are rejected by both readers. Auditing rejected premises against each other produces findings about nothing. The instrument is sound and should be retired with its findings intact.
2. **Lift the Mode A ban on `true-crime-genre-anatomy` and make it mandatory**, with Part Four's engine ranking reversed: 4, 5, 1 before 2, 3.
3. **Add a positive requirement: at least half the field must place an intimate relationship at the centre.** Not a workplace, a guild, a licence or a union.
4. **Add a comprehension gate.** A premise must be stateable in three sentences containing no term a general reader would have to look up. Trish is the instrument; there is no substitute and no auditor can stand in for her.
5. **Add a plausibility-of-behaviour gate distinct from Gate Zero.** Would a real person, with nothing at stake, actually do the small thing this premise needs? Every reader objection about believability lands here.
6. **Build from the two survivors and the two liked mechanics**, rather than generating a third clean field. Claude 3 and Claude 8 have reader consent. Crowd-sourced media reconstruction has reader enthusiasm and is the best merge-board fit anyone has found.
7. **Canon add, ruled by Stephen 2026-08-25:** a private club for Havenbay's rich, famous and powerful, retained as a setting for later episodes.

---

## 7. The general lesson, for the record

v2.2's admission said a generator constrained only by prohibitions converges on whatever survives them. The reader test extends it.

**Every instrument this project built measures whether a premise is defensible. Not one measures whether it is interesting.** Four RC rounds, a cross-audit instrument, fifteen canon rulings and a delta analysis were spent on twenty-one premises that two readers rejected in an afternoon, and no amount of further auditing between two models could have found it, because neither of us was reading as an audience.
