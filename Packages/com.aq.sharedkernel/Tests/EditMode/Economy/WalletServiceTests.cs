// Assembly: com.aq.sharedkernel.tests (EditMode)
// File: Tests/EditMode/Economy/WalletServiceTests.cs

using NUnit.Framework;
using AQ.SharedKernel.Economy;
using System.Collections.Generic;

namespace AQ.SharedKernel.Tests.Economy
{
    public class WalletServiceTests
    {
        [Test]
        public void Grant_IncreasesBalance_AndRaisesEvents()
        {
            var wallet = new WalletService();
            var changed = new List<WalletChanged>();
            var granted = new List<RewardsGranted>();
            wallet.Changed += changed.Add;
            wallet.Granted += granted.Add;

            wallet.Grant("ftue", Reward.Soft(500), Reward.Energy(10));

            Assert.AreEqual(500, wallet.Get(Currency.Soft));
            Assert.AreEqual(10,  wallet.Get(Currency.Energy));
            Assert.AreEqual(2, changed.Count);
            Assert.AreEqual(1, granted.Count);
            Assert.AreEqual("ftue", granted[0].Reason);
        }

        [Test]
        public void TrySpend_Succeeds_WhenSufficient()
        {
            var wallet = new WalletService();
            wallet.Grant(Reward.Soft(100));
            var ok = wallet.TrySpend(Currency.Soft, 60, "test");
            Assert.IsTrue(ok);
            Assert.AreEqual(40, wallet.Get(Currency.Soft));
        }

        [Test]
        public void TrySpend_Fails_WhenInsufficient()
        {
            var wallet = new WalletService();
            wallet.Grant(Reward.Soft(20));
            var ok = wallet.TrySpend(Currency.Soft, 25, "test");
            Assert.IsFalse(ok);
            Assert.AreEqual(20, wallet.Get(Currency.Soft));
        }
    }
}
