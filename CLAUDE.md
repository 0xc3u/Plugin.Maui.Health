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
```

There is **no test project** in this repo. Validation is done by running the sample app on a physical
device. HealthKit is **not available in the iOS Simulator** (all calls throw); use a real iPhone/iPad.
On Android, Health Connect must be installed (pre-installed on Android 14+; Play Store install on 11–13).

## Architecture

### Partial-class multi-targeting

The public surface is one type, `HealthDataProviderImplementation`, declared as `partial class ... : IHealth`
with a **per-platform file** holding the entire implementation:

- `IHealth.cs` — the contract; read this first, it carries the authoritative XML-doc semantics for every method.
- `Health.shared.cs` — `HealthDataProvider.Default` (lazy singleton, the DI registration target).
- `Health.android.cs` — Health Connect implementation (~1450 lines).
- `Health.macios.cs` — HealthKit implementation (~1000 lines).

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

- **`RequestPermissionAsync` / `RequestWorkoutPermissionAsync`** — the cross-platform way to obtain
  consent. iOS presents the HealthKit sheet; Android short-circuits if already granted, otherwise
  launches the Health Connect consent UI **internally** by registering against
  `Platform.CurrentActivity`'s `ActivityResultRegistry` (the non-lifecycle `Register(key, contract,
  callback)` overload, valid at call time) on the main thread, then re-checks. Consumers do **not**
  register their own launcher or re-implement the permission-string mapping — that was the old pattern.
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

### Android Kotlin interop

Health Connect is a Kotlin/coroutine API. `Health.android.cs` bridges it with `KotlinContinuation`
(implements `IContinuation` to await Kotlin `suspend` functions from C#) and the `HealthConnectExtensions`
helpers (`ToInstant` / `ToDateTimeUtc` for `DateTimeOffset` ↔ `Java.Time.Instant`). Failures surface as a
Kotlin `Result.Failure` inner class — handle that path when adding new Health Connect calls.

### Units

Always pass a unit string from `Constants/Units.cs` alongside a `HealthParameter`. Read/write methods take
the unit explicitly and convert; mismatched units produce wrong values silently.

## Conventions & gotchas

- Target frameworks and minimum OS versions are pinned in the `.csproj`; the iOS `SupportedOSPlatformVersion`
  floor is 15.0 (HealthKit requirement), Android `AndroidMinSdkVersion` is 30 (Health Connect requirement).
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
