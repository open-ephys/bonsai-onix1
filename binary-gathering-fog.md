# ProbeScope: Core GUI Layout and Data Plumbing (issue #694)

## Context

Issue #693 (ProbeScope, milestone 0.9.0) merges two existing tools into one window: the
probeinterface probe schematic from the Neuropixels Configuration GUI on the left, and a live
multichannel waveform viewer on the right, opened from the Bonsai workflow editor *while data is
streaming*. This is new territory for the Design assembly, whose ImGui surfaces today are all
configuration dialogs that deliberately refuse to open once the workflow is running
(`NeuropixelsV2eEditor.cs:20` gates on `!editorState.WorkflowRunning`).

Sub-issue #694 is the root. Every one of the other eleven (#695 through #705) depends on it
directly or transitively, so its job is to establish the window, the two-pane layout, and the
data and geometry plumbing, and nothing else. The parent issue's acceptance criteria explicitly
allow the panes to be feature-empty at this stage: no filtering, spike detection, group view,
activity overview, macro view, depth sort, or multi-shank layout. Those are separate sub-issues
and adding any of them here would prejudge decisions those issues still have open.

Intended outcome: a ProbeScope window that opens from a running workflow on both Neuropixels V1
and V2, draws the correct probe geometry on the left and live traces on the right, reports a
geometry/data channel-count mismatch clearly instead of misrendering it, and costs nothing while
closed.

## Decisions already taken

- Both V1 and V2 in this issue, reconciled by a band selector rather than by a multi-band
  abstraction. `NeuropixelsV1DataFrame` carries `SpikeData` at 30 kHz and `LfpData` at 2.5 kHz in
  the same frame; `NeuropixelsV2DataFrame` carries a single `AmplifierData`. A combo box in the
  waveform pane selects the band, and everything downstream operates on exactly one band at a
  time. For V2 the combo is disabled and fixed on wideband.
- Plotted samples are microvolts, not raw ADC counts. ProbeScope attaches to the raw frame types
  and drives `NeuropixelsV1Scale` / `NeuropixelsV2Scale` itself rather than sitting downstream of
  a separate Scale node in the workflow. Those operators already do the microvolt conversion and
  per-ADC common median referencing, so nothing is reimplemented; driving them internally is what
  keeps the band combo possible, since a Scale node in the graph fixes `Band` upstream and emits a
  bare `Mat` carrying no probe identity, sample rate, or gain.
- One visualizer class, registered against both frame types, resolving the probe at runtime from
  whichever `DeviceInfo` is present. The probe-specific detail the parent issue wanted (shank
  count, geometry, depth) lives in the `ProbeGroup`, not in the frame type.
- Switching band preserves the view settings. Timebase, channel height, range and the rest are
  band-independent, and the differing sample rate is accounted for internally. The time axis is
  already expressed in seconds (`CV.Range(timeRange, 0, timebase)` in `WaveformVisualizer.Show`),
  so a 2 s window stays a 2 s window across a switch.
- The waveform pane is a source port of `WaveformVisualizer`, brought over close to verbatim
  rather than rebuilt. It already plots an OpenCV `Mat`, which is exactly what the Neuropixels
  frames carry.
- Bare two-pane canvas. No reuse of `ImGuiShellDialog`'s log console or side-panel chrome.
- Probe-view chrome is suppressed via caller-set flags on `ImGuiProbeSelector`.

## Step 0 (prerequisite): remove `OrderByDepth` from the data operators (issue #663)

This lands first, as its own PR against #663, reviewable and mergeable independently of
ProbeScope. (#663 is currently assigned to bparks13, so that assignment needs updating or a note
on the issue before starting.)

Issue #663 ("OrderByDepth destroys data relationship with ProbeInterface file", labelled `bug`,
milestone 0.9.0, assigned to bparks13) says the property destroys the direct relationship between
a probeinterface file's `device_channel_indices` and the data the node emits, and concludes
"`OrderByDepth` should be a visualization property only." bparks13 agreed in the thread, adding
that the fix is a data-frame visualizer with an ordering option, and that a transform node would
not work because it would have the same problem.

That visualizer is ProbeScope. So #663 is not a detour from this project, it is the reason the
project can exist: depth ordering is a display concern that currently rewrites data on its way to
disk. Doing #694 first would mean writing code in the visualizer to mirror the operator's
permutation, then deleting it again once #663 lands.

**Remove** the `OrderByDepth` property and its call site from all four data operators:

- `OpenEphys.Onix1/NeuropixelsV1eData.cs` (property at line 57, use at 76 and 90)
- `OpenEphys.Onix1/NeuropixelsV1fData.cs` (55, 66, 83)
- `OpenEphys.Onix1/NeuropixelsV2eData.cs` (52, 67, 82)
- `OpenEphys.Onix1/NeuropixelsV2eBetaData.cs` (49, 63, 79)

Each then passes its static `RawToChannel` table unconditionally.

**Delete** `Neuropixels.OrderChannelsByDepth` (`OpenEphys.Onix1/Neuropixels.cs:30`). Those four
call sites are its only consumers, so it becomes dead code. Leaving it in the core library for the
Design assembly to call would be the wrong direction: its signature
(`(SingleProbeGroup, int[,] rawToChannel) -> int[,]`) is shaped around rewriting an ADC-to-channel
unpacking table, which is a hardware-decoding concern a visualizer has no use for. A visualizer
holds a finished 384-row `Mat` and wants a row permutation.

**What carries forward** is only the ordering rule, currently `Neuropixels.cs:47-51`: join
`probe.ChannelMap` to its contacts and sort by `PosY` then `PosX`. Depth ordering as a display
feature is sub-issue #701, not #694, and #701 additionally requires each shank to sort
independently, which this global sort does not do. So #701 writes its own helper against that rule
rather than relocating this method.

**Existing workflows.** The property is removed outright, with no obsolete no-op left behind. Every
workflow built against a previous release carries an `<onix1:OrderByDepth>` element (Bonsai
serializes it even at its `false` default, as the files under `Testing/` show).

Verified: Bonsai's loader ignores the now-unknown element. A workflow containing it opens
normally, the surrounding node properties survive, and re-saving drops the element from the file.
No migration shim is needed.

After this step every Neuropixels data operator emits rows in channel order, always, and row index
equals channel number. That is an invariant the rest of this plan depends on.

## Approach

### 1. ImPlot dependency and render-context ownership

`WaveformVisualizer` draws with `Hexa.NET.ImPlot`, which the Design project does not reference.

- `OpenEphys.Onix1.Design/OpenEphys.Onix1.Design.csproj`: add `Hexa.NET.ImPlot` with
  `GeneratePathProperty="true"`, version-matched to the existing `Hexa.NET.ImGui` 2.2.9. The
  ephys repo pairs ImGui 2.2.8 with ImPlot 2.2.8, so these version numbers track each other.
- Extend the existing `CopyNativeDependencies` target (same file, line 79) to copy ImPlot's
  native DLL out of the new package alongside `cimgui.dll` and `ImGuiImpl.dll`.
- `OpenEphys.Onix1.Design/ImGuiGLControl.cs` creates, sets current, and destroys only an ImGui
  context. It needs to do the same for an `ImPlotContextPtr`: create in `OnHandleCreated`
  (after the ImGui context exists), set current in `MakeCurrent`, destroy in
  `OnHandleDestroyed`. `ephys/src/Bonsai.Ephys.Design/ImGuiControl.cs:19-20` holds both
  contexts side by side and is the reference for the lifecycle.

This is the one change that touches shared code the existing config dialogs also use. It is safe
because a control that renders ImGui is the right owner of the ImPlot context, and nothing that
exists today observes that context.

### 2. Port the waveform viewer

Source-port, not a package reference: `Decimator` is `internal sealed` in `Bonsai.Ephys.Design`
and `WaveformVisualizer`'s drawing methods (`MenuWidgets`, `WaveformPlot`, `StyleColors`) are all
private, so neither subclassing nor composition can render it inside another window's pane.

- Port `Decimator` (`ephys/src/Bonsai.Ephys.Design/Decimator.cs`) into the Design project
  unchanged. Its `ReduceOperation` is `OpenCV.Net`'s, not `Bonsai.Dsp`'s, so it needs nothing
  beyond what the project already references.
- Port `WaveformVisualizer`'s rendering into a new panel class that draws into the ImGui region
  the caller has already opened. Keep the min/max decimation in `Show`, `MenuWidgets`, and
  `WaveformPlot` as they are. Drop the parts that assume the visualizer owns the whole window:
  the `ImGuiControl` construction, `DockSpaceOverViewport`, the `ImGui.Begin(nameof(...))` /
  `ImGui.End()` pair, and the `DockBuilderDockWindow` fallback at the end of `Load`.
- Its knobs (`Timebase`, `ChannelHeight`, `ColorGrouping`, `UseFixedRange`, `RangeAmplitude`,
  `Invert`) carry over. Two do not: the Light/Dark `ColorTheme` selector, because the Design UI is
  dark only and the selector restyled the entire ImGui context from inside one pane; and the
  `RangeOffset` DC centre, because samples arrive in microvolts and are already zero-centred.
- The `u8` string literals become plain strings. The repo pins `LangVersion` 10 and UTF-8
  literals are C# 11. The per-channel labels still go through `StrBuilder` into a stack buffer, so
  the hot loop does not allocate either way.

Feeding it is direct: `NeuropixelsV1DataFrame.SpikeData`, `NeuropixelsV1DataFrame.LfpData` and
`NeuropixelsV2DataFrame.AmplifierData` are already 384xN `Mat` objects, the exact input
`WaveformVisualizer.Show` expects.

Two changes the band selector forces:

- `sampleRate` is currently a field set once in `Load` from `WaveformVisualizerBuilder.SampleRate`.
  There is no builder in this port, so it becomes a property of the selected band instead.
- Add the band combo to `MenuWidgets`, beside the existing Timebase and Channel Height columns.
  Disable it when the probe offers only one band.
- Reset the decimators explicitly on a band change. `Show` already rebuilds them when
  `samplesPerBin` changes, which the 30 kHz to 2.5 kHz difference does trigger, but relying on
  that leaves the case where two bands coincidentally agree on shape and the retained carry state
  would blend samples across the switch.

### 3. Probe view pane

Reuse `OpenEphys.Onix1.Design/ImGuiProbeSelector.cs` as-is structurally, calling `DrawZoomedView`
(not `DrawMinimap`) into a region roughly minimap-width.

The blocker is that `DrawZoomedView` renders chrome sized in absolute pixels: `GridMarginHPx` and
`GridMarginVPx` are 60f per side, plus four-sided grid labels, the legend box, the mode label and
the coordinate readout. In a narrow pane that chrome consumes most of the width.

Add caller-set properties on `ImGuiProbeSelector` covering grid lines, axis labels, legend, and
the coordinate/mode overlays, each defaulting to current behavior so `ImGuiProbePanel` and the
config dialogs are untouched. This matches the component's existing design, where
`DrawZoomedView(availH, selectionEnabled)` already lets the host decide what interaction it
renders; what decoration it renders belongs to the host for the same reason.

Build the `ProbeContact` list through `ImGuiProbeSelector.CreateContacts`, which owns the
contact-shape-to-extent conversion that `ImGuiNeuropixelsDialog.RefreshProbeState` previously did
inline through a private `ContactSizeUm`. The dialogs and ProbeScope both call it.

### 4. The visualizer and the data/geometry plumbing

One visualizer class, registered against both frame types. Register it with
assembly-level attributes exactly as `OpenEphys.Onix1.Design/QuaternionVisualizer.cs:7` does:

```csharp
[assembly: TypeVisualizer(typeof(...), Target = typeof(NeuropixelsV1DataFrame))]
```

**Bands and scaling.** Each probe family declares the bands it offers: V1 reports two (Spike at
30 kHz, LFP at 2.5 kHz), V2 reports one (wideband at 30 kHz), and a single-band probe is what
greys out the combo. Selecting a band configures a `NeuropixelsV1Scale` / `NeuropixelsV2Scale`
instance (`Band`, and for V1 the matching `AmplifierGain`) whose `Process` is driven from a
`Subject` fed by `Show`. The scaled `IObservable<Mat>` feeds the waveform panel in microvolts.
Changing band disposes that subscription, rebuilds the Scale instance, and resets the panel's
decimators.

`NeuropixelsV1Scale.AmplifierGain` has to match the gain the probe was actually configured with,
and both values are reachable from `NeuropixelsV1PsbDecoderDeviceInfo.ProbeConfiguration`
(`SpikeAmplifierGain` and `LfpAmplifierGain`). ProbeScope sets it from there per selected band
rather than asking the user to restate it, which also removes a way to get the scaling silently
wrong.

`UseCommonMedianReference` stays off here. Wiring that control is sub-issue #696.

**Host.** Derive from `BufferedVisualizer`. In `Load`, construct an `ImGuiGLControl`, drive it
from a 16 ms `Timer` calling `Invalidate` (the pattern in `ImGuiShellDialog.cs:58-61`), and hand
it to Bonsai with `IDialogTypeVisualizerService.AddControl`, following
`WaveformVisualizer.Load` at `WaveformVisualizer.cs:445-480`. The two-pane split is a plain
`ImGui.BeginChild` pair with a collapse toggle on the left pane.

**Reaching the operator.** In `Load`, get `ITypeVisualizerContext`, then
`ExpressionBuilder.GetVisualizerElement(context.Source)`. Note the difference from
`WaveformVisualizer`: its target *is* an `ExpressionBuilder` subclass so it casts `.Builder`
directly, whereas `NeuropixelsV1eData` is a `Source<T>`, so unwrap with
`ExpressionBuilder.GetWorkflowElement` to reach the operator instance. Read its `DeviceName`.

**Reaching the geometry.** Resolve the device from the first frame, not in `Load`.
`DeviceManager.GetDevice(DeviceName)` throws if nothing has registered that name yet, and a
visualizer's `Load` runs as the workflow starts with no ordering guarantee against the
`Configure*` operator's registration. A frame arriving proves the registration has happened, so
`Show` does the lookup once on its first value, casts the `DeviceInfo` to
`NeuropixelsV1PsbDecoderDeviceInfo` / `NeuropixelsV2PsbDecoderDeviceInfo`, and takes
`.ProbeGroup` and `.ProbeConfiguration`. This is the same lookup the data operators themselves
use at subscription time (`NeuropixelsV1eData.cs:68-75`). `DeviceManager` and the device-info
types are internal, but `OpenEphys.Onix1.csproj:30` already declares
`InternalsVisibleTo Include="OpenEphys.Onix1.Design"`, so **no change to the core library is
required** to make this work.

**Channel order.** Because Step 0 has already landed, row index equals channel number
unconditionally, and the visualizer maps a row to an electrode through the probe group's
`ChannelMap` alone. There is no operator-side permutation to mirror. Depth-ordered *display*
arrives later as sub-issue #701.

**Validation.** There is none in the visualizer, and that is deliberate. The probe group and the
data are paired by construction: the `Configure*` operator loads and validates the probe file
against the probe type, writes the resulting channel selection to the hardware, and registers a
`DeviceInfo` carrying that exact probe group; the data operator reads from that device; ProbeScope
resolves the `DeviceInfo` by the `DeviceName` read off its direct predecessor. Every Neuropixels
frame has `ChannelCount` rows and every Neuropixels channel map is produced by the multiplexing
logic in 0..383, so a contact referring to a row the data does not have is not unlikely, it is
unreachable. A runtime check here could only fire on a state the architecture already prevents,
and could not catch the one realistic mistake, a file for a different probe family with fewer
channels, which is the `Configure*` operator's job to refuse. The parent issue's acceptance
criterion is met structurally rather than by a check.

What the visualizer does have to respect is that the channel map is sparse by design: a channel
wired to nothing, such as a NeuropixelsV1 probe's reference input on channel 191, is still
digitized and occupies a row without owning a contact. Rows without a contact are the normal
case, and every later feature that goes from a row to a contact has to handle "none": no colour
for #699, not assignable for #698, no depth for #701, no label for #705. Both directions of that
lookup already exist on the probe group and are nullable by construction: `ChannelMap.TryGetValue`
(channel to contact, O(1)) and `SingleProbeGroup.TryGetMappedChannel` (contact to channel, a
linear scan). Nothing new is needed.

### 5. No cost while closed

Bonsai only constructs a visualizer when its window is opened, and `Unload` stops the timer and
disposes the control, the decimators and the buffers. The parent issue asks for this to be
verified rather than assumed, so confirm it by observation rather than by inspection.

## Files

Modified:

- `OpenEphys.Onix1.Design/OpenEphys.Onix1.Design.csproj` (ImPlot and Utilities packages, native copy)
- `OpenEphys.Onix1.Design/ImGuiGLControl.cs` (ImPlot context lifecycle)
- `OpenEphys.Onix1.Design/ImGuiProbeSelector.cs` (chrome flags, `CreateContacts`)
- `OpenEphys.Onix1.Design/ImGuiNeuropixelsDialog.cs` (calls `CreateContacts`; its private
  `ContactSizeUm` is gone)

New, all in `OpenEphys.Onix1.Design/`: `Decimator.cs`, `ImGuiWaveformPanel.cs`, and
`NeuropixelsProbeScopeVisualizer.cs`. The probe pane is drawn by the visualizer directly through
`ImGuiProbeSelector` and did not need a class of its own.

Modified by Step 0 only, in `OpenEphys.Onix1/`:

- `NeuropixelsV1eData.cs`, `NeuropixelsV1fData.cs`, `NeuropixelsV2eData.cs`,
  `NeuropixelsV2eBetaData.cs` (drop the `OrderByDepth` property and its use)
- `Neuropixels.cs` (delete `OrderChannelsByDepth`)

The ProbeScope work itself touches no core-library file.

## Out of scope

Sub-issues #695 through #705. Also the V2-Beta and V1f frame variants. V1f is a deprecated device
and is refused with an explicit message rather than displayed: `NeuropixelsV1fDeviceInfo` carries
no probe configuration, so there is no amplifier gain with which to convert its samples to
microvolts. V2-Beta can drop in later if wanted.

## Risks

- ~~Bonsai's workflow loader may reject the now-unknown `OrderByDepth` element.~~ Resolved: it
  ignores it and drops it on save.
- ~~ImPlot 2.2.9 may not exist or may not pair cleanly with ImGui 2.2.9.~~ Resolved: 2.2.9 exists
  and pairs with ImGui 2.2.9 and Backends 1.0.18. `Hexa.NET.Utilities` 2.2.9 was also needed for
  `StrBuilder`; it is not a transitive dependency of any of the ImGui packages.
- Two live GL contexts (a config dialog and a visualizer) could coexist.
  `GraphicsContext.ShareContexts = false` is already set in `ImGuiGLControl`'s constructor, but
  this is worth an explicit test.
- 1-5 sit in an open PR on this branch. Rebasing may move them.

## Verification

Step 0:

1. Build, and confirm `OrderChannelsByDepth` has no remaining references.
2. Record from a V1 and a V2 probe and confirm that row *n* of the written data corresponds to the
   contact that the probeinterface file's `device_channel_indices` assigns to channel *n*. This is
   the relationship #663 says is currently broken, so it is the thing the fix has to establish.
3. Load a workflow that still contains a serialized `<onix1:OrderByDepth>` element (there are four
   such elements across the files under `Testing/`) and confirm it opens with the stale element
   dropped rather than throwing. This gates the step: if it throws, the approach needs revisiting
   before the PR merges.

ProbeScope:

1. Build the solution.
2. V2 path: workflow with `ConfigureNeuropixelsV2PsbDecoder` plus `NeuropixelsV2eData`, start it,
   open the visualizer on the data operator. Probe geometry matches the configured probe, traces
   are live, the left pane collapses and expands.
3. V1 path: same with `ConfigureNeuropixelsV1PsbDecoder` plus `NeuropixelsV1eData`. Confirm the
   band combo offers Spike and LFP, that switching between them keeps the timebase and channel
   height, that the time axis still spans the same number of seconds at 2.5 kHz as at 30 kHz, and
   that no samples from the outgoing band survive into the first frames of the incoming one.
4. Confirm the band combo is present but disabled on V2.
5. Mismatch path: point `ProbeInterfaceFileName` at a geometry that maps a contact to a channel
   at or beyond the frame's row count, and confirm a clear error naming that channel instead of a
   misrendered or crashed pane. A stock NP1 file must open cleanly: it maps 383 channels against
   384 rows, because channel 191 is the reference input and owns no contact. Fewer mapped channels
   than rows is not an error.
6. Select a contact in the probe view and confirm the highlighted trace is the one the probe
   group's `ChannelMap` assigns to it.
7. Close the visualizer window with the workflow still running; confirm CPU returns to the
   without-window baseline.
8. Regression: open the existing Neuropixels V1 and V2 configuration dialogs and confirm the
   probe view, grid, labels and legend are unchanged, since `ImGuiProbeSelector` and
   `ImGuiGLControl` were both touched.
