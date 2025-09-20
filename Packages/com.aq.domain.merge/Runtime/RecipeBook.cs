using System;
using System.Collections.Generic;

namespace AQ.Domain.Merge
{
    // Concrete recipe lookup with symmetry: (a,b) == (b,a).
    public sealed class RecipeBook : IRecipeBook
    {
        private readonly Dictionary<string, ItemId> _map = new Dictionary<string, ItemId>();

        public void AddRecipe(ItemId a, ItemId b, ItemId result)
        {
            _map[MakeKey(a, b)] = result;
        }

        public bool TryGetResult(ItemId a, ItemId b, out ItemId result)
        {
            return _map.TryGetValue(MakeKey(a, b), out result);
        }

        private static string MakeKey(ItemId a, ItemId b)
        {
            var av = a.Value ?? string.Empty;
            var bv = b.Value ?? string.Empty;

            // Order the pair so (a,b) and (b,a) collide.
            if (string.Compare(av, bv, StringComparison.Ordinal) <= 0)
                return av + "|" + bv;
            return bv + "|" + av;
        }
    }
}
