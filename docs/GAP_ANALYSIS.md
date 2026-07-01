# Plugin.Maui.Health — Gap Analysis

*How the library's capabilities compare to what Apple HealthKit and Android Health Connect actually
expose. Verified against `Health.macios.cs`, `Health.android.cs`, `IHealth.cs` and the platform SDKs
(HealthKit iOS 17/18; `androidx.health.connect.client` current to early 2026). Last updated: 2026-07.*

> Legend: ✅ implemented · ⚠️ partial · ❌ missing.

## Headline findings

1. ~~**The README support matrix overstated iOS coverage.**~~ **RESOLVED (Tier 1).** The iOS
   `healthParameterMapping` dictionary ended at `VO2Max`; ~40 `HealthParameter` values marked iOS-"Y"
   actually threw `HealthException` because they were absent from the dictionary — despite HealthKit
   natively supporting them. Fixed by wiring the missing 37 quantity types, so the matrix is now
   accurate rather than aspirational.
2. **~30 enum values threw on *both* platforms** — vocabulary the API advertised but couldn't fulfil.
3. **Whole native capabilities are unused**: aggregation bucketing, delete, update/upsert, background/
   sync, data provenance, characteristics, correlations, series.
4. **Whole data domains are unmodelled** — several supported on *both* platforms: reproductive/cycle
   tracking, mindfulness, meal-level nutrition.

---

## 1. What's implemented today

| Area | Status |
|---|---|
| Read shapes | count, latest, latest-available, average, min, max, all ✅ |
| Write | single value ✅ |
| Workouts | read / latest / write + GPS routes + per-type statistics ✅ |
| Sleep | read / write + normalised stages ✅ |
| Permissions | single + batched request/check, workout, sleep ✅ |
| Quantity params wired | iOS: 65 (mapping dictionary) · Android: ~64 (GetPermissions) |

---

## 2. Data-type gaps

### 2a. iOS params that throw but HealthKit supports — *highest value, lowest effort*

Already in the enum (and previously claimed by the README), but missing from `healthParameterMapping`:

- **Cardiac:** `RestingHeartRate`, `HeartRateVariabilitySdnn`, `WalkingHeartRateAverage`,
  `HeartRateRecoveryOneMinute`, `AtrialFibrillationBurden`
- **Activity/energy:** `ExerciseTime`, `StandTime`, `MoveTime`, `PushCount`, `DistanceSwimming`,
  `DistanceWheelchair`, `DistanceDownhillSnowSports`, `SwimmingStrokeCount`
- **Running dynamics:** `RunningSpeed`, `RunningPower`, `RunningStrideLength`,
  `RunningGroundContactTime`, `RunningVerticalOscillation`
- **Mobility/gait:** `WalkingSpeed`, `WalkingStepLength`, `WalkingAsymmetryPercentage`,
  `WalkingDoubleSupportPercentage`, `WalkingSteadiness`, `StairAscentSpeed`, `StairDescentSpeed`,
  `SixMinuteWalkTestDistance`
- **Body/vitals/other:** `WaistCircumference`, `BasalBodyTemperature`, `SleepingWristTemperature`,
  `InsulinDelivery`, `DietaryWater`, `UVExposure`, `ElectrodermalActivity`,
  `EnvironmentalAudioExposure`, `HeadphoneAudioExposure`, `NumberOfAlcoholicBeverages`,
  `UnderwaterDepth`, `WaterTemperature`

> **Sharpest inconsistency:** `RestingHeartRate`, `HeartRateVariabilitySdnn`, `WalkingSpeed`,
> `RunningSpeed`, `RunningPower`, `ExerciseTime`, `BasalBodyTemperature`, `DietaryWater` were wired on
> **Android but not iOS**, though HealthKit supports them all.

### 2b. Android records with no enum representation (Health Connect supports them)

`TotalCaloriesBurnedRecord` (only Active/Basal exposed today), `BoneMassRecord`, `BodyWaterMassRecord`,
`ElevationGainedRecord`, `WheelchairPushesRecord` (→ `PushCount`), `StepsCadenceRecord`, cycling
`PowerRecord`/`CyclingPedalingCadenceRecord`, `SkinTemperatureRecord` (2024), `ActivityIntensityRecord`
(2025).

### 2c. Truly iOS-only (no Health Connect equivalent — correctly asymmetric)

`BodyMassIndex`, `NikeFuel`, `BloodAlcoholContent`, `PeripheralPerfusionIndex`, `ForcedVitalCapacity`,
`ForcedExpiratoryVolume1`, `PeakExpiratoryFlowRate`, `NumberOfTimesFallen`, `InhalerUsage`, plus most of
the mobility / running-dynamics / audio-exposure set above.

---

## 3. API / capability gaps (cross-cutting)

| Capability | HealthKit | Health Connect | In library? |
|---|---|---|---|
| Delete data | `deleteObjects` | `deleteRecords` (id / time-range) | ❌ |
| Update / upsert (+ dedup id) | sync-identifier metadata | `updateRecords` + `clientRecordId`/version | ❌ |
| Batch write | `save([...])` | `insertRecords(List)` | ❌ single only |
| Aggregation bucketing (daily/weekly/hourly) | `HKStatisticsCollectionQuery` | `aggregateGroupByDuration/Period` | ❌ whole-range only* |
| Background / live updates / sync | `HKObserverQuery` + background delivery, `HKAnchoredObjectQuery` | `getChangesToken`/`getChanges` + `READ_HEALTH_DATA_IN_BACKGROUND` | ❌ |
| Data provenance / filtering | `HKSource`/`HKDevice`/`wasUserEntered` | `dataOrigin`, `device`, `recordingMethod` + `dataOriginFilter` | ⚠️ `Sample.Source` string only |
| Historic data (Android) | n/a | `READ_HEALTH_DATA_HISTORY` (else 30-day limit) | ❌ |
| Paging control | limit/anchor | `pageToken`, 1000-record cap | ⚠️ silent truncation |
| Feature gating (Android) | n/a | `getFeatureStatus(FEATURE_*)` | ❌ |
| Read-auth status (iOS) | `getRequestStatusForAuthorization` | grant state readable | ⚠️ unused |

\* The sample app already pays for this — it sums steps client-side per day because there's no bucketed
aggregation API.

---

## 4. Data domains not modelled

| Domain | HealthKit | Health Connect | Notes |
|---|---|---|---|
| Reproductive / cycle tracking | menstrualFlow, ovulation, cervicalMucus, sexualActivity, pregnancy… | `MenstruationFlow/Period`, `Ovulation`, `CervicalMucus`, `SexualActivity`, `IntermenstrualBleeding` | **Both** — largest missing cross-platform domain |
| Mindfulness / meditation | `mindfulSession` | `MindfulnessSessionRecord` (2025) | **Both** — small, high-appeal |
| Meal-level nutrition | `food` correlation | `NutritionRecord` (42 fields + mealType) | Currently per-nutrient single writes |
| Symptoms | ~40 category types | — | iOS only |
| ECG | `HKElectrocardiogram` | — | `AtrialFibrillationBurden` enum exists but no ECG API |
| Audiogram / heartbeat series | ✅ | — | iOS only |
| Activity rings / intensity | `HKActivitySummary` | `ActivityIntensityRecord` | Different models |
| State of Mind / mood | iOS 17.4+ | — | iOS only |
| Characteristics (DOB, sex, blood type, skin type, wheelchair) | read-only | — | iOS only; no method exists |
| Clinical / FHIR / PHR | `HKClinicalRecord` (read-only) | Personal Health Records (2024–25) | Advanced/optional |
| Planned / coached workouts | — | `PlannedExerciseSessionRecord` | Android only |

---

## 5. Correctness notes

- **Units** missing from `Constants/Units.cs` for new types: insulin `IU`, audio `dBASPL`/`dBHL`,
  ECG `V`.
- **Blood pressure** is modelled as two independent params; on iOS a real reading is an `HKCorrelation`
  of systolic+diastolic — writing them separately may not produce a proper BP entry.
- **`ReadAllAsync`** silently caps at 1000 records on Android with no page token exposed → data loss on
  high-frequency sensors over long ranges.

---

## 6. Prioritised roadmap

### Tier 1 — cheap, high trust/value (in progress)
- [x] Correct the README support matrix to match code. *(Achieved by wiring the code so the matrix's
      iOS-"Y" claims are now backed by real mappings.)*
- [x] Wire the ~40 HealthKit quantity types already in the enum. *(37 added to
      `healthParameterMapping` — cardiac, activity/energy, running dynamics, mobility/gait, and body/
      vitals/other. iOS quantity coverage is now comprehensive.)*
- [x] Surface `ReadAllAsync` paging to stop silent truncation. *(All Android read helpers now follow
      the page token via `ReadAllPagesAsync<T>` instead of a single 1,000-record read.)*
- [x] Add low-hanging Android records. *(Added `TotalEnergyBurned`, `ElevationGained`, `BoneMass`,
      `BodyWaterMass` as new Android-only `HealthParameter` values, and wired `PushCount` to Health
      Connect's `WheelchairPushesRecord` (now cross-platform). All five write→read round-trips verified
      on the emulator.)*

**Tier 1 complete.**

### Tier 2 — medium effort, broad value
- [x] Bucketed aggregation API — `ReadStatisticsAsync` (hourly/daily/weekly/monthly, sum vs average).
      *(Shared client-side bucketing over the paged `ReadAllAsync`; verified daily step totals on the
      emulator. Native `HKStatisticsCollectionQuery` / `aggregateGroupByPeriod` is a future optimisation.)*
- [x] `DeleteAsync` (delete-by-range). *(iOS `DeleteObjects`; Android `DeleteRecords` by record type +
      time range. Round-trip verified.)*
- [x] Upsert — `UpsertAsync` with a client id. *(Android `clientRecordId` + version; iOS
      `HKMetadataKeySyncIdentifier` + `SyncVersion`. Verified: two writes with one id → one record,
      updated value.)*
- [x] Batch write — `WriteAllAsync`. *(One `SaveObjects` / `InsertRecords`. Round-trip verified.)*
- [x] Data provenance — `Sample.Device` + `Sample.RecordingMethod`, and `Source` now reports the real
      data-origin package (Android) / source name (iOS). *(Verified: manual-entry seed reads back as
      `method=Manual`, source = app package.)* Native source-filtering on reads remains a follow-up.

**Tier 2 complete.** (Follow-ups: native aggregation for cumulative types; source-filtered reads.)

### Tier 3 — differentiating
- [ ] Background/observer + change-based sync (+ Android background/history permissions & feature gating).
- [ ] Reproductive / cycle tracking (cross-platform).
- [ ] Mindfulness (cross-platform).
- [ ] iOS characteristics; blood-pressure & meal correlations.

### Tier 4 — specialised / optional
- [ ] ECG, audiogram, heartbeat series (iOS); activity rings / intensity; state of mind; symptoms;
      clinical / PHR.
