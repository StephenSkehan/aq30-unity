using System.Collections.Generic;
using AQ.App.UI.Specials;

namespace AQ.App.UI.Dossiers
{
    public enum DossierRewardKind { Item, Special, Cassette }

    public sealed class DossierReward
    {
        public DossierRewardKind kind;
        public string family;     // item family when Item; cassette resource when Cassette
        public int tier;          // item tier index when Item
        public SpecialId special; // when Special
        public string label;      // display text for the reward chip
    }

    public sealed class DossierFact
    {
        public string text;
        public int price;
        public bool gatedOnEpisodeClose; // needs aq.lead.e1_close.seen as well as CC
        public DossierReward reward;
    }

    public sealed class DossierDef
    {
        public string key;          // portrait token
        public string displayName;
        public string intro;        // free
        public List<DossierFact> facts = new();
        public DossierReward completion;      // special granted on completion
        public string completionCassette;     // resource path, null = slot reserved
    }

    /// <summary>
    /// Ep1 dossier content — Ally's case files on the cast. All facts approved
    /// by Stephen 2026-08-11 (SAS/feature-dossiers-v1.md, inventions included).
    /// Price ladder: 50 per step, capped at 300 CC. Variable fact counts are by
    /// design; the UI only ever shows the NEXT locked fact.
    /// </summary>
    public static class DossierCatalog
    {
        static DossierReward Item(string family, int tier, string label) =>
            new DossierReward { kind = DossierRewardKind.Item, family = family, tier = tier, label = label };
        static DossierReward Special(SpecialId id, string label) =>
            new DossierReward { kind = DossierRewardKind.Special, special = id, label = label };

        /// <summary>Display order for the case-file index.
        /// (Vera cut 2026-08-12, Stephen-ruled: no case file for her.)</summary>
        public static readonly string[] Order = { "ally", "gerald", "mo", "del", "dot" };

        public static readonly Dictionary<string, DossierDef> All = new()
        {
            ["ally"] = new DossierDef
            {
                key = "ally", displayName = "Ally Quinn",
                intro = "Host of Echoes of Havenbay. Licensed investigator. Tells the city what it won't tell itself.",
                facts =
                {
                    new DossierFact { price = 50,  text = "Born March 30th. An Aries, which Gerald says explains everything and excuses nothing.",
                                      reward = Item("food_gifts", 2, "Coffee and Donut") },
                    new DossierFact { price = 100, text = "The silver locket she never takes off holds a photo of her father. Thomas Quinn wrote for the Gazette. He died when she was sixteen.",
                                      reward = Item("audio_investigation", 2, "Recorder & Headphones") },
                    new DossierFact { price = 150, text = "Her favourite film is Spotlight. Her favourite food is pancakes at a late-night diner. She maintains these are professional influences.",
                                      reward = Special(SpecialId.BoxKnife, "Box Knife") },
                    new DossierFact { price = 200, gatedOnEpisodeClose = true,
                                      text = "The PI licence is real. She just knows that people talk to a podcaster long before they talk to an investigator.",
                                      reward = Special(SpecialId.SearchWarrant, "Search Warrant") },
                },
                completion = Special(SpecialId.CarbonCopy, "Carbon Copy"),
                completionCassette = null // reserved: what is a cassette of the host?
            },
            ["gerald"] = new DossierDef
            {
                key = "gerald", displayName = "Gerald Quinn",
                intro = "Retired Brookford PD detective. Ally's grandfather, first reader, and favourite bad influence.",
                facts =
                {
                    new DossierFact { price = 50,  text = "Thirty years a detective. He taught Ally what to write down, when to shut up, and how to spot a lie.",
                                      reward = Item("rusty_anchor", 3, "Beer Bottle") },
                    new DossierFact { price = 100, text = "The corner booth at the Rusty Anchor is held for him without asking. The older dockhands remember what kind of cop he was, and that credit extends to Ally.",
                                      reward = Item("rusty_anchor", 4, "Wine Glass Red") },
                    new DossierFact { price = 150, text = "The teal suit and the magenta shirt are not a phase. He says a detective should be memorable to friends and forgettable to suspects, and refuses to explain further.",
                                      reward = Special(SpecialId.BoltCutters, "Bolt Cutters") },
                    new DossierFact { price = 200, gatedOnEpisodeClose = true,
                                      text = "He checks Ally's ribs when he hugs her. Old habit. He has been inspecting her for damage since she was sixteen.",
                                      reward = Special(SpecialId.CarbonCopy, "Carbon Copy") },
                },
                completion = Special(SpecialId.SkeletonKey, "Skeleton Key"),
                completionCassette = null
            },
            ["mo"] = new DossierDef
            {
                key = "mo", displayName = "Mo Callahan",
                intro = "Owner of the Rusty Anchor. Keeper of the neutral ground and everything said across it.",
                facts =
                {
                    new DossierFact { price = 50,  text = "Third generation behind the bar. Patrick Callahan opened the doors in 1924; her father Seamus salvaged a shipwreck anchor, bolted it by the door, and renamed the place around it.",
                                      reward = Item("rusty_anchor", 2, "Tall Glass Orange") },
                    new DossierFact { price = 100, text = "House rule older than she is: violence goes outside, whoever you are. Everyone trusts the rule. That is why Mo hears everything.",
                                      reward = Item("rusty_anchor", 5, "Champagne Flute") },
                    new DossierFact { price = 150, text = "She calls exactly one person 'my lovely' at a time and rations it like the good whiskey.",
                                      reward = Special(SpecialId.BoxKnife, "Box Knife") },
                    new DossierFact { price = 200, gatedOnEpisodeClose = true,
                                      text = "Upstairs, behind a locked door: ledgers, old matchbooks, and a shoebox of favours owed. Mo collects debts patiently, the way the tide does.",
                                      reward = Special(SpecialId.EvidenceTag, "Evidence Tag") },
                },
                completion = Special(SpecialId.SearchWarrant, "Search Warrant"),
                completionCassette = null
            },
            ["del"] = new DossierDef
            {
                key = "del", displayName = "Del Cruz",
                intro = "Sergeant, Harbor Ward precinct. Calls Ally 'Quinny'. Nobody else is allowed.",
                facts =
                {
                    new DossierFact { price = 50,  text = "The kestrel on her right wrist points home to Kestrel Point. The Saint Michael medallion hangs on its own chain, never the one with the badge.",
                                      reward = Item("forensic_tools", 1, "Evidence Bag") },
                    new DossierFact { price = 100, text = "Her table outside the Kestrel Corner Diner has the best sightlines on the street. Back to the wall, eyes on the crossing and everyone's hands. Coffee makes you a friend at that table.",
                                      reward = Item("food_gifts", 1, "Hot Coffee Cup") },
                    new DossierFact { price = 150, text = "At twenty, a rookie constable, she stood on the Quinn doorstep the night Thomas died. Ally was sixteen. Del has been quietly watching out for her ever since.",
                                      reward = Special(SpecialId.SearchWarrant, "Search Warrant") },
                    new DossierFact { price = 200, gatedOnEpisodeClose = true,
                                      text = "She bends rules the way a locksmith bends wire: precisely, quietly, and only for doors that should never have been locked.",
                                      reward = Special(SpecialId.SkeletonKey, "Skeleton Key") },
                },
                completion = Special(SpecialId.BoltCutters, "Bolt Cutters"),
                completionCassette = null
            },
            ["dot"] = new DossierDef
            {
                key = "dot", displayName = "Dot Ellis",
                intro = "Retired school cleaner, Rivermouth. For three years she said goodnight to the whole city, one voicemail at a time.",
                facts =
                {
                    new DossierFact { price = 50,  text = "Thirty years cleaning the Chandler Road school. She knows every floorboard in Rivermouth and precisely which gulls are troublemakers.",
                                      reward = Item("food_gifts", 0, "Paper Cup") },
                    new DossierFact { price = 100, text = "Tuesday quiz night at the Rusty Anchor, table six. She has never once missed it.",
                                      reward = Item("rusty_anchor", 2, "Tall Glass Orange") },
                    new DossierFact { price = 150, text = "The garden behind the blue gate is the most loved thing on Chandler Road. Hollyhocks, roses, and a fence you could walk with your eyes shut.",
                                      reward = Item("food_gifts", 2, "Coffee and Donut") },
                    new DossierFact { price = 200, gatedOnEpisodeClose = true,
                                      text = "She went out through the allotments and caught the first bus of the morning, and told nobody, to keep her sister out of it. Brave, dry, and allergic to fuss.",
                                      reward = Special(SpecialId.EvidenceTag, "Evidence Tag") },
                },
                completion = null, // her cassette IS the completion reward
                completionCassette = "App/Audio/Cassettes/cassette_dot_goodnight"
            },
            // Vera's dossier CUT 2026-08-12 (Stephen-ruled). Her fact set is
            // preserved in SAS/feature-dossiers-v1.md if she ever earns a file.
        };
    }
}
