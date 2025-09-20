// Assembly: AQ.SharedKernel
// File: Runtime/Economy/Reward.cs
// Purpose: Reward primitives (no Unity refs)

using System;

namespace AQ.SharedKernel.Economy
{
    public readonly struct Reward
    {
        public Currency Currency { get; }
        public int Amount { get; }

        public Reward(Currency currency, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Reward amount must be >= 0");
            Currency = currency;
            Amount = amount;
        }

        public static Reward Soft(int amount)    => new Reward(Currency.Soft, amount);
        public static Reward Premium(int amount) => new Reward(Currency.Premium, amount);
        public static Reward Energy(int amount)  => new Reward(Currency.Energy, amount);
    }

    /// <summary>Named container of rewards (useful for FTUE or IAP packs).</summary>
    public sealed class RewardBundle
    {
        public string Id { get; }
        public Reward[] Rewards { get; }

        public RewardBundle(string id, params Reward[] rewards)
        {
            Id = string.IsNullOrWhiteSpace(id) ? "unnamed" : id;
            Rewards = rewards ?? Array.Empty<Reward>();
        }

        public override string ToString() => $"{Id} x{Rewards.Length}";
    }
}
