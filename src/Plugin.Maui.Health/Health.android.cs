using Android.Content.PM;
using Microsoft.Maui.ApplicationModel;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
//using Plugin.Maui.Health.Extensions;
using Plugin.Maui.Health.Models;

namespace Plugin.Maui.Health;

partial class HealthDataProviderImplementation : IHealth
{
	public bool IsSupported => throw new NotImplementedException();

	public Task<bool> CheckPermissionAsync(HealthParameter healthParameter, PermissionType permissionType)
	{
		throw new NotImplementedException();
	}

	public Task<List<Sample>> ReadAllAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<double?> ReadAverageAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<double> ReadCountAsync(HealthParameter healthParameter, DateTime from, DateTime until)
	{
		throw new NotImplementedException();
	}

	public Task<double?> ReadLatestAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<double?> ReadMaxAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<double?> ReadMinAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		throw new NotImplementedException();
	}

	public Task<bool> WriteAsync(HealthParameter healthParameter, DateTime? date, double valueToStore, string unit)
	{
		throw new NotImplementedException();
	}
}

//partial class HealthDataProviderImplementation : IHealth
//{

//	// Set of static methods for working with POSIX time
//	public static class TimeUtility
//	{
//		const string tag = "TimeUtility";

//		// Start of POSIX time
//		static readonly DateTime unixEpoch =
//			new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

//		// A week in milliseconds
//		const long weekInMillis = 1000 * 60 * 60 * 24 * 7;

//		// 48 hours in milliseconds
//		const long twoDaysInMillis = 1000 * 60 * 60 * 24 * 2;

//		// Current POSIX time
//		public static long CurrentMillis()
//		{
//			return (long)(DateTime.UtcNow - unixEpoch).TotalMilliseconds;
//		}

//		// A week ago in POSIX time
//		public static long WeekAgoMillis()
//		{
//			return (long)(DateTime.UtcNow - unixEpoch).TotalMilliseconds - weekInMillis;
//		}

//		// 48 hours ago in POSIX time
//		public static long TwoDaysAgoMillis()
//		{
//			// We add a 1 to ensure only two buckets of 24 hours apiece are created
//			return (long)(DateTime.UtcNow - unixEpoch).TotalMilliseconds - twoDaysInMillis + 1;
//		}

//		// Converts POSIX time to DateTime
//		public static DateTime FromMillis(long millis)
//		{
//			return unixEpoch.AddMilliseconds(millis);
//		}

//		// Calculate how many days in the past a day is from today
//		public static uint DaysInPast(DateTime dateTime)
//		{
//			return (uint)(DateTime.Now.Date - dateTime.Date).Days;
//		}

//	}

//	readonly Dictionary<HealthParameter, Android.Gms.Fitness.Data.DataType> healthParameterMapping = new()
//	{
//		{HealthParameter.ActiveEnergyBurned, Android.Gms.Fitness.Data.DataType.TypeCaloriesExpended},
//		//{HealthParameter.AtrialFibrillationBurden, /*not supported*/}
//		//{HealthParameter.BasalBodyTemperature,  /*not supported*/},
//		{HealthParameter.BasalEnergyBurned, Android.Gms.Fitness.Data.DataType.TypeBasalMetabolicRate},
//		//{HealthParameter.BloodAlcoholContent, /*not supported*/},
//		//{HealthParameter.BloodGlucose, /*not supported*/},
//		//{HealthParameter.BloodPressureDiastolic,  /*not supported*/},
//		//{HealthParameter.BloodPressureSystolic, /*not supported*/},
//		{HealthParameter.BodyFatPercentage, Android.Gms.Fitness.Data.DataType.TypeBodyFatPercentage},
//		{HealthParameter.BodyMass, Android.Gms.Fitness.Data.DataType.TypeWeight},
//		//{HealthParameter.BodyMassIndex, /*not supported*/},
//		//{HealthParameter.BodyTemperature, /*not supported*/},	
//		{HealthParameter.DietaryBiotin, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryCaffeine, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryCalcium, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryCarbohydrates, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryChloride, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryCholesterol, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryChromium, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryCopper, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryEnergyConsumed, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryFatMonounsaturated, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryFatPolyunsaturated, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryFatSaturated, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryFatTotal, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryFiber, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryFolate, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryIodine, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryIron, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryMagnesium, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryManganese, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryMolybdenum, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryNiacin, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryPantothenicAcid, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryPhosphorus, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryPotassium, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryProtein, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryRiboflavin, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietarySelenium, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietarySodium, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietarySugar, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryThiamin, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryVitaminA, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryVitaminB12, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryVitaminB6, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryVitaminC, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryVitaminD, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryVitaminE, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryVitaminK, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryWater, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.DietaryZinc, Android.Gms.Fitness.Data.DataType.TypeNutrition},
//		{HealthParameter.HeartRate, Android.Gms.Fitness.Data.DataType.TypeHeartRateBpm},
//		{HealthParameter.Height, Android.Gms.Fitness.Data.DataType.TypeHeight},
//		{HealthParameter.LeanBodyMass, Android.Gms.Fitness.Data.DataType.TypeWeight},
//		{HealthParameter.DistanceCycling, Android.Gms.Fitness.Data.DataType.AggregateDistanceDelta},
//		{HealthParameter.DistanceWalkingRunning, Android.Gms.Fitness.Data.DataType.AggregateDistanceDelta},
//		{HealthParameter.DistanceWheelchair, Android.Gms.Fitness.Data.DataType.AggregateDistanceDelta},
//		// {HealthParameter.ElectrodermalActivity, /*not supported*/ },
//		// {HealthParameter.EnvironmentalAudioExposure, /*not supported*/ },
//		{HealthParameter.ExerciseTime, Android.Gms.Fitness.Data.DataType.TypeMoveMinutes},
//		{HealthParameter.FlightsClimbed, Android.Gms.Fitness.Data.DataType.TypeHeight},
//		//{HealthParameter.ForcedExpiratoryVolume1, /*not supported*/ }
//		{HealthParameter.StepCount, Android.Gms.Fitness.Data.DataType.TypeStepCountDelta},
//	};

//	readonly SemaphoreSlim semaphore = new(1, 1);

//	readonly PackageManager? packageManager = Application.Context.PackageManager;

//	public bool IsSupported => true;
//	void CreateGoogleApiClient()
//	{
		


//		//GoogleApiClient mClient = new GoogleApiClient.Builder(Application.Context)
//		//		.AddApi(FitnessClass.HISTORY_API)
//		//		.AddScope(new Scope(Scopes.FitnessActivityRead))
//		//		.AddConnectionCallbacks(this)
//		//		.AddOnConnectionFailedListener(this)
//		//	.Build();

//	}


//	public Task<bool> CheckPermissionAsync(HealthParameter healthParameter, PermissionType permissionType)
//	{
//		var activity = Platform.CurrentActivity;
//		//Android.Gms.Fitness.Data.DataType.TypeStepCountDelta

//		throw new NotImplementedException();
//	}

//	public Task<List<Sample>> ReadAllAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
//	{
//		throw new NotImplementedException();
//	}

//	public Task<List<Workout>> ReadAllWorkoutsAsync(WorkoutType workoutType, DateTime from, DateTime until)
//	{
//		throw new NotImplementedException();
//	}

//	public Task<double?> ReadAverageAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
//	{
//		throw new NotImplementedException();
//	}

//	public Task<double> ReadCountAsync(HealthParameter healthParameter, DateTime from, DateTime until)
//	{
//		throw new NotImplementedException();
//	}

//	public async Task<double?> ReadLatestAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
//	{

//		await semaphore.WaitAsync();
//		var tcs = new TaskCompletionSource<double?>();

//		try
//		{
//			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
//			{
//				throw new HealthException($"{healthParameter} not available");
//			}

//			// Prepare the time range
//			var startTime = from.ToUnixTimeMilliseconds();
//			var endTime = until.ToUnixTimeMilliseconds();

//			// Build the data request
//			var readRequest = new DataReadRequest.Builder()
//				.Aggregate(requestedHealthParameter)
//				.BucketByTime(1, Java.Util.Concurrent.TimeUnit.Microseconds)
//				.SetTimeRange(startTime, endTime, Java.Util.Concurrent.TimeUnit.Microseconds)
//				.Build();


//		}
//		catch (Exception ex)
//		{
//			tcs.SetException(new HealthException(ex.Message, ex));
//		}
//		finally
//		{
//			semaphore.Release();
//		}

//		return await tcs.Task;
//	}

//	public Task<Workout?> ReadLatestWorkoutAsync(WorkoutType workoutType, DateTime from, DateTime until)
//	{
//		throw new NotImplementedException();
//	}

//	public Task<double?> ReadMaxAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
//	{
//		throw new NotImplementedException();
//	}

//	public Task<double?> ReadMinAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
//	{
//		throw new NotImplementedException();
//	}

//	public Task<bool> WriteAsync(HealthParameter healthParameter, DateTime? date, double valueToStore, string unit)
//	{
//		throw new NotImplementedException();
//	}
	
//}