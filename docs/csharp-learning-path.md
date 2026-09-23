# C# 12 in a Nutshell — 12-week path, anchored to Polar

**No book required.** This file is the **schedule, the exercise set, and the progress log**; the
teaching material lives in `docs/lessons/`, written for this codebase. Every exercise is a real change
to this repo.

The `Ch. N` labels on each week are a **topic map** to standard C# reference material (and to Albahari's
*C# 12 in a Nutshell*, whose chapter structure this follows) — they're signposts for looking things up,
not required reading.

### Lessons

| Week | Lesson file | Status |
|---|---|---|
| 1 | [`lessons/week-01-language-basics.md`](lessons/week-01-language-basics.md) | ✅ written |
| 2–12 | — | written on request, one at a time |

Lessons are deliberately written **one at a time, as you reach them**. Twelve weeks of material
delivered up front would go stale against the code and wouldn't get read. Ask for the next one when
you're ready for it.

> **Part 2 is a separate path:** [`city-builder-architecture-path.md`](city-builder-architecture-path.md)
> — city-builder architecture in three tracks (bootstrap/DI, specs-as-data, everything else),
> cross-referenced with Timberborn and RimWorld. This path is about the *language*; that one is about
> *how the game is put together*. Four of its lessons depend on weeks here (see its sequencing table),
> and two exercises that used to live here now belong there — noted inline in Weeks 9 and 11.

Budget: ~3–4 h/week × 12 weeks ≈ 45 h. The book is ~1,200 pages, so weeks are marked **DRILL**
(read closely + exercise) or **SURVEY** (read once for a mental index of what exists). Pretending
45 hours covers 25 chapters equally is how this gets abandoned in week 5.

---

## ⚠️ Read this before Week 1: this project is C# 9, the book is C# 12

`Assembly-CSharp.csproj` sets `<LangVersion>9.0</LangVersion>`. Unity 6000.3 pins runtime scripts to
**C# 9**. A large slice of what the book teaches *will not compile here*. Know which slice, or you
will lose evenings to "why is this a syntax error".

| Feature | C# ver | Usable in Polar? |
|---|---|---|
| `record` (class), `init` accessors | 9 | ✅ (but Unity can't serialise a `record`) |
| Target-typed `new()`, `is not` / `and` / `or` patterns | 9 | ✅ already used in this repo |
| Native ints, function pointers, lambda discards | 9 | ✅ |
| **`record struct`, `readonly record struct`** | 10 | ❌ **use `readonly struct` + `IEquatable<T>`** |
| File-scoped namespaces, global usings | 10 | ❌ |
| `required` members, list patterns, raw strings | 11 | ❌ |
| Static abstract interface members (generic math) | 11 | ❌ |
| Primary constructors, collection expressions `[1,2,3]` | 12 | ❌ |

**Do not** try to raise this with a `csc.rsp` containing `-langversion:latest`. It appears to work,
then breaks Burst and IL2CPP in ways that are miserable to diagnose. Read the C# 10–12 chapters for
knowledge (they matter the moment you write a non-Unity .NET project); write C# 9 in this repo.

When a chapter teaches a post-C#-9 feature, note it in your recall write-up under a "can't use here,
but here's what it replaces" heading. That contrast is itself good learning.

---

## Method

Per ~3.5 h week:

1. **Read (~60 min)** — the week's lesson file, one pass, no notes.
2. **Drills (~30 min)** — the predict-then-run snippets in the lesson. Write your prediction down
   *before* running. Each lesson has an answer key, so you can check yourself without a book.
3. **Recall (~20 min)** — close the file, write from memory what it covered and what surprised you.
   Compare. *The gap is the lesson.*
4. **Exercise (~70 min)** — the repo change. Make it compile and run.
5. **Spaced review (~10 min)** — re-read your recall notes from weeks N−1 and N−3. Not the lesson.

Rule: **never read two lessons without an exercise between them.** Reading C# feels like learning C#,
and it isn't.

**Add a fifth step on the weeks that have drills (below): predict, then run.** Write down what you
think the snippet prints *before* executing it. A wrong prediction is worth more than three correct
ones — it locates a belief you didn't know you held. This is the single most effective technique for
rebuilding intuition about copies, boxing and equality, and it's why the drills exist.

---

## The fundamentals spine — read this to see how Weeks 1–6 connect

Stack vs. heap, `Equals`, hashing and dictionaries are usually taught as four separate topics. They
are **one story**, and in this repo they're one story about a single type. Following that thread is the
point of the first half of this path:

1. **Layout (Week 1).** `PolarGridPosition` is a *value*. Where it lives depends on where it's
   declared: a local → the stack; a field of `PolarNode` (a class) → **on the heap, inside that
   object**; inside a `NativeArray` → unmanaged memory. It is *copied* on assignment, on being passed,
   on being returned.
2. **Identity (Weeks 2 & 5).** Because it's a value, "the same node position" has to mean "all fields
   equal" — and you must define that yourself. The default `ValueType.Equals` does work, but it boxes
   and can fall back to reflection.
3. **Hashing (Week 5).** A `Dictionary` finds a *bucket* by hash, then confirms the match by equality.
   So `GetHashCode` and `Equals` must agree. If they disagree, a key can be inserted and then never
   found again.
4. **Lookup (Week 6).** Only once 1–3 are right can you replace `GridNodes.FirstOrDefault(...)`
   (`PolarGrid.cs:184`) with an O(1) dictionary. **That's why the weeks are in this order** — Week 6's
   payoff is impossible without Week 5's contract, which is meaningless without Week 1's layout.

### The drills

Short, throwaway, predict-first. Use LINQPad or a scratch MonoBehaviour — not the real code.

**☐ Drill 1 — copies (Week 1).**
```csharp
var a = new PolarGridPosition(0, 1, 90, 0f);
var b = a;
b.D = 99;
// Predict: what is a.D? What is b.D? Why?
```
Then repeat with `PolarNode` (a class) instead of the struct. Explain the difference out loud.

**☐ Drill 2 — where a struct actually lives (Week 1).** `PolarNode` is a class holding a
`PolarGridPosition` property. Is that struct on the stack or the heap? Answer before reading on, then
justify it. *(Most people say "stack, it's a struct" — that's the belief this drill is for.)*

**☐ Drill 3 — the indexer copy (Week 1).** Put `PolarGridPosition` values in a `List<T>` and in a
`T[]`. Try `list[0].D = 5;` and `array[0].D = 5;`. One compiles, one doesn't. Predict which, then
explain the error message in terms of copies.

**☐ Drill 4 — boxing (Week 5).** Call `Equals(object)` on a `PolarGridPosition` in a loop, then the
`IEquatable<T>` version. Watch the allocation counter in the Profiler. This makes Week 5's refactor
stop being theoretical.

**☐ Drill 5 — the float-in-a-hash-key bug (Weeks 5 & 6). Do this one; it's a live risk in your code.**
`PolarGridPosition.GetHashCode` folds in `H`, a `float`, and `operator ==` compares it with `==`:
```csharp
var direct = 1.0f;
var accumulated = 0f;
for (var i = 0; i < 10; i++) accumulated += 0.1f;
// Predict: direct == accumulated ?  Same GetHashCode() ?
// Then put a position with H = direct in a Dictionary and look it up with H = accumulated.
```
**Why accumulation and not `0.1f + 0.2f`:** in *single* precision `0.1f + 0.2f == 0.3f` actually holds
(the rounding happens to land on the same float) — the famous inequality is a `double` result. Repeated
accumulation is what reliably drifts in float, and it's the realistic case here: `PolarGrid` builds
rings by **accumulating radius outward**, so a computed `H` or radius can easily miss a literal one.
Decide what to do about it in Week 5.

**☐ Drill 6 — the mutable-key trap (Week 6).** Use a *class* as a dictionary key, insert it, then
mutate a field that its `GetHashCode` uses. Try to find it again. Then reason about whether a
**struct** key has the same problem, and why not. *(Precision matters here: the classic "lost key" bug
is a reference-type bug, because the dictionary stores a copy of a struct key. The struct equivalent is
Drill 5.)*

### Self-check — answer without looking

If a question is uncomfortable, that's the topic to re-read. Revisit this list at Week 6 and again at
Week 12.

1. Where does a struct declared as a field of a class live?
2. Name three places a struct is silently copied.
3. What does `readonly struct` prevent, and what does it enable the compiler to do?
4. Why does `Equals(object)` allocate when called on a struct?
5. What exactly breaks if `GetHashCode` and `Equals` disagree? Describe the symptom.
6. Two equal objects must have equal hash codes. Must two objects with equal hash codes be equal?
7. Why is `Dictionary` lookup O(1) *amortised* rather than simply O(1)?
8. What does `IEquatable<T>` buy you that overriding `Equals(object)` doesn't?
9. When is `List<T>.Contains` O(n) and when is `HashSet<T>.Contains` O(1)? What does the difference cost in memory?
10. Why is a mutable struct a poor dictionary key even though mutating your local copy can't corrupt the dictionary?

---

## Phase 1 — Core language

### ☐ Week 1 — Ch. 2 *C# Language Basics* — DRILL
Value vs. reference types, stack/heap, definite assignment, `var`, `out`/`ref`/`in`/`params`,
numeric conversion and overflow.

**☐ Drills 1–3** from the fundamentals spine above. Do these *before* the exercise — the audit is much
faster once you can predict copies reliably.

**☐ Exercise — copy-semantics audit.** `PolarGridPosition` (`Assets/_Scripts/_Game/Grid/PolarGridPosition.cs`)
is a *mutable* struct passed by value across `PolarGrid`, `PolarNode`, the placement handlers and
several DOTS components. Trace its call sites; write down every point a silent copy happens and
whether any mutation is lost. **Fix nothing** — Week 2 does. Output: a findings list.

### ☐ Week 2 — Ch. 3 *Creating Types in C#* — DRILL
Classes vs. structs, `readonly struct`, properties, indexers, inheritance, interfaces, enums and
their underlying values. Read the records/`init`/`required` sections as **knowledge, not homework**
(see the C# 9 table).

**☐ Exercise A — the `[Flags]` bug.** `PolarNodeType` (`_Game/Grid/PolarNode.cs:100-105`) is marked
`[Flags]` but its members are `Clear = 0, Resource = 1` by implicit numbering — it cannot behave as a
flags enum past two members. Decide: explicit powers of two, or drop `[Flags]`. Justify in a comment.

**☐ Exercise B — make `PolarGridPosition` a `readonly struct`.** Not a `record struct` (C# 10, won't
compile). Hard constraint: it is embedded in DOTS components — `StructurePlacementOrder`,
`PeopleSpawnOrder`, `WorkplaceLocation`, `PathfindingParams` — so it **must stay unmanaged and
blittable**. No reference fields, no auto-property tricks that break Burst.

> Risk check: it is *not* a `[SerializeField]` anywhere, so scene/prefab churn is unlikely. But it
> *is* in baked ECS data — after changing it, reopen `Assets/Scenes/GameScene/EntitiesSubScene.unity`
> and confirm entities still bake. Commit first.

### ☐ Week 3 — Ch. 4 *Advanced C#*, part 1 — DRILL
Delegates, events, lambdas, **closures and captured-variable lifetime**, operator overloading,
`try`/`finally` and exception semantics.

**☐ Exercise — delegate identity, on code that's already correct.** Unusually, this repo's event
hygiene is clean: `StructureInfoController`, `ResourceBarController`, `PopulationController`,
`PlacementStatusController` and `BuildBarController` all pair `+=` with `-=`. So the exercise is to
explain *why it works* and find the two places it can't:

1. `PlacementStatusController.cs:29` and `:49` subscribe and unsubscribe
   `_placementManager.CancelPlacement` — a **method group**, which allocates a *new* delegate object
   each time. Why does `-=` still remove it? (Answer lives in delegate equality: target + method,
   not reference.) Prove it in a scratch test.
2. `StructureButtonFactory.cs:34` (`button.clicked += () => _signalBus.Fire(...)`) and
   `BuildBarController.cs:60` (`RegisterCallback<PointerEnterEvent>(_ => ...)`) subscribe
   **lambdas**, which hold no recoverable reference and can never be unsubscribed. Determine whether
   that leaks, given UIToolkit element lifetime. Note that `capturedStructure` in `BuildBarController`
   exists precisely to dodge the loop-variable capture trap — make sure you can explain that trap.

### ☐ Week 4 — Ch. 4 *Advanced C#*, part 2 — DRILL
**Iterators and the compiler-generated state machine**, pattern matching, tuples, nullable value
types, `??` / `?.`. Read nullable *reference* types as knowledge — enabling them across this repo is
a bigger job than a week allows.

**☐ Exercise A.** `PlacementManager.RunPlacement` (`PlacementManager.cs:88`) is an iterator — Unity
coroutines *are* C# iterators. Write out, by hand, the state machine the compiler generates.
Coroutine bugs stop being mysterious once you've done this once.

**☐ Exercise B.** Collapse the two parallel `switch` expressions in `GetPlacementHandler` /
`GetPlacementValidator` (`PlacementManager.cs:112-136`) into one lookup — the
`//TODO change into ValidatorFactory` the file already admits to.

---

## Phase 2 — The BCL that pays rent

### ☐ Week 5 — Ch. 5 *.NET Overview* (SURVEY) + Ch. 6 *.NET Fundamentals* (DRILL)
Strings and text, formatting and culture, and above all **equality and comparison**: `Equals` vs.
`==` vs. `ReferenceEquals`, the `GetHashCode` contract, `IEquatable<T>`, `IComparable<T>`,
`StringComparer`.

**☐ Drills 4–5** from the fundamentals spine. Drill 5 decides part of the exercise for you.

**☐ Exercise — the payoff for Weeks 1–2.** `PolarGridPosition` has `IEquatable<PolarGridPosition>`
**commented out on line 5** while overriding `Equals(object)` (which boxes on every call) and being
compared inside a per-frame linear scan. Implement `IEquatable<T>` properly and remove the boxing
path. Then settle a real design question: `GetHashCode` currently folds in `H`, a **`float`**
(line 36), and `operator ==` compares that float with `==`. Decide whether a float belongs in a hash
key at all, and document the decision.

### ☐ Week 6 — Ch. 7 *Collections* — DRILL
Collection interfaces, `List`/`Dictionary`/`HashSet` internals and complexity, custom equality
comparers, `IReadOnlyList`, and when `ToList()` is a bug.

**☐ Drill 6**, then **the self-check list** from the fundamentals spine. This is the checkpoint: if
questions 1–10 are comfortable, the spine landed and the rest of the path is downhill.

**☐ Exercise — the week the fundamentals visibly pay off.** Kill the O(n) scans, using the hashable
struct from Week 5:
- `PolarGrid.GetPolarNode` (`PolarGrid.cs:184`) — `GridNodes.FirstOrDefault(...)` across *every node
  in the grid*, on a path hit constantly during placement hover.
- `PolarGrid.GetRingAt` (`PolarGrid.cs:166`) — linear scan over rings.
- `StructureDictionary.Get` ×2 (`Data/Dictionaries/StructureDictionary.cs:19-27`).

Build a `Dictionary<PolarGridPosition, PolarNode>` alongside `GridNodes`. **Profile before and after**
and record both numbers below.

> Before: `________ ms` After: `________ ms`

### ☐ Week 7 — Ch. 8 *LINQ Queries* + Ch. 9 *LINQ Operators* — DRILL
**Deferred execution**, streaming vs. buffering operators, query vs. lambda syntax, operator
families, re-enumeration cost.

**☐ Exercise.** `GuidReferenceFinder.CollectFiles()` (`Editor/GuidReferenceFinder.cs:221-232`) is a
genuine five-stage pipeline over `Directory.EnumerateFiles`. Work out exactly when each element is
produced and why the trailing `.ToList()` is load-bearing. Then look at
`EnumerableExtensions.GetRandom` (`Extensions/EnumerableExtensions.cs`): it calls `.Any()` then
indexes by count — harmless on `List<T>`/`T[]`, but a double-enumeration hazard the moment someone
generalises it to `IEnumerable<T>`. Make the constraint explicit.

### ☐ Week 8 — Ch. 12 *Disposal and GC* + Ch. 13 *Diagnostics* — DRILL
`IDisposable`, `using` declarations, finalizers, GC generations, why per-frame allocation matters,
`Stopwatch`, conditional compilation.

**☐ Exercise.** `FindPathJob.Execute` (`_Game/Grid/Pathfinders/FindPathJob.cs:28-152`) allocates four
native containers and disposes them by hand at the bottom — any early return or exception leaks all
four. Restructure with `try`/`finally`. While in there, note the two dead `if (... == -1) { }` empty
branches (lines 141, 178) and the unused third `CalculatePath` overload (line 196).

---

## Phase 3 — Advanced chapters, via city-builder features

These chapters have no footprint in the current codebase. Each week proposes a
**genre-appropriate feature** that forces the material. Build the smallest version that works — the
concept is the goal, not a shipped system.

### ☐ Week 9 — Ch. 14 *Concurrency and Asynchrony* — DRILL *(highest-value week here)*
`Task`, `async`/`await` and its state machine, synchronisation context, `CancellationToken`,
`async void` as a trap, exception propagation across `await`.

There is **not one `async`, `await`, `Task` or `lock` in `Assets/_Scripts`.** That's the single
largest gap between this codebase and professional C#.

> **UniTask is already in the project** — `com.cysharp.unitask` in `Packages/manifest.json`, though
> nothing imports it yet. So play-mode code *is* a legitimate async environment here, not just the
> Editor. Learn plain `Task`/`async` from the book first, then read UniTask's rationale: it is
> allocation-free, runs on Unity's PlayerLoop instead of the thread pool, and avoids the
> `SynchronizationContext` round-trip that makes `Task` awkward in Unity. Knowing *why* it exists is
> the real lesson; the book's chapter is what makes that legible.

**☐ Exercise — make a blocking Editor tool async.** `GuidReferenceFinder.Search()`
(`GuidReferenceFinder.cs:158-219`) synchronously `File.ReadAllText`s every text asset in the project,
freezing the whole Editor behind a `DisplayCancelableProgressBar`. Make it genuinely async and
cancellable, with a real `CancellationToken` rather than a polled bool. Do it twice if you have the
time — once with `Task`, once with UniTask — and compare allocations in the Profiler.

> **Not here:** the async *save system* belongs to Part 2 Lesson 11, which builds it on the spec
> loader. Don't build a save format twice — this week is about async mechanics, not persistence.

### ☐ Week 10 — Ch. 23 *Span&lt;T&gt;/Memory&lt;T&gt;* + Ch. 22 *Parallel Programming* (DRILL), Ch. 21 *Advanced Threading* (SURVEY)
Stack allocation, slicing without copying, `ref struct` rules; `Parallel.For`, PLINQ, and how they
relate to the Jobs system you already use.

**☐ Proposed feature — parallel economy tick.** Per-building production/consumption across thousands
of structures per simulation tick. Write it **twice** — once `Parallel.For`, once `IJobParallelFor` —
and compare. You already have the DOTS reflex from `FindPathJob`; this connects it to plain .NET.

**☐ Span exercise.** `Extensions/RoundMeshGenerator.cs` allocates four managed arrays per ring mesh
(lines 13, 24, 34, 42). Explore `Span<T>`/`stackalloc` and Unity's `NativeArray`↔`Span` interop.

### ☐ Week 11 — Ch. 15 *Streams and I/O* + Ch. 11 *JSON* + Ch. 25 *Regex* (DRILL), Ch. 10 *LINQ to XML* (SURVEY)
Stream composition, `FileStream` options, text encoding; JSON serialisation; regex syntax, groups, and
when regex is the wrong tool.

> **What's actually available here.** `System.Text.Json` and Newtonsoft are **both absent** from this
> project. You have Unity's `JsonUtility` (`com.unity.modules.jsonserialize`) and — already present as
> a dependency of `com.unity.entities` — **`Unity.Serialization.Json`** (`com.unity.serialization`
> 3.1.3, `Packages/packages-lock.json:227`). Read the book's JSON chapter for the concepts, then map
> them onto these two. The decisive difference: `JsonUtility` supports neither dictionaries nor
> polymorphism; `Unity.Serialization.Json` supports both. That single fact determines whether a
> spec/blueprint system is possible — see Part 2 Lesson 6.

**☐ Proposed feature — in-game debug console.** `add wood 1000`, `spawn house 3 45`, `settime 14`.
A real parsing problem: regex for the command grammar, streams for a command-history file, and a
natural home for the reflection work in Part 2. Wire it to `ResourceStore.Add` and
`StructureManager`, both of which already exist.

**☐ Study the model first.** `Editor/Kanban/KanbanStore.cs` already does atomic writes (temp file →
`File.Replace`) with a documented rationale, and never throws on a corrupt board. It's the
best-written file in this repo — read it as the reference for how any file writing here should behave.

> **Not here:** the save format itself is Part 2 Lesson 11. This week is streams, encoding, JSON and
> regex as skills; the persistence *architecture* needs the spec system first.

### ☐ Week 12 — Ch. 17 *Assemblies* + Ch. 18 *Reflection* (DRILL), Ch. 19 *Dynamic* (SURVEY), Ch. 16/20/24 (SURVEY)
Assembly loading and `asmdef` boundaries, `Type`/`MethodInfo`, custom attributes and
attribute-driven discovery, `Activator`.

**☐ Proposed feature — attribute-driven command registry.** Mark console commands with
`[ConsoleCommand("add")]` and discover them by reflection instead of a hand-maintained switch — the
same pattern that would later let mods register structures. Hits assemblies (Unity `asmdef`s *are*
the assembly boundary), reflection and attributes together.

**☐ Optional survey hooks:** save-file integrity checksum (Ch. 20 Cryptography); upload a city to a
share endpoint (Ch. 16 Networking). Ch. 24 (Native/COM interop) is not relevant to you — read the
opening section and move on.

---

## Not drilled, deliberately

Ch. 1, 5, 10, 16, 19, 20, 21, 24. Read once for an index of what exists. None are on the critical
path for Unity/DOTS work.

---

## Verification

Weeks 2–8 change live game code. Verify each, don't just compile.

1. **Compile check** — Editor must be *closed* (it locks the project):
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe" -quit -batchmode -nographics -projectPath . -logFile -
   ```
2. **Play-mode smoke test** in `Assets/Scenes/GameScene.unity` after any grid/placement change:
   grid generates with the expected ring count → select a structure → hover highlights the correct
   footprint → place a building → place a road between two nodes and confirm the path follows →
   people spawn and walk.
3. **Week 2 / 5 / 6 specifically:** `PolarGridPosition` feeds baked ECS data. Reopen
   `Assets/Scenes/GameScene/EntitiesSubScene.unity` and confirm entities still bake and spawn.
4. **Week 6:** capture a Profiler sample of placement hover before and after. The `FirstOrDefault`
   scan should vanish from the frame.
5. **Commit before each week's exercise.** An enum renumber or a struct layout change should be one
   `git checkout` away from undone.

---

## Step 0 — before Week 1

Nothing to buy. Two things worth setting up:

**A scratchpad.** The drills need somewhere throwaway to run. Either a `Test.cs` MonoBehaviour in the
scene (one already exists at `Assets/_Scripts/Test.cs`) or — better — **[LINQPad](https://www.linqpad.net/)**,
which is free, instant, and shows you allocations and IL. LINQPad also sidesteps the C# 9 ceiling
entirely, so it's the right place to try C# 10–12 features the Unity compiler rejects.

**Bookmarks.** The free references at the end of each lesson. The two that carry the most weight:
[Microsoft Learn's C# reference](https://learn.microsoft.com/dotnet/csharp/language-reference/) for
everything, and [Albahari's *Threading in C#*](https://www.albahari.com/threading/) — the same author
as the Nutshell book, published free in full, covering most of what Week 9 needs.

---

## Progress log

| Week | Chapters | Done | Notes / what surprised me |
|---|---|---|---|
| 1 | 2 | ☐ | |
| 2 | 3 | ☐ | |
| 3 | 4 (1/2) | ☐ | |
| 4 | 4 (2/2) | ☐ | |
| 5 | 5, 6 | ☐ | |
| 6 | 7 | ☐ | |
| 7 | 8, 9 | ☐ | |
| 8 | 12, 13 | ☐ | |
| 9 | 14 | ☐ | |
| 10 | 23, 22, 21 | ☐ | |
| 11 | 15, 11, 25, 10 | ☐ | |

| 12 | 17, 18, 19, 16, 20, 24 | ☐ | |
