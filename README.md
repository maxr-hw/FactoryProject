# FactoryProject

A top-down 3D factory automation game built in **Unity 6** (URP).

> For a full technical deep-dive, read [`HANDOVER.md`](./HANDOVER.md).

---

## Overview

Players build and connect automated factories to fulfill delivery contracts. Chain together resource sources, conveyors, and processing machines to produce goods, earn money, and unlock new technologies.

**Core Loop:**
1. 🏭 **Build** — place machines and conveyors on a grid
2. ⚙️ **Process** — raw materials flow through constructors and assemblers
3. 📦 **Deliver** — send finished goods to the Delivery Machine
4. 💰 **Earn** — collect contract rewards and expand your factory

---

## Getting Started

### Requirements
- **Unity** `6000.3.2f1` (Unity 6)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Platform**: Windows PC (primary target)

### Running the Project
1. Clone or download the repository.
2. Open the project in Unity `6000.3.2f1`.
3. Open the main game scene from `Assets/Scenes/`.
4. Ensure the `GameBootstrap` GameObject in the scene has `BuildManager`, `GridManager`, `EconomyManager`, and `ContractManager` assigned in its Inspector.
5. Press **Play**.

> The game uses a self-healing bootstrap — if UI components are missing from the scene, they are created automatically at runtime.

---

## Controls

| Key / Input | Action |
|---|---|
| `[Tab]` | Toggle Shop |
| `[P]` | Toggle Recipe Codex |
| `[Esc]` | Pause Menu / Cancel |
| `[1]–[6], [8], [9]` | Select machine by hotkey |
| `[R]` | Rotate ghost building |
| `[X]` | Toggle Delete Mode |
| `Left Click` | Place building / confirm |
| `Right Click` | Cancel placement |
| `Middle Mouse + Drag` | Pan camera |
| `Scroll Wheel` | Zoom |
| `[Q]` / `[E]` | Rotate camera |

---

## Project Structure

```
Assets/
├── Scenes/             # Unity scenes (Main Menu, Game)
├── Scripts/
│   ├── Core/           # Managers, ScriptableObjects, settings, audio
│   ├── Factory/        # Buildings, conveyors, machines, item logic
│   ├── Contracts/      # Contract system and definitions
│   ├── Economy/        # Currency management
│   ├── Saving/         # Save/load system
│   └── UI/             # All canvas and HUD scripts
├── Prefabs/            # Machine and conveyor prefabs
└── Resources/
    └── Factory/
        └── Contracts/  # ContractDefinition assets (auto-loaded)
```

---

## Key Systems

| System | Entry Point | Notes |
|---|---|---|
| Building Placement | `BuildManager.cs` | Ghost preview, snapping, grid validation |
| Grid | `GridManager.cs` | Logical 50×50 tile map |
| Item Transport | `ConveyorSegment.cs` | Physics-free belt simulation |
| Processing | `Machine.cs` | Recipe-driven input→output loop |
| Contracts | `ContractManager.cs` | Timed delivery quests with scaling rewards |
| Economy | `EconomyManager.cs` | Currency earn/spend |
| Audio | `AudioManager.cs` | Persistent music playlist + SFX |
| Settings | `SettingsManager.cs` | Persisted via `PlayerPrefs` |
| Save/Load | `SaveSystem.cs` | JSON to `persistentDataPath` ⚠️ WIP |

---

## Adding Content

### New Item
1. Right-click in Project → **Create > Factory > Item Definition**
2. Fill in `itemName`, `icon`, and `value`

### New Recipe
1. Right-click → **Create > Factory > Recipe**
2. Set `inputs`, `outputs`, and `processingTime`
3. Assign the recipe to a Constructor or Assembler in the Inspector

### New Machine
1. Right-click → **Create > Factory > Machine Definition**
2. Assign the prefab, footprint size, cost, and icon
3. Add the definition to `BuildManager.machineCatalog` in the Inspector

### New Contract
1. Right-click → **Create > Factory > Contract Definition**
2. Fill in the company name, required items, reward, time limit, and difficulty
3. Place the asset in `Resources/Factory/Contracts/` — it will be auto-loaded

---

## Known Issues

| Severity | Issue |
|---|---|
| 🔴 Critical | Save/Load does not restore factory buildings (WIP stub) |
| 🔴 Critical | Contract popup can appear over the Main Menu after returning from a game session |
| 🟠 High | No object pooling for conveyor items — GC pressure in large factories |
| 🟡 Medium | All 3D models and materials are **placeholder primitives** — not final art |

See [`HANDOVER.md § Known Bugs`](./HANDOVER.md#19-known-bugs--technical-debt) for the full list.

---

## Roadmap Highlights

- ✅ Grid-based building placement with snapping
- ✅ Conveyor belt item transport
- ✅ Contract system with difficulty scaling
- ✅ Audio system with playlist and SFX
- 🔲 Functional save/load
- 🔲 Final 3D art & materials
- 🔲 Power system
- 🔲 Spline conveyors
- 🔲 CO₂ consumption mechanic
- 🔲 VR support (XR Interaction Toolkit)

---

## Authors

- **Maxime Hombreux-Wang**
- **Baptiste Jusseaume**

---

*Unity 6 · Universal Render Pipeline · C#*
