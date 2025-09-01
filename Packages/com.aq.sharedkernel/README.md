# com.aq.sharedkernel

This package is pure C#. **No UnityEngine / UnityEditor references.**
Only deterministic RNG and mockable time are permitted.

Guidelines:
- Keep domain logic framework-agnostic (no Unity types, no MonoBehaviours).
- Expose interfaces for RNG and time; implement adapters in Unity layers.
- Write unit tests without Unity runners.

