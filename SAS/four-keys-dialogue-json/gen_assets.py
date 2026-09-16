"""Generate Four Keys chapter 2..10 Lead / Package / CaseGraph assets from the writers' JSON.
Idempotent: GUIDs are kept in guids.json so reruns rewrite the same assets."""
import json, pathlib, re, uuid, sys

ROOT = pathlib.Path(r"C:/users/user/dev/aq30-unity")
SCR = pathlib.Path(__file__).parent
DLG = SCR / "dialogue"
GUIDS = SCR / "guids.json"
LEADS = ROOT / "Assets/Content/FourKeys/Leads"
PKGS = ROOT / "Assets/Content/FourKeys/Packages"
GRAPHS = ROOT / "Assets/Content/FourKeys/Dialogue"
CATALOG = ROOT / "Assets/Resources/App/FourKeys/FourKeysCh1Catalog.asset"
DATABASE = ROOT / "Assets/Resources/App/FourKeys/FourKeysCh1Database.asset"
PROSE = ROOT / "SAS/four-keys-prose"

LEAD_SCRIPT = "222fba07fae7fa343a6d2a96dfd2ee48"
PKG_SCRIPT = "53887cd0b9ce427895664a3c4544c256"
GRAPH_SCRIPT = "7e8140d05737a11468020c371c32ff33"

ITEMS = {
    "A": {1: "aad15000b1c2d3e4f5a6b7c8d9e0f101", 2: "aad15000b1c2d3e4f5a6b7c8d9e0f102", 3: "aad15000b1c2d3e4f5a6b7c8d9e0f103",
          4: "aad15000b1c2d3e4f5a6b7c8d9e0f104", 5: "aad15000b1c2d3e4f5a6b7c8d9e0f105", 6: "aad15000b1c2d3e4f5a6b7c8d9e0f106"},
    "F": {1: "3c787d60fbd852f4abc60f29989f0123", 2: "396f63cfaf7106c47bdaf61671377b24", 3: "ed7637afffe76c1428ae3bece78c9829",
          4: "4426ca7e5c440644793d4f17cc690acf", 5: "f80f6006132ef7643a5914a70001d526"},
    "D": {1: "f9d5d610dd60ae64aba8b42b34e925a4", 2: "abd1a75e4f876994eafb7f2ab137296d", 3: "a10bd1ec8cf686d4894f870d3b7fa2d7",
          4: "e75756c20bf853a4d987fbc21775a7f8", 5: "aa1431f6b45ac784ba1b01417415c0e5", 6: "2f54822b995d319418590bb0b19fcae9"},  # food_gifts (what the diner drops)
    "R": {1: "8f0bf91667254c147a05247a5d54fbd6", 2: "84f73dd9521c77441865d116fe4921d8", 3: "26e209f3237ee81459398dad9baf75da",
          4: "cc9c941c9760f9341aecdd32cfd7888d", 5: "95ccc8a89377a3d488b11202fcaed138", 6: "84f8d5a60806dc3488407f1eb600a404",
          7: "a8a2d784d98e4a3479815cb433f58d81", 8: "b5ee3a1771e8f2a43b3ddc27ee73ca91", 9: "be26dad8ae1883c489ee2ea85799c426",
          10: "1dddaea9458578f4ab305f72da6bc5eb"},  # rusty_anchor (what Salvage Stores drops once aq.loc.rusty_anchor.active is set)
}
FAMILY_LABEL = {"A": "Audio Evidence", "F": "Forensic Work", "D": "Diner", "R": "Rusty Anchor"}

ALLY = {"neutral": "792b5f88f6dd2724d84b6954cc0b5fad", "happy": "97959354725a96b4cb5e7dd0e72265cd", "sad": "a6ca2348df1d653488455433cabdac99",
        "angry": "230f35c9bb532ef499cee05b337028d0", "surprised": "7e03a4f198056f44ba2209764d9e8219", "worried": "e02460f0c092dde4891723eecbbdc368",
        "confused": "ae0c3985c1991244d8ee72fa0756eb08"}
DEL = {"neutral": "de1a4b5c6d7e8f90a1b2c3d4e5f6a701", "happy": "de1a4b5c6d7e8f90a1b2c3d4e5f6a702", "sad": "de1a4b5c6d7e8f90a1b2c3d4e5f6a703",
       "angry": "de1a4b5c6d7e8f90a1b2c3d4e5f6a704", "surprised": "de1a4b5c6d7e8f90a1b2c3d4e5f6a705", "worried": "de1a4b5c6d7e8f90a1b2c3d4e5f6a706",
       "confused": "de1a4b5c6d7e8f90a1b2c3d4e5f6a707"}
GERALD = "4e5f6a7b8c9d4e5f8a7b6c5d4e3f2a1b"
RUBY = {"neutral": "3eda8c3a8ba44593b8d95260ba6b7487", "happy": "ee933f5d948e45808a57817fd2e9d4fe", "sad": "0daf7b4e2afb4baca0c6660666687b94", "angry": "05596daefe01434b8af754b91499af1a", "surprised": "8acbbfbedc1e49c1808d453fff2486ab", "worried": "4f3c2252e9b24f179e5e92bb42f0a6bb", "confused": "0bf73a905b8a4f11a7398a494390fc6f"}  # imported 2026-09-16
EMOTION = {"neutral": 0, "happy": 1, "sad": 2, "angry": 3, "surprised": 4, "worried": 5, "confused": 6}
BG = {"studio": "b61e15d0a1b2c3d4e5f6a7b8c9d0e101", "studio_onair": "b61e15d0a1b2c3d4e5f6a7b8c9d0e102", "studio_dawn": "b61e15d0a1b2c3d4e5f6a7b8c9d0e103",
      "rusty_anchor": "b61e15d0a1b2c3d4e5f6a7b8c9d0e104", "kitchen": "b61e15d0a1b2c3d4e5f6a7b8c9d0e105", "street": "b61e15d0a1b2c3d4e5f6a7b8c9d0e106",
      "moorings": "b61e15d0a1b2c3d4e5f6a7b8c9d0e107", "del_bench": "b61e15d0a1b2c3d4e5f6a7b8c9d0e108", "allotments": "b61e15d0a1b2c3d4e5f6a7b8c9d0e109",
      "cottage": "b61e15d0a1b2c3d4e5f6a7b8c9d0e110", "diner": "be4bc156f1c11264dbd3b03b24114d6e", "rivermouth": "c26f32895ad18bd46be4538d6056e914",
      "caseboard": "2451cd1fa43fb724bacf7ad8ad419c9b"}
BEAT = {"ET": 0, "CF": 1, "AL": 2, "AC": 3}

# ---- chapter spec: (suffix, cards, cc, beat, title) ; cards "F4,A3,D3" ; extra keys per package below ----
def P(s, cards, cc, beat, title): return dict(s=s, cards=cards, cc=cc, beat=beat, title=title)
SPEC = {
 2: [P("01","D2",15,"AL","Ruby's Kitchen"), P("02","F2,A2",20,"AC","The Note Under Glass"), P("03","A3,D3",45,"ET","The Letter"),
     P("04","D2,D2,D2",20,"CF","Ruby Walker"), P("05","A3",20,"CF","Four in the Morning"), P("06","R4,R3,R3",60,"ET","The Room"),
     P("07","A4",25,"CF","Four Habits"), P("08","F4,F2",45,"ET","The Covering Slip"), P("09","D4",30,"CF","Saturdays"), P("10","A3",20,"AL","Episode Two")],
 3: [P("01","D2,D2",20,"AC","The Kestrel Corner Diner"), P("02","D4,A3",55,"ET","Calling It In"), P("03","D3,F2",30,"ET","The Sister"),
     P("04","F3,F3",20,"AC","The Regent"), P("05","D3,D2,A2",35,"CF","The Wrong Red"), P("06","A4,D4",70,"ET","The Panel"),
     P("07","R4,D3",50,"ET","Two Signwriters"), P("08","R4,R2,R1",45,"ET","The Slip Fall"), P("09","A5",75,"ET","One Town"), P("10","A4,F2,F2",50,"AL","Episode Three")],
 4: [P("01","R3,D2,D2",35,"AC","The Boatyard"), P("02","R4",35,"CF","The Tenor"), P("03","A4,D3",50,"ET","Liam Bryce"),
     P("04","A3,F3",35,"ET","The Letter's Lines"), P("05","R4,A3",50,"ET","The Second Pot"), P("06","A5,R4",100,"ET","Built"),
     P("07","D4",35,"CF","August 2014"), P("08","R4,D3,F2",60,"AC","The Boatsheds"), P("09","R5,F4,A3",120,"ET","The Monday Visitor"), P("10","A4,D3,D1",60,"AL","Episode Four")],
 5: [P("01","D3,R3",35,"AC","The Office"), P("02","R4,A3",50,"CF","The County Swimmer"), P("03","D4",35,"CF","Don't Be a Martyr"),
     P("04","F3,D3",35,"CF","Violet's Bag"), P("05","D4,A4,R3,R3",100,"ET","A Silence With a Shape"), P("06","F4,A5,R4",140,"ET","The Man in the Lane"),
     P("07","F4,D3,A2",60,"ET","Ten Minutes Early"), P("08","A5,A3,D3",100,"AL","The Ask"), P("09","D4,D3",50,"CF","Twice"), P("10","A5,R4,F3",125,"AL","Episode Five")],
 6: [P("01","A4,F3,F1",55,"ET","One Letter"), P("02","D5,D3",85,"AC","The Coast Road"), P("03","D4,A4",70,"CF","Tessa"),
     P("04","F4,A3",50,"CF","The Card in the Drawer"), P("05","A4",35,"CF","A Sentence Is an Address"), P("06","A5,F4,F3",120,"ET","The Screenshot"),
     P("07","F5,A4",100,"ET","Line by Line"), P("08","F5,A5,D4",170,"ET","The Plainer Question"), P("09","F4,A4,D3",85,"ET","That"), P("10","A5,D3,D3",110,"AL","Episode Six")],
 7: [P("01v","F4,A3,D3",70,"ET","Two Letters"), P("01p","F4,A3,D3",70,"ET","One Letter"), P("02v","F5,F3",85,"AC","Side by Side"), P("02p","F5,F3",85,"AC","Half of Another"),
     P("03","A5,F4",100,"ET","The Slip"), P("04","F4,A4",70,"ET","The Times"), P("05","A5,F3",85,"ET","The Kind Line"), P("06","R4,D4,A2",75,"CF","The Marathon"),
     P("07","A5,F5,F3",155,"ET","The Box Number"), P("08","R5,D3",85,"AL","Gerald's Method"), P("09","A5,F4,R4,D3,A1",160,"ET","Built to Air"), P("10","A4",40,"AL","Episode Seven")],
 8: [P("01","A5,R4",100,"ET","The Skipper's Letter"), P("02","R5,R4,A4,D3,F3",170,"ET","The Harbour Years"), P("03","R4,D3",50,"AL","The Ledger of Leavers"),
     P("04","A3",20,"AL","The Drawer"), P("05","A5,F4",100,"ET","The First Train"), P("06","F5,R4,A3",120,"ET","The Timetable"),
     P("07","D4,R3",50,"CF","The Clerk"), P("08","A5,A3",85,"AL","Write to Me"), P("09","F5,A5,R4,D4,F3",230,"ET","The Re-String"), P("10","A3",20,"AL","Episode Eight")],
 9: [P("01","A4,D4,D3",85,"ET","Four Describers"), P("02","F4,D3,D3",70,"ET","Cross-Check One"), P("03","F4,D3",50,"ET","Cross-Check Two"),
     P("04","F5,A4",100,"ET","Cross-Check Three"), P("05","D4,A3",50,"ET","Her Own Alphabet"), P("06","R3",20,"CF","The Brush"),
     P("07","A4,F3,R3",70,"ET","Del's Ask"), P("08","R5,A5,F5,F4",240,"ET","One of a Kind"), P("09","A5,D4,R4",135,"AL","Episode Nine"), P("10","A4,R4,F3,F3,D3",125,"AL","The Tip Line Light")],
 10:[P("01","A5,F4",100,"ET","The Landlady"), P("02","A4,F3,D2",60,"ET","Del's Bench"), P("03","D2",10,"AL","The Wait"),
     P("04","A5,A4,F4,R4,D3",185,"ET","Built to Sound the Same"), P("05","D4,A3,R3,D3",80,"CF","Never Evidence"), P("06","A3",20,"AL","Episode Ten"),
     P("07","A5,R4,F3",120,"ET","Day 333"), P("08a","A4,F4,R4,D4,F3",150,"ET","The Flood"), P("08h","F5,A4,R4,D3,F3",150,"ET","One Chain"),
     P("09a","A4,D3,F3",70,"CF","Margo, Re-read"), P("09h","A4,D3,F3",70,"CF","Margo, Re-read"),
     P("10a","A5,R4,D4,F3,D2",180,"AL","Goodnight, Harbour"), P("10h","A5,R4,D4,F3,D2",180,"AL","Goodnight, Harbour")],
}
# predecessor graph: package suffix -> list of predecessor package ids (full ids) ; "FLAG:x" = requiresFlag-only gate
def pid(ch, s): return f"fk_p{ch:02d}_{s}"
PRED = {}
def chain(ch, order, parallels):
    """order: list of suffixes in linear order; parallels: dict suffix -> explicit predecessor suffix list."""
    prev = None
    for s in order:
        if s in parallels: PRED[pid(ch, s)] = [pid(ch, x) for x in parallels[s]]
        else: PRED[pid(ch, s)] = [prev] if prev else []
        prev = pid(ch, s)
PRED[pid(2,"01")] = ["fk_p01_10b_LEAD"]  # special: ch1 last lead id
chain(2, ["01","02","03","04","05","06","07","08","09","10"], {"05": ["03"], "06": ["04","05"]})
PRED[pid(2,"01")] = ["fk_p01_10b_LEAD"]
for ch, first in [(3,"fk_p02_10"),(4,"fk_p03_10"),(5,"fk_p04_10"),(6,"fk_p05_10"),(8,"fk_p07_10"),(9,"fk_p08_10"),(10,"fk_p09_10")]:
    pass
chain(3, ["01","02","03","04","05","06","07","08","09","10"], {"05": ["03"], "06": ["04","05"]}); PRED[pid(3,"01")] = ["fk_p02_10"]
chain(4, ["01","02","03","04","05","06","07","08","09","10"], {"05": ["03"], "06": ["04","05"]}); PRED[pid(4,"01")] = ["fk_p03_10"]
chain(5, ["01","02","03","04","05","06","07","08","09","10"], {"04": ["02"], "05": ["03","04"]}); PRED[pid(5,"01")] = ["fk_p04_10"]
chain(6, ["01","02","03","04","05","06","07","08","09","10"], {"04": ["02"], "05": ["03","04"]}); PRED[pid(6,"01")] = ["fk_p05_10"]
chain(7, ["01v","01p","02v","02p","03","04","05","06","07","08","09","10"],
      {"01v": [], "01p": [], "02v": ["01v"], "02p": ["01p"], "03": []})
PRED[pid(7,"01v")] = ["fk_p06_10"]; PRED[pid(7,"01p")] = ["fk_p06_10"]
chain(8, ["01","02","03","04","05","06","07","08","09","10"], {"04": ["02"], "05": ["03","04"]}); PRED[pid(8,"01")] = ["fk_p07_10"]
chain(9, ["01","02","03","04","05","06","07","08","09","10"], {"03": ["01"], "04": ["02","03"], "06": ["04"], "07": ["05","06"]}); PRED[pid(9,"01")] = ["fk_p08_10"]
chain(10, ["01","02","03","04","05","06","07","08a","08h","09a","09h","10a","10h"],
      {"08a": ["07"], "08h": ["07"], "09a": ["08a"], "09h": ["08h"], "10a": ["09a"], "10h": ["09h"]}); PRED[pid(10,"01")] = ["fk_p09_10"]

# flag gates on leads
REQ_FLAG = {pid(7,"01v"): "aq.fk.d3.verbatim", pid(7,"01p"): "aq.fk.d3.paraphrase",
            pid(7,"02v"): "aq.fk.d3.verbatim", pid(7,"02p"): "aq.fk.d3.paraphrase",
            pid(7,"03"): "fk.p07_02.done",
            pid(10,"08a"): "aq.fk.d4.mark_aired", pid(10,"08h"): "aq.fk.d4.mark_held"}
# narrative flags set by every card of a package on resolve
NARR = {pid(7,"02v"): ["fk.p07_02.done"], pid(7,"02p"): ["fk.p07_02.done"]}
for ch in range(2, 10): NARR[pid(ch, "10")] = NARR.get(pid(ch, "10"), []) + [f"fk.ch{ch}.complete"]
NARR[pid(10,"10a")] = ["fk.ch10.complete"]; NARR[pid(10,"10h")] = ["fk.ch10.complete"]
PKG_ENERGY = {pid(5,"10"): 20, pid(7,"07"): 20}
PKG_PREMIUM = {pid(9,"08"): 2}
GEN_GRANT = {}  # lead id -> generator type id, filled below (first D card -> corner_diner, first R card -> gen_junk)

# ---- helpers ----
def load_guids():
    return json.loads(GUIDS.read_text()) if GUIDS.exists() else {}
def guid_for(reg, key):
    if key not in reg: reg[key] = uuid.uuid4().hex
    return reg[key]
def q(s):
    s = s.replace("\\", "\\\\").replace('"', '\\"')
    return '"' + s + '"'
def meta(guid):
    return f"fileFormatVersion: 2\nguid: {guid}\nNativeFormatImporter:\n  externalObjects: {{}}\n  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
HEAD = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
def portrait(speaker, emotion):
    if speaker == "Ally Quinn": return ALLY[emotion]
    if speaker == "Del Cruz": return DEL[emotion]
    if speaker == "Gerald": return GERALD
    if speaker == "Ruby Walker": return RUBY[emotion]
    return None
def ref(guid, fid=21300000, t=3):
    return f"{{fileID: {fid}, guid: {guid}, type: {t}}}" if guid else "{fileID: 0}"

def lead_yaml(name, lead_id, title, reqs, required_ids, req_flag, narr, gen, subtitle="DRAFT"):
    y = HEAD + f"  m_Script: {{fileID: 11500000, guid: {LEAD_SCRIPT}, type: 3}}\n  m_Name: {name}\n  m_EditorClassIdentifier: AQ.App::AQ.App.Leads.LeadData\n"
    y += f"  leadId: {lead_id}\n  title: {q(title)}\n  subtitle: {q(subtitle)}\n  actorPortrait: {ref(ALLY['neutral'])}\n  state: 0\n  ActionType: 2\n  EnergyCost: 0\n  requirements:\n"
    for fam, tier in reqs:
        y += f"  - label: {FAMILY_LABEL[fam]} T{tier}\n    icon: {{fileID: 0}}\n    satisfied: 0\n    itemDefinition: {ref(ITEMS[fam][tier], 11400000, 2)}\n    quantity: 1\n"
    y += "  RequiredLeadIds: " + ("[]\n" if not required_ids else "\n" + "".join(f"  - {r}\n" for r in required_ids))
    y += f"  requiresFlag: {req_flag}\n  forbidsFlag: \n"
    y += "  EvidenceIds: []\n  SpawnLeadIds: []\n"
    y += "  NarrativeFlags: " + ("[]\n" if not narr else "\n" + "".join(f"  - {f}\n" for f in narr))
    y += f"  SoftCurrency: 0\n  EnergyGrant: 0\n  PremiumGrant: 0\n  generatorRewardTypeId: {gen}\n  generatorRewardTier: 0\n  specialRewardId: \n  specialRewardCount: 1\n  resolutionDialogue: {{fileID: 0}}\n  OutcomeHints: 0\n  boardConnections: []\n  boardPhase: 1\n"
    return y

def pkg_yaml(name, package_id, ch, title, members, beat, graph_guid, cc, energy, premium):
    y = HEAD + f"  m_Script: {{fileID: 11500000, guid: {PKG_SCRIPT}, type: 3}}\n  m_Name: {name}\n  m_EditorClassIdentifier: AQ.App::AQ.App.Leads.Packages.PackageData\n"
    y += f"  packageId: {package_id}\n  chapter: {ch}\n  title: {q(title)}\n  memberCardIds: \n" + "".join(f"  - {m}\n" for m in members)
    y += f"  beatType: {beat}\n  beatDialogue: {ref(graph_guid, 11400000, 2)}\n  beatArt: {{fileID: 0}}\n  beatCaption: \n  softCurrency: {cc}\n  energyGrant: {energy}\n  premiumGrant: {premium}\n  specialRewardId: \n  specialRewardCount: 1\n"
    return y

def node_yaml(nid, speaker, emotion, line, next_id, req_flag="", sets_flag="", choices=None):
    y = f"  - id: {nid}\n    speaker: {q(speaker)}\n    line: {q(line)}\n    portrait: {ref(portrait(speaker, emotion))}\n    emotion: {EMOTION[emotion]}\n    voiceClip: {{fileID: 0}}\n    waitForAudio: 0\n"
    y += f"    requiresFlag: {req_flag}\n    skipIfFlagMissing: 1\n    setsFlag: {sets_flag}\n    nextId: {next_id}\n"
    if choices:
        y += "    choices:\n" + "".join(f"    - text: {q(c['label'])}\n      nextId: {c['nid']}\n      requiresFlag: \n" for c in choices)
    else:
        y += "    choices: []\n"
    return y

def graph_yaml(name, prefix, bg, pkg):
    """Linear nodes; optional decision (prompt node with choices -> result nodes -> after nodes)."""
    seq = []  # (nid, speaker, emotion, line, req_flag, sets_flag, choices)
    n = 0
    for nd in pkg["nodes"]:
        n += 1; seq.append((f"{prefix}_N{n}", nd["speaker"], nd["emotion"], nd["line"], nd.get("variant", ""), "", None))
    after_nodes = []
    for nd in pkg.get("after", []):
        n += 1; after_nodes.append((f"{prefix}_N{n}", nd["speaker"], nd["emotion"], nd["line"], nd.get("variant", ""), "", None))
    dec = pkg.get("decision")
    out = HEAD + f"  m_Script: {{fileID: 11500000, guid: {GRAPH_SCRIPT}, type: 3}}\n  m_Name: {name}\n  m_EditorClassIdentifier: AQ.App::AQ.App.CaseGraph\n"
    out += f"  startId: {prefix}_N1\n  stageBackground: {ref(BG[bg])}\n  nodes:\n"
    if dec:
        results = []
        for i, opt in enumerate(dec["options"]):
            results.append((f"{prefix}_R{i+1}", "Ally Quinn", "neutral", opt["result"], "", opt["flag"], None))
        prompt = (f"{prefix}_D", "Ally Quinn", "worried", dec["prompt"], "", "", [{"label": o["label"], "nid": f"{prefix}_R{i+1}"} for i, o in enumerate(dec["options"])])
        after_first = after_nodes[0][0] if after_nodes else ""
        # main nodes -> prompt
        for i, s in enumerate(seq):
            nxt = seq[i+1][0] if i+1 < len(seq) else prompt[0]
            out += node_yaml(s[0], s[1], s[2], s[3], nxt, s[4], s[5], s[6])
        out += node_yaml(prompt[0], prompt[1], prompt[2], prompt[3], results[0][0], "", "", prompt[6])
        for r in results:
            out += node_yaml(r[0], r[1], r[2], r[3], after_first, "", r[5], None)
        for i, s in enumerate(after_nodes):
            nxt = after_nodes[i+1][0] if i+1 < len(after_nodes) else ""
            out += node_yaml(s[0], s[1], s[2], s[3], nxt, s[4], s[5], s[6])
    else:
        allseq = seq + after_nodes
        for i, s in enumerate(allseq):
            nxt = allseq[i+1][0] if i+1 < len(allseq) else ""
            out += node_yaml(s[0], s[1], s[2], s[3], nxt, s[4], s[5], s[6])
    return out

def parse_cards(spec):
    return [(c[0], int(c[1:])) for c in spec.split(",")]

def main():
    reg = load_guids()
    ch1_leads = ["fk_p01_01a","fk_p01_02a","fk_p01_03a","fk_p01_04a","fk_p01_04b","fk_p01_05a","fk_p01_06a","fk_p01_06b","fk_p01_07a","fk_p01_08a","fk_p01_09a","fk_p01_09b","fk_p01_10a","fk_p01_10b"]
    members = {}  # package id -> lead ids
    # pass 1: member ids
    for ch, pkgs in SPEC.items():
        for p in pkgs:
            cards = parse_cards(p["cards"])
            members[pid(ch, p["s"])] = [f"{pid(ch, p['s'])}{chr(ord('a')+i)}" for i in range(len(cards))]
    # generator grants land one card BEFORE the first card that needs the family (a card cannot grant
    # the generator its own requirement needs). corner_diner + aq.loc.rusty_anchor.active sit on ch1's
    # fk_p01_10b (patched outside this script); gen_junk sits on p02_05a, which every p02_06 card requires.
    GEN_GRANT["fk_p02_05a"] = "gen_junk"
    # pass 2: write
    pkg_guids, lead_guids = [], []
    problems = []
    for ch, pkgs in SPEC.items():
        data = {p["id"]: p for p in json.loads((DLG / f"ch{ch:02d}.json").read_text(encoding="utf-8"))}
        prose = [f"# The Friends with Four Keys, chapter {ch} (generated from the writers' JSON; Stephen rules phrasing in play)\n"]
        for p in pkgs:
            P_ID = pid(ch, p["s"])
            if P_ID not in data: problems.append(f"missing dialogue for {P_ID}"); continue
            d = data[P_ID]
            for nd in d["nodes"] + d.get("after", []) + ([{"line": d["decision"]["prompt"]}] + [{"line": o["result"]} for o in d["decision"]["options"]] if d.get("decision") else []):
                if "\u2014" in nd["line"] or "\u2013" in nd["line"]: problems.append(f"dash in {P_ID}: {nd['line'][:60]}")
            if d["background"] not in BG: problems.append(f"bad background {d['background']} in {P_ID}"); continue
            tag = f"FK_P{ch:02d}_{p['s'].upper()}"
            # graph
            gname = f"Resolve_{tag}"; gg = guid_for(reg, gname)
            (GRAPHS / f"{gname}.asset").write_text(graph_yaml(gname, tag, d["background"], d), encoding="utf-8")
            (GRAPHS / f"{gname}.asset.meta").write_text(meta(gg), encoding="utf-8")
            # predecessors -> required lead ids
            req_ids = []
            for pre in PRED[P_ID]:
                if pre.endswith("_LEAD"): req_ids.append(pre[:-5])
                else: req_ids += members[pre]
            cards = parse_cards(p["cards"])
            for i, (fam, tier) in enumerate(cards):
                lid = members[P_ID][i]
                lname = f"Lead_{tag}{chr(ord('a')+i)}"; lg = guid_for(reg, lname)
                title = p["title"] + (f" ({chr(ord('A')+i)})" if len(cards) > 1 else "")
                (LEADS / f"{lname}.asset").write_text(lead_yaml(lname, lid, title, [(fam, tier)], req_ids, REQ_FLAG.get(P_ID, ""), NARR.get(P_ID, []), GEN_GRANT.get(lid, ""), f"Chapter {ch} · Scene {int(p['s'][:2])}"), encoding="utf-8")
                (LEADS / f"{lname}.asset.meta").write_text(meta(lg), encoding="utf-8")
                lead_guids.append(lg)
            pname = f"Package_{tag}"; pg = guid_for(reg, pname)
            (PKGS / f"{pname}.asset").write_text(pkg_yaml(pname, P_ID, ch, p["title"], members[P_ID], BEAT[p["beat"]], gg, p["cc"], PKG_ENERGY.get(P_ID, 0), PKG_PREMIUM.get(P_ID, 0)), encoding="utf-8")
            (PKGS / f"{pname}.asset.meta").write_text(meta(pg), encoding="utf-8")
            pkg_guids.append(pg)
            # prose record
            prose.append(f"\n## {P_ID} · {p['title']} · {p['cards']} · {p['cc']} CC · {d['background']}\n")
            for nd in d["nodes"]:
                v = f" [{nd['variant']}]" if nd.get("variant") else ""
                prose.append(f"**{nd['speaker']}** ({nd['emotion']}){v}: {nd['line']}\n")
            if d.get("decision"):
                prose.append(f"\n*Decision:* {d['decision']['prompt']}\n")
                for o in d["decision"]["options"]:
                    prose.append(f"- **{o['label']}** → `{o['flag']}`: {o['result']}\n")
            for nd in d.get("after", []):
                prose.append(f"**{nd['speaker']}** ({nd['emotion']}): {nd['line']}\n")
        PROSE.mkdir(exist_ok=True)
        (PROSE / f"ch{ch:02d}.md").write_text("\n".join(prose), encoding="utf-8")
    GUIDS.write_text(json.dumps(reg, indent=1))
    # catalog + database: keep ch1 entries, append ours
    def rewrite(path, key, count_keep, guids):
        txt = path.read_text(encoding="utf-8")
        m = re.search(rf"  {key}:\n((?:  - \{{fileID: 11400000, guid: [0-9a-f]+, type: 2\}}\n)+)", txt)
        keep = m.group(1).splitlines()[:count_keep]
        new = "\n".join(keep + [f"  - {{fileID: 11400000, guid: {g}, type: 2}}" for g in guids]) + "\n"
        path.write_text(txt[:m.start(1)] + new + txt[m.end(1):], encoding="utf-8")
    rewrite(CATALOG, "packages", 12, pkg_guids)
    rewrite(DATABASE, "leads", 14, lead_guids)
    print("packages", len(pkg_guids), "leads", len(lead_guids))
    for pr in problems: print("PROBLEM", pr)

main()
