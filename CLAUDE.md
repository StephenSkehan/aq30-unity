# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Ally Quinn: True Crime Merge (AQ30)** is a Unity 6000.3.7f1 mobile game for iOS/Android. It combines merge-grid puzzle gameplay with a true crime detective narrative, framed as a podcast hosted by protagonist Ally Quinn. The project is a 30-day vertical slice targeting a single playable case ("The Ghost Student") as a market-ready demo.

Solo developer project. Pragmatic hybrid architecture: domain-driven design principles where they add value, Unity-idiomatic shortcuts where they don't.

## Running Tests

Tests run inside the Unity Editor via **Window > General > Test Runner**.

- **Edit Mode tests** (`Assets/Tests/EditMode/`) — NUnit, no scene loading, fast
- **Play Mode tests** (`Assets/Tests/PlayMode/`, `Assets/Tests/PlayModeNew/`) — full scene boot, slower

There is no CLI build or test script. All test execution goes through the Unity Editor or Unity's `-runTests` batch mode flag.

## Architecture

### Assembly / Module Boundaries

| Assembly | Location | Rule |
|---|---|---|
| `AQ.SharedKernel` | `Packages/com.aq.sharedkernel/` | **Zero Unity dependencies.** Pure C# domain primitives, interfaces, RNG, time. No `UnityEngine`/`UnityEditor` references allowed. |
| `AQ.App` | `Assets/App/` | Unity application layer. Bridges SharedKernel interfaces to MonoBehaviours, scene wiring, UI. |
| `AQ.Domain.Board` | `Assets/Scripts/Domain/Board/` | Pure merge-rules logic. No Unity deps. |
| `AQ.Editor` | Editor-only | In-editor tools and audit scripts. |

**Important:** A domain-layer merge system was previously built and archived — it is intentionally unused. The active merge implementation lives in the app layer (`MergeService` MonoBehaviour). Do not attempt to revive or integrate the archived domain merge system.

### Event Bus

All cross-system communication goes through `GlobalBus.Bus` (`Assets/App/Presentation/GlobalBus.cs`), which holds an `IEventBus` (defined in SharedKernel). Events implement the `IGameEvent` marker interface and are defined in `Assets/App/Events/GameEvents.cs`.

Publish: `GlobalBus.Bus.Publish(new SomeEvent(...));`
Subscribe: `GlobalBus.Bus.Subscribe<SomeEvent>(handler);`

### Service Locator Pattern

Runtime service access uses static locators — not DI containers:

- `CaseFlowLocator.Instance` → `ICaseFlowService`
- `WalletLocator.Instance` → `IWallet`
- `AnalyticsLocator.Instance` → `IAnalytics`

### Bootstrapping

`BootstrapperAutoAssign` (`Assets/App/Composition/`) runs via `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`. It creates a `DontDestroyOnLoad` `MergeService` if one doesn't exist and auto-wires it into all `MergeInputAdapter` components in the scene.

`CaseFlowOrchestratorMB` owns the episode lifecycle: calls `ICaseFlowService.Begin(episodeId, steps[])` on Start, then advances the step index as FTUE and game milestones complete.

### Merge System

- **`MergeRules`** (`Assets/Scripts/Domain/Board/MergeRules.cs`) — pure static class; takes two `Tile` value objects, returns `Outcome` (Move / Swap / Merge / CeilingSwap). Family-aware: tiles with known but differing families always Swap.
- **`MergeService`** (MonoBehaviour) — bridges `MergeRules` to Unity scene objects.
- **`MergeBoardView` / `MergeBoardController`** — MVC split for the board UI. `MergeInputAdapter` receives drag/drop input and delegates to `MergeService`.

**Known drag/drop behaviour:** The board uses Unity event handling for drag-and-drop. `SetAllDirty()` calls are required for UI refresh after board state changes. Drag/click alternation failures were a previously resolved bug — be cautious when modifying input handling code.

### Dialogue System

The dialogue system is fully functional with animated character portraits. Key details:

- `DialoguePanel` prefab — **must not have a Canvas component directly on it**; this was a previously resolved rendering bug. The Canvas lives on a parent object.
- Ally Quinn has 7 emotion states driving portrait animation: `neutral`, `happy`, `sad`, `angry`, `surprised`, `worried`, `confused`.
- Portrait animation uses sprite-pair animation (idle + expression frames).
- Dialogue is driven by data; do not hardcode dialogue strings in MonoBehaviours.

### Leads / Case Flow

- **`LeadsDatabase`** (ScriptableObject) holds the master list of `LeadData`.
- **`LeadsRepository`** (MonoBehaviour) is the runtime store; it populates from the database on Start and broadcasts changes via `LeadsRuntimeBus` and a C# `LeadsChanged` event.
- **`CaseFlowOrchestratorMB`** drives the episode step machine; it subscribes to `IWallet.Granted` and auto-advances when an FTUE reward is granted.

### SharedKernel Constraints

- No `UnityEngine` or `UnityEditor` references — ever.
- Expose interfaces for RNG (`IDeterministicRandom`) and time; implement Unity adapters in `AQ.App`.
- SharedKernel types must be testable with plain NUnit (no Unity test runner required).

## Known Technical Debt

- The FTUE flow is partially implemented; some step transitions are stubbed.
- Save/Load system exists but has not been stress-tested against mid-session crashes.
- Analytics layer is wired but events are not fully mapped to all user actions.
- Some editor tooling scripts were removed in a codebase cleanup (~46% reduction in C# script count). Backups exist in OneDrive if needed.

## Git Hygiene

This repo had a serious LFS corruption event in its history (2.65 GiB bloat). Rules to follow:
- Never commit Unity Library/, Temp/, or build output folders.
- Large binary assets (textures, audio) must go through Git LFS.
- Keep commits focused — one logical change per commit.
- Do not force-push to main without explicit instruction.

## Narrative Context (for dialogue / content work)

- **Setting:** Fictional coastal city of Havenbay.
- **Protagonist:** Ally Quinn — true crime podcaster and amateur detective.
- **Case 1:** "The Ghost Student" — the planned vertical slice case.
- **Framing device:** The game is presented as Ally's podcast episodes.
- Tone: noir-inspired, grounded, character-driven. Not campy.