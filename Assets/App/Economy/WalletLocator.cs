// Assembly: AQ.App
// File: Assets/App/Economy/WalletLocator.cs
// Purpose: App-level locator to provide a wallet instance to MonoBehaviours.

using AQ.SharedKernel.Economy;

namespace AQ.App.Economy
{
    public static class WalletLocator
    {
        public static IWallet Instance { get; private set; }

        public static void Set(IWallet wallet) => Instance = wallet;
    }
}
