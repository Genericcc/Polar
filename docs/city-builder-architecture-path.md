# Part 2 — City-builder architecture, cross-referenced with Timberborn

A second, independent learning path. **Part 1** (`csharp-learning-path.md`) is about the *language*;
this is about *how a city builder is put together*. The reference implementation is **Timberborn**,
whose architecture is publicly documented for modders:
<https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture>

10 lessons, ~3–4 h each. Not chapter-driven, so the order is by dependency, not by book.

---

## Why Timberborn is the right reference

It's a shipped, commercially successful city builder **in Unity**, and because it supports modding,
its internal architecture is documented rather than guessed at. Its DI framework, **Bindito, is
based on Zenject** — which Polar already uses. So this isn't a foreign paradigm: Timberborn's
`IConfigurator` is your `GameInstaller`. You're reading a mature version of the architecture you
already started.

### ⚠️ Read this before Lesson 1: do not copy Timberborn wholesale

Timberborn is a **managed OOP component model** — `BaseComponent`, classes, virtual dispatch, a DI
container resolving object graphs. Polar is **hybrid**: MonoBehaviour + Zenject on the input/grid/
placement side, Unity DOTS (unmanaged structs, systems, buffers) on the spawning/movement side.

Unity DOTS is a *different* ECS with incompatible rules. `BaseComponent` and `IPersistentEntity`
cannot cross into an `IComponentData`. Applying Timberborn patterns to your DOTS systems will fight
Burst and lose.

**The rule for this entire path:**

| Polar layer | Apply Timberborn patterns? |
|---|---|
| Managed side — installers, placement, structures data, UI | ✅ Yes, directly |
| The bridge — `StructureManager` buffer appends | ⚠️ Design deliberately; this is where the two models meet |
| DOTS side — systems, `IComponentData`, jobs | ❌ No. Use DOTS idioms |

Most of Polar's architectural pain is on the managed side, so most of this transfers. But every
lesson below states which layer it applies to.

---

## The four architectural weaknesses this path fixes

Found by reading the code, not assumed:

1. **Inheritance where composition belongs.** `BaseStructureData` → `HouseStructureData` /
   `RoadStructureData` / `WallStructureData` / `WorkStructureData`, plus an abstract `Structure`
   MonoBehaviour. Every new structure *kind* needs a new subclass.
2. **Type-switching instead of polymorphism.** `PlacementManager.cs:112-136` switches on
   `StructureType` twice; `StructureManager.cs:120` does `if (structureData is WorkStructureData)`.
   Adding a structure kind means editing central files — the definition of a closed architecture.
3. **A god-method at the bridge.** `StructureManager.ConstructBuilding` (`StructureManager.cs:86-131`)
   checks affordability, mutates grid state, and appends to three different ECS buffers in sequence.
4. **Construction-time side effects.** `PolarGridManager.Construct` builds the entire grid *during
   dependency injection*, making startup order-dependent (CLAUDE.md already flags this).

---

## Lesson 1 — Composition over inheritance: the entity-component model
**Layer: managed.** Timberborn refs: `BaseComponent`, "entities are composed of components".

In Timberborn a beaver or a building is an **entity with a bag of components**. There is no
`FarmHouse : Building : Entity` chain. Behaviour is added by attaching a component, not by
subclassing.

**☐ Exercise (design, no code).** Take Polar's four `*StructureData` subclasses and re-express them
as components: what is `Inhabitants` (`PeopleSpawnOrder`)? What is `ShiftDuration`
(`WorkStructureData`)? Which structures would hold which components? Write the component list, then
find the combination that inheritance *cannot* express — e.g. a building that is both a workplace
and a home. That impossible case is the argument for the whole path.

## Lesson 2 — Specs: immutable, data-driven definitions
**Layer: managed.** Timberborn refs: `ComponentSpec`, `[Serialize]`, `{ get; init; }`,
`ImmutableArray`, JSON blueprints with `#append` / `#remove`.

Timberborn Specs are **immutable record types** — stateless definitions, separate from the live
runtime component that reads them. Polar uses mutable `ScriptableObject`s for the same job.

> ✅ Good news for the C# 9 ceiling (see Part 1): `record` and `{ get; init; }` are **both C# 9**, so
> the Spec pattern is reproducible in Polar as-is. `ImmutableArray` is a BCL type, also fine.

**☐ Exercise.** Convert one structure definition to a spec-shaped immutable record, with the
ScriptableObject reduced to an authoring shell that produces it. Then write down the honest
trade-off: ScriptableObjects give you Unity's inspector and asset references for free; specs give you
immutability, testability and mod-ability. Decide which matters for Polar and commit to it.

**☐ Stretch.** Polar matches structures by a hand-assigned `int ID` between the ScriptableObject and
the baked `StructureRegister` — CLAUDE.md notes an ID mismatch is the usual cause of a structure
spawning nothing. That's exactly the fragility specs remove. Design a name/spec-keyed alternative.

## Lesson 3 — The composition root and DI contexts
**Layer: managed.** Timberborn refs: `IConfigurator`, `[Context("Game")]`, `AsSingleton()`,
`AsTransient()`, `PrefabConfigurator`.

Timberborn splits bindings into many small configurators, each declaring the scene context it
belongs to, auto-discovered by attribute. Polar has one `GameInstaller` with five `#region` blocks
and a `UIInstaller`.

**☐ Exercise.** Split `GameInstaller` (`Zenject/Installers/GameInstaller.cs`) along its existing
region seams — Grid, Structures, Resources, Player — into focused installers. While there, confront
the `Resources/` string-path bindings (`"Prefabs/Worlds/PolarGrids/PolarNodePrefab"`,
`"Settings/PolarGridRingsSettings"`, …): renaming an asset breaks DI at *runtime* with no compile
error. Design a typed alternative (a ScriptableObject reference holder bound once).

## Lesson 4 — MultiBind: architecture that's open for extension
**Layer: managed.** Timberborn refs: `MultiBind<T>()` → injected as `IEnumerable<T>`.

This is the single most transferable idea in Timberborn. Instead of a central switch deciding which
handler to use, every handler is registered into a collection and **declares what it can handle**.
Adding a new one touches no existing file.

**☐ Exercise.** Part 1 Week 4 collapsed `PlacementManager`'s two switches into a lookup. Now do it
properly: give `IPlacementHandler` / `IPlacementValidator` a `CanHandle(IStructureData)` (or a
declared `StructureType`), multi-bind all implementations, and have `PlacementManager` select from
the injected collection. Zenject does this with `BindInterfacesTo<>` + injecting `List<IPlacementHandler>`.
Verify by adding a throwaway fifth structure type **without editing `PlacementManager`.** If you have
to edit it, the refactor failed.

## Lesson 5 — Lifecycle: ordered loading vs. side-effecting construction
**Layer: managed.** Timberborn refs: `ILoadableSingleton.Load()`, `IUpdatableSingleton`.

Timberborn separates *being constructed* from *being loaded*. `ILoadableSingleton.Load()` runs in
dependency order, after the container is built.

**☐ Exercise.** `PolarGridManager.Construct` builds the whole grid inside the `[Inject]` method, so
grid creation is DI-ordering-dependent. Introduce an explicit load phase — an `ILoadable` interface,
multi-bound, invoked in order by a bootstrapper after the container is ready. Move grid construction
into it. This also makes the grid rebuildable without rebuilding the container, which you'll need for
save/load in Lesson 8.

## Lesson 6 — Entity lifecycle interfaces
**Layer: managed.** Timberborn refs: `IInitializableEntity`, `IFinishedStateListener`
(`OnEnterFinishedState` / `OnExitFinishedState`), `IDeletableEntity`, `IAwakableComponent`.

Note what these interfaces buy: a component opts into *only* the lifecycle events it cares about,
rather than inheriting a base class with a dozen virtual no-ops. `IFinishedStateListener` exists
because a city-builder building has a **construction phase** distinct from being operational — Polar
has no such concept; buildings pop into existence complete.

**☐ Exercise.** Polar has `Structure.OnBuild()` / `OnDemolish()` as abstract methods, and
`PolarNode.ClearBuilding()` exists but **nothing calls it** — there is no demolition flow at all.
Build one, designed as lifecycle interfaces rather than base-class overrides: free the nodes, refund
or destroy resources, despawn the ECS entity, and notify the UI. Then add a construction-in-progress
state.

## Lesson 7 — Decorators: composing behaviour at spawn time
**Layer: managed + the bridge.** Timberborn refs: `AddDecorator<SourceSpec, TargetComponent>()`,
e.g. `FarmHouseSpec` → `FarmHouse` → `HaulCandidate`.

Decoration is how Timberborn attaches cross-cutting behaviour without the spec author knowing about
it: anything that is a farmhouse *also* becomes a haul candidate, declared in one place.

**☐ Exercise.** Kill `if (structureData is WorkStructureData workStructureData)`
(`StructureManager.cs:120`). A structure that is a workplace should contribute its `WorkplaceLocation`
buffer entry because a *workplace component/decorator* says so, not because the bridge method
type-checks it. Restructure `ConstructBuilding` so each buffer append is contributed by a registered
handler. When done, adding a "produces resources over time" structure should require zero edits to
`StructureManager`.

> This is the hardest lesson, because it's exactly on the managed↔DOTS seam. The decorators live
> managed; their output is buffer appends. Keep that direction — don't try to push decoration into
> the ECS side.

## Lesson 8 — Persistence: per-entity save, not a god-serializer
**Layer: managed + the bridge.** Timberborn refs: `IPersistentEntity` with `Save()` / `Load()`.
**Depends on Part 1 Week 11 (streams/JSON) and Week 9 (async).**

Each component saves its own state. There is no central `SaveGame` class that knows every type — the
architecture that would otherwise rot fastest.

**☐ Exercise.** Design (then build) Polar's save format around per-component save. The genuinely hard
part is specific to Polar and has no Timberborn equivalent: **your state is split across two worlds.**
Grid occupancy lives in `PolarNode` MonoBehaviours; people, paths and workplaces live in ECS
components and buffers. Decide what is authoritative, and whether ECS state is saved or *rebuilt*
from the managed state on load. Write that decision down before coding — it's the single most
consequential architectural choice in this document.

## Lesson 9 — The simulation loop and game time
**Layer: DOTS.** Timberborn refs: `IUpdatableSingleton.UpdateSingleton()`.
*Apply the concept, not the interface — this one lives on the ECS side.*

**☐ Exercise — finish the work already in flight.** `GameTimeManager`
(`_Game/DOTS/Systems/Times/GameTimeManager.cs`) is currently an empty stub: `OnUpdate` fetches
`GameTimeData` and does nothing with it. `GameTimeAuthoring` bakes a single `InGameHourDuration`.
Build it out: accumulate in-game time, expose hour/day, and drive day/night. Then add pause and speed
multipliers (×1/×2/×3 — every city builder has them) and make sure nothing downstream breaks at ×3 or
at pause.

Connect it to what already exists: `WorkShiftSystem` and `WorkStructureData.ShiftDuration` are already
written against a notion of time. Make them consume game time rather than raw `DeltaTime`.

**Key concept:** fixed simulation tick vs. frame-rate-dependent update. A city builder must produce
identical results at 30 fps and 144 fps, and at ×3 speed. Read up on fixed-timestep accumulators
before writing this.

## Lesson 10 — UI architecture: fragments, not controllers
**Layer: managed.** Timberborn refs: `IEntityPanelFragment`, `EntityPanelModule`,
`VisualElementLoader`, `UILayout` (`AddBottomRight()`), `PanelStack`, `DialogBoxShower`, `ITool` +
`BottomBarElementsProvider`.

Timberborn's selected-entity panel is **not one class**. It's a set of multi-bound
`IEntityPanelFragment`s, each rendering one aspect — `WorkplaceFragment` draws workplace controls and
nothing else. Select a building, and the panel is assembled from whichever fragments apply.

Polar is already closer here than anywhere else: you have UIToolkit + DI-bound controllers
(`StructureInfoController`, `ResourceBarController`, `BuildBarController`, `PlacementStatusController`)
with clean event subscribe/unsubscribe. The gap is that they're **hand-wired singletons**, not a
composable multi-bound set.

**☐ Exercise.** Convert `StructureInfoController` into a fragment-composed entity panel: one fragment
per structure component, multi-bound (Lesson 4), each deciding whether it applies to the selected
structure. Then add a fragment for a component that didn't exist when you wrote the panel — that's
the test.

---

## The capstone: the mod test

Timberborn's architecture looks the way it does **because it must be extensible by people who cannot
edit its source.** That constraint is what forces `MultiBind`, specs, decorators and per-entity
persistence. Polar has no mods, so nothing forces the discipline — which is exactly why it drifted
toward central switches.

**☐ Final exercise.** Add a new structure kind — say a warehouse that stores resources and employs
workers — and count how many existing files you had to edit. At the start of this path the answer is
roughly: `StructureType` enum, `PlacementManager` ×2 switches, `StructureManager.ConstructBuilding`,
`StructureDictionary`, `StructureRegister`, plus a new `*Data` subclass. **The path succeeded if the
answer becomes: one new spec, one new prefab, one registration.**

---

## Where Timberborn's answer does *not* fit Polar

Worth knowing so you don't over-apply this:

- **Timberborn is not DOTS.** Its component model is managed OOP. Polar's simulation is ECS by
  choice, for throughput. Lessons 1, 2, 6, 7 stop at the bridge.
- **Timberborn's grid is square; Polar's is polar.** Nothing in its spatial code transfers. Your
  footprint/rotation/wrapping problems (`Fi >= 360 → 0`, the clockwise/counter-clockwise sign
  convention) are yours alone.
- **Harmony/Cecil patching and `IModStarter`** are mod-loading infrastructure. Irrelevant unless you
  actually want mods — read once, don't build.
- **Blueprint JSON with `#append`/`#remove`** is a data-merging system that only earns its complexity
  with third-party content. Understand the idea; don't implement it yet.

---

## Suggested sequencing with Part 1

They're independent, but three lessons here have prerequisites:

| This lesson | Needs from Part 1 |
|---|---|
| 2 (Specs) | Week 2 — records, `init`, immutability |
| 4 (MultiBind) | Week 6/7 — collections and LINQ over injected sets |
| 8 (Persistence) | Week 9 (async) **and** Week 11 (streams/JSON) |

Easiest schedule: run Part 1 weeks 1–8 first, then alternate — one Part 1 week, one Part 2 lesson.
Lesson 9 (game time) is the exception: it's unblocked, it's already half-written in your working
tree, and it's the most immediately useful thing in this document. **Consider doing it first.**

---

## Progress log

| # | Lesson | Layer | Done | Notes |
|---|---|---|---|---|
| 1 | Composition over inheritance | managed | ☐ | |
| 2 | Specs / immutable definitions | managed | ☐ | |
| 3 | Composition root & contexts | managed | ☐ | |
| 4 | MultiBind & open extension | managed | ☐ | |
| 5 | Ordered loading lifecycle | managed | ☐ | |
| 6 | Entity lifecycle interfaces | managed | ☐ | |
| 7 | Decorators | managed + bridge | ☐ | |
| 8 | Persistence | managed + bridge | ☐ | |
| 9 | Simulation loop & game time | DOTS | ☐ | |
| 10 | UI fragments | managed | ☐ | |
| ★ | The mod test | all | ☐ | files edited: ____ |

## Sources

- [Timberborn architecture (official modding wiki)](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture)
- [Timberborn user interface (official modding wiki)](https://github.com/mechanistry/timberborn-modding/wiki/User-interface)
- [Timberborn coding basics (official modding wiki)](https://github.com/mechanistry/timberborn-modding/wiki/Coding-basics)
- [mechanistry/timberborn-modding overview (DeepWiki)](https://deepwiki.com/mechanistry/timberborn-modding/1-overview)
