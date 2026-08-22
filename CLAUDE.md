# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity **6000.0.25f1** (URP) city-builder prototype built on a **polar (ring/segment) grid** instead of a square grid. Hybrid architecture: MonoBehaviour + Zenject for input/grid/placement logic, Unity DOTS (Entities 1.3.2) for spawning, rendering and agent movement.

All first-party code lives in `Assets/_Scripts`. Everything under `Assets/Plugins`, `Assets/ThirdParties`, `Assets/Samples`, `Assets/ComputeShaderTutorials` is vendored third-party or scratch material — don't refactor it.

## Commands

There is no test suite, lint config, or CI in this repo. Development happens in the Unity Editor.

Headless compile check (only works when the Editor does **not** have the project open — it holds a lock on the project):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.25f1\Editor\Unity.exe" -quit -batchmode -nographics -projectPath . -logFile -
```

Note that three Editor versions are installed side by side; always pin `6000.0.25f1`. Generated `.csproj`/`.sln` files at the repo root are gitignored artifacts — never hand-edit them.

`Assets/Scenes/GameScene.unity` is the only scene in build settings. `Assets/Scenes/GameScene/EntitiesSubScene.unity` (baked entity prefabs and configs) and `Singletons.unity` are loaded as sub-scenes from it.

## Architecture

### Polar grid coordinate system

`PolarGridPosition` (`Assets/_Scripts/_Game/Grid/PolarGridPosition.cs`) is the core coordinate type: `ParentRingIndex`, `D` (depth ring-index within its ring), `Fi` (integer degrees), `H` (height). It is a struct with hand-written `==`/`Equals` — node lookup (`PolarGrid.GetPolarNode`) is a linear `FirstOrDefault` over all nodes comparing these four fields, so exact integer `Fi` values matter (see the `Fi >= 360 → 0` wrap in `PolarGrid.TryGetNodesForBuilding`).

The grid is built from `PolarGridRingsSettings` (a ScriptableObject list of `RingSettings { depth, fi, height, material }`). `PolarGridManager.CreateGrid()` → `PolarGrid.PopulateGrid()` walks rings outward, accumulating radius, generating a procedural round mesh per ring (`RoundMeshGenerator`) and instantiating one `PolarNode` MonoBehaviour per (depth, fi) cell.

**The grid runs clockwise while trigonometry runs counter-clockwise**, so every world↔polar conversion negates the angle / Z (`Mathf.Atan2(-worldPosition.z, worldPosition.x)`, `Mathf.Cos(-Fi * Deg2Rad)`). Keep that sign convention when touching conversions in `PolarGrid`.

Each `PolarNode` distinguishes `WorldPosition` (the node's corner/anchor, y = 0) from `CentrePosition` (the visual centre of the cell, shifted by half a column height and half a `fi`). Structures are placed at `CentrePosition`; the grid is queried by `WorldPosition`.

### Placement flow (MonoBehaviour side → DOTS side)

This is the main cross-cutting path; changes usually touch several links:

1. `StructureSelectionButton` fires `StructureSelectedSignal` (Zenject SignalBus).
2. `PlacementManager.OnStructureSelectedSignal` picks a handler + validator by `IStructureData.StructureType` (switch statements; the `TODO`s about factories are intentional/unfinished) and starts the handler's `_WaitForInput` **coroutine**, cancelling any previous one.
3. `StructurePlacementHandler` / `RoadPlacementHandler` poll `InputReader` and `MouseWorld` each frame, resolve the hovered `PolarNode`, ask `PolarGridManager.TryGetNodesForStructure` for the footprint (`StructureSizeType` → `(side, depth)` search range), run an `IPlacementValidator`, compute a `LocalTransform`, and fire `RequestStructurePlacementSignal`. Roads additionally run `Pathfinder.FindPath` between two clicked anchor nodes and emit one road segment per path step.
4. `StructureManager.OnRequestBuildingPlacementSignal` marks the nodes occupied (`PolarNode.SetBuilding`; roads deliberately skip this) and **appends to DynamicBuffers on a single manager entity** it creates in `Construct`: `StructurePlacementOrder`, `PeopleSpawnOrder`, `StructureWaypointBuffer`. This buffer append is the only bridge between the managed and ECS worlds.
5. ECS systems drain those buffers: `StructureSpawningSystem` (matches `StructureId` against the `AvailableStructure` buffer baked by `StructureRegister`, instantiates the prefab, clears the buffer), `PeopleSpawningSystem` (spawns inhabitants with random speed/home/work), `PathFindingSystem` (fills the `Waypoint` buffer, removes `PathfindingParams`), `PathFollowSystem` (walks the waypoint buffer **backwards** — `CurrentPathNodeIndex` counts down and `-1` means "no path").

`StructureRegister` is the baking link: an `int ID` on each `BaseStructureData` ScriptableObject is matched to a baked entity prefab. If a structure spawns nothing, the ID mismatch between the ScriptableObject and the register is the usual cause.

### Zenject wiring

`GameSceneInstaller` (MonoInstaller on the scene) installs `SignalBusInstaller`, then `GameInstaller` and `UIInstaller`.

Bindings resolve heavily **from `Resources/` string paths** (`Prefabs/Worlds/PolarGrids/PolarNodePrefab`, `Settings/PolarGridRingsSettings`, `Dictionaries/StructureDictionary`, …). Renaming or moving anything under `Assets/Resources` silently breaks DI at runtime with no compile error — update `GameInstaller`/`UIInstaller` alongside.

Conventions in use:
- Injection is via `[Inject] public void Construct(...)` methods, not constructors, on MonoBehaviours. `PolarGridManager.Construct` also builds the grid, so grid creation is DI-ordering-dependent.
- `PlaceholderFactory` + a paired `Custom*Factory : IFactory<...>` (see `PolarNodeFactory`, `RingFactory`) — the custom factory does `InstantiatePrefabForComponent` then calls `Initialise`.
- Signals are declared `.OptionalSubscriber()` and bound `.ToMethod<T>(x => x.Handler).FromResolveAll()`.

### DOTS conventions

Components are split by kind under `Assets/_Scripts/_Game/DOTS/Components/`: `Tags`, `ComponentData`, `Buffers`, `Configs`, `BlobAssets`. Authoring MonoBehaviours + nested `Baker<T>` classes live in `DOTS/Authoring/`.

Systems are `ISystem` structs gated with `state.RequireForUpdate<...>` in `OnCreate` and use `BeginInitializationEntityCommandBufferSystem.Singleton` for structural changes. Several `OnUpdate`s have `[BurstCompile]` deliberately commented out because they touch managed data — don't re-enable it without checking.

## Working in this codebase

- Namespaces mirror folders including the underscore prefixes (`_Scripts._Game.Managers`, `_Scripts._Game.DOTS.Systems.People`).
- Some comments are in Polish (e.g. the sign-convention note in `PolarGrid.GetPurePolarFromWorld`). Keep them; they explain non-obvious geometry.
- Large commented-out blocks are the author's parked alternatives (Zenject binding variants, an old `InputReader` ScriptableObject version, `FogSpawningSystem` which is entirely commented out). Leave them unless the task is about that code.
- Odin Inspector (`[Button]`, `[TableList]`, `[OnInspectorInit]`) and `InspectorButton` are used for in-Editor tooling such as `PolarGridManager.CreateGrid` and `StructureManager.TestPlaceBuildings`.
- `Pathfinder` holds two implementations: an A* used through `FindPathJob` (`FindPath(PolarNode, PolarNode)`, called by road placement) and an older inline `FindPath(int2, …)` that only logs. Only the first is live.
- `PolarNodeOld.cs` and `StructureFactory` are dead/parked code paths.
