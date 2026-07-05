# Grill: crease editor fixes
Date: 2026-05-30

## Intent
Stabilize the crease pattern editor around confirmed behavior: safe attribute editing, mode switching without losing materials, boundary-default crease creation, one-way fold slider control, fresh editor state accessors, and strict save-file naming.

## Constraints
- Do not change the face generation algorithm yet.
- Do not change move-vertex selection behavior yet.
- Do not hand-edit scene UI layout for angle inputs; code should fail safely and explain required Inspector bindings.
- Slider semantics are one-way: value 0 means the initial/rest angle, value 1 means the maximum fold angle.
- `minAngle` is the initial/rest angle, `maxAngle` is the maximum fold angle, and both are positive values in the 0-180 range.

## Key decisions
- Decision: Add runtime validation around `CreaseAttributePanel` bindings. Reason: missing TMP input references currently cause null references. Alternative considered: editing scene YAML directly.
- Decision: Move crease material cleanup from `OnDisable` to `OnDestroy`. Reason: mode switching disables the editor and should not destroy reusable materials. Alternative considered: rebuilding materials in `OnEnable`.
- Decision: Remove `Flat` from editor behavior and default new creases to `Boundary`. Reason: the simulator data model does not support Flat, and the intended workflow starts with boundaries. Alternative considered: extending the full simulator pipeline with Flat.
- Decision: Keep `restAngle` as `minAngle`. Reason: `minAngle` is now defined as the initial/rest angle. Alternative considered: adding a separate target angle UI.
- Decision: Make the slider start at 0 and map directly to `foldProgress`. Reason: the desired interaction is one-way folding from initial to maximum angle. Alternative considered: midpoint bidirectional folding.
- Decision: Replace `currentVertices/currentCreases` public caches with realtime getters. Reason: cached fields become stale after edits. Alternative considered: syncing caches after every operation.
- Decision: Normalize save names by rejecting unsupported characters. Reason: filenames should be predictable and not silently rewritten. Alternative considered: replacing invalid characters with underscores.

## Open questions
- The UI still needs two TMP input fields bound manually in Unity for min/max angle editing.
- The current face generator remains fragile for complex or intersecting crease graphs.

## Out of scope
- Rewriting `OrigamiFaceGenerator`.
- Changing move-vertex selection behavior.
- Directly editing `SampleScene.unity` to add UI controls.
