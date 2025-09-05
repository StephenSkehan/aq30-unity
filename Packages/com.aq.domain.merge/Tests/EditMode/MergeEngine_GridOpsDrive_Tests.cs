using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergeEngine_GridOpsDrive_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Mix_of_valid_invalid_and_cooldown_paths_drives_get_set()
        {
            var grid = new Grid();
            var bus = new EventBusSpy();
            var recipes = new RecipeBookTable();
            var time = new FixedTime();
            var rng = new FixedRandom(0.5);

            var twig = Id("twig");
            var branch = Id("branch");
            recipes.Add(twig, twig, branch);

            // Prime a few cells
            grid.Set(0, twig);
            grid.Set(1, twig);
            grid.Set(2, Id("rock"));

            var engine = new MergeEngine(grid, recipes, bus, rng, time);

            // Valid merge 0->1
            Assert.IsTrue(engine.TryMerge(0,1).IsSuccess);

            // Invalid: dissimilar
            grid.Set(0, Id("leaf"));
            Assert.IsFalse(engine.TryMerge(0,1).IsSuccess);

            // Cooldown: try branch->2 immediately (different anyway)
            grid.Set(0, branch);
            Assert.IsFalse(engine.TryMerge(0,2).IsSuccess);

            // After cooldown, place twig->1 again (prepare) and merge to branch
            time.AdvanceSeconds(2);
            grid.Set(0, twig);
            grid.Set(1, twig);
            Assert.IsTrue(engine.TryMerge(0,1).IsSuccess);

            Assert.GreaterOrEqual(bus.Published.Count, 2);
        }
    }
}
