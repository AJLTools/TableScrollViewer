# Table Scroll Viewer

A recyclable scroll-list extension for Unity uGUI.

`TableScrollViewer` is built on top of `ScrollRect` and only renders the nodes
that are visible on screen, so it can display very long tables smoothly. It is
designed as a navigable list: cursor movement, selection and cancel are driven by
an input hook, so it works with **keyboard, gamepad, mouse and touch** alike.

## Features

- Recyclable viewport rendering — only visible nodes are instantiated.
- Navigable list semantics with cursor + selection + cancel events.
- Input-source agnostic: drive it with **keyboard** or **gamepad** via the
  `OnKeyDown` hook (see [Input](#input)), plus mouse hover / click and touch.
- Vertical / horizontal orientation, Near / Center / Far alignment.
- Per-row custom size (`GetCustomWidth` / `GetCustomHeight`).
- Focus effects, sub-nodes (multi-column rows), automatic scrollbar fade-out.
- Table add / remove / update with `BeginUpdateTable` / `EndUpdateTable`.

## Installation

This package is embedded in the project under
`Packages/com.uindies.tablescrollviewer/`. In a different project, copy this
folder into your `Packages/` directory, or add it via the Package Manager
"Add package from disk..." and select the `package.json`.

## Dependencies

- `com.unity.textmeshpro` (samples) — transitively provides `com.unity.ugui`, whose `UnityEngine.UI` (`ScrollRect`, `Image`, `Scrollbar`) the runtime references.

## Assembly definitions

| Assembly | Location | References |
| --- | --- | --- |
| `Uindies.TableScrollViewer` (Runtime) | `Runtime/` | `UnityEngine.UI` |

The runtime assembly does **not** depend on TextMeshPro; only the samples do.
There is no separate samples assembly — when imported, sample scripts compile
into the project's default `Assembly-CSharp`, which auto-references the runtime
assembly and `Unity.TextMeshPro` (declared as a package dependency).

## Usage

1. Place a `TableScrollViewer` component on a `ScrollRect` (with a `CanvasGroup`).
2. Create a node prefab that derives from `TableNodeElement` (override
   `onEffectFocus`, `onEffectChange`, `onEffectClick`, `GetCustomWidth` /
   `GetCustomHeight` as needed) and assign it to `SourceNode`.
3. Call `Initialize()` then `SetTable(List<object>)`.
4. Subscribe to `OnSelect` / `OnCursorMove` for selection and cursor events, and
   to `OnKeyDown` to feed keyboard / gamepad input (see [Input](#input)).

See the imported sample (`SampleScene.unity`, `SampleScene2.unity`) for complete
working setups.

## Input

`TableScrollViewer` does not read hardware input itself — instead it exposes an
`OnKeyDown` event (`UnityEvent<KeyDownArgs>`) and a set of move flags. Translate
whatever input source you use into one of these flags:

```csharp
public enum eKeyMoveFlag
{
    None, Select, Cancel,
    Up, Down, Left, Right,
    PageUp, PageDown, PageLeft, PageRight,
    ToTop, ToBottom,
}
```

Example — keyboard (Space / arrows):

```csharp
viewer.OnKeyDown.AddListener(args =>
{
    if (Input.GetKeyDown(KeyCode.Space))      args.Flag = TableScrollViewer.eKeyMoveFlag.Select;
    else if (Input.GetKeyDown(KeyCode.UpArrow))    args.Flag = TableScrollViewer.eKeyMoveFlag.Up;
    else if (Input.GetKeyDown(KeyCode.DownArrow))  args.Flag = TableScrollViewer.eKeyMoveFlag.Down;
});
```

Example — gamepad (buttons via the Input System or any gamepad API):

```csharp
viewer.OnKeyDown.AddListener(args =>
{
    if (Gamepad.current != null)
    {
        if (Gamepad.current.buttonSouth.wasPressedThisFrame) args.Flag = TableScrollViewer.eKeyMoveFlag.Select;
        if (Gamepad.current.buttonEast.wasPressedThisFrame)  args.Flag = TableScrollViewer.eKeyMoveFlag.Cancel;
        var stick = Gamepad.current.dpad.ReadValue();
        if (stick.y >  0.5f) args.Flag = TableScrollViewer.eKeyMoveFlag.Up;
        if (stick.y < -0.5f) args.Flag = TableScrollViewer.eKeyMoveFlag.Down;
        if (stick.x < -0.5f) args.Flag = TableScrollViewer.eKeyMoveFlag.Left;
        if (stick.x >  0.5f) args.Flag = TableScrollViewer.eKeyMoveFlag.Right;
    }
});
```

`OnKeyDown` is invoked every frame while the viewer is usable; set `args.Flag`
to anything other than `None` to trigger that move. The viewer then handles
scrolling, focus and selection automatically.

## Samples

Import the sample from **Package Manager > Table Scroll Viewer > Samples**.
It contains:

- `Scenes/` - `SampleScene.unity` (vertical / horizontal) and `SampleScene2.unity`.
- `Scripts/` - `TestScrollviewVertical`, `TestScrollviewHorizontal`, `TestScene2`.
- `ScrollViewNodes/` - node prefabs + scripts (`NodeHorizontal`, `NodeVerticalFreeSize`,
  `NodeHorizontalFreeSize`, `NodeVerticalSubButtons`, `NodeSubButtons`, `NodeScene2`).
- `Textures/` - sample icons / frame.
- `TextMesh Pro/` - TMP Essential Resources + LiberationSans SDF so the sample is self-contained.

## License

MIT - Copyright (c) catsnipe. See [LICENSE.md](LICENSE.md).
