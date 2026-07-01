# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

`Plugin.Maui.Health` is a cross-platform .NET MAUI library (published to NuGet) that exposes a single
unified API for reading and writing device health/fitness data: **Apple HealthKit** on iOS and
**Android Health Connect** on Android. There is no Windows/macOS health backend — the library targets
`net10.0-ios` and `net10.0-android` only.

## Build & Test

```bash
# Build the library (CI builds this exact solution, Release config, on windows-latest)
dotnet workload restore src/Plugin.Maui.Health.sln
dotnet build src/Plugin.Maui.Health.sln -c Release

# Build the sample app
dotnet build samples/Plugin.Maui.Health.Sample.sln -c Debug

# Run the sample on a simulator/emulator (deploy + launch)
dotnet build samples/Plugin.Maui.Health.Sample/Plugin.Maui.Health.Sample.csproj -f net10.0-android -c Debug -t:Run -p:AdbTarget="-s <emulator-id>"
dotnet build samples/Plugin.Maui.Health.Sample/Plugin.Maui.Health.Sample.csproj -f net10.0-ios     -c Debug -t:Run -p:_DeviceName=:v2:udid=<simulator-udid>
```

There is **no test project**. Validation is done by running the sample app.

- **iOS**: HealthKit **is** available on modern iOS Simulators (the earlier "not available on Simulator"
  belief is outdated) — the sample seeds and reads real data there. Note `-t:Run` streams its output
  through `tail`, so it only prints at the end; and `Console.WriteLine` from managed code does **not**
  surface via `simctl launch --console-pty` or the unified log — use an on-screen bound label to debug.
  If iOS builds hang for minutes producing no output, kill stray `MSBuild.dll …*.proj` nodes and run
  `dotnet build-server shutdown` (Rider's MSBuild server contends with the CLI).
- **Android**: Health Connect must be present (pre-installed on Android 14+; Play Store on 11–13).
  Health permissions can be granted headlessly with `adb shell pm grant <pkg> android.permission.health.<PERM>`.

### CI & release (semantic-release)

`.github/workflows/ci.yml` builds on push and runs **semantic-release** (`.releaserc`); versioning is
driven by **Conventional Commits** (`feat:`→minor, `fix:`→patch, `BREAKING CHANGE:`→major, `build:`/`ci:`/
`docs:`→no release). Tagging (`vX.Y.Z`) triggers `release-nuget.yml` which packs & pushes to NuGet.
Requires repo secrets `GH_TOKEN` (PAT) + `NUGET_API_KEY` and a GitHub environment named `Release`.
Commit messages end with the `Co-Authored-By` trailer; commit only when asked, and never push without
asking (a push publishes a real NuGet version).

## Architecture

### Partial-class multi-targeting

The public surface is one type, `HealthDataProviderImplementation`, declared as `partial class ... : IHealth`
with a **per-platform file** holding the entire implementation:

- `IHealth.cs` — the contract; read this first, it carries the authoritative XML-doc semantics for every method.
- `Health.shared.cs` — `HealthDataProvider.Default` (lazy singleton, the DI registration target).
- `Health.android.cs` — Health Connect implementation (~1700 lines).
- `Health.macios.cs` — HealthKit implementation (~1150 lines).

The `.android.cs` / `.macios.cs` / `.ios.cs` suffix convention is wired into `Plugin.Maui.Health.csproj`
via `<Compile Remove>` item groups — files are included/excluded by target framework based on filename.
There is **no `Health.shared.cs` partial of the implementation class**; each platform file is a complete
`IHealth` implementation. When adding a method, add it to `IHealth.cs` **and both** platform files.

### Consumption model (DI)

Consumers register `builder.Services.AddSingleton(HealthDataProvider.Default);` and inject `IHealth`.
See `samples/.../MauiProgram.cs:22`.

### The HealthParameter → platform mapping pattern

`Enums/HealthParameter.cs` is the cross-platform vocabulary (~100 parameters spanning body, vitals,
activity, nutrition/vitamins, mobility). Each platform file maps these enum values to native types via
large `switch` expressions — this is the core of the codebase and where most edits happen:

- **Android** (`Health.android.cs`): `GetPermissions(param)` maps to `android.permission.health.*`
  Read/Write permission strings; further switches map parameters to Health Connect record types and
  `ExtractNutritionField` / unit conversions. Many iOS-only parameters are **unsupported** on Android and
  throw `HealthException`.
- **iOS** (`Health.macios.cs`): switches map parameters to `HKQuantityType`/`HKObjectType` and `HKUnit`.

When a parameter isn't supported on a platform, the convention is to throw `HealthException`
(`Exceptions/HealthException.cs`). The full support matrix lives in `README.md` — keep it in sync when
adding parameters.

### Permissions / consent — platform asymmetry (important)

Two families of methods, with deliberately different semantics:

- **`RequestPermissionAsync` / `RequestPermissionsAsync` / `RequestWorkoutPermissionAsync`** — the
  cross-platform way to obtain consent. iOS presents the HealthKit sheet; Android short-circuits if
  already granted, otherwise launches the Health Connect consent UI **internally** by registering against
  `Platform.CurrentActivity`'s `ActivityResultRegistry` (the non-lifecycle `Register(key, contract,
  callback)` overload, valid at call time) on the main thread, then re-checks. Consumers do **not**
  register their own launcher or re-implement the permission-string mapping — that was the old pattern.
  **Prefer the batched `RequestPermissionsAsync(requests)`**: requesting permissions one-by-one shows a
  string of prompts and, on iOS, presenting multiple `RequestAuthorizationToShareAsync` sheets in quick
  succession can hang the app. The batched call maps to a single HealthKit sheet / single Health Connect
  launch. Parameters unsupported on a platform are skipped, not failed.
- **`CheckPermissionAsync` / `CheckWorkoutPermissionAsync`** — query current grant state.
  - **iOS**: actually calls `RequestAuthorizationToShareAsync` (so it can prompt). HealthKit never
    reveals read-grant status, so read checks return `true` after the sheet is shown; only **write**
    denials are detectable.
  - **Android**: **only checks already-granted permissions — never launches the consent UI.** Returns
    `false` (and read/write methods throw `HealthException`) for parameters Health Connect doesn't
    support.

Regardless of how consent is requested, the consuming app must declare a privacy-policy / rationale
entry point in its manifest. This is **two-part and both parts are required**: an
`androidx.health.ACTION_SHOW_PERMISSIONS_RATIONALE` activity (Android ≤13) **and** a
`ViewPermissionUsageActivity` `<activity-alias>` guarded by `START_VIEW_PERMISSION_USAGE` with the
`VIEW_PERMISSION_USAGE` action + `HEALTH_PERMISSIONS` category (Android 14+). On Android 14+ the system
will not present the permission request without the alias, and the consent UI hangs with no result. See
`samples/.../Platforms/Android/PermissionsRationaleActivity.cs` and `AndroidManifest.xml`. The Android
request plumbing lives in `Health.android.cs` (`RequestFromHealthConnectAsync` + `PermissionResultCallback`).

### Android Kotlin interop (hang-prone — read before touching)

Health Connect is a Kotlin/coroutine API. `Health.android.cs` bridges it with `KotlinContinuation`
(implements `IContinuation` to await Kotlin `suspend` functions from C#) and the `HealthConnectExtensions`
helpers (`ToInstant` / `ToDateTimeUtc`). Failures surface as a Kotlin `Result.Failure` inner class.

**`InvokeCoroutine` must honor `COROUTINE_SUSPENDED`.** A Kotlin `suspend` function returns the
`COROUTINE_SUSPENDED` sentinel only when it actually suspends (result then arrives via the continuation);
if it completes **synchronously** it returns the result directly and never calls the continuation. The
helper checks the sentinel and returns the inline result otherwise — without this, `ReadRecords` hangs
forever whenever it completes synchronously while `Aggregate` (ReadCount) happened to suspend and work.

**`GetKClass<T>` reference ownership.** `JNIEnv.FindClass` returns a runtime-cached *global* ref; wrap it
with `JniHandleOwnership.DoNotTransfer` (not `TransferLocalRef`) and don't dispose it — otherwise a
`DeleteLocalRef` on a global ref aborts the process under CheckJNI on Android 14+.

### iOS HealthKit gotchas

- **Always complete the `tcs` inside completion handlers.** An exception thrown in an `HKSampleQuery` /
  `SaveObject` callback (e.g. `error.LocalizedDescription` when `error` is null, or `Cast<>` on an
  unexpected sample type) is swallowed by HealthKit and leaves the awaiting caller hanging forever. Every
  handler wraps its body in try/catch → `tcs.TrySetException`, uses null-safe error messages, and prefers
  `OfType<>` over `Cast<>`.
- **Entitlement + privacy manifest.** HealthKit is dropped silently ("no entitlements") unless the app
  wires `<CodesignEntitlements>` (see the sample `.csproj`); a `PrivacyInfo.xcprivacy` declares health-data
  usage. The Simulator applies entitlements ad-hoc (no provisioning profile needed).

### Error handling

Every `IHealth` method surfaces failures as `HealthException` — never a raw platform exception, JNI abort,
or silent hang. Native errors are translated via `HealthException.Wrap(ex, "Android"|"iOS")`
(`Exceptions/HealthException.cs`), which uses `[CallerMemberName]` to capture the operation, keeps the
native error as `InnerException`, and carries `Operation`/`Platform`/`Parameter`. `IsSupported` never
throws (safe to gate on); `GetClient()` on Android throws a clear "Health Connect not available" message.

### Units

Always pass a unit string from `Constants/Units.cs` alongside a `HealthParameter`. Read/write methods take
the unit explicitly and convert; mismatched units produce wrong values silently.

### Sample app

A card-based dashboard (light theme) that showcases the plugin with hand-drawn **MAUI.Graphics** charts —
`Controls/{LineChartView,BarChartView,RingChartView}.cs` (each a `GraphicsView` + `IDrawable`; reassign the
bound collection to redraw). Theme lives in `Resources/Styles/{Colors,Theme}.xaml`.

Each screen's VM `InitializeAsync` does a real **write-then-read** round-trip: request core permissions
once (batched via `BaseViewModel.EnsureCorePermissionsAsync`), seed representative records (guarded by
`Preferences`; steps are keyed per-day so "today" is always populated), then read them back so charts show
genuine device data. It falls back to in-memory demo data when unsupported/denied. Gotchas baked in:
`InitializeAsync` is guarded to run once (OnAppearing fires repeatedly), the seed/load is wrapped in a
timeout so a flaky platform callback can't spin forever, and `ActivityIndicator` binds **both**
`IsRunning` and `IsVisible` to `IsBusy` (on iOS a stopped indicator otherwise stays visible → looks like a
permanent spinner). Steps daily totals use `ReadAllAsync` summed per day (robust) rather than the
`ReadCountAsync` aggregate (returns 0 for freshly-written records on the Android emulator).

## Conventions & gotchas

- Minimum OS versions live in `Directory.Build.props`: iOS `SupportedOSPlatformVersion` floor is 15.0
  (HealthKit requirement); Android `AndroidMinSdkVersion` is 28 (Health Connect is available from Android 9
  — Play Store app on API 28–33, system component on 34+). Gate all calls behind `IsSupported`.
- Android `ReadAllAsync` returns **at most 1,000 records per call** — split long ranges for high-frequency
  sensors (heart rate, speed).
- Package versions are managed centrally via `Directory.Packages.props` (CPM); shared MSBuild
  settings and per-platform minimum OS versions live in `Directory.Build.props` at the repo root.
  Individual `.csproj` files reference packages without a `Version` attribute.
- The Android build needs a pinned `Xamarin.AndroidX.Lifecycle.LiveData.Core` (2.11.0.1) to resolve an
  NU1107 conflict in the transitive AndroidX Lifecycle graph (Health Connect + MAUI) — referenced in
  the sample `.csproj`, version set in `Directory.Packages.props`.
- All async methods accept a trailing `CancellationToken cancellationToken = default`; preserve this signature shape.
- The README is the user-facing documentation and the NuGet `PackageReadmeFile`; update its support matrix
  and setup steps when changing platform capabilities.
