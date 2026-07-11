// Assets/Scripts/App/Services/EnergyRuntime.cs
using System;
using UnityEngine;
using AQ.App.Config;

namespace AQ.App.Services
{
    /// <summary>
    /// Lightweight static holder for energy config/runtime objects.
    /// </summary>
    public static class EnergyRuntime
    {
        /// <summary>Assigned by ConfigInstaller (or scene) at startup.</summary>
        public static EnergyConfig Config { get; set; }

        /// <summary>Created by ConfigInstaller / SaveSystem when EnergySystem is enabled.</summary>
        public static EnergyManager Manager { get; set; }
    }
}
