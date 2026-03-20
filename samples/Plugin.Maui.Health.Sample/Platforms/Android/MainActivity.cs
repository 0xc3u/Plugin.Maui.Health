using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Health.Connect.Client;
using Plugin.Maui.Health.Enums;

namespace Plugin.Maui.Health.Sample;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	// Accessible by BaseViewModel to trigger the Health Connect consent UI.
	public static MainActivity? Current { get; private set; }

	ActivityResultLauncher? _permissionLauncher;
	TaskCompletionSource? _pendingRequest;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		Current = this;
		base.OnCreate(savedInstanceState);

		_permissionLauncher = RegisterForActivityResult(
			PermissionController.CreateRequestPermissionResultContract(),
			new PermissionResultCallback(() =>
			{
				_pendingRequest?.TrySetResult();
				_pendingRequest = null;
			}));
	}

	protected override void OnDestroy()
	{
		if (Current == this)
			Current = null;
		base.OnDestroy();
	}

	/// <summary>
	/// Launches the Health Connect consent UI for the given health parameter and permission type.
	/// Returns after the user has dismissed the consent sheet (granted or denied).
	/// </summary>
	public Task RequestHealthPermissionsAsync(HealthParameter parameter, PermissionType permissionType)
	{
		if (_permissionLauncher is null)
			return Task.CompletedTask;

		var (readPerm, writePerm) = GetPermissions(parameter);

		var permissions = new Java.Util.HashSet();
		if (permissionType.HasFlag(PermissionType.Read) && readPerm is not null)
			permissions.Add(new Java.Lang.String(readPerm));
		if (permissionType.HasFlag(PermissionType.Write) && writePerm is not null)
			permissions.Add(new Java.Lang.String(writePerm));

		if (permissions.IsEmpty)
			return Task.CompletedTask;

		_pendingRequest = new TaskCompletionSource();
		_permissionLauncher.Launch(permissions);
		return _pendingRequest.Task;
	}

	// Mirrors the mapping in Health.android.cs so the sample can request the right permissions.
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

	// Bridges the Java ActivityResultCallback block to a C# Action.
	sealed class PermissionResultCallback : Java.Lang.Object, IActivityResultCallback
	{
		readonly Action _onResult;
		public PermissionResultCallback(Action onResult) => _onResult = onResult;
		public void OnActivityResult(Java.Lang.Object? result) => _onResult();
	}
}
