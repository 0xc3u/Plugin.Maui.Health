<img src="https://github.com/0xc3u/Plugin.Maui.Health/blob/main/src/Plugin.Maui.Health/icon.png?raw=true" alt="Plugin.Maui.Health" width="120" height="120" />

# Plugin.Maui.Health

`Plugin.Maui.Health` provides the ability to access personal health data in your .NET MAUI application.


## Build Status
![ci](https://github.com/0xc3u/Plugin.Maui.Health/actions/workflows/ci.yml/badge.svg)


## Install Plugin

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.Health.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.Health/)

Available on [NuGet](http://www.nuget.org/packages/Plugin.Maui.Health).

Install with the dotnet CLI: `dotnet add package Plugin.Maui.Health`, or through the NuGet Package Manager in Visual Studio.

## Platform Support

| Platform | Minimum Version | Health Backend |
|----------|----------------|----------------|
| iOS      | 15.0+          | Apple HealthKit |
| Android  | API 28+ (Android 9+) | Health Connect |

> **Android note:** Health Connect is pre-installed on Android 14+ and available as a standalone app from the Play Store on Android 9–13. `IsSupported` returns `false` if Health Connect is not installed/available.


## Setup Guide

### iOS Setup

#### Step 1 — Apple Developer Portal

HealthKit is a **restricted capability**. Before your app can access any health data you must explicitly enable it for your App ID in the Apple Developer Portal, otherwise entitlements signing will fail at build time and HealthKit will be unavailable at runtime.

1. Sign in to [developer.apple.com](https://developer.apple.com) and go to **Certificates, Identifiers & Profiles → Identifiers**.
2. Select (or create) the App ID for your app.
3. In the **Capabilities** list, enable **HealthKit**.
4. Save the changes and **regenerate** any provisioning profiles that use this App ID (Xcode or the portal will prompt you).

> HealthKit is **not available in the iOS Simulator** — all HealthKit calls will throw `"HealthKit not available on your device"`. Use a physical iPhone or iPad for testing.

---

#### Step 2 — .csproj: target framework and minimum OS version

```xml
<PropertyGroup>
  <TargetFrameworks>net10.0-ios;net10.0-android</TargetFrameworks>
</PropertyGroup>

<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0-ios'">
  <!-- HealthKit features used by this plugin require iOS 15 at minimum -->
  <SupportedOSPlatformVersion>15.0</SupportedOSPlatformVersion>
</PropertyGroup>
```

---

#### Step 3 — Entitlements.plist

Create `Platforms/iOS/Entitlements.plist` in your project. The `com.apple.developer.healthkit` key is required — without it the app will be rejected during code signing.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.developer.healthkit</key>
    <true/>
</dict>
</plist>
```

.NET MAUI automatically picks up `Platforms/iOS/Entitlements.plist` during the iOS build. If you store the file elsewhere, point to it explicitly in your `.csproj`:

```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0-ios'">
  <CodesignEntitlements>path\to\Entitlements.plist</CodesignEntitlements>
</PropertyGroup>
```

---

#### Step 4 — Info.plist: usage descriptions

iOS requires a human-readable explanation for every HealthKit access type. Without these keys the app will crash on the first HealthKit call.

Add the following to `Platforms/iOS/Info.plist`:

```xml
<!-- Required if you read any health data -->
<key>NSHealthShareUsageDescription</key>
<string>This app reads your health data to display your activity and body metrics.</string>

<!-- Required if you write any health data -->
<key>NSHealthUpdateUsageDescription</key>
<string>This app writes health data such as steps and body measurements on your behalf.</string>
```

Use clear, specific descriptions — Apple reviewers reject vague strings.

---

#### Step 5 — Register the plugin

In `MauiProgram.cs`, register `HealthDataProvider.Default` with the DI container:

```csharp
builder.Services.AddSingleton(HealthDataProvider.Default);
```

---

### Android Setup

Android uses **Health Connect** as the health data backend. The setup involves `.csproj` settings, a manifest change, a NuGet conflict fix, and integrating the Health Connect consent flow into your `MainActivity`.

---

#### Step 1 — Health Connect availability on device

| Android version | Health Connect |
|----------------|---------------|
| Android 14+ (API 34+) | Pre-installed as a system component |
| Android 9–13 (API 28–33) | Must be installed by the user from the Play Store |
| Below Android 9 (API < 28) | Not supported — `IsSupported` returns `false` |

Your app should check `HealthDataProvider.Default.IsSupported` before making any calls and guide users to install Health Connect when it is missing:

```csharp
if (!healthDataProvider.IsSupported)
{
    // Direct the user to install Health Connect from the Play Store.
    var intent = new Android.Content.Intent(
        Android.Content.Intent.ActionView,
        Android.Net.Uri.Parse("market://details?id=com.google.android.apps.healthdata"));
    Platform.CurrentActivity?.StartActivity(intent);
    return;
}
```

---

#### Step 2 — .csproj: SDK versions and NuGet conflict fix

```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0-android'">
  <!-- Health Connect is available from API 28 (Android 9) -->
  <AndroidMinSdkVersion>28</AndroidMinSdkVersion>
  <!-- Target API 35 for Google Play compliance -->
  <AndroidTargetSdkVersion>35</AndroidTargetSdkVersion>
  <SupportedOSPlatformVersion>28.0</SupportedOSPlatformVersion>
</PropertyGroup>

<ItemGroup Condition="$(TargetFramework.Contains('-android'))">
  <!-- Health Connect and MAUI split the transitive AndroidX Lifecycle graph.
       Pin LiveData.Core to the highest required version to resolve the NU1107
       conflict at restore time (this is the version NuGet recommends referencing). -->
  <PackageReference Include="Xamarin.AndroidX.Lifecycle.LiveData.Core" Version="2.11.0.1" />
</ItemGroup>
```

---

#### Step 3 — AndroidManifest.xml

Edit `Platforms/Android/AndroidManifest.xml`. Three things are required:

**a) `<queries>` block** — tells Android that your app intends to interact with the Health Connect app. Without this the OS will not route Health Connect intents to your app on Android 11+.

**b) Privacy-policy / rationale entry point** — Health Connect shows a "privacy policy" link on its permission sheet and requires your app to own a screen explaining why it accesses health data. This must be declared **two ways**, and **both are required** — on **Android 14+ (API 34+) the system will not present the permission request at all if the `<activity-alias>` is missing, leaving your permission launcher hanging with no result**:

- Android 13 and below — an `<activity>` with the `androidx.health.ACTION_SHOW_PERMISSIONS_RATIONALE` intent filter.
- Android 14+ — an `<activity-alias>` named `ViewPermissionUsageActivity`, guarded by `android.permission.START_VIEW_PERMISSION_USAGE`, with the `VIEW_PERMISSION_USAGE` action and `HEALTH_PERMISSIONS` category, pointing at that same activity.

`android:exported="true"` is required on Android 12+ for any activity (or alias) with an intent filter. Use a **dedicated** rationale activity rather than your launcher `MainActivity`.

**c) `<uses-permission>` declarations** — declare every health permission your app needs. Only declare what you actually use; superfluous permissions slow down the consent sheet and may trigger Play Store review.

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
  <application
    android:allowBackup="true"
    android:icon="@mipmap/appicon"
    android:roundIcon="@mipmap/appicon_round"
    android:supportsRtl="true">

    <!-- b) Android 13 and below: rationale screen shown inside Health Connect.
         The activity itself is declared in code via [Activity]/[IntentFilter] —
         here we only need the Android 14+ alias that points to it. -->
    <activity-alias
      android:name="ViewPermissionUsageActivity"
      android:exported="true"
      android:targetActivity="com.companyname.yourapp.PermissionsRationaleActivity"
      android:permission="android.permission.START_VIEW_PERMISSION_USAGE">
      <intent-filter>
        <action android:name="android.intent.action.VIEW_PERMISSION_USAGE" />
        <category android:name="android.intent.category.HEALTH_PERMISSIONS" />
      </intent-filter>
    </activity-alias>
  </application>

  <!-- a) Allow the OS to discover the Health Connect app -->
  <queries>
    <package android:name="com.google.android.apps.healthdata" />
  </queries>

  <!-- c) Health Connect permissions — add only what your app uses -->
  <uses-permission android:name="android.permission.health.READ_STEPS" />
  <uses-permission android:name="android.permission.health.WRITE_STEPS" />
  <uses-permission android:name="android.permission.health.READ_HEART_RATE" />
  <uses-permission android:name="android.permission.health.WRITE_HEART_RATE" />
  <!-- ... see the feature matrix below for all available permission strings -->
</manifest>
```

Declare the rationale activity itself in `Platforms/Android/`. Set an explicit `Name` so the `<activity-alias>` above can target it reliably:

```csharp
[Activity(Name = "com.companyname.yourapp.PermissionsRationaleActivity", Exported = true)]
[IntentFilter(new[] { "androidx.health.ACTION_SHOW_PERMISSIONS_RATIONALE" })]
public class PermissionsRationaleActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        var text = new TextView(this) { Text = "Explain why your app reads/writes health data here." };
        text.SetPadding(48, 48, 48, 48);
        SetContentView(text);
    }
}
```

See the [official Health Connect guidance](https://developer.android.com/health-and-fitness/health-connect/get-started#show-privacy-policy) for details.

---

#### Step 4 — Requesting consent

Health Connect does **not** auto-launch its permission sheet, but you no longer need to wire up your own
`ActivityResultLauncher` — the plugin drives the consent flow for you. Just call `RequestPermissionAsync`
(or `RequestWorkoutPermissionAsync`); it maps the `HealthParameter` to the right Health Connect
permissions, launches the consent UI using the current `Activity` when needed, and reports whether the
permission ended up granted:

```csharp
// Works on both Android and iOS. On Android the Health Connect consent UI is shown only when the
// permission isn't already granted; on iOS HealthKit presents its own authorization sheet.
var granted = await health.RequestPermissionAsync(
    HealthParameter.StepCount, PermissionType.Read | PermissionType.Write);

if (granted)
{
    var steps = await health.ReadCountAsync(HealthParameter.StepCount,
        DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now);
}
```

Your `MainActivity` only needs to be a standard `MauiAppCompatActivity` (the default). No launcher,
no permission-string mapping, no `MainActivity.Current` plumbing.

> Use `CheckPermissionAsync` when you only want to know the current grant state **without** prompting
> (Android only — on iOS, checking and requesting are the same HealthKit operation).

---

#### Step 5 — Register the plugin

In `MauiProgram.cs`, register `HealthDataProvider.Default` with the DI container:

```csharp
builder.Services.AddSingleton(HealthDataProvider.Default);
```

---

#### Step 6 — Google Play Store requirements

Apps that access Health Connect data must comply with [Google's Health Connect permissions policy](https://support.google.com/googleplay/android-developer/answer/12991145):

- Your app's **Privacy Policy** must disclose which health data types you access and how they are used.
- In the Play Console, go to **App content → Health Connect** and complete the declaration form.
- The rationale activity you registered in Step 3 (reachable via both `ACTION_SHOW_PERMISSIONS_RATIONALE` and the `ViewPermissionUsageActivity` alias) must display a clear screen explaining why the app needs each permission.

Apps that skip these steps will be rejected during Play Store review.

---

## Dependency Injection

Register the plugin with the `MauiAppBuilder` (in `MauiProgram.cs`):

```csharp
builder.Services.AddSingleton(HealthDataProvider.Default);
```

You can then depend on `IHealth` in your ViewModels:

```csharp
public class HealthViewModel
{
    readonly IHealth health;

    public HealthViewModel(IHealth health)
    {
        this.health = health;
    }
}
```

## Static Usage

Alternatively, use `HealthDataProvider.Default` directly without DI:

```csharp
var stepsCount = await HealthDataProvider.Default.ReadCountAsync(
    HealthParameter.StepCount,
    DateTimeOffset.Now.AddDays(-1),
    DateTimeOffset.Now);
```

---

## API Reference

### Properties

| Member | Description |
|--------|-------------|
| `IsSupported` | Returns `true` if the platform health backend is available on this device. |

### Methods

| Method | Description |
|--------|-------------|
| `RequestPermissionAsync(param, type)` | Requests the specified read/write permission via the platform consent UI and returns whether it ended up granted. The cross-platform way to obtain consent. |
| `RequestPermissionsAsync(requests)` | Requests **several** parameters in a single consent prompt. Preferred over multiple `RequestPermissionAsync` calls — on iOS, presenting several HealthKit sheets in a row can hang the app. |
| `RequestWorkoutPermissionAsync(type)` | Requests read/write access to workout sessions and GPS routes via the consent UI. |
| `CheckPermissionAsync(param, type)` | Checks whether the specified read/write permission is currently granted (does not prompt on Android). |
| `CheckWorkoutPermissionAsync(type)` | Checks whether workout (exercise) read/write permission is currently granted. |
| `ReadCountAsync(param, from, until)` | Returns the cumulative sum for count-based parameters (e.g. steps, flights climbed). |
| `ReadLatestAsync(param, from, until, unit)` | Returns the most recent single value within the date range. |
| `ReadLatestAvailableAsync(param, unit)` | Returns the most recent value ever recorded, as a `Sample`. |
| `ReadAverageAsync(param, from, until, unit)` | Returns the average over the date range. |
| `ReadMinAsync(param, from, until, unit)` | Returns the minimum value over the date range. |
| `ReadMaxAsync(param, from, until, unit)` | Returns the maximum value over the date range. |
| `ReadAllAsync(param, from, until, unit)` | Returns all samples in the date range as `IEnumerable<Sample>`. |
| `WriteAsync(param, date, value, unit)` | Writes a single value to the health store. |
| `ReadWorkoutsAsync(type, from, until)` | Returns all workout sessions in the date range, including GPS routes if available. |
| `ReadLatestWorkoutAsync(type, from, until)` | Returns the most recent workout session. |
| `WriteWorkoutAsync(session)` | Writes a workout session (with optional GPS route) to the health store. |

All async methods accept an optional `CancellationToken` as the last parameter.

### Models

**`Sample`** — returned by read methods:

| Property | Type | Description |
|----------|------|-------------|
| `From` | `DateTimeOffset?` | Sample start time |
| `Until` | `DateTimeOffset?` | Sample end time |
| `Value` | `double?` | The numeric value |
| `Unit` | `string?` | Unit string (e.g. `"kg"`, `"count/min"`) |
| `Source` | `string?` | App or device that recorded the sample |
| `Description` | `string` | Human-readable label |

**`WorkoutSession`** — returned by workout read methods:

| Property | Type | Description |
|----------|------|-------------|
| `WorkoutType` | `WorkoutType` | Activity type (Running, Cycling, …) |
| `From` | `DateTimeOffset` | Start time |
| `Until` | `DateTimeOffset` | End time |
| `TotalDistanceInMeters` | `double?` | Total distance |
| `EnergyBurnedInCalories` | `double?` | Active calories burned |
| `Title` | `string?` | Display name (Android only; always `null` on iOS) |
| `Route` | `IReadOnlyList<RoutePoint>` | GPS track points |

**`RoutePoint`** — a single GPS sample within a `WorkoutSession.Route`:

| Property | Type | Description |
|----------|------|-------------|
| `Time` | `DateTimeOffset` | Timestamp of the point |
| `Latitude` / `Longitude` | `double` | Coordinates |
| `AltitudeInMeters` | `double?` | Altitude |
| `HorizontalAccuracyInMeters` / `VerticalAccuracyInMeters` | `double?` | Accuracy |

### Enums

| Enum | Values |
|------|--------|
| `PermissionType` | `Read`, `Write` — combine with the bitwise OR (`PermissionType.Read \| PermissionType.Write`). |
| `HealthParameter` | ~100 values (steps, heart rate, body metrics, blood values, nutrition/vitamins, mobility …). See the matrix below. |
| `WorkoutType` | Activity types (`Running`, `Cycling`, `Swimming`, …). Use `WorkoutType.Other` to match/return **all** types. |

### Units

Use the `Constants.Units` static class to avoid typos in unit strings:

```csharp
Units.Mass.Kilograms          // "kg"
Units.Mass.Pounds             // "lb"
Units.Length.Meters           // "m"
Units.Length.Centimeters      // "cm"
Units.Energy.Kilocalories     // "kcal"
Units.Concentration.MillilitersPerKilogramPerMinute  // for VO2Max
Units.Concentration.MillimolesPerLiter               // for BloodGlucose
```

---

## Code Examples

### Request permissions

Always check `IsSupported` first, then request the permissions you need. The plugin presents the platform
consent UI (HealthKit sheet on iOS, Health Connect screen on Android) and maps parameters to the right
native permissions — no platform-specific code, launcher, or permission strings required.

```csharp
if (!health.IsSupported)
    return; // Health Connect not installed (Android 11–13) or backend unavailable

// Single parameter
var granted = await health.RequestPermissionAsync(
    HealthParameter.StepCount,
    PermissionType.Read | PermissionType.Write);
```

**Requesting several permissions? Batch them** — this shows a single consent prompt and avoids an iOS hang
that can occur when multiple HealthKit sheets are presented back-to-back:

```csharp
var granted = await health.RequestPermissionsAsync(new[]
{
    (HealthParameter.StepCount,   PermissionType.Read | PermissionType.Write),
    (HealthParameter.BodyMass,    PermissionType.Read | PermissionType.Write),
    (HealthParameter.HeartRate,   PermissionType.Read),
    (HealthParameter.DietaryVitaminC, PermissionType.Read | PermissionType.Write),
});
```

Check the current grant state **without** prompting (Android only — on iOS checking and requesting are the
same HealthKit operation):

```csharp
bool granted = await health.CheckPermissionAsync(HealthParameter.HeartRate, PermissionType.Read);
```

### Read step count (last 24 hours)

```csharp
var hasPermission = await health.RequestPermissionAsync(HealthParameter.StepCount, PermissionType.Read);
if (hasPermission)
{
    var steps = await health.ReadCountAsync(
        HealthParameter.StepCount,
        DateTimeOffset.Now.AddDays(-1),
        DateTimeOffset.Now);
}
```

### Read latest body mass

```csharp
var sample = await health.ReadLatestAvailableAsync(HealthParameter.BodyMass, Units.Mass.Kilograms);
double kg = sample?.Value ?? 0;
```

### Write body mass

```csharp
var hasPermission = await health.RequestPermissionAsync(HealthParameter.BodyMass, PermissionType.Write);
if (hasPermission)
{
    await health.WriteAsync(HealthParameter.BodyMass, DateTimeOffset.Now, 75.5, Units.Mass.Kilograms);
}
```

### Read all heart rate samples

```csharp
var samples = await health.ReadAllAsync(
    HealthParameter.HeartRate,
    DateTimeOffset.Now.AddHours(-6),
    DateTimeOffset.Now,
    Units.Others.CountPerMinute);
```

### Read workouts with GPS route

```csharp
var workouts = await health.ReadWorkoutsAsync(
    WorkoutType.Running,
    DateTimeOffset.Now.AddDays(-7),
    DateTimeOffset.Now);

foreach (var w in workouts)
{
    Console.WriteLine($"{w.WorkoutType}: {w.TotalDistanceInMeters:F0} m, {w.Route.Count} GPS points");
}
```

### Statistics over a range (average / min / max)

```csharp
var from = DateTimeOffset.Now.AddDays(-7);
var until = DateTimeOffset.Now;

double? avgHr = await health.ReadAverageAsync(HealthParameter.HeartRate, from, until, Units.Others.CountPerMinute);
double? minHr = await health.ReadMinAsync(HealthParameter.HeartRate, from, until, Units.Others.CountPerMinute);
double? maxHr = await health.ReadMaxAsync(HealthParameter.HeartRate, from, until, Units.Others.CountPerMinute);
```

### Most recent value within a range

```csharp
// Latest reading inside a window (null if none in range)
double? latestGlucose = await health.ReadLatestAsync(
    HealthParameter.BloodGlucose, from, until, Units.Concentration.MillimolesPerLiter);

// Latest reading ever recorded, with full sample metadata
Sample? sample = await health.ReadLatestAvailableAsync(HealthParameter.BodyMass, Units.Mass.Kilograms);
```

### Write a workout (with an optional GPS route)

```csharp
var session = new WorkoutSession
{
    WorkoutType = WorkoutType.Running,
    From = DateTimeOffset.Now.AddMinutes(-30),
    Until = DateTimeOffset.Now,
    TotalDistanceInMeters = 5200,
    EnergyBurnedInCalories = 410,
    Route = new List<RoutePoint>
    {
        new() { Time = DateTimeOffset.Now.AddMinutes(-30), Latitude = 47.37, Longitude = 8.54, AltitudeInMeters = 408 },
        new() { Time = DateTimeOffset.Now,                 Latitude = 47.38, Longitude = 8.55, AltitudeInMeters = 412 },
    }
};

if (await health.RequestWorkoutPermissionAsync(PermissionType.Write))
    await health.WriteWorkoutAsync(session);
```

### Error handling

Every method throws a single exception type — `HealthException` — for hard failures (unsupported
parameter, backend unavailable, native error). It carries context and preserves the original platform
error as `InnerException`. Reads simply return `null`/`0`/an empty list when there is no data (that is not
an error).

```csharp
using Plugin.Maui.Health.Exceptions;

try
{
    var steps = await health.ReadCountAsync(HealthParameter.StepCount, from, until);
}
catch (HealthException ex)
{
    // ex.Message is human-readable, e.g.
    //   "Plugin.Maui.Health: ReadCountAsync failed on Android: <native detail>"
    Console.WriteLine($"{ex.Operation} on {ex.Platform} [{ex.Parameter}]: {ex.Message}");
    Console.WriteLine(ex.InnerException); // original HealthKit / Health Connect error
}
```

---

## Feature & Permission Matrix

Legend: **Y** = supported, **—** = not supported on this platform

### Body Measurements

| HealthParameter | iOS | Android | Android Permission |
|----------------|-----|---------|-------------------|
| `BodyMassIndex` | Y | — | — |
| `BodyMass` | Y | Y | `health.READ_WEIGHT` / `WRITE_WEIGHT` |
| `Height` | Y | Y | `health.READ_HEIGHT` / `WRITE_HEIGHT` |
| `BodyFatPercentage` | Y | Y | `health.READ_BODY_FAT` / `WRITE_BODY_FAT` |
| `LeanBodyMass` | Y | Y | `health.READ_LEAN_BODY_MASS` / `WRITE_LEAN_BODY_MASS` |
| `WaistCircumference` | Y | — | — |

### Heart & Cardiovascular

| HealthParameter | iOS | Android | Android Permission |
|----------------|-----|---------|-------------------|
| `HeartRate` | Y | Y | `health.READ_HEART_RATE` / `WRITE_HEART_RATE` |
| `RestingHeartRate` | Y | Y | `health.READ_RESTING_HEART_RATE` / `WRITE_RESTING_HEART_RATE` |
| `WalkingHeartRateAverage` | Y | — | — |
| `HeartRateVariabilitySdnn` | Y | Y | `health.READ_HEART_RATE_VARIABILITY` / `WRITE_HEART_RATE_VARIABILITY` |
| `HeartRateRecoveryOneMinute` | Y | — | — |
| `AtrialFibrillationBurden` | Y | — | — |

### Activity & Fitness

| HealthParameter | iOS | Android | Android Permission |
|----------------|-----|---------|-------------------|
| `StepCount` | Y | Y | `health.READ_STEPS` / `WRITE_STEPS` |
| `DistanceWalkingRunning` | Y | Y | `health.READ_DISTANCE` / `WRITE_DISTANCE` |
| `DistanceCycling` | Y | Y | `health.READ_DISTANCE` / `WRITE_DISTANCE` |
| `DistanceSwimming` | Y | — | — |
| `DistanceWheelchair` | Y | — | — |
| `DistanceDownhillSnowSports` | Y | — | — |
| `ActiveEnergyBurned` | Y | Y | `health.READ_ACTIVE_CALORIES_BURNED` / `WRITE_ACTIVE_CALORIES_BURNED` |
| `BasalEnergyBurned` | Y | Y | `health.READ_BASAL_METABOLIC_RATE` / `WRITE_BASAL_METABOLIC_RATE` |
| `FlightsClimbed` | Y | Y | `health.READ_FLOORS_CLIMBED` / `WRITE_FLOORS_CLIMBED` |
| `ExerciseTime` | Y | Y | `health.READ_EXERCISE` / `WRITE_EXERCISE` |
| `StandTime` | Y | — | — |
| `MoveTime` | Y | — | — |
| `PushCount` | Y | — | — |
| `SwimmingStrokeCount` | Y | — | — |
| `NikeFuel` | Y | — | — |

### Vitals & Clinical

| HealthParameter | iOS | Android | Android Permission |
|----------------|-----|---------|-------------------|
| `BloodGlucose` | Y | Y | `health.READ_BLOOD_GLUCOSE` / `WRITE_BLOOD_GLUCOSE` |
| `BloodPressureSystolic` | Y | Y | `health.READ_BLOOD_PRESSURE` / `WRITE_BLOOD_PRESSURE` |
| `BloodPressureDiastolic` | Y | Y | `health.READ_BLOOD_PRESSURE` / `WRITE_BLOOD_PRESSURE` |
| `OxygenSaturation` | Y | Y | `health.READ_OXYGEN_SATURATION` / `WRITE_OXYGEN_SATURATION` |
| `BodyTemperature` | Y | Y | `health.READ_BODY_TEMPERATURE` / `WRITE_BODY_TEMPERATURE` |
| `BasalBodyTemperature` | Y | Y | `health.READ_BASAL_BODY_TEMPERATURE` / `WRITE_BASAL_BODY_TEMPERATURE` |
| `RespiratoryRate` | Y | Y | `health.READ_RESPIRATORY_RATE` / `WRITE_RESPIRATORY_RATE` |
| `VO2Max` | Y | Y | `health.READ_VO2_MAX` / `WRITE_VO2_MAX` |
| `InsulinDelivery` | Y | — | — |
| `BloodAlcoholContent` | Y | — | — |
| `PeripheralPerfusionIndex` | Y | — | — |

### Respiratory

| HealthParameter | iOS | Android | Android Permission |
|----------------|-----|---------|-------------------|
| `ForcedVitalCapacity` | Y | — | — |
| `ForcedExpiratoryVolume1` | Y | — | — |
| `PeakExpiratoryFlowRate` | Y | — | — |
| `InhalerUsage` | Y | — | — |

### Mobility & Gait

| HealthParameter | iOS | Android | Android Permission |
|----------------|-----|---------|-------------------|
| `WalkingSpeed` | Y | Y | `health.READ_SPEED` / `WRITE_SPEED` |
| `RunningSpeed` | Y | Y | `health.READ_SPEED` / `WRITE_SPEED` |
| `RunningPower` | Y | Y | `health.READ_POWER` / `WRITE_POWER` |
| `WalkingStepLength` | Y | — | — |
| `WalkingAsymmetryPercentage` | Y | — | — |
| `WalkingDoubleSupportPercentage` | Y | — | — |
| `WalkingSteadiness` | Y | — | — |
| `SixMinuteWalkTestDistance` | Y | — | — |
| `StairAscentSpeed` | Y | — | — |
| `StairDescentSpeed` | Y | — | — |
| `RunningGroundContactTime` | Y | — | — |
| `RunningStrideLength` | Y | — | — |
| `RunningVerticalOscillation` | Y | — | — |

### Workouts

| Feature | iOS | Android | Android Permission |
|--------|-----|---------|-------------------|
| Read workouts | Y | Y | `health.READ_EXERCISE` |
| Write workouts | Y | Y | `health.WRITE_EXERCISE` |
| GPS route (read) | Y | Y | `health.READ_EXERCISE_ROUTES` |
| GPS route (write) | Y | Y | `health.WRITE_EXERCISE_ROUTE` |

### Nutrition & Hydration

All nutrition parameters use `health.READ_NUTRITION` / `WRITE_NUTRITION` on Android. Water uses `health.READ_HYDRATION` / `WRITE_HYDRATION`.

| HealthParameter | iOS | Android |
|----------------|-----|---------|
| `DietaryWater` | Y | Y |
| `DietaryEnergyConsumed` | Y | Y |
| `DietaryProtein` | Y | Y |
| `DietaryCarbohydrates` | Y | Y |
| `DietaryFatTotal` | Y | Y |
| `DietaryFatSaturated` | Y | Y |
| `DietaryFatMonounsaturated` | Y | Y |
| `DietaryFatPolyunsaturated` | Y | Y |
| `DietaryFiber` | Y | Y |
| `DietarySugar` | Y | Y |
| `DietaryCholesterol` | Y | Y |
| `DietarySodium` | Y | Y |
| `DietaryCalcium` | Y | Y |
| `DietaryIron` | Y | Y |
| `DietaryPotassium` | Y | Y |
| `DietaryVitaminA` | Y | Y |
| `DietaryVitaminB6` | Y | Y |
| `DietaryVitaminB12` | Y | Y |
| `DietaryVitaminC` | Y | Y |
| `DietaryVitaminD` | Y | Y |
| `DietaryVitaminE` | Y | Y |
| `DietaryVitaminK` | Y | Y |
| `DietaryBiotin` | Y | Y |
| `DietaryCaffeine` | Y | Y |
| `DietaryChloride` | Y | Y |
| `DietaryChromium` | Y | Y |
| `DietaryCopper` | Y | Y |
| `DietaryFolate` | Y | Y |
| `DietaryIodine` | Y | Y |
| `DietaryMagnesium` | Y | Y |
| `DietaryManganese` | Y | Y |
| `DietaryMolybdenum` | Y | Y |
| `DietaryNiacin` | Y | Y |
| `DietaryPantothenicAcid` | Y | Y |
| `DietaryPhosphorus` | Y | Y |
| `DietaryRiboflavin` | Y | Y |
| `DietarySelenium` | Y | Y |
| `DietaryThiamin` | Y | Y |
| `DietaryZinc` | Y | Y |

### Environmental & Other (iOS only)

| HealthParameter | iOS | Android |
|----------------|-----|---------|
| `UVExposure` | Y | — |
| `ElectrodermalActivity` | Y | — |
| `EnvironmentalAudioExposure` | Y | — |
| `HeadphoneAudioExposure` | Y | — |
| `NumberOfTimesFallen` | Y | — |
| `NumberOfAlcoholicBeverages` | Y | — |
| `SleepingWristTemperature` | Y | — |
| `UnderwaterDepth` | Y | — |
| `WaterTemperature` | Y | — |

---

## Sample App

The sample app is a card-based dashboard with charts drawn using **MAUI.Graphics** (a goal ring, a
weekly steps bar chart and a weight-trend line chart). On first launch it seeds representative data
into the device's health store via the plugin and reads it back, so every chart reflects genuine
round-tripped data on **both** Android (Health Connect) and iOS (HealthKit):

**Dashboard** — goal ring + weekly steps bar chart:

| Android (Health Connect) | iOS (HealthKit) |
|---|---|
| ![Sample app dashboard on Android](https://github.com/0xc3u/Plugin.Maui.Health/blob/main/screenshots/sample_android_dashboard.png?raw=true) | ![Sample app dashboard on iOS](https://github.com/0xc3u/Plugin.Maui.Health/blob/main/screenshots/sample_ios_dashboard.png?raw=true) |

**Detail screens** — BMI ring + weight-trend line chart, a heart-rate time-series line chart, and a vitamin-intake bar chart:

| Body Measurements | Heart Rate | Vitamins |
|---|---|---|
| ![Body Measurements screen](https://github.com/0xc3u/Plugin.Maui.Health/blob/main/screenshots/sample_body_measurements.png?raw=true) | ![Heart Rate screen](https://github.com/0xc3u/Plugin.Maui.Health/blob/main/screenshots/sample_heart_rate.png?raw=true) | ![Vitamins screen](https://github.com/0xc3u/Plugin.Maui.Health/blob/main/screenshots/sample_vitamins.png?raw=true) |

Try the sample app to test the plugin on your own device.

---

## Remarks

- Always pass the correct `HealthParameter` together with its matching unit string. Use the `Constants.Units` helper class to avoid typos.
- On Android, `ReadAllAsync` returns at most 1,000 records per call. For high-frequency sensors (heart rate, speed) over long time ranges, split your query into smaller intervals.
- On iOS, `CheckPermissionAsync` for **read** permission always returns `true` after the user has seen the consent sheet — HealthKit does not reveal whether read access was actually granted. Only write permission denials are detectable.
- On Android, `CheckPermissionAsync` returns `false` for parameters that are not supported by Health Connect (see matrix above). These parameters will also throw `HealthException` if passed to any read/write method.

# Acknowledgements

This project could not have came to be without these projects and people, thank you! <3

- We thank Gerald Versluis (@jfversluis) for his excellent template [Plugin.Maui.Template](https://github.com/jfversluis/Plugin.Maui.Feature)
