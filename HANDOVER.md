# Project Handover: FactoryProject

> **For anyone picking this project up in the future.**
> This document covers everything you need to understand the current state, architecture, and intended direction of this project. Read it end-to-end before touching the code.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Getting Started](#2-getting-started)
3. [Architecture Overview](#3-architecture-overview)
4. [Data Model (ScriptableObjects)](#4-data-model-scriptableobjects)
5. [Initialization & Scene Setup](#5-initialization--scene-setup)
6. [The Grid System](#6-the-grid-system)
7. [The Build System](#7-the-build-system)
8. [Factory Logic & Item Flow](#8-factory-logic--item-flow)
9. [The Contract System](#9-the-contract-system)
10. [Economy](#10-economy)
11. [Audio System](#11-audio-system)
12. [Settings System](#12-settings-system)
13. [Save System](#13-save-system)
14. [Camera & Input](#14-camera--input)
15. [UI System](#15-ui-system)
16. [Coding Standards & Conventions](#16-coding-standards--conventions)
17. [AI Development Workflow (Vibe Unity)](#17-ai-development-workflow-vibe-unity)
18. [Controls Reference (Player)](#18-controls-reference-player)
19. [Known Bugs & Technical Debt](#19-known-bugs--technical-debt)
20. [Roadmap & Future Features](#20-roadmap--future-features)

---

## 1. Project Overview

**FactoryProject** is a top-down 3D factory automation game built in Unity. The core loop is:

1. **Build** resource sources (ore, wood, ingots).
2. **Connect** them with conveyors to processing machines (Constructors, Assemblers).
3. **Fulfill Contracts** by delivering finished goods to a Delivery Machine.
4. **Earn money** to fund more complex factories and unlock harder contracts.

The game is a **prototype**. Several systems (power, spline conveyors, load/save) are partially implemented and noted as such throughout this document.

| Property | Value |
|---|---|
| **Unity Version** | `6000.3.2f1` (Unity 6) |
| **Render Pipeline** | Universal Render Pipeline (URP) |
| **Input System** | Unity New Input System (`UnityEngine.InputSystem`) |
| **Primary Language** | C# |
| **Target Platform** | Windows (PC) |

> ⚠️ **Placeholder Art**: All 3D models and materials in the project are **temporary primitives and developer placeholders** (cubes, capsules, untextured materials). Before any public release or presentation, every machine, conveyor, item, and environment asset must be replaced with final, production-quality meshes and PBR materials. Machine prefabs are set up so swapping the mesh under the root `GameObject` is the only required change — the scripts are mesh-agnostic. Material slots in URP use the `Lit` shader; use that as your baseline for replacements.

---

## 2. Getting Started

1. **Open the project** in Unity `6000.3.2f1` or later. Opening in a different version may cause upgrade dialogs — accept them, but verify nothing broke in the console.
2. **Open the main game scene** (check under `Assets/Scenes/`) and ensure `GameBootstrap`, `BuildManager`, `GridManager`, `EconomyManager`, and `ContractManager` are assigned in the Inspector on the Bootstrap GameObject.
3. **Hit Play.** The game bootstraps itself automatically. Read the bootstrap behavior section if it misbehaves.
4. **After any C# edits**, run the compile-check script before committing (see [AI Workflow section](#17-ai-development-workflow-vibe-unity)).

---

## 3. Architecture Overview

The project follows a **Manager-centric singleton architecture**. A dedicated `MonoBehaviour` manager owns each domain. They communicate through **public methods** and **C# events (Actions)** rather than direct cross-referencing where possible.

```
GameBootstrap
│
├─ GridManager        ← "The map" (which tile is occupied?)
├─ BuildManager       ← "The builder" (place/rotate/delete)
├─ EconomyManager     ← "The bank" (spend/earn money)
├─ ContractManager    ← "The quest board" (active missions)
├─ AudioManager       ← "The sound" (DontDestroyOnLoad)
├─ SettingsManager    ← "The prefs" (DontDestroyOnLoad)
└─ SaveSystem         ← "The disk" (read/write JSON)

Factory Objects (per-instance, in the scene):
├─ FactoryBuilding (abstract)
│   ├─ Machine (abstract) ← Constructor, Assembler, Splitter, Merger...
│   ├─ ConveyorSegment
│   ├─ SourceMachine
│   ├─ StorageContainer
│   └─ DeliveryMachine
└─ ConveyorItem       ← The physical item traveling on a belt
```

**Singleton Pattern**: All managers use a standard Unity pattern:
```csharp
public static FooManager Instance { get; private set; }
private void Awake() {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
}
```
`AudioManager` and `SettingsManager` also call `DontDestroyOnLoad(gameObject)` so they persist across scene loads.

---

## 4. Data Model (ScriptableObjects)

Static game data lives in `ScriptableObject` assets. These are created via **Assets > Create > Factory** menus (or right-click in the Project window). You never need to code to add new items or machines — just fill out the asset.

### `ItemDefinition` (`Assets/Scripts/Core/ItemDefinition.cs`)
The atomic unit of the economy.
| Field | Purpose |
|---|---|
| `itemName` | Display name and the key used for contract matching |
| `icon` | `Sprite` shown in UI |
| `value` | Base sell price in `$` |

> ⚠️ **Item Name Matching**: The `ContractManager` normalizes item names (trims whitespace, replaces underscores with spaces) to compare deliveries against contract requirements. If you rename an `ItemDefinition`, check that `itemName` is updated and consistent with existing `ContractDefinition` asset references.

### `Recipe` (`Assets/Scripts/Core/Recipe.cs`)
Used by `Constructor` and `Assembler` to define a crafting operation.
| Field | Purpose |
|---|---|
| `inputs` | `List<ItemStack>` — what goes in |
| `outputs` | `List<ItemStack>` — what comes out |
| `processingTime` | Seconds per craft cycle |

### `MachineDefinition` (`Assets/Scripts/Core/MachineDefinition.cs`)
The blueprint for a placeable machine.
| Field | Purpose |
|---|---|
| `machineName` | Display name in shop |
| `type` | `MachineType` enum — used for hotkey lookup |
| `prefab` | The `GameObject` to instantiate in the world |
| `size` | `Vector2Int` grid footprint (e.g., `(1,1)`, `(2,2)`) |
| `cost` | Price in economy currency |
| `icon` | Shop button sprite |

### `ContractDefinition` (`Assets/Scripts/Contracts/ContractDefinition.cs`)
A "quest" definition offered to the player.
| Field | Purpose |
|---|---|
| `companyName` | Flavor text for the UI |
| `requiredItems` | `List<ItemStack>` of what must be delivered |
| `rewardMoney` | Base reward (scaled by player level at accept time) |
| `timeLimit` | Base time in seconds (also scaled by level) |
| `difficultyRating` | Controls at which level this contract becomes available |
| `unlocksMachines` | `List<MachineDefinition>` added to shop on completion |

> **Auto-Loading**: If the `contractPool` list on `ContractManager` is empty in the Inspector, it automatically loads all `ContractDefinition` assets found under `Resources/Factory/Contracts/`. Put new contracts there to have them picked up with zero code changes.

---

## 5. Initialization & Scene Setup

### `GameBootstrap.cs` (`Assets/Scripts/Core`)

`GameBootstrap.Start()` runs a **self-healing** setup. If any critical UI component is missing from the scene, it adds it to the Bootstrap `GameObject` at runtime:
- `ContractPopupUI`
- `FactoryHUD`
- `MaterialsRecipesUI`
- `SettingsUI`
- `InGameMenuManager`
- `EventSystem` (with `InputSystemUIInputModule` for New Input System, or `StandaloneInputModule` as fallback)

This means the game **will not crash** if a UI component is accidentally deleted from a scene — it regenerates itself. However, the injected components will be missing their Inspector-wired references (sprites, text fields, etc.), so they may look blank. **The authoritative source of truth is the Inspector-assigned components** on their scene GameObjects, not the runtime injections.

---

## 6. The Grid System

### `GridManager.cs` (`Assets/Scripts/Core`)

The grid is a logical (not visual) `Dictionary<Vector2Int, FactoryBuilding>`. It tracks occupancy only; there is no visual grid overlay in the current build.

| Method | What it does |
|---|---|
| `GetNearestPointOnGrid(Vector3)` | Snaps a world position to the nearest grid intersection |
| `WorldToGridPos(Vector3)` | Converts world XZ to grid coordinates |
| `IsAreaOccupied(pos, size)` | Checks if a rectangle of tiles is free |
| `RegisterBuilding(pos, size, building)` | Fills tiles with a building reference |
| `RemoveBuilding(pos, size)` | Clears tiles |
| `GetAllBuildings()` | Returns a `HashSet` of unique placed buildings |

**Important Detail**: Multi-tile buildings (e.g., a 2x2 Assembler) register on **every tile they cover**. `GetAllBuildings()` deduplicates with a `HashSet`, but be careful if you add a direct grid lookup — you'll get the same building for each of its tiles.

**Grid Parameters** (set in Inspector on the `GridManager` GameObject):
- `width` / `height`: 50 × 50 by default
- `cellSize`: `1f` (1 Unity unit per tile)

---

## 7. The Build System

### `BuildManager.cs` (`Assets/Scripts/Core`) — the most complex system in the project

#### State Machine
`BuildManager` operates in three modes:
```
BuildMode.None       ← default, camera only
BuildMode.Placement  ← ghost building follows cursor
BuildMode.Delete     ← hovering shows orange highlight, click to confirm
```

#### Ghost Building Flow
1. Player selects a machine (shop or hotkey) → `SetSelectedBuilding(definition)` is called.
2. The machine's `prefab` is instantiated as a ghost:
   - All `Collider` components are disabled.
   - All children are moved to the `Ignore Raycast` layer.
   - `EnsureGhostMaterials()` creates transparent Red/Green/Orange materials **at runtime** if none are set in the Inspector (URP `Lit` shader or `Standard` as fallback).
3. Every `Update()`, `UpdateGhostPosition()` raycasts from the camera to the `Terrain` layer.
4. The ghost snaps to the nearest grid point, with a center offset:
   ```csharp
   float offsetX = (Mathf.Floor(size.x * 0.5f) + 0.5f) * cellSize;
   ```
   This keeps the ghost centered under the cursor rather than bottom-left aligned.
5. `TrySnapToConnection()` then checks if any ghost `ConnectionPoint` is within `SnapRange = 1.5f` units of a placed building's complementary `ConnectionPoint`. If so, the ghost snaps to align perfectly.
6. Validity is checked every frame: not occupied + player can afford it → green ghost. Otherwise → red ghost.
7. Left-click while valid → `TryPlaceBuilding()`: deducts cost, instantiates the real prefab, calls `GridManager.RegisterBuilding` and `building.OnPlaced()`.

#### Delete Mode
- Toggled with `[X]` key.
- Raycasts against `Interactable` layer.
- Hovered building gets all renderers replaced with orange highlight material. Original materials are cached in `Dictionary<Renderer, Material[]>` and restored on un-hover.
- Left-click confirms deletion: refunds **50%** of building cost, removes from grid, calls `building.OnRemoved()`, and destroys the `GameObject`.

#### Hotkeys
```
[1] Conveyor      [2] Splitter     [3] Merger
[4] Constructor   [5] Assembler    [6] Storage
[8] Source        [9] Delivery     [R] Rotate ghost
[X] Delete mode   [Tab] Shop UI    [Esc] Cancel / Menu
```

---

## 8. Factory Logic & Item Flow

### Class Hierarchy

```
FactoryBuilding (abstract MonoBehaviour)
    ↓ inherits
Machine (abstract) — has Recipe, InputInventory, OutputInventory
    ↓ inherits
Constructor, Assembler, Splitter, Merger, StorageContainer, DeliveryMachine...

ConveyorSegment — derives from FactoryBuilding directly (not Machine)
SourceMachine — also derives from FactoryBuilding directly
```

### Interfaces

Defined in `IItemInterfaces.cs`:

| Interface | Purpose |
|---|---|
| `IItemReceiver` | Anything that can accept an item. Methods: `CanReceive(ItemDefinition)`, `ReceiveItem(ConveyorItem)` |
| `IItemProvider` | Anything that can offer an item. Methods: `HasItem()`, `PeekItem()`, `ExtractItem()` |
| `IFactoryConnection` | Exposes grid position and I/O directions (used for routing/connection logic) |

The `Direction` enum (`North, East, South, West`) maps to 0, 90, 180, 270 degree rotations respectively.

### `ConveyorItem.cs`
A thin `MonoBehaviour` wrapper that represents **a physical item in the world** — it's a game object with a visual mesh that slides along the conveyor. It holds a reference to an `ItemDefinition`.

When a machine **receives** an item from a conveyor, it calls `Destroy(item.gameObject)` to make it visually disappear, then increments its internal `InputInventory` count. The item is **not pooled** — it's destroyed and re-created by `ConveyorItem.Spawn()` when ejected.

### `ConveyorSegment.cs` — Belt Logic in Detail
Each conveyor is a 1×1, 1-unit-long segment with a `speed = 2f` (m/s).

**Per-frame `MoveItems()`:**
- Items are stored in `List<ConveyorItem> itemsOnBelt`, front item is index `[0]`.
- The front item moves forward at `speed * deltaTime`.
- Each trailing item can only advance to within `itemSpacing = 0.5f` units behind the item in front.
- This naturally creates **belt backups** — when the front is blocked, items queue behind it.

**`TryPassToNext()`:**
- When the front item is ≥ `length - 0.05f` (i.e., at the end of the belt), a `FindNextReceiver()` call fires.
- `FindNextReceiver()` uses `Physics.OverlapSphere` at the belt's exit point (radius 0.4f, layer `Interactable`) to find any `IItemReceiver`.
- If found and `CanReceive()` returns true, `ReceiveItem()` is called on the neighbour.

> ⚠️ **Performance Note**: Each `ConveyorSegment` calls `Physics.OverlapSphere` every frame once it has a queued item at its tip. In large factories (50+ belts), this creates hundreds of physics queries per frame. A future optimization is to cache the `nextReceiver` and only re-query it when either end changes (e.g., a machine is placed or removed nearby).

### `Machine.cs` — Processing Loop

Every frame in `Update()`:
1. If a `Recipe` is assigned and `CanProcess()` returns true → `StartProcessing()`.
   - `CanProcess()` checks: all recipe inputs are present + output buffer < 100 items.
   - `StartProcessing()` immediately deducts input items from `InputInventory`.
2. `processingProgress += Time.deltaTime`; when it reaches `CurrentRecipe.processingTime` → `FinishProcessing()`.
   - Adds output items to `OutputInventory`.
3. Every frame, `TryEjectOutput()` checks for an adjacent `IItemReceiver` at the machine's output `ConnectionPoint` and calls `ConveyorItem.Spawn()` + `ReceiveItem()` to pass the item out.

**Output Ports**: Machines eject through `ConnectionPoint` children with `PointType.Output`. If none exist, they fall back to `transform.forward`.

### `Splitter.cs` and `Merger.cs`

- **`Splitter`**: Takes 1 input, round-robins output across 2–3 outputs. Keeps an internal index to alternate which output gets the next item.
- **`Merger`**: Accepts items from multiple inputs, passes them to a single output. Priority is first-come-first-served.

### `StorageContainer.cs`

A simple buffer. It implements both `IItemReceiver` and `IItemProvider`. Its internal `List<ItemStack>` has a configurable max capacity. It does not process items.

---

## 9. The Contract System

### `ContractManager.cs` (`Assets/Scripts/Contracts`)

#### Contract Lifecycle
```
Pool of ContractDefinitions
   ↓ (timer fires, < 2 active contracts)
TryOfferRandomContract()  ← filtered by difficulty vs. CurrentLevel
   ↓
OnContractOffered event fired → ContractPopupUI shows popup
   ↓ (player accepts)
AcceptContract() ← quantities & rewards scaled by CurrentLevel
   ↓
Active contract timer counts down
   ↓
DeliveryMachine calls HandleItemDelivered() per item dropped in
   ↓
CheckCompletion() → if all items delivered → CompleteContract()
                  → if timer hits 0 → FailContract()
```

#### Scaling Formula
```csharp
CurrentLevel = (completedContractsCount / 5) + 1;
levelMultiplier = 1f + (CurrentLevel - 1) * 0.2f; // +20% rewards per level
timeMultiplier  = 1f + (CurrentLevel - 1) * 0.3f; // +30% time per level
```
Accept time is when scaling is locked in — a level 3 player accepting a difficulty-1 contract still gets level-3 quantities.

#### Item Name Normalization
This was a known bug that has been fixed. Item matching uses `GetNormalizedName()`:
```csharp
string raw = string.IsNullOrEmpty(item.itemName) ? item.name : item.itemName;
return raw.Replace("_", " ").Trim();
```
Delivery tracking dictionary keys are **normalized strings** (not object references), so items with slightly different asset names but the same `itemName` will still match correctly.

#### Machine Unlocking on Completion
When a contract completes, `ac.definition.unlocksMachines` is iterated and each entry is added to `BuildManager.Instance.machineCatalog` if not already present. The `ShopManager` should be refreshed after this to show the new button — this is currently manual/triggered via `OnContractCompleted` event.

#### Available Events
```csharp
ContractManager.Instance.OnContractUpdated   // Any state change
ContractManager.Instance.OnContractCompleted // A contract was won
ContractManager.Instance.OnContractFailed    // A contract's timer expired
ContractManager.Instance.OnContractOffered   // A new contract popup should show
```

---

## 10. Economy

### `EconomyManager.cs` (`Assets/Scripts/Economy`)

Dead simple. Tracks `CurrentMoney` as an `int`. Key methods:
- `CanAfford(int cost)` → `bool`
- `Spend(int amount)` → deducts (should check `CanAfford` first)
- `Earn(int amount)` → adds

The UI (HUD) listens to an event or polls `CurrentMoney` each frame to display the player's balance.

---

## 11. Audio System

### `AudioManager.cs` + `SoundLibrary.cs` (`Assets/Scripts/Core`)

`AudioManager` is a **DontDestroyOnLoad singleton**. It uses two `AudioSource` components (both added at runtime in `InitializeSources()`):
- `musicSource` — looping background music playlist
- `sfxSource` — one-shot SFX via `PlayOneShot()`

#### Music Playlist
- `SoundLibrary.menuMusic` plays on a loop when the `MainMenu` scene is loaded.
- `SoundLibrary.playlist` is a sequential list of tracks played for gameplay. When a track ends, `PlayNextMusicTrack()` automatically advances the index (wrapping around).

#### SFX Convenience Methods
All SFX calls go through `PlaySFX(AudioClip)`. Convenience wrappers:
```csharp
AudioManager.Instance.PlayClick();
AudioManager.Instance.PlayPlace();
AudioManager.Instance.PlayDelete();
AudioManager.Instance.PlayRotate();
AudioManager.Instance.PlayError();
AudioManager.Instance.PlayOpenUI();
AudioManager.Instance.PlayCloseUI();
AudioManager.Instance.PlayContractStarted();
AudioManager.Instance.PlayContractCompleted();
```

#### Adding a New Sound Effect
1. Add a new `AudioClip` field to `SoundLibrary.cs`.
2. Assign the clip in the Inspector on the `SoundLibrary` ScriptableObject asset.
3. Add a convenience method in `AudioManager.cs`:
   ```csharp
   public void PlayMyNewSound() => PlaySFX(library?.myNewSound);
   ```

#### Volume
Volumes are the product `masterVolume * musicVolume` (or `sfxVolume`). Settings are applied via `AudioManager.UpdateVolumes()`, which is called automatically when `SettingsManager.ApplySettings()` runs.

---

## 12. Settings System

### `SettingsManager.cs` (`Assets/Scripts/Core`)

A **lazy-initialized DontDestroyOnLoad singleton**. If no `SettingsManager` exists in the scene, accessing `SettingsManager.Instance` creates a new GameObject automatically. Settings are persisted using `PlayerPrefs` (key: `"GameSettingsJSON"`) via `JsonUtility`.

`GameSettings` fields:
| Category | Fields |
|---|---|
| Audio | `masterVolume`, `musicVolume`, `sfxVolume` |
| Graphics | `qualityLevel`, `shadowsEnabled` |
| Display | `resolutionWidth/Height`, `refreshRate`, `fullScreen`, `vsync` |
| Controls | `mouseSensitivity`, `invertY` |

Call `SettingsManager.Instance.SaveSettings()` to persist + apply. Call `ApplySettings()` to re-apply without saving (e.g., for live preview).

---

## 13. Save System

### `SaveSystem.cs` (`Assets/Scripts/Saving`)

Uses `File.WriteAllText` to `Application.persistentDataPath/factory_save.json`.

#### `FactorySaveData` structure
```
FactorySaveData
├─ currentMoney          : int
├─ activeContracts[]
│   ├─ companyName       : string (used to re-look-up definition)
│   ├─ timeRemaining     : float
│   └─ deliveredItems[]  : { itemId: string, amount: int }
└─ buildings[]
    ├─ gridPos           : Vector2Int
    ├─ facing            : Direction (enum)
    ├─ machineType       : string (type name)
    ├─ recipeId          : string
    └─ inputs/outputs[]  : { itemId: string, amount: int }
```

> ⚠️ **CRITICAL BUG**: The `SaveGame()` method collects money and contracts, but the `GridManager.GetAllBuildings()` loop is **stubbed out** — it never actually serializes factory buildings. The `LoadGame()` method parses the JSON but has no restoration logic (just a `Debug.Log`). **The save system is non-functional for factory layout.** This is the highest-priority feature gap.

---

## 14. Camera & Input

### `TopDownCameraController.cs` (root `Assets/Scripts/`)

Uses Unity's New Input System (`UnityEngine.InputSystem`).

| Control | Action |
|---|---|
| Mouse Wheel | Zoom in/out (clamped between `minZoom=2` and `maxZoom=20` on Y axis) |
| Middle Mouse Hold + Drag | Pan camera (XZ plane, camera-relative) |
| `[Q]` Hold | Rotate camera left around screen-center pivot |
| `[E]` Hold | Rotate camera right around screen-center pivot |

Both zoom sensitivity and pan respect `SettingsManager.settings.mouseSensitivity` and `invertY`.

**Y-Axis Zoom**: Zoom moves the camera along its local `forward` vector. The Y clamp (`minZoom` / `maxZoom`) acts as a proxy for zoom distance — works because the camera is angled down at ~45°.

---

## 15. UI System

All UI uses **Unity UI (UGUI)**. Scripts are in `Assets/Scripts/UI/`.

| Script | Responsibility |
|---|---|
| `MainMenuManager` | Main menu scene: Play, Settings, Quit buttons |
| `InGameMenuManager` | Pause menu: Resume, Save, Main Menu |
| `ShopManager` | Builds shop buttons from `BuildManager.machineCatalog` via `ShopButton` prefabs |
| `ShopButton` | Individual button prefab — holds a `MachineDefinition`, calls `BuildManager.SetSelectedBuilding()` on click |
| `FactoryHUD` | In-game overlay: displays money, active contract timers, etc. |
| `ContractPopupUI` | Modal popup shown when `ContractManager.OnContractOffered` fires |
| `MaterialsRecipesUI` | Codex panel showing all recipes and item values |
| `SettingsUI` | Wraps `SettingsManager` — drives sliders, dropdowns, and toggles |

### Opening/Closing UI
Most panels toggle via `SetActive(true/false)` driven by keyboard shortcuts captured in Managers. The `EventSystem` ensures clicks on UI elements do not pass through to the world (BuildManager checks `EventSystem.current.IsPointerOverGameObject()`).

---

## 16. Coding Standards & Conventions

### Namespaces
| Namespace | Contents |
|---|---|
| `Factory.Core` | Engine/infrastructure: Managers, ScriptableObjects, data types |
| `Factory.Factory` | Building logic, conveyor, machine base classes |
| `Factory.UI` | All MonoBehaviours attached to Canvas objects |
| `Factory.Economy` | EconomyManager |
| `Factory.Contracts` | Contract data + ContractManager |
| `Factory.Saving` | SaveSystem + save data DTOs |

*(Note: `TopDownCameraController` is in the global namespace — an inconsistency worth fixing.)*

### Naming
- `PascalCase` for public fields, properties, and methods.
- `camelCase` for private fields.
- Prefix private fields with `_` is **optional but preferred** for new code.
- Prefix booleans with `is`, `has`, `can`, `should` (e.g., `isProcessing`, `canAfford`).

### Unity Layers
| Layer Name | Purpose |
|---|---|
| `Terrain` | Ground plane; used by BuildManager for placement raycasts |
| `Interactable` | Placed buildings; used for click selection and conveyor handoff |
| `Ignore Raycast` | Ghost buildings during placement; prevents interference |

### Event Pattern
Public `System.Action` fields are used for decoupled communication:
```csharp
// Subscribing
ContractManager.Instance.OnContractCompleted += MyHandler;

// Always unsubscribe in OnDestroy to avoid memory leaks
private void OnDestroy() {
    if (ContractManager.Instance != null)
        ContractManager.Instance.OnContractCompleted -= MyHandler;
}
```

---

## 17. AI Development Workflow (Vibe Unity)

The project uses **Vibe Unity** — a tool that lets AI assistants (Claude) drive Unity scene creation and compilation checks.

### Key Files
| File/Dir | Purpose |
|---|---|
| `CLAUDE.md` | Instructions for AI agents working on this project |
| `claude-compile-check.sh` | Shell script to compile the project and return exit code 0 (ok) or 1 (errors) |
| `.vibe-unity/commands/` | Drop JSON command files here; Vibe Unity processes them automatically |
| `.vibe-unity/commands/logs/latest.log` | Check here after a JSON command for success/failure output |

### After Any C# Edit — Run This
```bash
./claude-compile-check.sh
# Exit 0 = clean compile, proceed
# Exit 1 = errors, fix before anything else
```

### JSON Command Example
```json
{
  "commands": [
    {"action": "create-scene", "name": "GameScene", "path": "Assets/Scenes"},
    {"action": "add-canvas", "name": "HUDCanvas"}
  ]
}
```

---

## 18. Controls Reference (Player)

| Key / Input | Action |
|---|---|
| `[Tab]` | Toggle Shop UI |
| `[P]` | Toggle Recipes/Materials UI |
| `[Esc]` | Menu / Cancel placement |
| `[B]` | Enter Build Mode |
| `[X]` | Toggle Delete Mode |
| `[R]` | Rotate ghost building |
| `[1]` | Select Conveyor |
| `[2]` | Select Splitter |
| `[3]` | Select Merger |
| `[4]` | Select Constructor |
| `[5]` | Select Assembler |
| `[6]` | Select Storage |
| `[8]` | Select Source |
| `[9]` | Select Delivery Machine |
| `Middle Mouse + Drag` | Pan camera |
| `Scroll Wheel` | Zoom |
| `[Q]` / `[E]` | Rotate camera |
| `Left Click` | Place building / Confirm delete |
| `Right Click` | Cancel placement |

---

## 19. Known Bugs & Technical Debt

| Severity | Description |
|---|---|
| 🔴 Critical | **Save System is non-functional.** `SaveGame()` never serializes placed buildings. `LoadGame()` parses JSON but does not restore any state. |
| 🔴 Critical | **Building inventory not restored on load.** Even once building serialization is added, machine input/output inventories need to be re-created at load time. |
| 🔴 Critical | **Contract popup persists after returning to Main Menu.** `ContractPopupUI` and the `ContractManager` both live on `DontDestroyOnLoad`-adjacent objects and are not torn down when the player exits to the Main Menu scene. Active contract timers keep ticking, and the popup can appear over the Main Menu UI. Fix: subscribe to `SceneManager.sceneLoaded` in `ContractManager` and `ContractPopupUI`, detect the `MainMenu` scene, reset all active contracts, and hide/destroy the popup canvas. |
| 🟠 High | **No object pooling for `ConveyorItem`.** Items are `Destroy()`'d and re-`Instantiate()`'d constantly. In factories with 20+ belts, this creates per-frame GC pressure. |
| 🟠 High | **`ConveyorSegment` nulls `nextReceiver` after every handoff**, causing an `OverlapSphere` query every frame when an item is at the belt tip. Should cache and only invalidate on world changes. |
| 🟡 Medium | `TopDownCameraController` is in the global namespace (no `namespace Factory.*` wrapping). |
| 🟡 Medium | `BuildManager.ConfirmDeleteBuilding()` reconstructs the rotated footprint size manually. If building rotation tracking is ever changed, this logic breaks silently. |
| 🟡 Medium | `GameBootstrap` injects UI components at runtime without Inspector references — injected instances will render blank (no sprites, no references to panels). |
| 🟢 Low | Several commented-out debug logs left in `ContractManager.Update()`. |
| 🟢 Low | `FactoryBuilding.OnRemoved()` is a no-op — machines don't clean up items on their belts when deleted. ConveyorItems orphan in the scene. |

---

## 20. Roadmap & Future Features

These were discussed or prototyped but not completed:

- **Functional Save/Load**: Complete `SaveGame()` building serialization and write `LoadGame()` scene reconstruction (instantiate prefabs, restore inventories, restore contracts).
- **Spline Conveyors**: Replace rigid 1×1 segment chains with drawable spline belts for more natural factory layouts.
- **Power System**: `PowerPole.cs` exists as a stub. Planned: machines require power, poles connect in radius, global blackouts when demand > supply.
- **Item Sprites on Belts**: `ConveyorItem` currently uses a primitive mesh. Should swap to `ItemDefinition.icon` displayed as a billboard sprite.
- **Undo/Redo**: `BuildManager` has no undo stack. Common in factory games — implementing a Command pattern here would be high-value.
- **Camera Edge-Scrolling**: Mouse-to-screen-edge should pan the camera (similar to RTS games).
- **Machine Inspector UI**: Clicking a placed machine (via `SelectionManager`) should open a panel showing its current recipe, input/output buffers, and allow recipe selection.
- **Unlock System**: Contracts that `unlocksMachines` currently add to the live `machineCatalog` list but the ShopManager UI is not automatically refreshed. Wire `OnContractCompleted` → `ShopManager.Refresh()`.
- **CO₂ Consumption System**: Each `ItemDefinition` should gain a `co2PerUnit` field representing the carbon cost to produce one unit (factoring in machine energy, transport, and raw material extraction). Each `ContractDefinition` should gain a `co2Limit` field. When a contract is fulfilled, the total CO₂ consumed by all items in the delivery chain is calculated and compared against the limit. If the player's production **exceeds the CO₂ budget**, a percentage penalty is applied to the contract reward — the larger the overage, the steeper the cut (suggested curve: `-2% reward per 1% CO₂ overage`, capped at -80%). A new HUD widget should display the live CO₂ footprint of the current factory output rate. This mechanic adds a strategic layer: players must choose between fast/cheap machines and eco-friendly alternatives.
- **VR Support**: If a VR build is ever required, the recommended approach for Unity 6 + URP is the **XR Interaction Toolkit** (`com.unity.xr.interaction.toolkit`). Key changes needed: replace `TopDownCameraController` with an XR Rig; replace mouse-based `BuildManager` raycasts with controller ray interactors; convert the UGUI shop and HUD panels to **World Space Canvases** positioned in 3D; and verify that `Physics.OverlapSphere` calls in `ConveyorSegment` and `Machine` remain frame-rate independent under the fixed VR render loop. The singleton/manager architecture requires no changes for VR — only the input and camera layers need replacement.

---

*Document last updated: April 27, 2026. Author: Maxime Hombreux-Wang.*
