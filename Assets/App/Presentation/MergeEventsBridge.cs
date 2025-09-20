using System;
using UnityEngine;
using AQ.App.Presentation;
using AQ.Domain.Merge;

namespace AQ.App.Presentation
{
  /// Bridges domain MergePerformed into the presentation layer (stub: log only).
  public sealed class MergeEventsBridge : MonoBehaviour
  {
    IDisposable _sub;

    void OnEnable()
    {
      var bus = GlobalBus.Bus;
      // Typical IEventBus shape: Subscribe<T>(Action<T>) -> IDisposable
      try { _sub = bus.Subscribe<MergePerformed>(OnMergePerformed); }
      catch { /* if API differs, ignore for now */ }
    }

    void OnDisable()
    {
      try { _sub?.Dispose(); } catch {}
      _sub = null;
    }

    void OnMergePerformed(MergePerformed e)
    {
      Debug.Log($"[MergeEventsBridge] MergePerformed {e.A}+{e.B}→{e.Result}");
      // TODO: forward to UI systems as needed
    }
  }
}
