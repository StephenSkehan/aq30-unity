using System.Collections.Generic;

namespace AQ.App.UI.EvidenceBoard
{
    /// <summary>
    /// Code-defined Ep1 location set (DossierCatalog pattern). Locations are
    /// keyed off CaseGraph.stageBackground sprite names, so the board derives
    /// WHERE from data the same way the cast row derives WHO from portraits.
    /// Prices 40/80/120 CC per Stephen's ruling 2026-08-14; detail 0 is free
    /// at discovery; the last detail also unlocks the wide view.
    /// </summary>
    public static class LocationCatalog
    {
        public class Detail
        {
            public string text;
            public int price;      // 0 = free at discovery
            public bool wideView;  // unlocks the full-bleed location image
        }

        public class Def
        {
            public string key;
            public string displayName;
            public string epigraph;
            public Detail[] details;
        }

        // Sprite-name fragment -> location key. Checked with Contains so the
        // studio variants (dawn/onair) and both Chandler sprites group.
        // del_bench folds into the diner (Stephen-ruled 2026-08-14: Del's spot
        // is the Kestrel Corner table; L9's "her bench" reconciles in the
        // L5+ script pass).
        private static readonly (string fragment, string key)[] SpriteMap =
        {
            ("studio",          "studio"),
            ("chandler",        "chandler"),
            ("allotments",      "allotments"),
            ("hill_cottage",    "cottage"),
            ("moorings",        "moorings"),
            ("rusty_anchor",    "anchor"),
            ("diner",           "diner"),
            ("del_bench",       "diner"),
        };

        /// <summary>Null/unknown sprites bucket to the studio (the default
        /// backdrop IS the studio scrim), so no replay is ever orphaned.</summary>
        public static string KeyForSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return "studio";
            var lower = spriteName.ToLowerInvariant();
            foreach (var (fragment, key) in SpriteMap)
                if (lower.Contains(fragment)) return key;
            return "studio";
        }

        public static Def Get(string key) => Defs.TryGetValue(key, out var d) ? d : null;
        public static IEnumerable<Def> All => Defs.Values;

        private static Detail D(string text, int price, bool wide = false)
            => new Detail { text = text, price = price, wideView = wide };

        private static readonly Dictionary<string, Def> Defs = new()
        {
            ["studio"] = new Def
            {
                key = "studio", displayName = "The Studio",
                epigraph = "Where the city leaves its messages.",
                details = new[]
                {
                    D("Rented over a chandlery. The landlord thinks podcasting is a phase.", 0),
                    D("The Tip Line machine predates the show. Gerald found it at a police auction and never said which precinct.", 40),
                    D("Three years of tapes in the cupboard. Ally labels them by weather, not by date.", 80),
                    D("The on-air lamp was Thomas Quinn's desk lamp. It runs hot. She never switches it off mid-story.", 120, wide: true),
                }
            },
            ["chandler"] = new Def
            {
                key = "chandler", displayName = "Chandler Road",
                epigraph = "The blue gate at eleven.",
                details = new[]
                {
                    D("Dot's street. Thirty years walking to the same school gate.", 0),
                    D("The garden is postcard-neat. The fence is walkable. Both mattered on the night.", 40),
                    D("Mrs Vale next door holds the spare key and most of the street's secrets.", 80),
                    D("From the kitchen you can hear the pump house. That detail closed the case.", 120, wide: true),
                }
            },
            ["allotments"] = new Def
            {
                key = "allotments", displayName = "The Allotments",
                epigraph = "An escape route dressed as a hobby.",
                details = new[]
                {
                    D("Sheds, bean rows, one gate that never locks.", 0),
                    D("Dot knew the path in the dark. Thirty years of shortcuts.", 40),
                    D("The first bus stops two streets over. The driver knew her by name.", 80),
                    D("Someone else learned the path too. The grass remembered before anyone else did.", 120, wide: true),
                }
            },
            ["cottage"] = new Def
            {
                key = "cottage", displayName = "The Hill Cottage",
                epigraph = "Larkhill. Vera says hello.",
                details = new[]
                {
                    D("Dot's sister Vera lives here. For Vera, hello is a parade.", 0),
                    D("The cottage keeps no phone. Letters, or nothing.", 40),
                    D("You can see the bay from the kitchen window. Vera counts the boats.", 80),
                    D("Two sisters, one case between them, and neither would say the word afraid.", 120, wide: true),
                }
            },
            ["moorings"] = new Def
            {
                key = "moorings", displayName = "The Moorings",
                epigraph = "Quiet boats keep their secrets.",
                details = new[]
                {
                    D("Rivermouth's cheap berths. Paint peels, and nobody watches.", 0),
                    D("A pale van parked here three nights running. Harbourline Security, dissolved on paper.", 40),
                    D("The berth ledger runs on cash and first names.", 80),
                    D("Del walked the pontoons off the record. Not on a podcast's say-so, she said. She came anyway.", 120, wide: true),
                }
            },
            ["anchor"] = new Def
            {
                key = "anchor", displayName = "The Rusty Anchor",
                epigraph = "Neutral ground. Fair prices. No questions.",
                details = new[]
                {
                    D("Founded 1924 by Patrick Callahan. Renamed by his son. Kept by Mo.", 0),
                    D("Gerald's booth is the second from the window. Nobody else sits there twice.", 40),
                    D("Mo hears everything and repeats nothing. That is the licence she really trades on.", 80),
                    D("The back room is not on any floor plan. Ask Mo, and only once.", 120, wide: true),
                }
            },
            ["diner"] = new Def
            {
                key = "diner", displayName = "Kestrel Corner Diner",
                epigraph = "Del's table. Back to the wall.",
                details = new[]
                {
                    D("The corner diner under the kestrel sign. Del's sidewalk table faces the street.", 0),
                    D("She picks sightlines the way other people pick seats. Watch the hands, she says.", 40),
                    D("The takeout boxes open doors in Rivermouth better than a badge does.", 80),
                    D("Del has kept this table since her rookie year. Some habits are armour.", 120, wide: true),
                }
            },
        };
    }
}
