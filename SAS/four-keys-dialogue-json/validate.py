import re, pathlib
root = pathlib.Path(r"C:/users/user/dev/aq30-unity/Assets/Content/FourKeys")
leads = {}
for f in (root/"Leads").glob("*.asset"):
    t = f.read_text(encoding="utf-8"); lid = re.search(r"leadId: (\S+)", t).group(1)
    req = re.findall(r"^  - (fk_\S+)$", t.split("RequiredLeadIds:")[1].split("requiresFlag")[0], re.M) if "RequiredLeadIds: \n" in t else []
    leads[lid] = dict(req=req, flag=(lambda m: m.group(1) if m else "")(re.search(r"requiresFlag: (\S*)", t)), narr=re.findall(r"^  - (fk\.\S+|aq\.\S+)$", t.split("NarrativeFlags:")[1].split("SoftCurrency")[0], re.M))
bad = [(l, r) for l, d in leads.items() for r in d["req"] if r not in leads]
print("leads", len(leads), "bad required refs", bad)
members = set(); pk = 0
for f in (root/"Packages").glob("*.asset"):
    t = f.read_text(encoding="utf-8"); pk += 1
    ms = re.findall(r"^  - (fk_\S+)$", t.split("memberCardIds:")[1].split("beatType")[0], re.M)
    members.update(ms)
    g = re.search(r"beatDialogue: \{fileID: 11400000, guid: ([0-9a-f]+)", t)
    if not g: print("no graph", f.name)
print("packages", pk, "leads not in any package", sorted(set(leads) - members), "members without lead", sorted(members - set(leads)))
set_flags = set(); need = set()
for f in (root/"Dialogue").glob("*.asset"):
    t = f.read_text(encoding="utf-8")
    set_flags |= set(re.findall(r"setsFlag: (\S+)", t)); need |= set(re.findall(r"requiresFlag: (\S+)", t))
    ids = re.findall(r"^  - id: (\S+)", t, re.M); nxt = set(re.findall(r"nextId: (\S+)", t))
    if nxt - set(ids): print("dangling next", f.name, nxt - set(ids))
    if "\u2014" in t: print("EM DASH", f.name)
narr = {n for d in leads.values() for n in d["narr"]}
print("dialogue setsFlags", sorted(set_flags)); print("needed flags not set anywhere", sorted((need | {d["flag"] for d in leads.values() if d["flag"]}) - set_flags - narr))
# reachability: simulate unlocks with all flags set
done = {"fk_p01_"+s for s in "01a 02a 03a 04a 04b 05a 06a 06b 07a 08a 09a 09b 10a 10b".split()}
changed = True
while changed:
    changed = False
    for l, d in leads.items():
        if l in done or l.startswith("fk_p01_"): continue
        if all(r in done for r in d["req"]): done.add(l); changed = True
print("unreachable with all flags", sorted(set(leads) - done))
