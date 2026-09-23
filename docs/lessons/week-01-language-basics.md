# Week 1 — Language basics: values, references, and copies

**Part 1, Week 1.** Self-contained: no book needed. Budget ~3–4 h.
Topic map (if you ever do get a C# reference, this is "language basics" / Albahari Ch. 2).

**Goal:** by the end you should be able to look at any line in `PolarGrid.cs` and say, without running
it, whether a copy just happened. That single skill is the foundation for Weeks 2, 5 and 6.

---

## 1. Why this week matters

`PolarGridPosition` is the most important type in this codebase. It's a **struct**, it's compared with
a hand-written `==`, it's used as the lookup key in a linear scan on a hot path, and it's embedded in
DOTS components. Almost everything that's fragile about it comes from one property: **it's a value, so
it gets copied.**

This week is only about copies. Weeks 2 and 5 fix the consequences.

---

## 2. Value types vs. reference types — the actual model

Forget "structs are on the stack" for a moment. The real distinction is **what a variable holds**:

- A **value type** variable holds *the data itself*. `int`, `float`, `bool`, `enum`, any `struct`
  (`Vector3`, `int2`, `PolarGridPosition`).
- A **reference type** variable holds *the address of* data stored elsewhere. `class`, `interface`,
  `delegate`, arrays, `string` (`PolarNode`, `Ring`, `List<T>`, `MonoBehaviour`).

Everything else follows from that one sentence.

```csharp
var a = new PolarGridPosition(0, 1, 90, 0f);
var b = a;        // b gets a COPY of all four fields
b.D = 99;         // mutates b's copy only
// a.D is still 1

var n1 = somePolarNode;
var n2 = n1;      // n2 gets a copy of the ADDRESS
n2.SetBuilding(x);// both n1 and n2 see the change - same object
```

`PolarNode` is a class, so `n1` and `n2` are two names for one object. `PolarGridPosition` is a struct,
so `a` and `b` are two independent sets of four fields.

### Assignment copies the *whole* struct

`PolarGridPosition` is 4 fields — 3 `int` + 1 `float` = **16 bytes**. Every assignment, every argument
pass, every return copies all 16. That's cheap here. It stops being cheap around 32–48 bytes, and it
matters that you know which side of that line you're on.

---

## 3. Where things actually live

The rule most people learn — "value types on the stack, reference types on the heap" — is wrong often
enough to cause bugs. **A value lives wherever its storage lives:**

| Declaration | Where the struct's bytes are |
|---|---|
| Local variable in a method | Stack (usually) |
| **Field of a class** | **Heap — inside that object** |
| Element of an array | Heap — arrays are objects |
| Field of another struct | Wherever the outer struct lives |
| Captured by a lambda | Heap — in the compiler's closure object |
| Local in an iterator or `async` method | Heap — in the state-machine object |
| Inside a `NativeArray<T>` | Unmanaged memory, outside the GC heap |
| Boxed (`object o = myStruct`) | Heap, in a box |

So `PolarNode.PolarGridPosition` — a struct property on a `MonoBehaviour` — is **on the heap**, living
inside the `PolarNode` object. It is not on the stack, and it never was.

> **What actually matters.** "Stack or heap" is an implementation detail the runtime is free to change.
> The two things that genuinely affect your code are **copy semantics** (does assignment duplicate the
> data?) and **lifetime** (who keeps it alive?). Reason about those, not about stack frames.

### Why this isn't pedantry

`FindPathJob` puts `PathNode` structs in a `NativeArray` (`FindPathJob.cs:28`). That memory is
unmanaged, which is exactly *why* Burst can compile the job and why you must `Dispose` it (Week 8).
"It's a struct so it's on the stack" gives you no way to understand that.

---

## 4. The six places a struct gets copied silently

This is the list to internalise. In every case, mutating the result does **not** affect the original.

1. **Assignment** — `var b = a;`
2. **Passing an argument** — `GetWorldFromPolar(position)` receives a copy.
3. **Returning** — `return _position;` hands back a copy.
4. **Reading a property** — this is the sneaky one. `node.PolarGridPosition` invokes a getter that
   *returns* a copy. So `node.PolarGridPosition.D = 5` cannot compile; there's nothing to assign to.
5. **`foreach` over most collections** — the loop variable is a copy, and in C# it's read-only anyway.
6. **Boxing** — `object o = position;` copies the struct into a heap box.

`PolarGrid.GetPolarNode` (`PolarGrid.cs:184`) does
`GridNodes.FirstOrDefault(x => x.PolarGridPosition == polarGridPosition)`. Count the copies per
element: one property read (#4), and `polarGridPosition` was already copied into the lambda's closure.
Across every node in the grid, every frame you hover. Week 6 deletes this; this week is about *seeing*
it.

---

## 5. `ref`, `out`, `in`, `params`

Four ways to change the default copy behaviour.

**`out`** — callee must assign it; caller needn't initialise it. Used all over this repo for the
Try-pattern: `TryGetNodesForBuilding(originNode, shift, out List<PolarNode> nodes)`.

**`ref`** — passes the variable itself. Callee's writes are visible to the caller. No copy.

**`in`** — passes by *read-only* reference. Intended to avoid copying a large struct while promising not
to modify it.

```csharp
void Consume(in PolarGridPosition p) { var d = p.D; }
```

### The `in` trap — and why it matters for Week 2

If the struct is **not** declared `readonly`, the compiler cannot prove that reading a member won't
mutate it. So at every member access through an `in` parameter it makes a **defensive copy** — silently
doing the exact thing `in` was supposed to avoid, plus extra work.

`PolarGridPosition` is currently **not** `readonly`. So `in` on it would be worse than passing by
value. Making it a `readonly struct` in Week 2 is what makes `in` actually free. That's the
Week 1 → Week 2 link: this week explains *why* the next week's refactor pays off.

**`params`** — variable arguments, at the cost of allocating an array per call. Fine in setup code, not
in a per-frame path.

---

## 6. Definite assignment

Locals must be assigned before you read them; the compiler enforces it. Fields are automatically
zero-initialised (`0`, `false`, `null`).

For structs at **C# 9** (this project's ceiling — see the main path doc), a struct constructor must
assign **every** field before returning. `PolarGridPosition`'s constructor assigns all four, so it
complies. Newer C# relaxed this with auto-defaulting; you don't have it, and relying on it would be a
compile error here.

Note also: `new PolarGridPosition()` — or `default` — gives you all-zero fields with no constructor
called. For a struct you **cannot** prevent this, which means "a `PolarGridPosition` that was never
properly initialised" is always a representable state. `FindPathJob.cs:42` does exactly this
(`var pathNode = new PathNode();` then fills fields in). Valid, but worth recognising as a deliberate
choice rather than an accident.

---

## 7. `var`

`var` is purely compile-time inference — the type is fixed and static, identical to writing it out.
It is not `dynamic` and has zero runtime cost. This repo uses it heavily, which is fine.

One place it genuinely misleads: `var x = someStruct;` looks lightweight but is a full copy, and `var`
hides the type so you can't see whether you're dealing with a value or a reference. When the
value/reference distinction is the point of the line, write the type.

---

## 8. Numeric conversion, overflow, and float equality

**Implicit conversions** are allowed when no information can be lost (`int` → `float`, `int` → `long`).
**Explicit casts** are required when it can (`float` → `int` truncates; `long` → `int` wraps).

**Overflow is unchecked by default** in C#: `int.MaxValue + 1` silently becomes `int.MinValue` at
runtime. `checked { }` makes it throw `OverflowException`; `unchecked { }` forces the silent behaviour.
Constant expressions are checked at *compile* time regardless.

Relevant to your grid: `Fi` is an `int` in degrees, and `PolarGrid.TryGetNodesForBuilding` wraps
`Fi >= 360 → 0` manually. That's not an overflow concern at these magnitudes, but it *is* the same
class of thinking — a value with a legal range and a wrap rule that the type system doesn't enforce.

### Floats are the real hazard here

`float` cannot represent most decimals exactly. The consequence that will bite this codebase:

```csharp
var direct = 1.0f;
var accumulated = 0f;
for (var i = 0; i < 10; i++) accumulated += 0.1f;
// accumulated is 1.0000001f, not 1.0f
```

Each `+=` rounds to the nearest representable float, and the errors compound. **`PolarGrid` builds
rings by accumulating radius outward**, so a radius or height arrived at by accumulation will not
necessarily `==` the same number written as a literal.

`PolarGridPosition.H` is a `float`, it's compared with `==` in `operator ==`, and it's fed into
`GetHashCode` via `HashCode.Combine`. That combination is a latent bug, and Week 5 is where you decide
what to do about it.

> A common myth to discard: `0.1f + 0.2f == 0.3f` is **true** in single precision — the rounding
> happens to land on the same float. The famous inequality is a `double` result. Don't test float
> fragility with that example; test it with accumulation, as above.

---

## 9. Drills — predict first, then run

Put these in a scratch MonoBehaviour or a throwaway `Test.cs`. **Write your prediction down before
running.** Answers are in §11 — don't read ahead.

**Drill 1 — copies.**
```csharp
var a = new PolarGridPosition(0, 1, 90, 0f);
var b = a;
b.D = 99;
Debug.Log($"a.D={a.D} b.D={b.D}");
```
Predict both values. Then do the same with two `PolarNode` references and `SetBuilding`, and explain
the difference.

**Drill 2 — where does it live?** `PolarNode` is a class with a `PolarGridPosition` property. Is that
struct's memory on the stack or the heap? Answer before checking.

**Drill 3 — the indexer copy.**
```csharp
var list  = new List<PolarGridPosition> { a };
var array = new PolarGridPosition[] { a };
list[0].D = 5;   // ?
array[0].D = 5;  // ?
```
One of these is a compile error. Predict which, and predict the error message's *reason*.

**Drill 4 — boxing.** Loop 100k times calling `a.Equals((object)b)`, then 100k times calling a
hand-written `Equals(PolarGridPosition other)`. Watch GC allocations in the Profiler. Predict which
allocates and roughly how much.

**Drill 5 — float accumulation.** The snippet in §8. Predict `direct == accumulated`, and predict
whether their `GetHashCode()`s match. Then put a `PolarGridPosition` with `H = direct` into a
`Dictionary` and try to look it up with `H = accumulated`.

**Drill 6 — `in` and defensive copies.** Write `void Consume(in PolarGridPosition p)` that reads
`p.D` in a loop. Then mark `PolarGridPosition` as `readonly struct` and do it again. Predict whether
behaviour changes, and whether *performance* changes.

---

## 10. The exercise — copy-semantics audit

**~80 minutes. Produce notes, change no code.**

Walk every use of `PolarGridPosition` across these files:

- `_Game/Grid/PolarGrid.cs` — conversions, `TryGetNodesForBuilding`, `GetPolarNode`
- `_Game/Grid/PolarNode.cs` — the property and `Initialise`
- `_Game/Managers/StructureManager.cs` — the three buffer appends
- `_Game/Managers/PlacementHandlers/*` — hover and placement paths
- `_Game/DOTS/Components/**` — `StructurePlacementOrder`, `PeopleSpawnOrder`, `WorkplaceLocation`, `PathfindingParams`

For each, record: **(a)** does a copy happen here, and which of the six kinds; **(b)** could a
mutation be silently lost; **(c)** is this on a per-frame path?

**Deliverable:** a list of the copies on the hover/placement path, and an explicit answer to one
question — *is `PolarGridPosition` ever mutated after construction anywhere in the codebase?* If the
answer is no, you've just proved Week 2's `readonly struct` refactor is safe, and you'll start it
knowing rather than hoping.

---

## 11. Answer key

<details>
<summary>Drill 1</summary>

`a.D=1 b.D=99`. `b = a` copied all four fields; the two are independent.

With `PolarNode` (a class), both references point at one object, so a change through either is visible
through both.
</details>

<details>
<summary>Drill 2</summary>

**The heap.** The struct's 16 bytes live *inside* the `PolarNode` object, which is a heap-allocated
class instance. A struct's storage location is determined by its container, not by being a struct.
</details>

<details>
<summary>Drill 3</summary>

`array[0].D = 5;` **compiles.** `list[0].D = 5;` is a **compile error** — CS1612, "Cannot modify the
return value of `List<T>.this[int]` because it is not a variable."

Reason: array indexing produces a *reference* to the element's storage, so you can assign into it.
`List<T>`'s indexer is a **property**, and its getter *returns a copy*. Assigning to a field of that
copy would be silently pointless, so the compiler rejects it. This is C# protecting you from copy #4.
</details>

<details>
<summary>Drill 4</summary>

`Equals((object)b)` allocates — **two boxes per call** (the cast plus, in the default `ValueType`
implementation, boxing internally), roughly 24–32 bytes each on a 64-bit runtime. So ~100k iterations
produce several MB of garbage.

The generic `Equals(PolarGridPosition other)` allocates **nothing**.

That difference is the entire practical argument for `IEquatable<T>` in Week 5 — and note the current
code has `IEquatable<PolarGridPosition>` *commented out* on line 5 while overriding `Equals(object)`.
</details>

<details>
<summary>Drill 5</summary>

`direct == accumulated` is **false**: `accumulated` is `1.0000001f`. Their hash codes **differ** — any
difference in bits gives a different hash.

The dictionary lookup **fails**. The entry is in there, you have a key that is "the same number" by any
human standard, and `TryGetValue` returns false. Nothing errors; you just get a miss.

This is why a `float` in a hash key is a design smell, and why `PolarGridPosition.GetHashCode` folding
in `H` deserves the scrutiny Week 5 gives it.
</details>

<details>
<summary>Drill 6</summary>

Behaviour is identical; **performance is not**. On a non-`readonly` struct, every member access through
an `in` parameter makes a **defensive copy**, because the compiler can't prove the member access won't
mutate. Mark the struct `readonly` and the copies disappear.

So `in` on a mutable struct is *slower* than passing by value — the opposite of the intent. This is the
concrete payoff of Week 2's refactor.
</details>

---

## 12. Free references

No book required, but these are worth bookmarking — all free and authoritative:

- [Microsoft Learn — C# language reference](https://learn.microsoft.com/dotnet/csharp/language-reference/) — the official spec-adjacent docs
- [Value types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-types) · [Boxing and unboxing](https://learn.microsoft.com/dotnet/csharp/programming-guide/types/boxing-and-unboxing) · [Choosing between class and struct](https://learn.microsoft.com/dotnet/standard/design-guidelines/choosing-between-class-and-struct)
- [Eric Lippert — "The stack is an implementation detail"](https://ericlippert.com/2009/04/27/the-stack-is-an-implementation-detail-part-one/) — the definitive correction to the stack/heap myth, from a C# compiler designer
- [Albahari — *Threading in C#*](https://www.albahari.com/threading/) — the same author as the Nutshell book, published **free in full**. This covers most of what Week 9 needs.
- [Unity — Writing high-performance C#](https://docs.unity3d.com/Manual/performance-managed-memory.html) — allocation and GC behaviour specific to Unity

---

**Next:** Week 2 — creating types: `readonly struct`, the `[Flags]` enum bug, and why
`PolarGridPosition` should not become a `record struct`. Ask and I'll write it.
