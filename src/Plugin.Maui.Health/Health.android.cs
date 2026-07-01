using Android.Runtime;
using AndroidX.Health.Connect.Client;
using AndroidX.Health.Connect.Client.Aggregate;
using AndroidX.Health.Connect.Client.Records;
using AndroidX.Health.Connect.Client.Records.Metadata;
using AndroidX.Health.Connect.Client.Request;
using AndroidX.Health.Connect.Client.Response;
using AndroidX.Health.Connect.Client.Time;
using AndroidX.Health.Connect.Client.Units;
using Java.Interop;
using Kotlin.Coroutines;
using Kotlin.Jvm;
using Kotlin.Reflect;
using Microsoft.Maui.ApplicationModel;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Models;

namespace Plugin.Maui.Health;

partial class HealthDataProviderImplementation : IHealth
{
	readonly SemaphoreSlim semaphore = new(1, 1);

	IHealthConnectClient GetClient()
	{
		if (!IsSupported)
		{
			throw new HealthException(
				"Plugin.Maui.Health: Health Connect is not available on this device. It is built in on " +
				"Android 14+ (API 34+); on Android 11–13 the user must install it from the Play Store. " +
				"Check IsSupported before calling any read/write method.")
			{
				Platform = "Android",
			};
		}

		return HealthConnectClient.GetOrCreate(Platform.AppContext);
	}

	public bool IsSupported
	{
		get
		{
			try
			{
				return HealthConnectClient.GetSdkStatus(Platform.AppContext) == HealthConnectClient.SdkAvailable;
			}
			catch
			{
				// IsSupported must never throw — callers rely on it to gate every other call.
				return false;
			}
		}
	}

	// ── Permission helpers ────────────────────────────────────────────────────

	static (string? Read, string? Write) GetPermissions(HealthParameter param) => param switch
	{
		HealthParameter.StepCount => ("android.permission.health.READ_STEPS", "android.permission.health.WRITE_STEPS"),
		HealthParameter.HeartRate => ("android.permission.health.READ_HEART_RATE", "android.permission.health.WRITE_HEART_RATE"),
		HealthParameter.RestingHeartRate => ("android.permission.health.READ_RESTING_HEART_RATE", "android.permission.health.WRITE_RESTING_HEART_RATE"),
		HealthParameter.HeartRateVariabilitySdnn => ("android.permission.health.READ_HEART_RATE_VARIABILITY", "android.permission.health.WRITE_HEART_RATE_VARIABILITY"),
		HealthParameter.BodyMass => ("android.permission.health.READ_WEIGHT", "android.permission.health.WRITE_WEIGHT"),
		HealthParameter.Height => ("android.permission.health.READ_HEIGHT", "android.permission.health.WRITE_HEIGHT"),
		HealthParameter.BodyFatPercentage => ("android.permission.health.READ_BODY_FAT", "android.permission.health.WRITE_BODY_FAT"),
		HealthParameter.LeanBodyMass => ("android.permission.health.READ_LEAN_BODY_MASS", "android.permission.health.WRITE_LEAN_BODY_MASS"),
		HealthParameter.DistanceWalkingRunning or HealthParameter.DistanceCycling =>
			("android.permission.health.READ_DISTANCE", "android.permission.health.WRITE_DISTANCE"),
		HealthParameter.ActiveEnergyBurned => ("android.permission.health.READ_ACTIVE_CALORIES_BURNED", "android.permission.health.WRITE_ACTIVE_CALORIES_BURNED"),
		HealthParameter.BasalEnergyBurned => ("android.permission.health.READ_BASAL_METABOLIC_RATE", "android.permission.health.WRITE_BASAL_METABOLIC_RATE"),
		HealthParameter.BloodGlucose => ("android.permission.health.READ_BLOOD_GLUCOSE", "android.permission.health.WRITE_BLOOD_GLUCOSE"),
		HealthParameter.BloodPressureSystolic or HealthParameter.BloodPressureDiastolic =>
			("android.permission.health.READ_BLOOD_PRESSURE", "android.permission.health.WRITE_BLOOD_PRESSURE"),
		HealthParameter.OxygenSaturation => ("android.permission.health.READ_OXYGEN_SATURATION", "android.permission.health.WRITE_OXYGEN_SATURATION"),
		HealthParameter.BodyTemperature => ("android.permission.health.READ_BODY_TEMPERATURE", "android.permission.health.WRITE_BODY_TEMPERATURE"),
		HealthParameter.BasalBodyTemperature => ("android.permission.health.READ_BASAL_BODY_TEMPERATURE", "android.permission.health.WRITE_BASAL_BODY_TEMPERATURE"),
		HealthParameter.RespiratoryRate => ("android.permission.health.READ_RESPIRATORY_RATE", "android.permission.health.WRITE_RESPIRATORY_RATE"),
		HealthParameter.VO2Max => ("android.permission.health.READ_VO2_MAX", "android.permission.health.WRITE_VO2_MAX"),
		HealthParameter.FlightsClimbed => ("android.permission.health.READ_FLOORS_CLIMBED", "android.permission.health.WRITE_FLOORS_CLIMBED"),
		HealthParameter.WalkingSpeed or HealthParameter.RunningSpeed =>
			("android.permission.health.READ_SPEED", "android.permission.health.WRITE_SPEED"),
		HealthParameter.RunningPower => ("android.permission.health.READ_POWER", "android.permission.health.WRITE_POWER"),
		HealthParameter.ExerciseTime => ("android.permission.health.READ_EXERCISE", "android.permission.health.WRITE_EXERCISE"),
		HealthParameter.TotalEnergyBurned => ("android.permission.health.READ_TOTAL_CALORIES_BURNED", "android.permission.health.WRITE_TOTAL_CALORIES_BURNED"),
		HealthParameter.BoneMass => ("android.permission.health.READ_BONE_MASS", "android.permission.health.WRITE_BONE_MASS"),
		HealthParameter.BodyWaterMass => ("android.permission.health.READ_BODY_WATER_MASS", "android.permission.health.WRITE_BODY_WATER_MASS"),
		HealthParameter.ElevationGained => ("android.permission.health.READ_ELEVATION_GAINED", "android.permission.health.WRITE_ELEVATION_GAINED"),
		HealthParameter.PushCount => ("android.permission.health.READ_WHEELCHAIR_PUSHES", "android.permission.health.WRITE_WHEELCHAIR_PUSHES"),
		HealthParameter.DietaryWater => ("android.permission.health.READ_HYDRATION", "android.permission.health.WRITE_HYDRATION"),
		HealthParameter.DietaryBiotin or HealthParameter.DietaryCaffeine or
		HealthParameter.DietaryCalcium or HealthParameter.DietaryCarbohydrates or
		HealthParameter.DietaryChloride or HealthParameter.DietaryCholesterol or
		HealthParameter.DietaryChromium or HealthParameter.DietaryCopper or
		HealthParameter.DietaryEnergyConsumed or HealthParameter.DietaryFatMonounsaturated or
		HealthParameter.DietaryFatPolyunsaturated or HealthParameter.DietaryFatSaturated or
		HealthParameter.DietaryFatTotal or HealthParameter.DietaryFiber or
		HealthParameter.DietaryFolate or HealthParameter.DietaryIodine or
		HealthParameter.DietaryIron or HealthParameter.DietaryMagnesium or
		HealthParameter.DietaryManganese or HealthParameter.DietaryMolybdenum or
		HealthParameter.DietaryNiacin or HealthParameter.DietaryPantothenicAcid or
		HealthParameter.DietaryPhosphorus or HealthParameter.DietaryPotassium or
		HealthParameter.DietaryProtein or HealthParameter.DietaryRiboflavin or
		HealthParameter.DietarySelenium or HealthParameter.DietarySodium or
		HealthParameter.DietarySugar or HealthParameter.DietaryThiamin or
		HealthParameter.DietaryVitaminA or HealthParameter.DietaryVitaminB12 or
		HealthParameter.DietaryVitaminB6 or HealthParameter.DietaryVitaminC or
		HealthParameter.DietaryVitaminD or HealthParameter.DietaryVitaminE or
		HealthParameter.DietaryVitaminK or HealthParameter.DietaryZinc =>
			("android.permission.health.READ_NUTRITION", "android.permission.health.WRITE_NUTRITION"),
		_ => (null, null),
	};

	// ── CheckPermissionAsync ──────────────────────────────────────────────────

	public async Task<bool> CheckPermissionAsync(HealthParameter healthParameter, PermissionType permissionType,
		CancellationToken cancellationToken = default)
	{
		var (readPerm, writePerm) = GetPermissions(healthParameter);
		if (readPerm is null && writePerm is null)
		{
			throw new HealthException($"Not supported on Android: {healthParameter}");
		}

		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var grantedRaw = await InvokeCoroutine(
				c => client.PermissionController.GetGrantedPermissions(c)).ConfigureAwait(false);

			var granted = new HashSet<string>();
			if (grantedRaw is Java.Lang.Object rawObj)
			{
				var iter = Java.Interop.JavaObjectExtensions.JavaCast<Java.Util.AbstractCollection>(rawObj)?.Iterator();
				if (iter != null)
				{
					while (iter.HasNext)
					{
						granted.Add(iter.Next()?.ToString() ?? "");
					}
				}
			}

			var required = new HashSet<string>();
			if (permissionType.HasFlag(PermissionType.Read) && readPerm is not null)
			{
				required.Add(readPerm);
			}

			if (permissionType.HasFlag(PermissionType.Write) && writePerm is not null)
			{
				required.Add(writePerm);
			}

			return required.All(p => granted.Contains(p));
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// ── CheckPermissionsAsync (batched) ───────────────────────────────────────

	public async Task<bool> CheckPermissionsAsync(
		IEnumerable<(HealthParameter healthParameter, PermissionType permissionType)> requests,
		CancellationToken cancellationToken = default)
	{
		var required = new HashSet<string>();

		foreach (var (parameter, permissionType) in requests)
		{
			// Skip parameters Health Connect doesn't support rather than failing the whole check.
			var (readPerm, writePerm) = GetPermissions(parameter);
			if (permissionType.HasFlag(PermissionType.Read) && readPerm is not null)
				required.Add(readPerm);
			if (permissionType.HasFlag(PermissionType.Write) && writePerm is not null)
				required.Add(writePerm);
		}

		if (required.Count == 0)
		{
			return true; // nothing supported on Android in this set
		}

		return await AreAllGrantedAsync(required, cancellationToken).ConfigureAwait(false);
	}

	// ── CheckWorkoutPermissionAsync ───────────────────────────────────────────

	public async Task<bool> CheckWorkoutPermissionAsync(PermissionType permissionType,
		CancellationToken cancellationToken = default)
	{
		const string readPerm = "android.permission.health.READ_EXERCISE";
		const string writePerm = "android.permission.health.WRITE_EXERCISE";

		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var grantedRaw = await InvokeCoroutine(
				c => client.PermissionController.GetGrantedPermissions(c)).ConfigureAwait(false);

			var granted = new HashSet<string>();
			if (grantedRaw is Java.Lang.Object rawObj)
			{
				var iter = Java.Interop.JavaObjectExtensions.JavaCast<Java.Util.AbstractCollection>(rawObj)?.Iterator();
				if (iter != null)
				{
					while (iter.HasNext)
						granted.Add(iter.Next()?.ToString() ?? "");
				}
			}

			var required = new HashSet<string>();
			if (permissionType.HasFlag(PermissionType.Read))
				required.Add(readPerm);
			if (permissionType.HasFlag(PermissionType.Write))
				required.Add(writePerm);

			return required.All(p => granted.Contains(p));
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// ── RequestPermissionAsync ────────────────────────────────────────────────

	public async Task<bool> RequestPermissionAsync(HealthParameter healthParameter, PermissionType permissionType,
		CancellationToken cancellationToken = default)
	{
		var (readPerm, writePerm) = GetPermissions(healthParameter);
		if (readPerm is null && writePerm is null)
		{
			throw new HealthException($"Not supported on Android: {healthParameter}");
		}

		// Already granted — don't prompt again.
		if (await CheckPermissionAsync(healthParameter, permissionType, cancellationToken).ConfigureAwait(false))
		{
			return true;
		}

		var permissions = new Java.Util.HashSet();
		if (permissionType.HasFlag(PermissionType.Read) && readPerm is not null)
			permissions.Add(new Java.Lang.String(readPerm));
		if (permissionType.HasFlag(PermissionType.Write) && writePerm is not null)
			permissions.Add(new Java.Lang.String(writePerm));

		await RequestFromHealthConnectAsync(permissions, cancellationToken).ConfigureAwait(false);

		return await CheckPermissionAsync(healthParameter, permissionType, cancellationToken).ConfigureAwait(false);
	}

	// ── RequestPermissionsAsync (batched) ─────────────────────────────────────

	public async Task<bool> RequestPermissionsAsync(
		IEnumerable<(HealthParameter healthParameter, PermissionType permissionType)> requests,
		CancellationToken cancellationToken = default)
	{
		var required = new HashSet<string>();
		var permissions = new Java.Util.HashSet();

		foreach (var (parameter, permissionType) in requests)
		{
			var (readPerm, writePerm) = GetPermissions(parameter);
			if (permissionType.HasFlag(PermissionType.Read) && readPerm is not null)
			{
				required.Add(readPerm);
				permissions.Add(new Java.Lang.String(readPerm));
			}
			if (permissionType.HasFlag(PermissionType.Write) && writePerm is not null)
			{
				required.Add(writePerm);
				permissions.Add(new Java.Lang.String(writePerm));
			}
		}

		if (required.Count == 0)
		{
			return true; // nothing supported on Android in this set
		}

		if (await AreAllGrantedAsync(required, cancellationToken).ConfigureAwait(false))
		{
			return true;
		}

		await RequestFromHealthConnectAsync(permissions, cancellationToken).ConfigureAwait(false);

		return await AreAllGrantedAsync(required, cancellationToken).ConfigureAwait(false);
	}

	async Task<bool> AreAllGrantedAsync(HashSet<string> required, CancellationToken cancellationToken)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var grantedRaw = await InvokeCoroutine(
				c => client.PermissionController.GetGrantedPermissions(c)).ConfigureAwait(false);

			var granted = new HashSet<string>();
			if (grantedRaw is Java.Lang.Object rawObj)
			{
				var iter = Java.Interop.JavaObjectExtensions.JavaCast<Java.Util.AbstractCollection>(rawObj)?.Iterator();
				if (iter != null)
				{
					while (iter.HasNext)
						granted.Add(iter.Next()?.ToString() ?? "");
				}
			}

			return required.All(granted.Contains);
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// ── RequestWorkoutPermissionAsync ─────────────────────────────────────────

	public async Task<bool> RequestWorkoutPermissionAsync(PermissionType permissionType,
		CancellationToken cancellationToken = default)
	{
		if (await CheckWorkoutPermissionAsync(permissionType, cancellationToken).ConfigureAwait(false))
		{
			return true;
		}

		var permissions = new Java.Util.HashSet();
		if (permissionType.HasFlag(PermissionType.Read))
		{
			permissions.Add(new Java.Lang.String("android.permission.health.READ_EXERCISE"));
			permissions.Add(new Java.Lang.String("android.permission.health.READ_EXERCISE_ROUTES"));
		}
		if (permissionType.HasFlag(PermissionType.Write))
		{
			permissions.Add(new Java.Lang.String("android.permission.health.WRITE_EXERCISE"));
			permissions.Add(new Java.Lang.String("android.permission.health.WRITE_EXERCISE_ROUTE"));
		}

		await RequestFromHealthConnectAsync(permissions, cancellationToken).ConfigureAwait(false);

		return await CheckWorkoutPermissionAsync(permissionType, cancellationToken).ConfigureAwait(false);
	}

	// Launches the Health Connect consent UI for the given permission strings and completes when the
	// user returns. Registers against the current Activity's ActivityResultRegistry at call time (the
	// non-lifecycle overload, valid at any time) so consumers don't have to wire up their own launcher.
	static Task RequestFromHealthConnectAsync(Java.Util.HashSet permissions, CancellationToken cancellationToken)
	{
		if (permissions.IsEmpty)
		{
			return Task.CompletedTask;
		}

		if (Platform.CurrentActivity is not AndroidX.Activity.ComponentActivity activity)
		{
			throw new HealthException(
				"Requesting Health Connect permissions requires the current Activity to derive from AndroidX " +
				"ComponentActivity (e.g. MauiAppCompatActivity).");
		}

		var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

		// Registering and launching must happen on the UI thread.
		MainThread.BeginInvokeOnMainThread(() =>
		{
			try
			{
				var contract = PermissionController.CreateRequestPermissionResultContract();
				var key = "plugin_maui_health_" + Guid.NewGuid().ToString("N");

				AndroidX.Activity.Result.ActivityResultLauncher? launcher = null;
				var callback = new PermissionResultCallback(() =>
				{
					launcher?.Unregister();
					tcs.TrySetResult();
				});

				launcher = activity.ActivityResultRegistry.Register(key, contract, callback);
				launcher.Launch(permissions);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(HealthException.Wrap(ex, "Android", operation: "RequestPermission"));
			}
		});

		return tcs.Task;
	}

	// ── ReadCountAsync ────────────────────────────────────────────────────────

	public async Task<double> ReadCountAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var timeFilter = TimeRangeFilter.Between(from.ToInstant(), until.ToInstant());

			switch (healthParameter)
			{
				case HealthParameter.StepCount:
				{
					var req = new AggregateRequest(
						new List<AggregateMetric> { StepsRecord.CountTotal },
						timeFilter,
						new List<DataOrigin>());
					var resultObj = await InvokeCoroutine(c => client.Aggregate(req, c)).ConfigureAwait(false);
					var result = (AggregationResult)resultObj!;
					var val = result.Get(StepsRecord.CountTotal);
					return val is Java.Lang.Long l ? (double)l.LongValue() : 0d;
				}
				case HealthParameter.FlightsClimbed:
				{
					var req = new AggregateRequest(
						new List<AggregateMetric> { FloorsClimbedRecord.FloorsClimbedTotal },
						timeFilter,
						new List<DataOrigin>());
					var resultObj = await InvokeCoroutine(c => client.Aggregate(req, c)).ConfigureAwait(false);
					var result = (AggregationResult)resultObj!;
					var val = result.Get(FloorsClimbedRecord.FloorsClimbedTotal);
					return val is Java.Lang.Double d ? d.DoubleValue() : 0d;
				}
				case HealthParameter.DistanceWalkingRunning:
				case HealthParameter.DistanceCycling:
				{
					var req = new AggregateRequest(
						new List<AggregateMetric> { DistanceRecord.DistanceTotal },
						timeFilter,
						new List<DataOrigin>());
					var resultObj = await InvokeCoroutine(c => client.Aggregate(req, c)).ConfigureAwait(false);
					var result = (AggregationResult)resultObj!;
					var val = result.Get(DistanceRecord.DistanceTotal);
					return val is Length len ? len.Meters : 0d;
				}
				case HealthParameter.ActiveEnergyBurned:
				{
					var req = new AggregateRequest(
						new List<AggregateMetric> { ActiveCaloriesBurnedRecord.ActiveCaloriesTotal },
						timeFilter,
						new List<DataOrigin>());
					var resultObj = await InvokeCoroutine(c => client.Aggregate(req, c)).ConfigureAwait(false);
					var result = (AggregationResult)resultObj!;
					var val = result.Get(ActiveCaloriesBurnedRecord.ActiveCaloriesTotal);
					return val is Energy energy ? energy.Kilocalories : 0d;
				}
				default:
				{
					// For all other supported params, sum samples
					var samples = await ReadAllInternalAsync(healthParameter, client, timeFilter, "").ConfigureAwait(false);
					return samples.Sum(s => s.Value ?? 0d);
				}
			}
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// ── ReadLatestAsync ───────────────────────────────────────────────────────

	public async Task<double?> ReadLatestAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var timeFilter = TimeRangeFilter.Between(from.ToInstant(), until.ToInstant());
			var sample = await ReadLatestSampleInternalAsync(healthParameter, client, timeFilter, unit).ConfigureAwait(false);
			return sample?.Value;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// ── ReadLatestAvailableAsync ──────────────────────────────────────────────

	public async Task<Sample?> ReadLatestAvailableAsync(HealthParameter healthParameter, string unit,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var timeFilter = TimeRangeFilter.Before(Java.Time.Instant.Now()!);
			return await ReadLatestSampleInternalAsync(healthParameter, client, timeFilter, unit).ConfigureAwait(false);
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// ── ReadAverageAsync / ReadMinAsync / ReadMaxAsync ────────────────────────

	// These methods intentionally do not acquire the semaphore; they delegate to ReadAllAsync which does.
	// Do NOT add semaphore.WaitAsync() here.
	public async Task<double?> ReadAverageAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var samples = await ReadAllAsync(healthParameter, from, until, unit, cancellationToken).ConfigureAwait(false);
			return samples.Count > 0 ? samples.Average(s => s.Value ?? 0d) : null;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
	}

	public async Task<double?> ReadMinAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var samples = await ReadAllAsync(healthParameter, from, until, unit, cancellationToken).ConfigureAwait(false);
			return samples.Count > 0 ? samples.Min(s => s.Value ?? 0d) : null;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
	}

	public async Task<double?> ReadMaxAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var samples = await ReadAllAsync(healthParameter, from, until, unit, cancellationToken).ConfigureAwait(false);
			return samples.Count > 0 ? samples.Max(s => s.Value ?? 0d) : null;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
	}

	// ── ReadAllAsync ──────────────────────────────────────────────────────────

	public async Task<List<Sample>> ReadAllAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var timeFilter = TimeRangeFilter.Between(from.ToInstant(), until.ToInstant());
			return await ReadAllInternalAsync(healthParameter, client, timeFilter, unit).ConfigureAwait(false);
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// ── WriteAsync ────────────────────────────────────────────────────────────

	public async Task<bool> WriteAsync(HealthParameter healthParameter, DateTimeOffset? date, double valueToStore, string unit,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var ts = date ?? DateTimeOffset.UtcNow;
			var startInstant = ts.ToInstant();
			var endInstant = ts.AddSeconds(1).ToInstant();
			var metadata = Metadata.ManualEntry();

			var record = BuildRecordForWrite(healthParameter, valueToStore, startInstant, endInstant, metadata);

			await InvokeCoroutine(c => client.InsertRecords(
				new List<IRecord> { Java.Interop.JavaObjectExtensions.JavaCast<IRecord>(record)! }, c))
				.ConfigureAwait(false);
			return true;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// Builds the Health Connect record for one (parameter, value, time) write. Shared by WriteAsync and
	// WriteAllAsync so the parameter→record mapping lives in a single place.
	static Java.Lang.Object BuildRecordForWrite(HealthParameter healthParameter, double valueToStore,
		Java.Time.Instant startInstant, Java.Time.Instant endInstant, Metadata metadata)
		=> healthParameter switch
		{
			HealthParameter.StepCount => new StepsRecord(
				startInstant, null, endInstant, null,
				(long)valueToStore, metadata),

				HealthParameter.HeartRate => new HeartRateRecord(
					startInstant, null, endInstant, null,
					new List<HeartRateRecord.Sample>
					{
						new HeartRateRecord.Sample(startInstant, (long)valueToStore)
					}, metadata),

				HealthParameter.RestingHeartRate => new RestingHeartRateRecord(
					startInstant, null, (long)valueToStore, metadata),

				HealthParameter.BodyMass => new WeightRecord(
					startInstant, null, Mass.InvokeKilograms(valueToStore), metadata),

				HealthParameter.Height => new HeightRecord(
					startInstant, null, Length.InvokeMeters(valueToStore), metadata),

				HealthParameter.BodyFatPercentage => new BodyFatRecord(
					startInstant, null, new Percentage(valueToStore / 100.0), metadata),

				HealthParameter.LeanBodyMass => new LeanBodyMassRecord(
					startInstant, null, Mass.InvokeKilograms(valueToStore), metadata),

				HealthParameter.DistanceWalkingRunning or HealthParameter.DistanceCycling =>
					new DistanceRecord(startInstant, null, endInstant, null,
						Length.InvokeMeters(valueToStore), metadata),

				HealthParameter.ActiveEnergyBurned => new ActiveCaloriesBurnedRecord(
					startInstant, null, endInstant, null,
					Energy.InvokeKilocalories(valueToStore), metadata),

				HealthParameter.BasalEnergyBurned => new BasalMetabolicRateRecord(
					startInstant, null, Power.InvokeWatts(valueToStore), metadata),

				// BloodGlucoseRecord: metadata before the value fields
				HealthParameter.BloodGlucose => new BloodGlucoseRecord(
					startInstant, null, metadata,
					BloodGlucose.InvokeMillimolesPerLiter(valueToStore),
					0,  // specimenSource unknown
					0,  // mealType unknown
					BloodGlucoseRecord.RelationToMealUnknown),

				// BloodPressureRecord: metadata before pressure values
				HealthParameter.BloodPressureSystolic => new BloodPressureRecord(
					startInstant, null, metadata,
					Pressure.InvokeMillimetersOfMercury(valueToStore),
					Pressure.InvokeMillimetersOfMercury(0),
					BloodPressureRecord.BodyPositionUnknown,
					BloodPressureRecord.MeasurementLocationUnknown),

				HealthParameter.BloodPressureDiastolic => new BloodPressureRecord(
					startInstant, null, metadata,
					Pressure.InvokeMillimetersOfMercury(0),
					Pressure.InvokeMillimetersOfMercury(valueToStore),
					BloodPressureRecord.BodyPositionUnknown,
					BloodPressureRecord.MeasurementLocationUnknown),

				HealthParameter.OxygenSaturation => new OxygenSaturationRecord(
					startInstant, null, new Percentage(valueToStore / 100.0), metadata),

				// BodyTemperatureRecord: metadata before temperature and location
				HealthParameter.BodyTemperature => new BodyTemperatureRecord(
					startInstant, null, metadata,
					Temperature.InvokeCelsius(valueToStore),
					BodyTemperatureMeasurementLocation.MeasurementLocationUnknown),

				HealthParameter.BasalBodyTemperature => new BasalBodyTemperatureRecord(
					startInstant, null, metadata,
					Temperature.InvokeCelsius(valueToStore),
					BodyTemperatureMeasurementLocation.MeasurementLocationUnknown),

				HealthParameter.RespiratoryRate => new RespiratoryRateRecord(
					startInstant, null, valueToStore, metadata),

				// Vo2MaxRecord: metadata before value and method
				HealthParameter.VO2Max => new Vo2MaxRecord(
					startInstant, null, metadata,
					valueToStore,
					Vo2MaxRecord.MeasurementMethodOther),

				HealthParameter.FlightsClimbed => new FloorsClimbedRecord(
					startInstant, null, endInstant, null, valueToStore, metadata),

				HealthParameter.TotalEnergyBurned => new TotalCaloriesBurnedRecord(
					startInstant, null, endInstant, null,
					Energy.InvokeKilocalories(valueToStore), metadata),

				HealthParameter.ElevationGained => new ElevationGainedRecord(
					startInstant, null, endInstant, null,
					Length.InvokeMeters(valueToStore), metadata),

				HealthParameter.PushCount => new WheelchairPushesRecord(
					startInstant, null, endInstant, null,
					(long)valueToStore, metadata),

				HealthParameter.BoneMass => new BoneMassRecord(
					startInstant, null, Mass.InvokeKilograms(valueToStore), metadata),

				HealthParameter.BodyWaterMass => new BodyWaterMassRecord(
					startInstant, null, Mass.InvokeKilograms(valueToStore), metadata),

				HealthParameter.WalkingSpeed or HealthParameter.RunningSpeed =>
					new SpeedRecord(startInstant, null, endInstant, null,
						new List<SpeedRecord.Sample>
						{
							new SpeedRecord.Sample(startInstant, Velocity.InvokeMetersPerSecond(valueToStore))
						}, metadata),

				HealthParameter.HeartRateVariabilitySdnn => new HeartRateVariabilityRmssdRecord(
					startInstant, null, valueToStore, metadata),

				HealthParameter.RunningPower => new PowerRecord(
					startInstant, null, endInstant, null,
					new List<PowerRecord.Sample>
					{
						new PowerRecord.Sample(startInstant, Power.InvokeWatts(valueToStore))
					}, metadata),

				HealthParameter.DietaryWater => new HydrationRecord(
					startInstant, null, endInstant, null,
					Volume.InvokeLiters(valueToStore), metadata),

				HealthParameter.DietaryBiotin or HealthParameter.DietaryCaffeine or
				HealthParameter.DietaryCalcium or HealthParameter.DietaryCarbohydrates or
				HealthParameter.DietaryChloride or HealthParameter.DietaryCholesterol or
				HealthParameter.DietaryChromium or HealthParameter.DietaryCopper or
				HealthParameter.DietaryEnergyConsumed or HealthParameter.DietaryFatMonounsaturated or
				HealthParameter.DietaryFatPolyunsaturated or HealthParameter.DietaryFatSaturated or
				HealthParameter.DietaryFatTotal or HealthParameter.DietaryFiber or
				HealthParameter.DietaryFolate or HealthParameter.DietaryIodine or
				HealthParameter.DietaryIron or HealthParameter.DietaryMagnesium or
				HealthParameter.DietaryManganese or HealthParameter.DietaryMolybdenum or
				HealthParameter.DietaryNiacin or HealthParameter.DietaryPantothenicAcid or
				HealthParameter.DietaryPhosphorus or HealthParameter.DietaryPotassium or
				HealthParameter.DietaryProtein or HealthParameter.DietaryRiboflavin or
				HealthParameter.DietarySelenium or HealthParameter.DietarySodium or
				HealthParameter.DietarySugar or HealthParameter.DietaryThiamin or
				HealthParameter.DietaryVitaminA or HealthParameter.DietaryVitaminB12 or
				HealthParameter.DietaryVitaminB6 or HealthParameter.DietaryVitaminC or
				HealthParameter.DietaryVitaminD or HealthParameter.DietaryVitaminE or
				HealthParameter.DietaryVitaminK or HealthParameter.DietaryZinc =>
					BuildNutritionRecord(healthParameter, valueToStore, startInstant, endInstant, metadata),

			_ => throw new HealthException($"Not supported on Android: {healthParameter}"),
		};

	public async Task<bool> WriteAllAsync(HealthParameter healthParameter, IEnumerable<(DateTimeOffset date, double value)> values,
		string unit, CancellationToken cancellationToken = default)
	{
		var list = values as IReadOnlyList<(DateTimeOffset date, double value)> ?? values.ToList();
		if (list.Count == 0)
		{
			return true;
		}

		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var records = new List<IRecord>(list.Count);
			foreach (var (date, value) in list)
			{
				var start = date.ToInstant();
				var end = date.AddSeconds(1).ToInstant();
				var record = BuildRecordForWrite(healthParameter, value, start, end, Metadata.ManualEntry());
				records.Add(Java.Interop.JavaObjectExtensions.JavaCast<IRecord>(record)!);
			}

			await InvokeCoroutine(c => client.InsertRecords(records, c)).ConfigureAwait(false);
			return true;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	public async Task<bool> DeleteAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default)
	{
		var recordClass = GetRecordClass(healthParameter)
			?? throw new HealthException($"Not supported on Android: {healthParameter}");

		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var timeFilter = TimeRangeFilter.Between(from.ToInstant(), until.ToInstant());
			await InvokeCoroutine(c => client.DeleteRecords(recordClass, timeFilter, c)).ConfigureAwait(false);
			return true;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// Maps a parameter to its Health Connect record KClass for delete-by-time-range. Parameters that
	// share a record type (blood pressure, distance, speed, all nutrients) resolve to that shared record.
	static IKClass? GetRecordClass(HealthParameter param) => param switch
	{
		HealthParameter.StepCount => GetKClass<StepsRecord>(),
		HealthParameter.HeartRate => GetKClass<HeartRateRecord>(),
		HealthParameter.RestingHeartRate => GetKClass<RestingHeartRateRecord>(),
		HealthParameter.HeartRateVariabilitySdnn => GetKClass<HeartRateVariabilityRmssdRecord>(),
		HealthParameter.BodyMass => GetKClass<WeightRecord>(),
		HealthParameter.Height => GetKClass<HeightRecord>(),
		HealthParameter.BodyFatPercentage => GetKClass<BodyFatRecord>(),
		HealthParameter.LeanBodyMass => GetKClass<LeanBodyMassRecord>(),
		HealthParameter.BoneMass => GetKClass<BoneMassRecord>(),
		HealthParameter.BodyWaterMass => GetKClass<BodyWaterMassRecord>(),
		HealthParameter.DistanceWalkingRunning or HealthParameter.DistanceCycling => GetKClass<DistanceRecord>(),
		HealthParameter.ActiveEnergyBurned => GetKClass<ActiveCaloriesBurnedRecord>(),
		HealthParameter.TotalEnergyBurned => GetKClass<TotalCaloriesBurnedRecord>(),
		HealthParameter.BasalEnergyBurned => GetKClass<BasalMetabolicRateRecord>(),
		HealthParameter.ElevationGained => GetKClass<ElevationGainedRecord>(),
		HealthParameter.PushCount => GetKClass<WheelchairPushesRecord>(),
		HealthParameter.BloodGlucose => GetKClass<BloodGlucoseRecord>(),
		HealthParameter.BloodPressureSystolic or HealthParameter.BloodPressureDiastolic => GetKClass<BloodPressureRecord>(),
		HealthParameter.OxygenSaturation => GetKClass<OxygenSaturationRecord>(),
		HealthParameter.BodyTemperature => GetKClass<BodyTemperatureRecord>(),
		HealthParameter.BasalBodyTemperature => GetKClass<BasalBodyTemperatureRecord>(),
		HealthParameter.RespiratoryRate => GetKClass<RespiratoryRateRecord>(),
		HealthParameter.VO2Max => GetKClass<Vo2MaxRecord>(),
		HealthParameter.FlightsClimbed => GetKClass<FloorsClimbedRecord>(),
		HealthParameter.WalkingSpeed or HealthParameter.RunningSpeed => GetKClass<SpeedRecord>(),
		HealthParameter.RunningPower => GetKClass<PowerRecord>(),
		HealthParameter.ExerciseTime => GetKClass<ExerciseSessionRecord>(),
		HealthParameter.DietaryWater => GetKClass<HydrationRecord>(),
		HealthParameter.DietaryBiotin or HealthParameter.DietaryCaffeine or HealthParameter.DietaryCalcium or
		HealthParameter.DietaryCarbohydrates or HealthParameter.DietaryChloride or HealthParameter.DietaryCholesterol or
		HealthParameter.DietaryChromium or HealthParameter.DietaryCopper or HealthParameter.DietaryEnergyConsumed or
		HealthParameter.DietaryFatMonounsaturated or HealthParameter.DietaryFatPolyunsaturated or HealthParameter.DietaryFatSaturated or
		HealthParameter.DietaryFatTotal or HealthParameter.DietaryFiber or HealthParameter.DietaryFolate or
		HealthParameter.DietaryIodine or HealthParameter.DietaryIron or HealthParameter.DietaryMagnesium or
		HealthParameter.DietaryManganese or HealthParameter.DietaryMolybdenum or HealthParameter.DietaryNiacin or
		HealthParameter.DietaryPantothenicAcid or HealthParameter.DietaryPhosphorus or HealthParameter.DietaryPotassium or
		HealthParameter.DietaryProtein or HealthParameter.DietaryRiboflavin or HealthParameter.DietarySelenium or
		HealthParameter.DietarySodium or HealthParameter.DietarySugar or HealthParameter.DietaryThiamin or
		HealthParameter.DietaryVitaminA or HealthParameter.DietaryVitaminB12 or HealthParameter.DietaryVitaminB6 or
		HealthParameter.DietaryVitaminC or HealthParameter.DietaryVitaminD or HealthParameter.DietaryVitaminE or
		HealthParameter.DietaryVitaminK or HealthParameter.DietaryZinc => GetKClass<NutritionRecord>(),
		_ => null,
	};

	// ── Workout methods ───────────────────────────────────────────────────────

	public async Task<List<WorkoutSession>> ReadWorkoutsAsync(WorkoutType workoutType, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var timeFilter = TimeRangeFilter.Between(from.ToInstant(), until.ToInstant());
			var klass = GetKClass<ExerciseSessionRecord>();
			var request = new ReadRecordsRequest(klass, timeFilter, new List<DataOrigin>(), true, 1000, null);
			var respObj = await InvokeCoroutine(c => client.ReadRecords(request, c)).ConfigureAwait(false);
			var response = (ReadRecordsResponse)respObj!;

			var targetType = workoutType == WorkoutType.Other ? (int?)null : MapWorkoutType(workoutType);
			var sessions = new List<WorkoutSession>();

			foreach (var rawRecord in response.Records)
			{
				if (rawRecord is not ExerciseSessionRecord record)
				{
					continue;
				}

				if (targetType.HasValue && record.ExerciseType != targetType.Value)
				{
					continue;
				}

				sessions.Add(MapExerciseSessionRecord(record, workoutType));
			}

			return sessions;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	public async Task<WorkoutSession?> ReadLatestWorkoutAsync(WorkoutType workoutType, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var timeFilter = TimeRangeFilter.Between(from.ToInstant(), until.ToInstant());
			var klass = GetKClass<ExerciseSessionRecord>();
			var request = new ReadRecordsRequest(klass, timeFilter, new List<DataOrigin>(), false, 50, null);
			var respObj = await InvokeCoroutine(c => client.ReadRecords(request, c)).ConfigureAwait(false);
			var response = (ReadRecordsResponse)respObj!;

			var targetType = workoutType == WorkoutType.Other ? (int?)null : MapWorkoutType(workoutType);

			foreach (var rawRecord in response.Records)
			{
				if (rawRecord is not ExerciseSessionRecord record)
				{
					continue;
				}

				if (targetType.HasValue && record.ExerciseType != targetType.Value)
				{
					continue;
				}

				return MapExerciseSessionRecord(record, workoutType);
			}

			return null;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	public async Task<bool> WriteWorkoutAsync(WorkoutSession session, CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var exerciseType = MapWorkoutType(session.WorkoutType);

			ExerciseRoute? exerciseRoute = null;
			if (session.Route.Count > 0)
			{
				var locations = session.Route.Select(p => new ExerciseRoute.Location(
					p.Time.ToInstant(),
					p.Latitude,
					p.Longitude,
					p.HorizontalAccuracyInMeters is double ha ? Length.InvokeMeters(ha) : null,
					p.VerticalAccuracyInMeters is double va ? Length.InvokeMeters(va) : null,
					p.AltitudeInMeters is double alt ? Length.InvokeMeters(alt) : null
				)).ToList();
				exerciseRoute = new ExerciseRoute(locations);
			}

			// ExerciseSessionRecord: metadata comes before exerciseType
			var record = new ExerciseSessionRecord(
				session.From.ToInstant(), null,
				session.Until.ToInstant(), null,
				Metadata.ManualEntry(),
				exerciseType,
				session.Title,
				null,
				new List<ExerciseSegment>(),
				new List<ExerciseLap>(),
				exerciseRoute);

			await InvokeCoroutine(c => client.InsertRecords(new[] { Java.Interop.JavaObjectExtensions.JavaCast<IRecord>(record)! }, c)).ConfigureAwait(false);
			return true;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	// ── Sleep methods ─────────────────────────────────────────────────────────

	public async Task<bool> RequestSleepPermissionAsync(PermissionType permissionType, CancellationToken cancellationToken = default)
	{
		const string readPerm = "android.permission.health.READ_SLEEP";
		const string writePerm = "android.permission.health.WRITE_SLEEP";

		var required = new HashSet<string>();
		var permissions = new Java.Util.HashSet();
		if (permissionType.HasFlag(PermissionType.Read)) { required.Add(readPerm); permissions.Add(new Java.Lang.String(readPerm)); }
		if (permissionType.HasFlag(PermissionType.Write)) { required.Add(writePerm); permissions.Add(new Java.Lang.String(writePerm)); }

		if (required.Count == 0)
		{
			return true;
		}

		if (await AreAllGrantedAsync(required, cancellationToken).ConfigureAwait(false))
		{
			return true;
		}

		await RequestFromHealthConnectAsync(permissions, cancellationToken).ConfigureAwait(false);

		return await AreAllGrantedAsync(required, cancellationToken).ConfigureAwait(false);
	}

	public async Task<List<SleepSession>> ReadSleepAsync(DateTimeOffset from, DateTimeOffset until, CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();
			var timeFilter = TimeRangeFilter.Between(from.ToInstant(), until.ToInstant());
			var klass = GetKClass<SleepSessionRecord>();
			var request = new ReadRecordsRequest(klass, timeFilter, new List<DataOrigin>(), true, 1000, null);

			var respObj = await InvokeCoroutine(c => client.ReadRecords(request, c)).ConfigureAwait(false);
			var response = (ReadRecordsResponse)respObj!;

			var result = new List<SleepSession>();
			foreach (var raw in response.Records)
			{
				if (raw is not SleepSessionRecord record)
				{
					continue;
				}

				var stages = new List<SleepStageSample>();
				foreach (var stageObj in record.Stages)
				{
					if (stageObj is SleepSessionRecord.Stage stage)
					{
						stages.Add(new SleepStageSample
						{
							From = stage.StartTime.ToDateTimeUtc(),
							Until = stage.EndTime.ToDateTimeUtc(),
							Stage = MapAndroidSleepStage(stage.GetStage()),
						});
					}
				}

				var f = record.StartTime.ToDateTimeUtc();
				var u = record.EndTime.ToDateTimeUtc();
				result.Add(new SleepSession
				{
					From = f,
					Until = u,
					DurationInSeconds = (u - f).TotalSeconds,
					Source = record.Metadata?.DataOrigin?.PackageName,
					Stages = stages,
				});
			}

			return result;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	public async Task<bool> WriteSleepAsync(SleepSession session, CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			var client = GetClient();

			var stages = new List<SleepSessionRecord.Stage>();
			foreach (var s in session.Stages)
			{
				stages.Add(new SleepSessionRecord.Stage(s.From.ToInstant(), s.Until.ToInstant(), MapToAndroidSleepStage(s.Stage)));
			}

			var record = new SleepSessionRecord(
				session.From.ToInstant(), null,
				session.Until.ToInstant(), null,
				Metadata.ManualEntry(),
				title: null,
				notes: null,
				stages);

			await InvokeCoroutine(c => client.InsertRecords(
				new[] { Java.Interop.JavaObjectExtensions.JavaCast<IRecord>(record)! }, c)).ConfigureAwait(false);
			return true;
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw HealthException.Wrap(ex, "Android");
		}
		finally
		{
			semaphore.Release();
		}
	}

	static SleepStage MapAndroidSleepStage(int stage)
	{
		if (stage == SleepSessionRecord.StageTypeAwake) return SleepStage.Awake;
		if (stage == SleepSessionRecord.StageTypeAwakeInBed) return SleepStage.InBed;
		if (stage == SleepSessionRecord.StageTypeOutOfBed) return SleepStage.Awake;
		if (stage == SleepSessionRecord.StageTypeSleeping) return SleepStage.Sleeping;
		if (stage == SleepSessionRecord.StageTypeLight) return SleepStage.Light;
		if (stage == SleepSessionRecord.StageTypeDeep) return SleepStage.Deep;
		if (stage == SleepSessionRecord.StageTypeRem) return SleepStage.Rem;
		return SleepStage.Unknown;
	}

	static int MapToAndroidSleepStage(SleepStage stage) => stage switch
	{
		SleepStage.Awake => SleepSessionRecord.StageTypeAwake,
		SleepStage.InBed => SleepSessionRecord.StageTypeAwakeInBed,
		SleepStage.Sleeping => SleepSessionRecord.StageTypeSleeping,
		SleepStage.Light => SleepSessionRecord.StageTypeLight,
		SleepStage.Deep => SleepSessionRecord.StageTypeDeep,
		SleepStage.Rem => SleepSessionRecord.StageTypeRem,
		_ => SleepSessionRecord.StageTypeUnknown,
	};

	// ── Private helpers ───────────────────────────────────────────────────────

	async Task<List<Sample>> ReadAllInternalAsync(
		HealthParameter healthParameter,
		IHealthConnectClient client,
		TimeRangeFilter timeFilter,
		string unit)
	{
		return healthParameter switch
		{
			HealthParameter.StepCount => await ReadSimpleRecordsAsync<StepsRecord>(
				client, timeFilter, unit,
				r => ((double)r.Count, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.HeartRate => await ReadHeartRateRecordsAsync(client, timeFilter, unit),

			HealthParameter.RestingHeartRate => await ReadSimpleInstantRecordsAsync<RestingHeartRateRecord>(
				client, timeFilter, unit,
				r => ((double)r.BeatsPerMinute, r.Time.ToDateTimeUtc())),

			HealthParameter.BodyMass => await ReadSimpleInstantRecordsAsync<WeightRecord>(
				client, timeFilter, unit,
				r => (r.Weight.Kilograms, r.Time.ToDateTimeUtc())),

			HealthParameter.Height => await ReadSimpleInstantRecordsAsync<HeightRecord>(
				client, timeFilter, unit,
				r => (r.Height.Meters, r.Time.ToDateTimeUtc())),

			HealthParameter.BodyFatPercentage => await ReadSimpleInstantRecordsAsync<BodyFatRecord>(
				client, timeFilter, unit,
				r => (r.Percentage.Value * 100.0, r.Time.ToDateTimeUtc())),

			HealthParameter.LeanBodyMass => await ReadSimpleInstantRecordsAsync<LeanBodyMassRecord>(
				client, timeFilter, unit,
				r => (r.Mass.Kilograms, r.Time.ToDateTimeUtc())),

			HealthParameter.DistanceWalkingRunning or HealthParameter.DistanceCycling =>
				await ReadSimpleRecordsAsync<DistanceRecord>(
					client, timeFilter, unit,
					r => (r.Distance.Meters, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.ActiveEnergyBurned => await ReadSimpleRecordsAsync<ActiveCaloriesBurnedRecord>(
				client, timeFilter, unit,
				r => (r.Energy.Kilocalories, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.BasalEnergyBurned => await ReadSimpleInstantRecordsAsync<BasalMetabolicRateRecord>(
				client, timeFilter, unit,
				r => (r.BasalMetabolicRate.Watts, r.Time.ToDateTimeUtc())),

			HealthParameter.BloodGlucose => await ReadSimpleInstantRecordsAsync<BloodGlucoseRecord>(
				client, timeFilter, unit,
				r => (r.Level.MillimolesPerLiter, r.Time.ToDateTimeUtc())),

			HealthParameter.BloodPressureSystolic => await ReadSimpleInstantRecordsAsync<BloodPressureRecord>(
				client, timeFilter, unit,
				r => (r.Systolic.MillimetersOfMercury, r.Time.ToDateTimeUtc())),

			HealthParameter.BloodPressureDiastolic => await ReadSimpleInstantRecordsAsync<BloodPressureRecord>(
				client, timeFilter, unit,
				r => (r.Diastolic.MillimetersOfMercury, r.Time.ToDateTimeUtc())),

			HealthParameter.OxygenSaturation => await ReadSimpleInstantRecordsAsync<OxygenSaturationRecord>(
				client, timeFilter, unit,
				r => (r.Percentage.Value * 100.0, r.Time.ToDateTimeUtc())),

			HealthParameter.BodyTemperature => await ReadSimpleInstantRecordsAsync<BodyTemperatureRecord>(
				client, timeFilter, unit,
				r => (r.Temperature.Celsius, r.Time.ToDateTimeUtc())),

			HealthParameter.BasalBodyTemperature => await ReadSimpleInstantRecordsAsync<BasalBodyTemperatureRecord>(
				client, timeFilter, unit,
				r => (r.Temperature.Celsius, r.Time.ToDateTimeUtc())),

			HealthParameter.RespiratoryRate => await ReadSimpleInstantRecordsAsync<RespiratoryRateRecord>(
				client, timeFilter, unit,
				r => (r.Rate, r.Time.ToDateTimeUtc())),

			HealthParameter.VO2Max => await ReadSimpleInstantRecordsAsync<Vo2MaxRecord>(
				client, timeFilter, unit,
				r => (r.Vo2MillilitersPerMinuteKilogram, r.Time.ToDateTimeUtc())),

			HealthParameter.FlightsClimbed => await ReadSimpleRecordsAsync<FloorsClimbedRecord>(
				client, timeFilter, unit,
				r => (r.Floors, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.TotalEnergyBurned => await ReadSimpleRecordsAsync<TotalCaloriesBurnedRecord>(
				client, timeFilter, unit,
				r => (r.Energy.Kilocalories, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.ElevationGained => await ReadSimpleRecordsAsync<ElevationGainedRecord>(
				client, timeFilter, unit,
				r => (r.Elevation.Meters, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.PushCount => await ReadSimpleRecordsAsync<WheelchairPushesRecord>(
				client, timeFilter, unit,
				r => ((double)r.Count, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.BoneMass => await ReadSimpleInstantRecordsAsync<BoneMassRecord>(
				client, timeFilter, unit,
				r => (r.Mass.Kilograms, r.Time.ToDateTimeUtc())),

			HealthParameter.BodyWaterMass => await ReadSimpleInstantRecordsAsync<BodyWaterMassRecord>(
				client, timeFilter, unit,
				r => (r.Mass.Kilograms, r.Time.ToDateTimeUtc())),

			HealthParameter.WalkingSpeed or HealthParameter.RunningSpeed =>
				await ReadSpeedRecordsAsync(client, timeFilter, unit),

			HealthParameter.HeartRateVariabilitySdnn => await ReadSimpleInstantRecordsAsync<HeartRateVariabilityRmssdRecord>(
				client, timeFilter, unit,
				r => (r.HeartRateVariabilityMillis, r.Time.ToDateTimeUtc())),

			HealthParameter.RunningPower => await ReadPowerRecordsAsync(client, timeFilter, unit),

			HealthParameter.ExerciseTime => await ReadSimpleRecordsAsync<ExerciseSessionRecord>(
				client, timeFilter, unit,
				r => ((r.EndTime.ToEpochMilli() - r.StartTime.ToEpochMilli()) / 1000.0,
					r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.DietaryWater => await ReadSimpleRecordsAsync<HydrationRecord>(
				client, timeFilter, unit,
				r => (r.Volume.Liters, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc())),

			HealthParameter.DietaryBiotin or HealthParameter.DietaryCaffeine or
			HealthParameter.DietaryCalcium or HealthParameter.DietaryCarbohydrates or
			HealthParameter.DietaryChloride or HealthParameter.DietaryCholesterol or
			HealthParameter.DietaryChromium or HealthParameter.DietaryCopper or
			HealthParameter.DietaryEnergyConsumed or HealthParameter.DietaryFatMonounsaturated or
			HealthParameter.DietaryFatPolyunsaturated or HealthParameter.DietaryFatSaturated or
			HealthParameter.DietaryFatTotal or HealthParameter.DietaryFiber or
			HealthParameter.DietaryFolate or HealthParameter.DietaryIodine or
			HealthParameter.DietaryIron or HealthParameter.DietaryMagnesium or
			HealthParameter.DietaryManganese or HealthParameter.DietaryMolybdenum or
			HealthParameter.DietaryNiacin or HealthParameter.DietaryPantothenicAcid or
			HealthParameter.DietaryPhosphorus or HealthParameter.DietaryPotassium or
			HealthParameter.DietaryProtein or HealthParameter.DietaryRiboflavin or
			HealthParameter.DietarySelenium or HealthParameter.DietarySodium or
			HealthParameter.DietarySugar or HealthParameter.DietaryThiamin or
			HealthParameter.DietaryVitaminA or HealthParameter.DietaryVitaminB12 or
			HealthParameter.DietaryVitaminB6 or HealthParameter.DietaryVitaminC or
			HealthParameter.DietaryVitaminD or HealthParameter.DietaryVitaminE or
			HealthParameter.DietaryVitaminK or HealthParameter.DietaryZinc =>
				await ReadNutritionFieldRecordsAsync(healthParameter, client, timeFilter, unit),

			_ => throw new HealthException($"Not supported on Android: {healthParameter}"),
		};
	}

	async Task<Sample?> ReadLatestSampleInternalAsync(
		HealthParameter healthParameter,
		IHealthConnectClient client,
		TimeRangeFilter timeFilter,
		string unit)
	{
		var samples = await ReadAllInternalPagedAsync(healthParameter, client, timeFilter, unit,
			ascending: false, pageSize: 1).ConfigureAwait(false);
		return samples.Count > 0 ? samples[0] : null;
	}

	async Task<List<Sample>> ReadAllInternalPagedAsync(
		HealthParameter healthParameter,
		IHealthConnectClient client,
		TimeRangeFilter timeFilter,
		string unit,
		bool ascending,
		int pageSize)
	{
		switch (healthParameter)
		{
			case HealthParameter.StepCount:
				return await ReadPagedAsync<StepsRecord>(client, timeFilter, ascending, pageSize,
					r => ((double)r.Count, r.StartTime.ToDateTimeUtc(), r.EndTime.ToDateTimeUtc()), unit);

			case HealthParameter.HeartRate:
			{
				var klass = GetKClass<HeartRateRecord>();
				var req = new ReadRecordsRequest(klass, timeFilter, new List<DataOrigin>(), ascending, pageSize, null);
				var respObj = await InvokeCoroutine(c => client.ReadRecords(req, c)).ConfigureAwait(false);
				var resp = (ReadRecordsResponse)respObj!;
				var result = new List<Sample>();
				foreach (var rawRecord in resp.Records)
				{
					if (rawRecord is not HeartRateRecord rec)
						{
							continue;
						}

						foreach (var s in rec.Samples)
						{
							result.Add(new Sample(s.Time.ToDateTimeUtc(), s.Time.ToDateTimeUtc(),
							(double)s.BeatsPerMinute, "Health Connect", unit));
						}
					}
				return result;
			}

			case HealthParameter.RestingHeartRate:
				return await ReadPagedAsync<RestingHeartRateRecord>(client, timeFilter, ascending, pageSize,
					r => ((double)r.BeatsPerMinute, r.Time.ToDateTimeUtc(), r.Time.ToDateTimeUtc()), unit);

			case HealthParameter.BodyMass:
				return await ReadPagedAsync<WeightRecord>(client, timeFilter, ascending, pageSize,
					r => (r.Weight.Kilograms, r.Time.ToDateTimeUtc(), r.Time.ToDateTimeUtc()), unit);

			case HealthParameter.Height:
				return await ReadPagedAsync<HeightRecord>(client, timeFilter, ascending, pageSize,
					r => (r.Height.Meters, r.Time.ToDateTimeUtc(), r.Time.ToDateTimeUtc()), unit);

			case HealthParameter.BodyFatPercentage:
				return await ReadPagedAsync<BodyFatRecord>(client, timeFilter, ascending, pageSize,
					r => (r.Percentage.Value * 100.0, r.Time.ToDateTimeUtc(), r.Time.ToDateTimeUtc()), unit);

			case HealthParameter.BloodPressureSystolic:
				return await ReadPagedAsync<BloodPressureRecord>(client, timeFilter, ascending, pageSize,
					r => (r.Systolic.MillimetersOfMercury, r.Time.ToDateTimeUtc(), r.Time.ToDateTimeUtc()), unit);

			case HealthParameter.BloodPressureDiastolic:
				return await ReadPagedAsync<BloodPressureRecord>(client, timeFilter, ascending, pageSize,
					r => (r.Diastolic.MillimetersOfMercury, r.Time.ToDateTimeUtc(), r.Time.ToDateTimeUtc()), unit);

			default:
				var all = await ReadAllInternalAsync(healthParameter, client, timeFilter, unit).ConfigureAwait(false);
				if (!ascending && all.Count > 1)
				{
					all.Reverse();
				}

				return all.Count > pageSize ? all.Take(pageSize).ToList() : all;
		}
	}

	// Extracts provenance (source app, device, recording method) from a record's Health Connect metadata.
	static (string source, string? device, RecordingMethod method) Provenance(Java.Lang.Object record)
	{
		var meta = (record as IRecord)?.Metadata;
		if (meta is null)
		{
			return ("Health Connect", null, RecordingMethod.Unknown);
		}

		var source = string.IsNullOrEmpty(meta.DataOrigin?.PackageName) ? "Health Connect" : meta.DataOrigin!.PackageName;
		var device = meta.Device?.Model;
		if (string.IsNullOrEmpty(device))
		{
			device = meta.Device?.Manufacturer;
		}

		var method = meta.RecordingMethod switch
		{
			Metadata.RecordingMethodManualEntry => RecordingMethod.Manual,
			Metadata.RecordingMethodAutomaticallyRecorded => RecordingMethod.Automatic,
			Metadata.RecordingMethodActivelyRecorded => RecordingMethod.ActivelyRecorded,
			_ => RecordingMethod.Unknown,
		};

		return (source, string.IsNullOrEmpty(device) ? null : device, method);
	}

	static Sample SampleFrom(Java.Lang.Object record, DateTimeOffset from, DateTimeOffset until, double value, string unit)
	{
		var (source, device, method) = Provenance(record);
		return new Sample(from, until, value, source, unit) { Device = device, RecordingMethod = method };
	}

	// Health Connect returns at most 1,000 records per ReadRecords call. Follow the page token so long
	// ranges / high-frequency sensors aren't silently truncated at 1,000.
	static async Task<List<T>> ReadAllPagesAsync<T>(
		IHealthConnectClient client, TimeRangeFilter filter, bool ascending = true)
		where T : Java.Lang.Object
	{
		var klass = GetKClass<T>();
		var all = new List<T>();
		string? pageToken = null;
		do
		{
			var req = new ReadRecordsRequest(klass, filter, new List<DataOrigin>(), ascending, 1000, pageToken);
			var resp = (ReadRecordsResponse)(await InvokeCoroutine(c => client.ReadRecords(req, c)).ConfigureAwait(false))!;
			all.AddRange(resp.Records.OfType<T>());
			pageToken = resp.PageToken;
		}
		while (!string.IsNullOrEmpty(pageToken));

		return all;
	}

	static async Task<List<Sample>> ReadSimpleRecordsAsync<T>(
		IHealthConnectClient client,
		TimeRangeFilter filter,
		string unit,
		Func<T, (double value, DateTimeOffset from, DateTimeOffset until)> extract)
		where T : Java.Lang.Object
	{
		var records = await ReadAllPagesAsync<T>(client, filter).ConfigureAwait(false);
		return records
			.Select(r => { var (v, f, u) = extract(r); return SampleFrom(r, f, u, v, unit); })
			.ToList();
	}

	static async Task<List<Sample>> ReadSimpleInstantRecordsAsync<T>(
		IHealthConnectClient client,
		TimeRangeFilter filter,
		string unit,
		Func<T, (double value, DateTimeOffset time)> extract)
		where T : Java.Lang.Object
	{
		var records = await ReadAllPagesAsync<T>(client, filter).ConfigureAwait(false);
		return records
			.Select(r => { var (v, t) = extract(r); return SampleFrom(r, t, t, v, unit); })
			.ToList();
	}

	static async Task<List<Sample>> ReadPagedAsync<T>(
		IHealthConnectClient client,
		TimeRangeFilter filter,
		bool ascending,
		int pageSize,
		Func<T, (double value, DateTimeOffset from, DateTimeOffset until)> extract,
		string unit)
		where T : Java.Lang.Object
	{
		var klass = GetKClass<T>();
		var req = new ReadRecordsRequest(klass, filter, new List<DataOrigin>(), ascending, pageSize, null);
		var respObj = await InvokeCoroutine(c => client.ReadRecords(req, c)).ConfigureAwait(false);
		var resp = (ReadRecordsResponse)respObj!;
		return resp.Records.OfType<T>()
			.Select(r => { var (v, f, u) = extract(r); return SampleFrom(r, f, u, v, unit); })
			.ToList();
	}

	static async Task<List<Sample>> ReadHeartRateRecordsAsync(
		IHealthConnectClient client, TimeRangeFilter filter, string unit)
	{
		var records = await ReadAllPagesAsync<HeartRateRecord>(client, filter).ConfigureAwait(false);
		var result = new List<Sample>();
		foreach (var rec in records)
		{
			var (source, device, method) = Provenance(rec);
			foreach (var s in rec.Samples)
			{
				result.Add(new Sample(s.Time.ToDateTimeUtc(), s.Time.ToDateTimeUtc(),
					(double)s.BeatsPerMinute, source, unit) { Device = device, RecordingMethod = method });
			}
		}
		return result;
	}

	static async Task<List<Sample>> ReadSpeedRecordsAsync(
		IHealthConnectClient client, TimeRangeFilter filter, string unit)
	{
		var records = await ReadAllPagesAsync<SpeedRecord>(client, filter).ConfigureAwait(false);
		var result = new List<Sample>();
		foreach (var rec in records)
		{
			var (source, device, method) = Provenance(rec);
			foreach (var s in rec.Samples)
			{
				result.Add(new Sample(s.Time.ToDateTimeUtc(), s.Time.ToDateTimeUtc(),
					s.Speed.MetersPerSecond, source, unit) { Device = device, RecordingMethod = method });
			}
		}
		return result;
	}

	static async Task<List<Sample>> ReadPowerRecordsAsync(
		IHealthConnectClient client, TimeRangeFilter filter, string unit)
	{
		var records = await ReadAllPagesAsync<PowerRecord>(client, filter).ConfigureAwait(false);
		var result = new List<Sample>();
		foreach (var rec in records)
		{
			var (source, device, method) = Provenance(rec);
			foreach (var s in rec.Samples)
			{
				result.Add(new Sample(s.Time.ToDateTimeUtc(), s.Time.ToDateTimeUtc(),
					s.Power.Watts, source, unit) { Device = device, RecordingMethod = method });
			}
		}
		return result;
	}

	static async Task<List<Sample>> ReadNutritionFieldRecordsAsync(
		HealthParameter param, IHealthConnectClient client, TimeRangeFilter filter, string unit)
	{
		var records = await ReadAllPagesAsync<NutritionRecord>(client, filter).ConfigureAwait(false);
		var result = new List<Sample>();
		foreach (var rec in records)
		{
			var value = ExtractNutritionField(param, rec);
			if (value.HasValue)
			{
				result.Add(SampleFrom(rec, rec.StartTime.ToDateTimeUtc(), rec.EndTime.ToDateTimeUtc(),
					value.Value, unit));
			}
		}
		return result;
	}

	static double? ExtractNutritionField(HealthParameter param, NutritionRecord record) => param switch
	{
		HealthParameter.DietaryBiotin => record.Biotin?.Grams,
		HealthParameter.DietaryCaffeine => record.Caffeine?.Grams,
		HealthParameter.DietaryCalcium => record.Calcium?.Grams,
		HealthParameter.DietaryCarbohydrates => record.TotalCarbohydrate?.Grams,
		HealthParameter.DietaryChloride => record.Chloride?.Grams,
		HealthParameter.DietaryCholesterol => record.Cholesterol?.Grams,
		HealthParameter.DietaryChromium => record.Chromium?.Grams,
		HealthParameter.DietaryCopper => record.Copper?.Grams,
		HealthParameter.DietaryEnergyConsumed => record.Energy?.Kilocalories,
		HealthParameter.DietaryFatMonounsaturated => record.MonounsaturatedFat?.Grams,
		HealthParameter.DietaryFatPolyunsaturated => record.PolyunsaturatedFat?.Grams,
		HealthParameter.DietaryFatSaturated => record.SaturatedFat?.Grams,
		HealthParameter.DietaryFatTotal => record.TotalFat?.Grams,
		HealthParameter.DietaryFiber => record.DietaryFiber?.Grams,
		HealthParameter.DietaryFolate => record.Folate?.Grams,
		HealthParameter.DietaryIodine => record.Iodine?.Grams,
		HealthParameter.DietaryIron => record.Iron?.Grams,
		HealthParameter.DietaryMagnesium => record.Magnesium?.Grams,
		HealthParameter.DietaryManganese => record.Manganese?.Grams,
		HealthParameter.DietaryMolybdenum => record.Molybdenum?.Grams,
		HealthParameter.DietaryNiacin => record.Niacin?.Grams,
		HealthParameter.DietaryPantothenicAcid => record.PantothenicAcid?.Grams,
		HealthParameter.DietaryPhosphorus => record.Phosphorus?.Grams,
		HealthParameter.DietaryPotassium => record.Potassium?.Grams,
		HealthParameter.DietaryProtein => record.Protein?.Grams,
		HealthParameter.DietaryRiboflavin => record.Riboflavin?.Grams,
		HealthParameter.DietarySelenium => record.Selenium?.Grams,
		HealthParameter.DietarySodium => record.Sodium?.Grams,
		HealthParameter.DietarySugar => record.Sugar?.Grams,
		HealthParameter.DietaryThiamin => record.Thiamin?.Grams,
		HealthParameter.DietaryVitaminA => record.VitaminA?.Grams,
		HealthParameter.DietaryVitaminB12 => record.VitaminB12?.Grams,
		HealthParameter.DietaryVitaminB6 => record.VitaminB6?.Grams,
		HealthParameter.DietaryVitaminC => record.VitaminC?.Grams,
		HealthParameter.DietaryVitaminD => record.VitaminD?.Grams,
		HealthParameter.DietaryVitaminE => record.VitaminE?.Grams,
		HealthParameter.DietaryVitaminK => record.VitaminK?.Grams,
		HealthParameter.DietaryZinc => record.Zinc?.Grams,
		_ => null,
	};

	// NutritionRecord has a single positional constructor with ~50 nullable params.
	// We set only the field corresponding to the given parameter; all others are null.
	static NutritionRecord BuildNutritionRecord(
		HealthParameter param, double value,
		Java.Time.Instant startInstant, Java.Time.Instant endInstant,
		Metadata metadata)
	{
		var mass = Mass.InvokeGrams(value);
		var energy = Energy.InvokeKilocalories(value);

		// Declare all fields as null; set exactly the one needed.
		Mass? biotin = null, caffeine = null, calcium = null, chloride = null,
			cholesterol = null, chromium = null, copper = null, dietaryFiber = null,
			folate = null, folicAcid = null, iodine = null, iron = null,
			magnesium = null, manganese = null, molybdenum = null, monounsaturatedFat = null,
			niacin = null, pantothenicAcid = null, phosphorus = null, polyunsaturatedFat = null,
			potassium = null, protein = null, riboflavin = null, saturatedFat = null,
			selenium = null, sodium = null, sugar = null, thiamin = null,
			totalCarbohydrate = null, totalFat = null, transFat = null, unsaturatedFat = null,
			vitaminA = null, vitaminB12 = null, vitaminB6 = null, vitaminC = null,
			vitaminD = null, vitaminE = null, vitaminK = null, zinc = null;
		Energy? energyVal = null, energyFromFat = null;

		switch (param)
		{
			case HealthParameter.DietaryBiotin: biotin = mass; break;
			case HealthParameter.DietaryCaffeine: caffeine = mass; break;
			case HealthParameter.DietaryCalcium: calcium = mass; break;
			case HealthParameter.DietaryCarbohydrates: totalCarbohydrate = mass; break;
			case HealthParameter.DietaryChloride: chloride = mass; break;
			case HealthParameter.DietaryCholesterol: cholesterol = mass; break;
			case HealthParameter.DietaryChromium: chromium = mass; break;
			case HealthParameter.DietaryCopper: copper = mass; break;
			case HealthParameter.DietaryEnergyConsumed: energyVal = energy; break;
			case HealthParameter.DietaryFatMonounsaturated: monounsaturatedFat = mass; break;
			case HealthParameter.DietaryFatPolyunsaturated: polyunsaturatedFat = mass; break;
			case HealthParameter.DietaryFatSaturated: saturatedFat = mass; break;
			case HealthParameter.DietaryFatTotal: totalFat = mass; break;
			case HealthParameter.DietaryFiber: dietaryFiber = mass; break;
			case HealthParameter.DietaryFolate: folate = mass; break;
			case HealthParameter.DietaryIodine: iodine = mass; break;
			case HealthParameter.DietaryIron: iron = mass; break;
			case HealthParameter.DietaryMagnesium: magnesium = mass; break;
			case HealthParameter.DietaryManganese: manganese = mass; break;
			case HealthParameter.DietaryMolybdenum: molybdenum = mass; break;
			case HealthParameter.DietaryNiacin: niacin = mass; break;
			case HealthParameter.DietaryPantothenicAcid: pantothenicAcid = mass; break;
			case HealthParameter.DietaryPhosphorus: phosphorus = mass; break;
			case HealthParameter.DietaryPotassium: potassium = mass; break;
			case HealthParameter.DietaryProtein: protein = mass; break;
			case HealthParameter.DietaryRiboflavin: riboflavin = mass; break;
			case HealthParameter.DietarySelenium: selenium = mass; break;
			case HealthParameter.DietarySodium: sodium = mass; break;
			case HealthParameter.DietarySugar: sugar = mass; break;
			case HealthParameter.DietaryThiamin: thiamin = mass; break;
			case HealthParameter.DietaryVitaminA: vitaminA = mass; break;
			case HealthParameter.DietaryVitaminB12: vitaminB12 = mass; break;
			case HealthParameter.DietaryVitaminB6: vitaminB6 = mass; break;
			case HealthParameter.DietaryVitaminC: vitaminC = mass; break;
			case HealthParameter.DietaryVitaminD: vitaminD = mass; break;
			case HealthParameter.DietaryVitaminE: vitaminE = mass; break;
			case HealthParameter.DietaryVitaminK: vitaminK = mass; break;
			case HealthParameter.DietaryZinc: zinc = mass; break;
		}

		// Constructor parameter order:
		// startTime, startZoneOffset, endTime, endZoneOffset, metadata,
		// biotin, caffeine, calcium, energy, energyFromFat,
		// chloride, cholesterol, chromium, copper, dietaryFiber,
		// folate, folicAcid, iodine, iron, magnesium,
		// manganese, molybdenum, monounsaturatedFat, niacin, pantothenicAcid,
		// phosphorus, polyunsaturatedFat, potassium, protein, riboflavin,
		// saturatedFat, selenium, sodium, sugar, thiamin,
		// totalCarbohydrate, totalFat, transFat, unsaturatedFat, vitaminA,
		// vitaminB12, vitaminB6, vitaminC, vitaminD, vitaminE, vitaminK, zinc,
		// name, mealType
		return new NutritionRecord(
			startInstant, null, endInstant, null, metadata,
			biotin, caffeine, calcium, energyVal, energyFromFat,
			chloride, cholesterol, chromium, copper, dietaryFiber,
			folate, folicAcid, iodine, iron, magnesium,
			manganese, molybdenum, monounsaturatedFat, niacin, pantothenicAcid,
			phosphorus, polyunsaturatedFat, potassium, protein, riboflavin,
			saturatedFat, selenium, sodium, sugar, thiamin,
			totalCarbohydrate, totalFat, transFat, unsaturatedFat, vitaminA,
			vitaminB12, vitaminB6, vitaminC, vitaminD, vitaminE, vitaminK, zinc,
			null, 0);
	}

	static WorkoutSession MapExerciseSessionRecord(ExerciseSessionRecord record, WorkoutType requestedType)
	{
		var workoutType = requestedType == WorkoutType.Other
			? ReverseMapExerciseType(record.ExerciseType)
			: requestedType;

		IReadOnlyList<RoutePoint> route = Array.Empty<RoutePoint>();
		if (record.ExerciseRouteResult is ExerciseRouteResult.Data dataResult)
		{
			route = dataResult.ExerciseRoute.Route
				.Select(loc => new RoutePoint
				{
					Time = loc.Time.ToDateTimeUtc(),
					Latitude = loc.Latitude,
					Longitude = loc.Longitude,
					AltitudeInMeters = loc.Altitude?.Meters,
					HorizontalAccuracyInMeters = loc.HorizontalAccuracy?.Meters,
					VerticalAccuracyInMeters = loc.VerticalAccuracy?.Meters,
				})
				.ToList()
				.AsReadOnly();
		}

		return new WorkoutSession
		{
			WorkoutType = workoutType,
			From = record.StartTime.ToDateTimeUtc(),
			Until = record.EndTime.ToDateTimeUtc(),
			DurationInSeconds = (record.EndTime.ToEpochMilli() - record.StartTime.ToEpochMilli()) / 1000.0,
			Title = record.Title,
			Source = record.Metadata?.DataOrigin?.PackageName,
			Route = route,
		};
	}

	static int MapWorkoutType(WorkoutType type) => type switch
	{
		WorkoutType.AmericanFootball => ExerciseSessionRecord.ExerciseTypeFootballAmerican,
		WorkoutType.Archery => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.AustralianFootball => ExerciseSessionRecord.ExerciseTypeFootballAustralian,
		WorkoutType.Badminton => ExerciseSessionRecord.ExerciseTypeBadminton,
		WorkoutType.Baseball => ExerciseSessionRecord.ExerciseTypeBaseball,
		WorkoutType.Basketball => ExerciseSessionRecord.ExerciseTypeBasketball,
		WorkoutType.Bowling => ExerciseSessionRecord.ExerciseTypeCalisthenics,
		WorkoutType.Boxing => ExerciseSessionRecord.ExerciseTypeBoxing,
		WorkoutType.Climbing => ExerciseSessionRecord.ExerciseTypeRockClimbing,
		WorkoutType.Cricket => ExerciseSessionRecord.ExerciseTypeCricket,
		WorkoutType.CrossTraining => ExerciseSessionRecord.ExerciseTypeStrengthTraining,
		WorkoutType.Curling => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.Cycling => ExerciseSessionRecord.ExerciseTypeBiking,
		WorkoutType.Dance or WorkoutType.CardioDance or WorkoutType.SocialDance =>
			ExerciseSessionRecord.ExerciseTypeDancing,
		WorkoutType.DanceInspiredTraining => ExerciseSessionRecord.ExerciseTypeExerciseClass,
		WorkoutType.Elliptical => ExerciseSessionRecord.ExerciseTypeElliptical,
		WorkoutType.EquestrianSports => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.Fencing => ExerciseSessionRecord.ExerciseTypeFencing,
		WorkoutType.Fishing => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.FunctionalStrengthTraining => ExerciseSessionRecord.ExerciseTypeStrengthTraining,
		WorkoutType.Golf => ExerciseSessionRecord.ExerciseTypeGolf,
		WorkoutType.Gymnastics => ExerciseSessionRecord.ExerciseTypeGymnastics,
		WorkoutType.Handball => ExerciseSessionRecord.ExerciseTypeHandball,
		WorkoutType.Hiking => ExerciseSessionRecord.ExerciseTypeHiking,
		WorkoutType.Hockey => ExerciseSessionRecord.ExerciseTypeIceHockey,
		WorkoutType.Hunting => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.Lacrosse => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.MartialArts => ExerciseSessionRecord.ExerciseTypeMartialArts,
		WorkoutType.MindAndBody => ExerciseSessionRecord.ExerciseTypeYoga,
		WorkoutType.MixedMetabolicCardioTraining or WorkoutType.MixedCardio =>
			ExerciseSessionRecord.ExerciseTypeHighIntensityIntervalTraining,
		WorkoutType.PaddleSports => ExerciseSessionRecord.ExerciseTypePaddling,
		WorkoutType.Play => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.PreparationAndRecovery => ExerciseSessionRecord.ExerciseTypeStretching,
		WorkoutType.Racquetball => ExerciseSessionRecord.ExerciseTypeRacquetball,
		WorkoutType.Rowing => ExerciseSessionRecord.ExerciseTypeRowing,
		WorkoutType.Rugby => ExerciseSessionRecord.ExerciseTypeRugby,
		WorkoutType.Running => ExerciseSessionRecord.ExerciseTypeRunning,
		WorkoutType.Sailing => ExerciseSessionRecord.ExerciseTypeSailing,
		WorkoutType.SkatingSports => ExerciseSessionRecord.ExerciseTypeSkating,
		WorkoutType.SnowSports => ExerciseSessionRecord.ExerciseTypeSnowshoeing,
		WorkoutType.Soccer => ExerciseSessionRecord.ExerciseTypeSoccer,
		WorkoutType.Softball => ExerciseSessionRecord.ExerciseTypeSoftball,
		WorkoutType.Squash => ExerciseSessionRecord.ExerciseTypeSquash,
		WorkoutType.StairClimbing => ExerciseSessionRecord.ExerciseTypeStairClimbing,
		WorkoutType.SurfingSports => ExerciseSessionRecord.ExerciseTypeSurfing,
		WorkoutType.Swimming => ExerciseSessionRecord.ExerciseTypeSwimmingPool,
		WorkoutType.TableTennis => ExerciseSessionRecord.ExerciseTypeTableTennis,
		WorkoutType.Tennis => ExerciseSessionRecord.ExerciseTypeTennis,
		WorkoutType.TrackAndField => ExerciseSessionRecord.ExerciseTypeRunning,
		WorkoutType.TraditionalStrengthTraining => ExerciseSessionRecord.ExerciseTypeWeightlifting,
		WorkoutType.Volleyball => ExerciseSessionRecord.ExerciseTypeVolleyball,
		WorkoutType.Walking => ExerciseSessionRecord.ExerciseTypeWalking,
		WorkoutType.WaterFitness => ExerciseSessionRecord.ExerciseTypeSwimmingPool,
		WorkoutType.WaterPolo => ExerciseSessionRecord.ExerciseTypeWaterPolo,
		WorkoutType.WaterSports => ExerciseSessionRecord.ExerciseTypeSwimmingOpenWater,
		WorkoutType.Wrestling => ExerciseSessionRecord.ExerciseTypeMartialArts,
		WorkoutType.Yoga => ExerciseSessionRecord.ExerciseTypeYoga,
		WorkoutType.Barre => ExerciseSessionRecord.ExerciseTypeExerciseClass,
		WorkoutType.CoreTraining => ExerciseSessionRecord.ExerciseTypeCalisthenics,
		WorkoutType.CrossCountrySkiing => ExerciseSessionRecord.ExerciseTypeSkiing,
		WorkoutType.DownhillSkiing => ExerciseSessionRecord.ExerciseTypeSkiing,
		WorkoutType.Flexibility => ExerciseSessionRecord.ExerciseTypeStretching,
		WorkoutType.HighIntensityIntervalTraining => ExerciseSessionRecord.ExerciseTypeHighIntensityIntervalTraining,
		WorkoutType.JumpRope => ExerciseSessionRecord.ExerciseTypeCalisthenics,
		WorkoutType.Kickboxing => ExerciseSessionRecord.ExerciseTypeBoxing,
		WorkoutType.Pilates => ExerciseSessionRecord.ExerciseTypePilates,
		WorkoutType.Snowboarding => ExerciseSessionRecord.ExerciseTypeSnowboarding,
		WorkoutType.Stairs => ExerciseSessionRecord.ExerciseTypeStairClimbing,
		WorkoutType.StepTraining => ExerciseSessionRecord.ExerciseTypeExerciseClass,
		WorkoutType.WheelchairWalkPace or WorkoutType.WheelchairRunPace =>
			ExerciseSessionRecord.ExerciseTypeWheelchair,
		WorkoutType.TaiChi => ExerciseSessionRecord.ExerciseTypeMartialArts,
		WorkoutType.HandCycling => ExerciseSessionRecord.ExerciseTypeBiking,
		WorkoutType.DiscSports => ExerciseSessionRecord.ExerciseTypeFrisbeeDisc,
		WorkoutType.FitnessGaming => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.Pickleball => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		WorkoutType.Cooldown => ExerciseSessionRecord.ExerciseTypeStretching,
		WorkoutType.SwimBikeRun or WorkoutType.Transition => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
		_ => ExerciseSessionRecord.ExerciseTypeOtherWorkout,
	};

	static WorkoutType ReverseMapExerciseType(int exerciseType) => exerciseType switch
	{
		var t when t == ExerciseSessionRecord.ExerciseTypeFootballAmerican => WorkoutType.AmericanFootball,
		var t when t == ExerciseSessionRecord.ExerciseTypeFootballAustralian => WorkoutType.AustralianFootball,
		var t when t == ExerciseSessionRecord.ExerciseTypeBadminton => WorkoutType.Badminton,
		var t when t == ExerciseSessionRecord.ExerciseTypeBaseball => WorkoutType.Baseball,
		var t when t == ExerciseSessionRecord.ExerciseTypeBasketball => WorkoutType.Basketball,
		var t when t == ExerciseSessionRecord.ExerciseTypeBoxing => WorkoutType.Boxing,
		var t when t == ExerciseSessionRecord.ExerciseTypeBiking => WorkoutType.Cycling,
		var t when t == ExerciseSessionRecord.ExerciseTypeCricket => WorkoutType.Cricket,
		var t when t == ExerciseSessionRecord.ExerciseTypeDancing => WorkoutType.Dance,
		var t when t == ExerciseSessionRecord.ExerciseTypeElliptical => WorkoutType.Elliptical,
		var t when t == ExerciseSessionRecord.ExerciseTypeFencing => WorkoutType.Fencing,
		var t when t == ExerciseSessionRecord.ExerciseTypeGolf => WorkoutType.Golf,
		var t when t == ExerciseSessionRecord.ExerciseTypeGymnastics => WorkoutType.Gymnastics,
		var t when t == ExerciseSessionRecord.ExerciseTypeHandball => WorkoutType.Handball,
		var t when t == ExerciseSessionRecord.ExerciseTypeHighIntensityIntervalTraining => WorkoutType.HighIntensityIntervalTraining,
		var t when t == ExerciseSessionRecord.ExerciseTypeHiking => WorkoutType.Hiking,
		var t when t == ExerciseSessionRecord.ExerciseTypeIceHockey => WorkoutType.Hockey,
		var t when t == ExerciseSessionRecord.ExerciseTypeMartialArts => WorkoutType.MartialArts,
		var t when t == ExerciseSessionRecord.ExerciseTypePaddling => WorkoutType.PaddleSports,
		var t when t == ExerciseSessionRecord.ExerciseTypePilates => WorkoutType.Pilates,
		var t when t == ExerciseSessionRecord.ExerciseTypeRacquetball => WorkoutType.Racquetball,
		var t when t == ExerciseSessionRecord.ExerciseTypeRockClimbing => WorkoutType.Climbing,
		var t when t == ExerciseSessionRecord.ExerciseTypeRowing => WorkoutType.Rowing,
		var t when t == ExerciseSessionRecord.ExerciseTypeRugby => WorkoutType.Rugby,
		var t when t == ExerciseSessionRecord.ExerciseTypeRunning => WorkoutType.Running,
		var t when t == ExerciseSessionRecord.ExerciseTypeSailing => WorkoutType.Sailing,
		var t when t == ExerciseSessionRecord.ExerciseTypeSkating => WorkoutType.SkatingSports,
		var t when t == ExerciseSessionRecord.ExerciseTypeSkiing => WorkoutType.DownhillSkiing,
		var t when t == ExerciseSessionRecord.ExerciseTypeSnowboarding => WorkoutType.Snowboarding,
		var t when t == ExerciseSessionRecord.ExerciseTypeSoccer => WorkoutType.Soccer,
		var t when t == ExerciseSessionRecord.ExerciseTypeSoftball => WorkoutType.Softball,
		var t when t == ExerciseSessionRecord.ExerciseTypeSquash => WorkoutType.Squash,
		var t when t == ExerciseSessionRecord.ExerciseTypeStairClimbing => WorkoutType.StairClimbing,
		var t when t == ExerciseSessionRecord.ExerciseTypeStrengthTraining => WorkoutType.TraditionalStrengthTraining,
		var t when t == ExerciseSessionRecord.ExerciseTypeStretching => WorkoutType.Flexibility,
		var t when t == ExerciseSessionRecord.ExerciseTypeSurfing => WorkoutType.SurfingSports,
		var t when t == ExerciseSessionRecord.ExerciseTypeSwimmingPool => WorkoutType.Swimming,
		var t when t == ExerciseSessionRecord.ExerciseTypeSwimmingOpenWater => WorkoutType.WaterSports,
		var t when t == ExerciseSessionRecord.ExerciseTypeTableTennis => WorkoutType.TableTennis,
		var t when t == ExerciseSessionRecord.ExerciseTypeTennis => WorkoutType.Tennis,
		var t when t == ExerciseSessionRecord.ExerciseTypeVolleyball => WorkoutType.Volleyball,
		var t when t == ExerciseSessionRecord.ExerciseTypeWalking => WorkoutType.Walking,
		var t when t == ExerciseSessionRecord.ExerciseTypeWaterPolo => WorkoutType.WaterPolo,
		var t when t == ExerciseSessionRecord.ExerciseTypeWeightlifting => WorkoutType.TraditionalStrengthTraining,
		var t when t == ExerciseSessionRecord.ExerciseTypeWheelchair => WorkoutType.WheelchairWalkPace,
		var t when t == ExerciseSessionRecord.ExerciseTypeYoga => WorkoutType.Yoga,
		var t when t == ExerciseSessionRecord.ExerciseTypeFrisbeeDisc => WorkoutType.DiscSports,
		_ => WorkoutType.Other,
	};

	// ── Coroutine bridge ──────────────────────────────────────────────────────

	// Bridge for calling Kotlin suspend functions from C#.
	// Health Connect APIs are always async (IPC), so the function will always suspend.
	static Task<Java.Lang.Object?> InvokeCoroutine(Func<IContinuation, Java.Lang.Object?> fn)
	{
		var cont = new KotlinContinuation();
		Java.Lang.Object? immediate;
		try
		{
			immediate = fn(cont);
		}
		catch (Exception ex)
		{
			return Task.FromException<Java.Lang.Object?>(ex);
		}

		// A Kotlin suspend function returns the COROUTINE_SUSPENDED sentinel only when it actually
		// suspends — in which case the result arrives later via the continuation. If it completes
		// synchronously it returns the result directly and never calls the continuation, so awaiting
		// cont.Task would hang forever (this is why ReadRecords could hang while Aggregate worked).
		if (immediate is not null && !IsCoroutineSuspended(immediate))
		{
			return Task.FromResult<Java.Lang.Object?>(immediate);
		}

		return cont.Task;
	}

	// kotlin.coroutines.intrinsics.CoroutineSingletons.COROUTINE_SUSPENDED — the marker a suspend
	// function returns when it suspends rather than completing inline.
	static bool IsCoroutineSuspended(Java.Lang.Object value)
		=> value.Class?.Name == "kotlin.coroutines.intrinsics.CoroutineSingletons"
			&& string.Equals(value.ToString(), "COROUTINE_SUSPENDED", StringComparison.Ordinal);

	// Get the Kotlin KClass for a managed Android type T.
	static IKClass GetKClass<T>() where T : Java.Lang.Object
	{
		// JNIEnv.FindClass returns a runtime-cached JNI *global* reference. Wrapping it with
		// TransferLocalRef makes the runtime later free it via DeleteLocalRef, which aborts the
		// process under CheckJNI (Android 14+) with "expected reference of kind Local but found
		// Global". Use DoNotTransfer so the wrapper doesn't take ownership of the cached global
		// ref, and don't dispose it (we don't own it).
		nint classRef = JNIEnv.FindClass(typeof(T));
		var javaClass = Java.Lang.Object.GetObject<Java.Lang.Class>(
			classRef, JniHandleOwnership.DoNotTransfer)!;
		return JvmClassMappingKt.GetKotlinClass(javaClass);
	}
}

// ── KotlinContinuation ────────────────────────────────────────────────────────

// Bridges a Kotlin suspend function call to a C# Task.
// ResumeWith is called by the Kotlin runtime with the result (success or Kotlin Result$Failure).
// Bridges the Java ActivityResultCallback block (Health Connect consent result) to a C# Action.
sealed class PermissionResultCallback : Java.Lang.Object, AndroidX.Activity.Result.IActivityResultCallback
{
	readonly Action onResult;
	public PermissionResultCallback(Action onResult) => this.onResult = onResult;
	public void OnActivityResult(Java.Lang.Object? result) => onResult();
}

sealed class KotlinContinuation : Java.Lang.Object, IContinuation
{
	readonly TaskCompletionSource<Java.Lang.Object?> tcs =
		new(TaskCreationOptions.RunContinuationsAsynchronously);

	public Task<Java.Lang.Object?> Task => tcs.Task;

	public ICoroutineContext Context => EmptyCoroutineContext.Instance;

	public void ResumeWith(Java.Lang.Object result)
	{
		// Kotlin encodes failures as kotlin.Result$Failure{exception}.
		// The simple name of that inner class is "Failure".
		if (result?.Class?.SimpleName == "Failure")
		{
			try
			{
				var field = result.Class.GetDeclaredField("exception");
				field!.Accessible = true;
				var throwable = Java.Interop.JavaObjectExtensions.JavaCast<Java.Lang.Throwable>(field.Get(result));
				tcs.TrySetException(
					new Plugin.Maui.Health.Exceptions.HealthException(
						throwable?.Message ?? "Health Connect operation failed"));
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}
		else
		{
			tcs.TrySetResult(result);
		}
	}
}

static class HealthConnectExtensions
{
	internal static Java.Time.Instant ToInstant(this DateTimeOffset dto)
		=> Java.Time.Instant.OfEpochMilli(dto.ToUnixTimeMilliseconds())!;

	internal static DateTimeOffset ToDateTimeUtc(this Java.Time.Instant instant)
		=> DateTimeOffset.FromUnixTimeMilliseconds(instant.ToEpochMilli());
}
