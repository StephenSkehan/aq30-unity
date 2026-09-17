import json, re, pathlib
root = pathlib.Path(r"C:/users/user/dev/aq30-unity")
dlg = root/"Assets/Content/FourKeys/Dialogue"; pk = root/"Assets/Content/FourKeys/Packages"
scr = pathlib.Path(r"C:/Users/User/AppData/Local/Temp/claude/C--users-user-dev-aq30-unity/6fd94396-1301-4d14-9a74-0328537c07d6/scratchpad/dialogue")
BGNAME = {"b61e15d0a1b2c3d4e5f6a7b8c9d0e101":"studio","b61e15d0a1b2c3d4e5f6a7b8c9d0e102":"studio, on air","b61e15d0a1b2c3d4e5f6a7b8c9d0e103":"studio, dawn","b61e15d0a1b2c3d4e5f6a7b8c9d0e104":"Rusty Anchor","b61e15d0a1b2c3d4e5f6a7b8c9d0e105":"Ruby's kitchen","b61e15d0a1b2c3d4e5f6a7b8c9d0e106":"street","b61e15d0a1b2c3d4e5f6a7b8c9d0e107":"moorings","b61e15d0a1b2c3d4e5f6a7b8c9d0e108":"Del's bench","b61e15d0a1b2c3d4e5f6a7b8c9d0e109":"allotments","b61e15d0a1b2c3d4e5f6a7b8c9d0e110":"cottage","be4bc156f1c11264dbd3b03b24114d6e":"diner","c26f32895ad18bd46be4538d6056e914":"rivermouth","2451cd1fa43fb724bacf7ad8ad419c9b":"case board"}
PRETTY = {"studio":"studio","studio_onair":"studio, on air","studio_dawn":"studio, dawn","rusty_anchor":"Rusty Anchor","kitchen":"Ruby's kitchen","street":"street","moorings":"moorings","del_bench":"Del's bench","allotments":"allotments","cottage":"cottage","diner":"diner","rivermouth":"rivermouth","caseboard":"case board"}
def unq(s):
    s = s.strip()
    if s.startswith('"') and s.endswith('"'): s = s[1:-1].replace('\\"','"').replace("\\\\","\\")
    return s
# package titles
titles = {}
for f in pk.glob("*.asset"):
    t = f.read_text(encoding="utf-8")
    titles[re.search(r"packageId: (\S+)", t).group(1)] = unq(re.search(r"title: (.*)", t).group(1))
CH_TITLES = {1:"FOUR KEYS",2:"THE KITCHEN TABLE",3:"THE TOWN",4:"THE MONDAY VISITOR",5:"MARGO",6:"THE SCREENSHOT",7:"THE BOX NUMBER",8:"THE FIRST TRAIN",9:"ONE OF A KIND",10:"THE MAN ON THE WALL"}
out = ["THE FRIENDS WITH FOUR KEYS", "All dialogue by chapter and scene. Generated 2026-09-15 from the game assets. Every line is a draft until ruled in play.", ""]
def flagnote(f):
    m = {"aq.fk.d1.aired":"only if the letter was aired","aq.fk.d1.held":"only if the letter was held","aq.fk.d2.direct":"only if Ally rang Del direct","aq.fk.d2.market":"only if Ally went to the sheds first","aq.fk.d3.verbatim":"only if Margo's words were aired verbatim","aq.fk.d3.paraphrase":"only if Margo's words were paraphrased","aq.fk.d4.mark_aired":"only if the mark was aired","aq.fk.d4.mark_held":"only if the mark was held"}
    return m.get(f, f)
# chapter 1 from assets
def ch1():
    lines = []
    files = sorted(dlg.glob("Resolve_FK_P01_*.asset"), key=lambda p: (int(re.search(r"P01_(\d+)", p.name).group(1)), p.name))
    for f in files:
        t = f.read_text(encoding="utf-8")
        pid = "fk_p01_" + re.search(r"P01_(\w+)", f.name).group(1).lower()
        bg = BGNAME.get((re.search(r"stageBackground: \{fileID: \d+, guid: ([0-9a-f]+)", t) or [None, ""])[1], "")
        lines += ["", f"Scene {pid}: {titles.get(pid, '')}" + (f"  [{bg}]" if bg else ""), ""]
        nodes = re.findall(r"  - id: (\S+)\n    speaker: (.*)\n    line: (.*)\n(?:.*\n){4}    requiresFlag: ?(.*)\n(?:.*\n)    setsFlag: ?(.*)\n", t)
        for nid, sp, ln, rf, sf in nodes:
            pre = f"  ({flagnote(rf.strip())}) " if rf.strip() else "  "
            lines.append(f"{pre}{unq(sp)}: {unq(ln)}")
        # choices
        for m in re.finditer(r"    - text: (.*)\n      nextId: (\S+)", t):
            lines.append(f"    > CHOICE: {unq(m.group(1))}")
    return lines
out += ["=" * 70, "CHAPTER 1 · " + CH_TITLES[1], "=" * 70] + ch1()
for ch in range(2, 11):
    data = json.loads((scr/f"ch{ch:02d}.json").read_text(encoding="utf-8"))
    out += ["", "", "=" * 70, f"CHAPTER {ch} · {CH_TITLES[ch]}", "=" * 70]
    for p in data:
        out += ["", f"Scene {p['id']}: {titles.get(p['id'], '')}  [{PRETTY.get(p['background'], p['background'])}]", ""]
        for nd in p["nodes"]:
            pre = f"  ({flagnote(nd['variant'])}) " if nd.get("variant") else "  "
            out.append(f"{pre}{nd['speaker']}: {nd['line']}")
        if p.get("decision"):
            out += ["", f"  DECISION. Ally: {p['decision']['prompt']}"]
            for o in p["decision"]["options"]:
                out += [f"    > {o['label']}", f"      Ally Quinn: {o['result']}"]
            out.append("")
        for nd in p.get("after", []):
            out.append(f"  {nd['speaker']}: {nd['line']}")
dest = root/"SAS/four-keys-dialogue-all-chapters.txt"
dest.write_text("\n".join(out) + "\n", encoding="utf-8")
print(dest, len(out), "lines")
