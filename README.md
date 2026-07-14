# Slice25Image

Slice25Image is a Unity 2022.3+ UGUI package for layouts that need more control
than a standard 9-sliced `Image`.

## Components

- `Slice25Image` keeps the sprite center at its natural size and distributes the
  remaining horizontal and vertical space according to ratios from `0` to `1`.
- `HalfImage` mirrors the sprite horizontally. `Top Y` selects the vertical split
  in source-sprite pixels.
- `HalfSlice15Image` mirrors the source while preserving sliced borders and adds
  a configurable center width in UI units.

All components support `Image.sprite`, `Image.overrideSprite`, sprite atlases,
trimmed sprites, and Canvas pixels-per-unit scaling.

## Installation

Add the repository URL in Unity Package Manager, or add this package as a local
package. Create components from `GameObject > UI` or add them to an existing
`RectTransform`.

The source sprite must use `Mesh Type: Full Rect` when predictable rectangular
slicing is required. Configure sprite borders in the Sprite Editor to enable the
custom sliced mesh; without borders, the components render as a simple image.

## Tests

In Unity Test Runner, enable package tests and run the
`Slice25Image.Tests.Editor` assembly in Edit Mode.
