# Part 2 — City-builder architecture: bootstrap, specs, and composition

A second, independent learning path. **Part 1** (`csharp-learning-path.md`) is about the *language*;
this is about *how a city builder is put together*.

Structured as **Lesson 1 standalone plus three tracks**, front-loading the two things that prompted
it: a single entry point that builds everything through DI, and buildings defined as data specs rather
than subclasses.

**Start at Lesson 1 (game time).** It's unblocked by everything else, it's already half-written in your
working tree, and `WorkShiftSystem` is waiting on it.

## References, and which one is right for what

| Reference | Good for | Caveat |
|---|---|---|
| [**Timberborn**](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture) (Unity, C#) | Blueprint/spec JSON format; DI contexts; `MultiBind`; fragment UI | Only the *modding surface* is documented, not internals. Not DOTS. |
| [**RimWorld**](https://rimworldwiki.com/wiki/Modding_Tutorials/Def_classes) (Unity, C#) | Name-keyed definition resolution (`DefDatabase<T>.GetNamed`); why composition beats def inheritance | XML not JSON; older codebase; no DOTS |
| **Composition-root pattern** (Seemann, *DI Principles, Practices & Patterns*) | The entry-point idea in its general form | Not game-specific |

### Verdict on the Timberborn choice: half right, and worth keeping

You said what you like is (a) the entry point everything builds from, and (b) buildings as JSON specs.
Those two halves deserve different answers:

**(b) Specs as JSON — Timberborn is an excellent reference.** Its blueprints are literally what you
described. A building is a JSON object composed of named specs:

```json
{ "BuildingSpec": { }, "BuildingModelSpec": { }, "WorkplaceSpec": { } }
```

That shape alone solves a real Polar bug: *presence of `WorkplaceSpec` makes something a workplace*,
which is precisely what `if (structureData is WorkStructureData)` (`StructureManager.cs:120`) is
faking with a type check.

**Pair it with RimWorld.** RimWorld's `Def` system is the same idea in the same engine and language,
and its docs state the principle Timberborn's don't: name-based resolution exists to allow
"cross-referencing without hardcoded object references." That is *exactly* Polar's worst coupling —
the hand-assigned `int ID` matched between a ScriptableObject and the baked `StructureRegister`, which
CLAUDE.md already records as the usual cause of a structure spawning nothing. RimWorld also explicitly
recommends `CompProperties`/`ThingComp` composition *over* extending Def classes, to avoid the
"inheritance nightmare" — which is a description of your `BaseStructureData` tree.

**(a) The entry point — Timberborn is the weakest justification.** Bindito is Zenject-based, and you
already use Zenject: `GameSceneInstaller` → `GameInstaller` + `UIInstaller` *is* a composition root.
You're not missing the idea; you're missing three refinements (scene contexts, multi-binding, and an
ordered load phase). So Track A teaches the general pattern and uses Timberborn as one instance of it,
rather than treating Timberborn as the source of the concept.

### ⚠️ Where the spec idea collides with DOTS — read before Track B

Polar **bakes** entity prefabs at edit time (`StructureRegister`, `EntitiesSubScene.unity`). JSON
specs load at **runtime**. These cannot fully replace each other, and Timberborn doesn't face this
problem because it isn't DOTS.

Also note the honest cost: Timberborn's blueprints reference assets **by string path**
(`"Model": "Buildings/Food/Bakery/Bakery.Folktails.Model"`) — the same runtime-resolved fragility
this document criticises in Polar's `Resources/` bindings. Specs trade *silently mismatched int IDs*
for *string keys that must be validated at load*. That's a real improvement, but only if you build the
validation (Lesson 8). It is not free.

**The realistic target architecture:**

| Owns | Mechanism |
|---|---|
| Numbers and behaviour (cost, inhabitants, shift duration, footprint) | JSON specs, loaded at runtime |
| Rendering and prefabs (mesh, material, baked entity prefab) | Unity assets, baked as now |
| The join between them | A **name key**, validated loudly at startup |

✅ **You already have the serializer.** `com.unity.serialization` 3.1.3 ships as a dependency of
`com.unity.entities` 1.4.8 (see `Packages/packages-lock.json:227`). `Unity.Serialization.Json`
handles polymorphic types and dictionaries; Unity's built-in `JsonUtility` handles **neither**, and a
`{"BuildingSpec": …, "WorkplaceSpec": …}` object is both. Newtonsoft is *not* in the project. No new
dependency is needed.

### Which layer each lesson applies to

Timberborn is a **managed OOP** component model. Polar is hybrid: MonoBehaviour+Zenject on the
input/grid/placement side, DOTS on spawning/movement. `BaseComponent` cannot become an
`IComponentData`, and forcing it will fight Burst.

| Polar layer | Apply these patterns? |
|---|---|
| Managed — installers, placement, structure data, UI | ✅ Directly |
| The bridge — `StructureManager` buffer appends | ⚠️ Deliberately; this is where the models meet |
| DOTS — systems, `IComponentData`, jobs | ❌ Use DOTS idioms |

---

## The four weaknesses this path fixes

Verified in code, not assumed:

1. **Inheritance where composition belongs** — `BaseStructureData` → `House`/`Road`/`Wall`/`Work`, plus
   an abstract `Structure` MonoBehaviour. A building that is both a home *and* a workplace is
   inexpressible.
2. **Type-switching instead of polymorphism** — `PlacementManager.cs:112-136` (twice) and
   `StructureManager.cs:120`.
3. **A god-method at the bridge** — `StructureManager.ConstructBuilding` (`:86-131`) does affordability,
   grid mutation, and three buffer appends.
4. **Construction-time side effects** — `PolarGridManager.Construct` builds the whole grid *during
   injection*, so startup is DI-order dependent.

---

# Lesson 1 — Start here (standalone)

## ☐ Lesson 1 — Simulation loop and game time
**Layer: DOTS.** Refs: Timberborn `IUpdatableSingleton` — *concept only, this lives on the ECS side.*

First because it depends on nothing else in this document, it's already in progress, and it unblocks
code you've already written.

**☐ Exercise.** `GameTimeManager` (`_Game/DOTS/Systems/Times/GameTimeManager.cs`) is a stub: `OnUpdate`
fetches `GameTimeData` and discards it. `GameTimeAuthoring` bakes a single `InGameHourDuration`. Build
it out — accumulate in-game time, expose hour/day, drive day/night — then add pause and ×1/×2/×3 speed
and verify nothing downstream breaks at either extreme.

Wire it to what exists: `WorkShiftSystem` and `WorkStructureData.ShiftDuration` are already written
against a notion of time. Make them consume game time, not raw `DeltaTime`.

**Key concept:** fixed simulation tick vs. frame-dependent update. Results must be identical at 30 fps
and 144 fps, and at ×3. Read up on fixed-timestep accumulators before writing this.

---

# Track A — The entry point and the container

## ☐ Lesson 2 — The composition root
**Layer: managed.** Refs: composition-root pattern; Timberborn `IConfigurator`, `[Context("Game")]`,
`PrefabConfigurator`, `AsSingleton()` / `AsTransient()`.

One place builds the object graph; nothing else calls `new` on a service. Timberborn splits this into
many small attribute-discovered configurators, each declaring its scene context.

**☐ Exercise A — map what you have.** Draw the actual startup graph: `GameSceneInstaller` →
`SignalBusInstaller` + `GameInstaller` + `UIInstaller` → what gets built, in what order, and what has
side effects. You cannot improve a composition root you can't draw.

**☐ Exercise B — split it.** `GameInstaller.cs` has five `#region` blocks (Grid, Structures,
Resources, Player, Signals). Split along those seams into focused installers. Then address the
string-path bindings (`"Prefabs/Worlds/PolarGrids/PolarNodePrefab"`,
`"Settings/PolarGridRingsSettings"`, …): renaming an asset breaks DI at runtime with **no compile
error**. Design a typed alternative — a single ScriptableObject reference holder, bound once.

## ☐ Lesson 3 — Ordered loading vs. side-effecting construction
**Layer: managed.** Refs: Timberborn `ILoadableSingleton.Load()`.

Timberborn separates *being constructed* from *being loaded*: `Load()` runs in dependency order,
after the container is built. Construction stays cheap and side-effect-free.

**☐ Exercise.** `PolarGridManager.Construct` builds the entire grid inside its `[Inject]` method.
Introduce an explicit load phase: an `ILoadable` interface, multi-bound, invoked in order by a
bootstrapper once the container is ready. Move grid construction into it. This is a prerequisite for
both spec loading (Lesson 7) and save/load (Lesson 11) — the grid must be rebuildable without
rebuilding the container.

## ☐ Lesson 4 — MultiBind: open for extension
**Layer: managed.** Refs: Timberborn `MultiBind<T>()` → injected as `IEnumerable<T>`.

The most transferable idea in Timberborn. Handlers register into a collection and **declare what they
handle**; adding one touches no existing file.

**☐ Exercise.** Give `IPlacementHandler` / `IPlacementValidator` a `CanHandle(…)`, multi-bind every
implementation, and have `PlacementManager` select from an injected `List<IPlacementHandler>` instead
of switching on `StructureType`. Zenject: `BindInterfacesTo<>` plus collection injection.

**Pass condition:** add a throwaway fifth structure type **without editing `PlacementManager`**. If
you have to edit it, the refactor failed.

---

# Track B — Specs: buildings as data

The heart of this path. Lessons 5–9 build one coherent system; don't split them across long gaps.

## ☐ Lesson 5 — Composition over inheritance
**Layer: managed.** Refs: Timberborn `BaseComponent`, "entities are composed of components";
RimWorld `CompProperties` / `ThingComp`.

In both games, a building is a **bag of components**, not a position in a class hierarchy. RimWorld's
docs are blunt about why: extending Def classes creates conflicts and an inheritance nightmare; write
a comp instead.

**☐ Exercise (design, no code).** Re-express Polar's four `*StructureData` subclasses as components.
Where does `Inhabitants` live? `ShiftDuration`? `Cost`? Then find the combination inheritance cannot
express — a building that is both a home and a workplace. Write that case down; it justifies the rest
of the track.

## ☐ Lesson 6 — Spec schema design
**Layer: managed.** Refs: Timberborn `ComponentSpec`, `[Serialize]`, `{ get; init; }`, template
blueprints; RimWorld `Def`, `defName`, `ParentName` inheritance for templates.

Specs are **immutable, stateless definitions**, separate from the live component that reads them.

> ✅ C# 9 check (see Part 1): `record` and `{ get; init; }` are **both C# 9**, so this pattern is
> reproducible in Polar as-is. Don't reach for `record struct` (C# 10) or `required` (C# 11).
> `ImmutableArray` availability in this Unity version is **unverified** — check before relying on it;
> `IReadOnlyList<T>` over an array works regardless and is what the repo already uses in
> `IStructureData.Cost`.

**☐ Exercise.** Design the spec set for Polar on paper, then implement two: a `StructureSpec`
(footprint, cost, display name) and a `WorkplaceSpec` (shift duration). Target the composed shape:

```json
{ "StructureSpec": { "Cost": [ { "ResourceType": "Wood", "Amount": 10 } ] },
  "WorkplaceSpec": { "ShiftDuration": 8 } }
```

Also design **template inheritance** (Timberborn's template blueprints, RimWorld's
`Name`/`ParentName`) — most buildings differ from a base by two fields, and without templates you get
massive duplication.

## ☐ Lesson 7 — Loading specs from JSON
**Layer: managed.** Refs: Timberborn `.blueprint.json`, `#append` / `#remove` / `#delete`.
**Depends on Part 1 Week 11 (streams & JSON).**

**☐ Exercise.** Load the Lesson 6 specs from disk with **`Unity.Serialization.Json`** (already
available — see above). Do it in the ordered load phase from Lesson 3.

Concretely confront why `JsonUtility` is not an option here: it supports neither dictionaries nor
polymorphism, and a blueprint is a **dictionary of polymorphic spec objects keyed by type name**.
Understanding *why* the built-in tool fails is most of this lesson.

**☐ Stretch.** Implement one merge operator (`#append`) for list fields. This is how Timberborn lets
a later file add to an earlier one's list without rewriting it. Don't build the full operator set —
it only earns its complexity with third-party content.

## ☐ Lesson 8 — Name-keyed registry, validated loudly
**Layer: managed + the bridge.** Refs: RimWorld `DefDatabase<T>.GetNamed("defName")`.
**This lesson fixes a live bug class.**

Polar joins ScriptableObjects to baked entity prefabs by a hand-assigned `int ID`
(`StructureDictionary.Get(int id)` → `StructureRegister` → `AvailableStructure`). Nothing checks the
two agree, so a mismatch produces a structure that silently spawns nothing.

**☐ Exercise.** Replace the int ID with a string spec key, resolved through a registry in the spirit
of `DefDatabase<T>.GetNamed()`. Then — the part that makes it an *improvement* rather than a lateral
move — **validate at load and fail loudly**: every spec key must resolve to exactly one baked prefab,
every prefab must be claimed, and any spec referencing a missing asset aborts startup with a message
naming the file and key. A silent int mismatch becomes a startup error you cannot miss.

**Pass condition:** deliberately misspell a key and confirm you get a precise error at startup, not a
building that places but never appears.

## ☐ Lesson 9 — Composition at spawn: decorators
**Layer: managed + the bridge.** Refs: Timberborn `AddDecorator<SourceSpec, TargetComponent>()`
(`FarmHouseSpec` → `FarmHouse` → `HaulCandidate`).

Decoration attaches cross-cutting behaviour without the spec author knowing: anything that is a
farmhouse also becomes a haul candidate, declared once.

**☐ Exercise.** Delete `if (structureData is WorkStructureData workStructureData)`
(`StructureManager.cs:120`). A structure contributes its `WorkplaceLocation` buffer entry because it
**has a `WorkplaceSpec`**, not because the bridge type-checks it. Restructure `ConstructBuilding` so
each buffer append (`StructurePlacementOrder`, `PeopleSpawnOrder`, `WorkplaceLocation`) is contributed
by a registered, multi-bound handler keyed on spec presence.

**Pass condition:** adding a "produces resources over time" structure requires **zero** edits to
`StructureManager`.

> Hardest lesson in the document, because it sits exactly on the managed↔DOTS seam. Decorators live
> managed; their output is buffer appends. Keep that direction — don't push decoration into ECS.

---

# Track C — The rest of the architecture

## ☐ Lesson 10 — Entity lifecycle interfaces
**Layer: managed.** Refs: Timberborn `IInitializableEntity`, `IFinishedStateListener`
(`OnEnterFinishedState`/`OnExitFinishedState`), `IDeletableEntity`, `IAwakableComponent`.

Note what these buy: a component opts into only the events it cares about, instead of inheriting a
base class of virtual no-ops. `IFinishedStateListener` exists because a city-builder building has a
**construction phase** — Polar has no such concept; buildings appear complete.

**☐ Exercise.** `Structure.OnBuild()`/`OnDemolish()` are abstract methods, and
`PolarNode.ClearBuilding()` exists but **nothing calls it** — there is no demolition flow at all.
Build one as opt-in lifecycle interfaces: free the nodes, refund resources, despawn the ECS entity,
notify the UI. Then add a construction-in-progress state.

## ☐ Lesson 11 — Persistence: per-entity save
**Layer: managed + the bridge.** Refs: Timberborn `IPersistentEntity` with `Save()` / `Load()`.
**Depends on Part 1 Week 9 (async) and Week 11 (JSON); reuses Lessons 3, 7, 8.**

Each component saves its own state. No central `SaveGame` class that knows every type.

**☐ Exercise.** Build the save system on the spec loader from Lesson 7. The hard part is specific to
Polar and has **no Timberborn equivalent**: your state spans two worlds. Grid occupancy lives in
`PolarNode` MonoBehaviours; people, paths and workplaces live in ECS components and buffers.

**Decide before coding, and write it down:** is ECS state *saved*, or *rebuilt* from managed state on
load? This is the most consequential architectural choice in either document, and it is far cheaper to
decide now than after a save format ships.

## ☐ Lesson 12 — UI architecture: fragments, not controllers
**Layer: managed.** Refs: Timberborn `IEntityPanelFragment`, `EntityPanelModule`,
`VisualElementLoader`, `UILayout.AddBottomRight()`, `PanelStack`, `DialogBoxShower`, `ITool` +
`BottomBarElementsProvider`.

Timberborn's selected-entity panel is not one class. It's multi-bound `IEntityPanelFragment`s, each
rendering one aspect — `WorkplaceFragment` draws workplace controls and nothing else.

Polar is already closest here: UIToolkit plus DI-bound controllers (`StructureInfoController`,
`ResourceBarController`, `BuildBarController`, `PlacementStatusController`) with clean event
subscribe/unsubscribe. The gap is that they're hand-wired singletons, not a composable set.

**☐ Exercise.** Convert `StructureInfoController` into a fragment-composed panel: one fragment per
spec (Lesson 6), multi-bound (Lesson 4), each deciding whether it applies to the selection. Then add a
fragment for a spec that didn't exist when you wrote the panel.

---

## ★ Capstone: the mod test

Timberborn and RimWorld look the way they do **because they must be extensible by people who cannot
edit the source.** That constraint is what forces specs, name keys, multi-binding and decorators.
Polar has no mods, so nothing forces the discipline — which is why it drifted toward central switches.

**☐ Final exercise.** Add a warehouse that stores resources and employs workers. Count the existing
files you had to edit.

- **Today:** `StructureType` enum, `PlacementManager` ×2 switches, `StructureManager.ConstructBuilding`,
  `StructureDictionary`, `StructureRegister`, a new `*Data` subclass — roughly six.
- **Target:** one new spec JSON, one prefab, one registration.

> Files edited: ______

---

## What does *not* transfer

- **Neither reference is DOTS.** Timberborn's and RimWorld's component models are managed OOP. Polar's
  simulation is ECS by choice, for throughput. Tracks A and B stop at the bridge.
- **Both grids are square; Polar's is polar.** No spatial code transfers. The `Fi >= 360 → 0` wrap and
  the clockwise/counter-clockwise sign convention are yours alone.
- **Harmony/Cecil patching, `IModStarter`** — mod-loading infrastructure. Read once; don't build unless
  you actually want mods.
- **The full `#append`/`#remove`/`#delete` operator set** — a data-merging system that only pays off
  with third-party content. Implement one operator in Lesson 6 to learn the idea; stop there.
- **RimWorld's XML** — the format is incidental; `defName` resolution is the transferable part.

---

## Sequencing with Part 1

Independent paths, but four lessons have prerequisites:

| Lesson | Needs from Part 1 |
|---|---|
| 1 (Game time) | **Nothing** — start here |
| 4 (MultiBind) | Weeks 6–7 — collections and LINQ over injected sets |
| 6 (Spec schema) | Week 2 — records, `init`, immutability |
| 7 (JSON loading) | Week 11 — streams & JSON |
| 11 (Persistence) | Week 9 (async) **and** Week 11 |

**Recommended order:** Lesson 1, then Part 1 weeks 1–8, then Track A (2–4), then Track B (5–9) as a
continuous block, then Track C (10–12).

---

## Progress log

| # | Lesson | Track | Layer | Done | Notes |
|---|---|---|---|---|---|
| 1 | **Simulation loop & game time** | — | DOTS | ☐ | ← start here |
| 2 | Composition root | A | managed | ☐ | |
| 3 | Ordered loading | A | managed | ☐ | |
| 4 | MultiBind | A | managed | ☐ | |
| 5 | Composition over inheritance | B | managed | ☐ | |
| 6 | Spec schema design | B | managed | ☐ | |
| 7 | JSON spec loading | B | managed | ☐ | |
| 8 | Name-keyed registry | B | managed + bridge | ☐ | |
| 9 | Decorators at spawn | B | managed + bridge | ☐ | |
| 10 | Entity lifecycle | C | managed | ☐ | |
| 11 | Persistence | C | managed + bridge | ☐ | |
| 12 | UI fragments | C | managed | ☐ | |
| ★ | The mod test | — | all | ☐ | files edited: ____ |

## Sources

- [Timberborn architecture](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture) · [Blueprints](https://github.com/mechanistry/timberborn-modding/wiki/Blueprints) · [User interface](https://github.com/mechanistry/timberborn-modding/wiki/User-interface) · [Coding basics](https://github.com/mechanistry/timberborn-modding/wiki/Coding-basics) (official modding wiki)
- [RimWorld: Def classes](https://rimworldwiki.com/wiki/Modding_Tutorials/Def_classes) · [XML Defs](https://rimworldwiki.com/wiki/Modding_Tutorials/XML_Defs) · [ThingDef](https://rimworldwiki.com/wiki/Modding_Tutorials/ThingDef) (RimWorld Wiki)
- [Abstracts and inheritance](https://spdskatr.github.io/RWModdingResources/abstracts.html) (RimWorld Modding Resources)
