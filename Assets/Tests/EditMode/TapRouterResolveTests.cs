using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using AQ.App.UI;

namespace AQ.Tests.EditMode
{
    /// <summary>
    /// TapRouter routing contract: topmost enabled region containing the tap
    /// wins, exactly one consumer per tap, and an EventSystem surface strictly
    /// above the winner blocks delivery. Resolve() is pure, so the contract is
    /// testable without a scene or an EventSystem.
    /// </summary>
    public sealed class TapRouterResolveTests
    {
        private static readonly Vector2 P = new Vector2(100f, 100f);
        private const int NoRaycastHit = int.MinValue;

        private static TapRouter.Region Region(string name, int layer,
            bool contains = true, bool enabled = true, System.Action<Vector2> onTap = null)
            => new TapRouter.Region
            {
                Name     = name,
                Layer    = layer,
                Contains = _ => contains,
                OnTap    = onTap ?? (_ => { }),
                Enabled  = () => enabled,
            };

        [Test]
        public void TopmostLayer_Wins_AmongOverlappingRegions()
        {
            var low  = Region("low", 5);
            var high = Region("high", 200);
            var mid  = Region("mid", 100);

            var winner = TapRouter.Resolve(P, new List<TapRouter.Region> { low, high, mid }, NoRaycastHit);
            Assert.AreSame(high, winner, "one tap, one consumer: the topmost region and only it");
        }

        [Test]
        public void DisabledRegion_IsSkipped_LowerEnabledWins()
        {
            var low          = Region("low", 5);
            var highDisabled = Region("high-disabled", 200, enabled: false);

            var winner = TapRouter.Resolve(P, new List<TapRouter.Region> { highDisabled, low }, NoRaycastHit);
            Assert.AreSame(low, winner);
        }

        [Test]
        public void RegionNotContainingPoint_IsSkipped()
        {
            var elsewhere = Region("elsewhere", 200, contains: false);
            var under     = Region("under", 5);

            var winner = TapRouter.Resolve(P, new List<TapRouter.Region> { elsewhere, under }, NoRaycastHit);
            Assert.AreSame(under, winner);
        }

        [Test]
        public void EventSystemSurface_StrictlyAbove_BlocksDelivery()
        {
            var stash = Region("stash", 200);

            Assert.IsNull(TapRouter.Resolve(P, new List<TapRouter.Region> { stash }, topmostRaycastOrder: 400),
                "a locker dim / popup scrim above the region owns the tap — this exact hole popped stash tiles behind modals");
        }

        [Test]
        public void EventSystemSurface_AtEqualOrder_DoesNotBlock()
        {
            // The region's own raycastable graphics live on its canvas: equal
            // order must not block, or every region would block itself.
            var stash = Region("stash", 200);
            Assert.AreSame(stash, TapRouter.Resolve(P, new List<TapRouter.Region> { stash }, topmostRaycastOrder: 200));
        }

        [Test]
        public void NoEligibleRegion_ReturnsNull()
        {
            Assert.IsNull(TapRouter.Resolve(P, new List<TapRouter.Region>(), NoRaycastHit));
            Assert.IsNull(TapRouter.Resolve(P,
                new List<TapRouter.Region> { Region("off", 10, enabled: false) }, NoRaycastHit));
        }

        [Test]
        public void LayerTie_FirstRegisteredWins_Deterministically()
        {
            var first  = Region("first", 200);
            var second = Region("second", 200);

            var winner = TapRouter.Resolve(P, new List<TapRouter.Region> { first, second }, NoRaycastHit);
            Assert.AreSame(first, winner);
        }

        [Test]
        public void ThrowingRegion_IsSkipped_NotFatal()
        {
            var broken = new TapRouter.Region
            {
                Name     = "broken",
                Layer    = 500,
                Contains = _ => throw new System.InvalidOperationException("destroyed closure"),
                OnTap    = _ => { },
            };
            var healthy = Region("healthy", 5);

            var winner = TapRouter.Resolve(P, new List<TapRouter.Region> { broken, healthy }, NoRaycastHit);
            Assert.AreSame(healthy, winner, "one broken region must not take all raw input down with it");
        }
    }
}
