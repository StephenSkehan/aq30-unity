// Assembly: AQ.SharedKernel
// File: Runtime/Economy/Events.cs
// Purpose: Lightweight event payloads (decoupled from any specific bus)

namespace AQ.SharedKernel.Economy
{
    public sealed class WalletChanged
    {
        public Currency Currency { get; }
        public int OldValue { get; }
        public int NewValue { get; }
        public string Reason { get; }

        public WalletChanged(Currency currency, int oldValue, int newValue, string reason)
        {
            Currency = currency;
            OldValue = oldValue;
            NewValue = newValue;
            Reason = reason ?? string.Empty;
        }

        public override string ToString() => $"{Currency} {OldValue}->{NewValue} reason='{Reason}'";
    }

    public sealed class RewardsGranted
    {
        public Reward[] Rewards { get; }
        public string Reason { get; }

        public RewardsGranted(Reward[] rewards, string reason)
        {
            Rewards = rewards ?? System.Array.Empty<Reward>();
            Reason = reason ?? string.Empty;
        }
    }
}
