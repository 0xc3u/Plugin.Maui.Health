using CoreLocation;
using HealthKit;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Models;

namespace Plugin.Maui.Health;

partial class HealthDataProviderImplementation : IHealth
{
	readonly HKHealthStore healthStore = new();

	readonly Dictionary<HealthParameter, HKQuantityTypeIdentifier> healthParameterMapping = new()
	{
		{HealthParameter.BodyMassIndex, HKQuantityTypeIdentifier.BodyMassIndex},
		{HealthParameter.BodyFatPercentage, HKQuantityTypeIdentifier.BodyFatPercentage},
		{HealthParameter.Height, HKQuantityTypeIdentifier.Height},
		{HealthParameter.BodyMass, HKQuantityTypeIdentifier.BodyMass},
		{HealthParameter.LeanBodyMass, HKQuantityTypeIdentifier.LeanBodyMass},
		{HealthParameter.HeartRate, HKQuantityTypeIdentifier.HeartRate},
		{HealthParameter.StepCount, HKQuantityTypeIdentifier.StepCount},
		{HealthParameter.DistanceWalkingRunning, HKQuantityTypeIdentifier.DistanceWalkingRunning},
		{HealthParameter.DistanceCycling, HKQuantityTypeIdentifier.DistanceCycling},
		{HealthParameter.BasalEnergyBurned, HKQuantityTypeIdentifier.BasalEnergyBurned},
		{HealthParameter.ActiveEnergyBurned, HKQuantityTypeIdentifier.ActiveEnergyBurned},
		{HealthParameter.FlightsClimbed, HKQuantityTypeIdentifier.FlightsClimbed},
		{HealthParameter.NikeFuel, HKQuantityTypeIdentifier.NikeFuel},
		{HealthParameter.OxygenSaturation, HKQuantityTypeIdentifier.OxygenSaturation},
		{HealthParameter.BloodGlucose, HKQuantityTypeIdentifier.BloodGlucose},
		{HealthParameter.BloodPressureSystolic, HKQuantityTypeIdentifier.BloodPressureSystolic},
		{HealthParameter.BloodPressureDiastolic, HKQuantityTypeIdentifier.BloodPressureDiastolic},
		{HealthParameter.BloodAlcoholContent, HKQuantityTypeIdentifier.BloodAlcoholContent},
		{HealthParameter.PeripheralPerfusionIndex, HKQuantityTypeIdentifier.PeripheralPerfusionIndex},
		{HealthParameter.ForcedVitalCapacity, HKQuantityTypeIdentifier.ForcedVitalCapacity},
		{HealthParameter.ForcedExpiratoryVolume1, HKQuantityTypeIdentifier.ForcedExpiratoryVolume1},
		{HealthParameter.PeakExpiratoryFlowRate, HKQuantityTypeIdentifier.PeakExpiratoryFlowRate},
		{HealthParameter.NumberOfTimesFallen, HKQuantityTypeIdentifier.NumberOfTimesFallen},
		{HealthParameter.InhalerUsage, HKQuantityTypeIdentifier.InhalerUsage},
		{HealthParameter.RespiratoryRate, HKQuantityTypeIdentifier.RespiratoryRate},
		{HealthParameter.BodyTemperature, HKQuantityTypeIdentifier.BodyTemperature},
		{HealthParameter.DietaryFatTotal, HKQuantityTypeIdentifier.DietaryFatTotal},
		{HealthParameter.DietaryFatPolyunsaturated, HKQuantityTypeIdentifier.DietaryFatPolyunsaturated},
		{HealthParameter.DietaryFatMonounsaturated, HKQuantityTypeIdentifier.DietaryFatMonounsaturated},
		{HealthParameter.DietaryFatSaturated, HKQuantityTypeIdentifier.DietaryFatSaturated},
		{HealthParameter.DietaryCholesterol, HKQuantityTypeIdentifier.DietaryCholesterol},
		{HealthParameter.DietarySodium, HKQuantityTypeIdentifier.DietarySodium},
		{HealthParameter.DietaryCarbohydrates, HKQuantityTypeIdentifier.DietaryCarbohydrates},
		{HealthParameter.DietaryFiber, HKQuantityTypeIdentifier.DietaryFiber},
		{HealthParameter.DietarySugar, HKQuantityTypeIdentifier.DietarySugar},
		{HealthParameter.DietaryEnergyConsumed, HKQuantityTypeIdentifier.DietaryEnergyConsumed},
		{HealthParameter.DietaryProtein, HKQuantityTypeIdentifier.DietaryProtein},
		{HealthParameter.DietaryVitaminA, HKQuantityTypeIdentifier.DietaryVitaminA},
		{HealthParameter.DietaryVitaminB6, HKQuantityTypeIdentifier.DietaryVitaminB6},
		{HealthParameter.DietaryVitaminB12, HKQuantityTypeIdentifier.DietaryVitaminB12},
		{HealthParameter.DietaryVitaminC, HKQuantityTypeIdentifier.DietaryVitaminC},
		{HealthParameter.DietaryVitaminD, HKQuantityTypeIdentifier.DietaryVitaminD},
		{HealthParameter.DietaryVitaminE, HKQuantityTypeIdentifier.DietaryVitaminE},
		{HealthParameter.DietaryVitaminK, HKQuantityTypeIdentifier.DietaryVitaminK},
		{HealthParameter.DietaryCalcium, HKQuantityTypeIdentifier.DietaryCalcium},
		{HealthParameter.DietaryIron, HKQuantityTypeIdentifier.DietaryIron},
		{HealthParameter.DietaryThiamin, HKQuantityTypeIdentifier.DietaryThiamin},
		{HealthParameter.DietaryRiboflavin, HKQuantityTypeIdentifier.DietaryRiboflavin},
		{HealthParameter.DietaryNiacin, HKQuantityTypeIdentifier.DietaryNiacin},
		{HealthParameter.DietaryFolate, HKQuantityTypeIdentifier.DietaryFolate},
		{HealthParameter.DietaryBiotin, HKQuantityTypeIdentifier.DietaryBiotin},
		{HealthParameter.DietaryPantothenicAcid, HKQuantityTypeIdentifier.DietaryPantothenicAcid},
		{HealthParameter.DietaryPhosphorus, HKQuantityTypeIdentifier.DietaryPhosphorus},
		{HealthParameter.DietaryIodine, HKQuantityTypeIdentifier.DietaryIodine},
		{HealthParameter.DietaryMagnesium, HKQuantityTypeIdentifier.DietaryMagnesium},
		{HealthParameter.DietaryZinc, HKQuantityTypeIdentifier.DietaryZinc},
		{HealthParameter.DietarySelenium, HKQuantityTypeIdentifier.DietarySelenium},
		{HealthParameter.DietaryCopper, HKQuantityTypeIdentifier.DietaryCopper},
		{HealthParameter.DietaryManganese, HKQuantityTypeIdentifier.DietaryManganese},
		{HealthParameter.DietaryChromium, HKQuantityTypeIdentifier.DietaryChromium},
		{HealthParameter.DietaryMolybdenum, HKQuantityTypeIdentifier.DietaryMolybdenum},
		{HealthParameter.DietaryChloride, HKQuantityTypeIdentifier.DietaryChloride},
		{HealthParameter.DietaryPotassium, HKQuantityTypeIdentifier.DietaryPotassium},
		{HealthParameter.DietaryCaffeine, HKQuantityTypeIdentifier.DietaryCaffeine},
		{HealthParameter.VO2Max, HKQuantityTypeIdentifier.VO2Max},
	};

	readonly Dictionary<WorkoutType, HKWorkoutActivityType> workoutTypeMapping = new()
	{
		{WorkoutType.AmericanFootball, HKWorkoutActivityType.AmericanFootball},
		{WorkoutType.Archery, HKWorkoutActivityType.Archery},
		{WorkoutType.AustralianFootball, HKWorkoutActivityType.AustralianFootball},
		{WorkoutType.Badminton, HKWorkoutActivityType.Badminton},
		{WorkoutType.Baseball, HKWorkoutActivityType.Baseball},
		{WorkoutType.Basketball, HKWorkoutActivityType.Basketball},
		{WorkoutType.Bowling, HKWorkoutActivityType.Bowling},
		{WorkoutType.Boxing, HKWorkoutActivityType.Boxing},
		{WorkoutType.Climbing, HKWorkoutActivityType.Climbing},
		{WorkoutType.Cricket, HKWorkoutActivityType.Cricket},
		{WorkoutType.CrossTraining, HKWorkoutActivityType.CrossTraining},
		{WorkoutType.Curling, HKWorkoutActivityType.Curling},
		{WorkoutType.Cycling, HKWorkoutActivityType.Cycling},
		{WorkoutType.Dance, HKWorkoutActivityType.Dance},
		{WorkoutType.DanceInspiredTraining, HKWorkoutActivityType.DanceInspiredTraining},
		{WorkoutType.Elliptical, HKWorkoutActivityType.Elliptical},
		{WorkoutType.EquestrianSports, HKWorkoutActivityType.EquestrianSports},
		{WorkoutType.Fencing, HKWorkoutActivityType.Fencing},
		{WorkoutType.Fishing, HKWorkoutActivityType.Fishing},
		{WorkoutType.FunctionalStrengthTraining, HKWorkoutActivityType.FunctionalStrengthTraining},
		{WorkoutType.Golf, HKWorkoutActivityType.Golf},
		{WorkoutType.Gymnastics, HKWorkoutActivityType.Gymnastics},
		{WorkoutType.Handball, HKWorkoutActivityType.Handball},
		{WorkoutType.Hiking, HKWorkoutActivityType.Hiking},
		{WorkoutType.Hockey, HKWorkoutActivityType.Hockey},
		{WorkoutType.Hunting, HKWorkoutActivityType.Hunting},
		{WorkoutType.Lacrosse, HKWorkoutActivityType.Lacrosse},
		{WorkoutType.MartialArts, HKWorkoutActivityType.MartialArts},
		{WorkoutType.MindAndBody, HKWorkoutActivityType.MindAndBody},
		{WorkoutType.MixedMetabolicCardioTraining, HKWorkoutActivityType.MixedMetabolicCardioTraining},
		{WorkoutType.PaddleSports, HKWorkoutActivityType.PaddleSports},
		{WorkoutType.Play, HKWorkoutActivityType.Play},
		{WorkoutType.PreparationAndRecovery, HKWorkoutActivityType.PreparationAndRecovery},
		{WorkoutType.Racquetball, HKWorkoutActivityType.Racquetball},
		{WorkoutType.Rowing, HKWorkoutActivityType.Rowing},
		{WorkoutType.Rugby, HKWorkoutActivityType.Rugby},
		{WorkoutType.Running, HKWorkoutActivityType.Running},
		{WorkoutType.Sailing, HKWorkoutActivityType.Sailing},
		{WorkoutType.SkatingSports, HKWorkoutActivityType.SkatingSports},
		{WorkoutType.SnowSports, HKWorkoutActivityType.SnowSports},
		{WorkoutType.Soccer, HKWorkoutActivityType.Soccer},
		{WorkoutType.Softball, HKWorkoutActivityType.Softball},
		{WorkoutType.Squash, HKWorkoutActivityType.Squash},
		{WorkoutType.StairClimbing, HKWorkoutActivityType.StairClimbing},
		{WorkoutType.SurfingSports, HKWorkoutActivityType.SurfingSports},
		{WorkoutType.Swimming, HKWorkoutActivityType.Swimming},
		{WorkoutType.TableTennis, HKWorkoutActivityType.TableTennis},
		{WorkoutType.Tennis, HKWorkoutActivityType.Tennis},
		{WorkoutType.TrackAndField, HKWorkoutActivityType.TrackAndField},
		{WorkoutType.TraditionalStrengthTraining, HKWorkoutActivityType.TraditionalStrengthTraining},
		{WorkoutType.Volleyball, HKWorkoutActivityType.Volleyball},
		{WorkoutType.Walking, HKWorkoutActivityType.Walking},
		{WorkoutType.WaterFitness, HKWorkoutActivityType.WaterFitness},
		{WorkoutType.WaterPolo, HKWorkoutActivityType.WaterPolo},
		{WorkoutType.WaterSports, HKWorkoutActivityType.WaterSports},
		{WorkoutType.Wrestling, HKWorkoutActivityType.Wrestling},
		{WorkoutType.Yoga, HKWorkoutActivityType.Yoga},
		{WorkoutType.Barre, HKWorkoutActivityType.Barre},
		{WorkoutType.CoreTraining, HKWorkoutActivityType.CoreTraining},
		{WorkoutType.CrossCountrySkiing, HKWorkoutActivityType.CrossCountrySkiing},
		{WorkoutType.DownhillSkiing, HKWorkoutActivityType.DownhillSkiing},
		{WorkoutType.Flexibility, HKWorkoutActivityType.Flexibility},
		{WorkoutType.HighIntensityIntervalTraining, HKWorkoutActivityType.HighIntensityIntervalTraining},
		{WorkoutType.JumpRope, HKWorkoutActivityType.JumpRope},
		{WorkoutType.Kickboxing, HKWorkoutActivityType.Kickboxing},
		{WorkoutType.Pilates, HKWorkoutActivityType.Pilates},
		{WorkoutType.Snowboarding, HKWorkoutActivityType.Snowboarding},
		{WorkoutType.Stairs, HKWorkoutActivityType.Stairs},
		{WorkoutType.StepTraining, HKWorkoutActivityType.StepTraining},
		{WorkoutType.Other, HKWorkoutActivityType.Other},
	};

	public bool IsSupported => HKHealthStore.IsHealthDataAvailable;

	readonly SemaphoreSlim semaphore = new(1, 1);

	/// <summary>
	/// Checks and requests the specified permissions for a given health parameter.
	/// </summary>
	/// <param name="healthParameter">The health parameter for which to check or request permission.</param>
	/// <param name="permissionType">The type of permission to check or request. Can be Read, Write, or both.</param>
	/// <returns>Returns a <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="bool"/> result indicating the success of the operation.</returns>
	/// <exception cref="HealthException">Throws when HealthKit is not available, the health parameter is not available, or permission is not granted.</exception>
	// NSDate ↔ DateTimeOffset helpers
	static NSDate ToNSDate(DateTimeOffset dto) => (NSDate)dto.UtcDateTime;
	static DateTimeOffset ToDateTimeOffset(NSDate date) => new DateTimeOffset((DateTime)date, TimeSpan.Zero);

	public async Task<bool> CheckPermissionAsync(HealthParameter healthParameter, PermissionType permissionType,
		CancellationToken cancellationToken = default)
	{
		if (!IsSupported)
		{
			throw new HealthException("HealthKit not available on your device");
		}

		if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
		{
			throw new HealthException($"{healthParameter} not available");
		}

		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

		NSSet? typesToRead = null;
		NSSet? typesToWrite = null;

		if (permissionType.HasFlag(PermissionType.Read))
		{
			typesToRead = new NSSet(HKQuantityType.Create(requestedHealthParameter));
		}

		if (permissionType.HasFlag(PermissionType.Write))
		{
			typesToWrite = new NSSet(HKQuantityType.Create(requestedHealthParameter));
		}

		try
		{
			var status = await healthStore.RequestAuthorizationToShareAsync(typesToWrite, typesToRead).ConfigureAwait(false);

			// Check read permission if required
			if (permissionType.HasFlag(PermissionType.Read) && !status.Item1)
			{
				throw new HealthException($"No read permission granted for {healthParameter}");
			}

			// Check write permission if required
			if (permissionType.HasFlag(PermissionType.Write) && !status.Item1)
			{
				throw new HealthException($"No write permission granted for {healthParameter}");
			}
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
		finally
		{
			semaphore.Release();
		}

		return true;
	}

	public async Task<bool> CheckWorkoutPermissionAsync(PermissionType permissionType,
		CancellationToken cancellationToken = default)
	{
		if (!IsSupported)
		{
			throw new HealthException("HealthKit not available on your device");
		}

		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			// Include both HKObjectType.WorkoutType and HKSeriesType.WorkoutRouteType so the
			// caller can read and write GPS routes without a separate authorization step.
			var objects = new NSObject[] { HKObjectType.WorkoutType, HKSeriesType.WorkoutRouteType };
			NSSet? typesToRead = permissionType.HasFlag(PermissionType.Read) ? new NSSet(objects) : null;
			NSSet? typesToWrite = permissionType.HasFlag(PermissionType.Write) ? new NSSet(objects) : null;

			var (success, _) = await healthStore.RequestAuthorizationToShareAsync(typesToWrite, typesToRead).ConfigureAwait(false);
			return success;
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
		finally
		{
			semaphore.Release();
		}
	}

	/// <summary>
	/// Reads the cumulative count of a specified <see cref="HealthParameter"/> within a given date range.
	/// </summary>
	/// <param name="healthParameter">The health parameter for which to retrieve the cumulative count (e.g., StepCount, HeartRate).</param>
	/// <param name="from">The start date for the date range within which to retrieve health data.</param>
	/// <param name="until">The end date for the date range within which to retrieve health data.</param>
	/// <returns>
	/// A <see cref="Task"/> that returns a <see cref="double"/> representing the cumulative count of the specified health parameter within the given date range.
	/// </returns>
	/// <exception cref="HealthException">
	/// Thrown when the specified <see cref="HealthParameter"/> is not available or an error occurs during query execution.
	/// </exception>
	/// <example>
	/// <code>
	/// var totalStepCount = await ReadCountAsync(HealthParameter.StepCount, DateTime.Now.AddDays(-7), DateTime.Now);
	/// </code>
	/// </example>
	public async Task<double> ReadCountAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			var tcs = new TaskCompletionSource<double>();

			var predicate = HKQuery.GetPredicateForSamples(ToNSDate(from), ToNSDate(until), HKQueryOptions.StrictStartDate);

			HKStatisticsQuery query = new(quantityType, predicate, HKStatisticsOptions.CumulativeSum,
				(HKStatisticsQuery _, HKStatistics result, NSError error) =>
				{
					if (error == null && result != null)
					{
						HKQuantity sum = result.SumQuantity();
						if (sum != null)
						{
							tcs.TrySetResult(sum.GetDoubleValue(HKUnit.Count));
						}
						else
						{
							tcs.TrySetResult(0);
						}
					}
					else
					{
						tcs.TrySetException(new HealthException(error?.LocalizedDescription ?? "An error occurred"));
					}
				});

			healthStore.ExecuteQuery(query);

			return await tcs.Task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
		finally
		{
			semaphore.Release();
		}
	}


	/// <summary>
	/// Reads the average value of a specified <see cref="HealthParameter"/> within a given date range.
	/// </summary>
	/// <param name="healthParameter">The health parameter for which to retrieve the average value (e.g., StepCount, HeartRate).</param>
	/// <param name="from">The start date for the date range within which to retrieve health data.</param>
	/// <param name="until">The end date for the date range within which to retrieve health data.</param>
	/// <param name="unit">The unit in which the average value should be returned (e.g., "count", "bpm").</param>
	/// <returns>
	/// A <see cref="Task"/> that returns a <see cref="double"/> representing the average value of the specified health parameter within the given date range.
	/// </returns>
	/// <exception cref="HealthException">
	/// Thrown when the specified <see cref="HealthParameter"/> is not available or an error occurs during query execution.
	/// </exception>
	/// <example>
	/// <code>
	/// var averageHeartRate = await ReadAverageAsync(HealthParameter.HeartRate, DateTime.Now.AddDays(-7), DateTime.Now, "bpm");
	/// </code>
	/// </example>
	// These methods intentionally do not acquire the semaphore; they delegate to ReadAllAsync which does.
	// Do NOT add semaphore.WaitAsync() here.
	public async Task<double?> ReadAverageAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var valuesRead = await ReadAllAsync(healthParameter, from, until, unit, cancellationToken).ConfigureAwait(false);
			return valuesRead.Count > 0 ? valuesRead.Average(e => e.Value) : null;
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
	}

	/// <summary>
	/// Reads the minimum value of a specified <see cref="HealthParameter"/> within a given date range.
	/// </summary>
	/// <param name="healthParameter">The health parameter for which to retrieve the minimum value (e.g., StepCount, HeartRate).</param>
	/// <param name="from">The start date for the date range within which to retrieve health data.</param>
	/// <param name="until">The end date for the date range within which to retrieve health data.</param>
	/// <param name="unit">The unit in which the minimum value should be returned (e.g., "count", "bpm").</param>
	/// <returns>
	/// A <see cref="Task"/> that returns a <see cref="double"/> representing the minimum value of the specified health parameter within the given date range.
	/// </returns>
	/// <exception cref="HealthException">
	/// Thrown when the specified <see cref="HealthParameter"/> is not available or an error occurs during query execution.
	/// </exception>
	/// <example>
	/// <code>
	/// var minHeartRate = await ReadMinAsync(HealthParameter.HeartRate, DateTime.Now.AddDays(-7), DateTime.Now, "bpm");
	/// </code>
	/// </example>
	public async Task<double?> ReadMinAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var valuesRead = await ReadAllAsync(healthParameter, from, until, unit, cancellationToken).ConfigureAwait(false);
			return valuesRead.Count > 0 ? valuesRead.Min(e => e.Value) : null;
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
	}

	/// <summary>
	/// Reads the maximum value of a specified <see cref="HealthParameter"/> within a given date range.
	/// </summary>
	/// <param name="healthParameter">The health parameter for which to retrieve the maximum value (e.g., StepCount, HeartRate).</param>
	/// <param name="from">The start date for the date range within which to retrieve health data.</param>
	/// <param name="until">The end date for the date range within which to retrieve health data.</param>
	/// <param name="unit">The unit in which the maximum value should be returned (e.g., "count", "bpm").</param>
	/// <returns>
	/// A <see cref="Task"/> that returns a <see cref="double"/> representing the maximum value of the specified health parameter within the given date range.
	/// </returns>
	/// <exception cref="HealthException">
	/// Thrown when the specified <see cref="HealthParameter"/> is not available or an error occurs during query execution.
	/// </exception>
	/// <example>
	/// <code>
	/// var maxHeartRate = await ReadMaxAsync(HealthParameter.HeartRate, DateTime.Now.AddDays(-7), DateTime.Now, "bpm");
	/// </code>
	/// </example>
	public async Task<double?> ReadMaxAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var valuesRead = await ReadAllAsync(healthParameter, from, until, unit, cancellationToken).ConfigureAwait(false);
			return valuesRead.Count > 0 ? valuesRead.Max(e => e.Value) : null;
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
	}

	/// <summary>
	/// Reads the latest health data value for a specified <see cref="HealthParameter"/> within a given date range.
	/// </summary>
	/// <param name="healthParameter">The health parameter for which to retrieve the latest data (e.g., StepCount, HeartRate).</param>
	/// <param name="from">The start date for the date range within which to retrieve health data.</param>
	/// <param name="until">The end date for the date range within which to retrieve health data.</param>
	/// <param name="unit">The unit in which the health data should be returned (e.g., "count/min" for HeartRate).</param>
	/// <returns>
	/// A <see cref="Task"/> that returns a <see cref="double?"/> representing the latest health data value for the specified parameter within the given date range, or <see langword="null"/> if no value is found.
	/// </returns>
	/// <exception cref="HealthException">
	/// Thrown when the specified <see cref="HealthParameter"/> is not available or an error occurs during query execution.
	/// </exception>
	/// <example>
	/// <code>
	/// var latestHeartRate = await ReadLatestAsync(HealthParameter.HeartRate, DateTime.Now.AddDays(-1), DateTime.Now, "count/min");
	/// </code>
	/// </example>
	public async Task<double?> ReadLatestAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		var tcs = new TaskCompletionSource<double?>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			NSPredicate predicate = HKQuery.GetPredicateForSamples(ToNSDate(from), ToNSDate(until), HKQueryOptions.StrictStartDate);

			HKSampleQuery query = new(quantityType, predicate, 1, new NSSortDescriptor[] { new NSSortDescriptor(HKSample.SortIdentifierEndDate, false) },
				(HKSampleQuery _, HKSample[] results, NSError error) =>
				{
					if (error == null && results?.Length > 0)
					{
						if (results[0] is HKQuantitySample sample)
						{
							HKUnit hkUnit = GetHKUnit(unit);
							double? returnValue = sample.Quantity.GetDoubleValue(hkUnit);
							tcs.TrySetResult(returnValue);
						}
						else
						{
							tcs.TrySetResult(0d);
						}
					}
					else
					{
						if (error != null)
						{
							tcs.TrySetException(new HealthException(error?.LocalizedDescription ?? error?.Description));
						}
						else
						{
							tcs.TrySetResult(0d);
						}
					}
				});

			healthStore.ExecuteQuery(query);
		}
		catch (Exception ex)
		{
			tcs.TrySetException(new HealthException(ex.Message, ex));
		}
		finally
		{
			semaphore.Release();
		}

		return await tcs.Task.ConfigureAwait(false);
	}

	/// <summary>
	/// Reads all health data for a specified <see cref="HealthParameter"/> within a given date range and returns them as a list of <see cref="Sample"/>.
	/// </summary>
	/// <param name="healthParameter">The health parameter for which to retrieve data (e.g., StepCount, HeartRate).</param>
	/// <param name="from">The start date for the date range within which to retrieve health data.</param>
	/// <param name="until">The end date for the date range within which to retrieve health data.</param>
	/// <param name="unit">The unit in which the health data should be returned (e.g., "count/min" for HeartRate).</param>
	/// <returns>
	/// A <see cref="Task"/> that returns a list of <see cref="double"/> representing the health data for the specified parameter within the given date range.
	/// </returns>
	/// <exception cref="HealthException">
	/// Thrown when the specified <see cref="HealthParameter"/> is not available or an error occurs during query execution.
	/// </exception>
	/// <example>
	/// <code>
	/// var heartRates = await ReadAllAsync(HealthParameter.HeartRate, DateTime.Now.AddDays(-7), DateTime.Now, "count/min");
	/// </code>
	/// </example>
	public async Task<List<Sample>> ReadAllAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		var tcs = new TaskCompletionSource<List<Sample>>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			NSPredicate predicate = HKQuery.GetPredicateForSamples(ToNSDate(from), ToNSDate(until), HKQueryOptions.StrictStartDate);

			List<Sample> healthValues = new();

			HKSampleQuery query = new(quantityType, predicate, HKSampleQuery.NoLimit, new NSSortDescriptor[] { new NSSortDescriptor(HKSample.SortIdentifierEndDate, true) },
				(HKSampleQuery _, HKSample[] results, NSError error) =>
				{
					if (error == null && results != null)
					{
						foreach (HKQuantitySample hkSample in results.Cast<HKQuantitySample>())
						{
							if (hkSample == null)
							{
								continue;
							}

							var source = hkSample.Source?.Name ?? string.Empty;
							HKUnit hkUnit = GetHKUnit(unit);

							var sample = new Sample(ToDateTimeOffset(hkSample.StartDate), ToDateTimeOffset(hkSample.EndDate),
									hkSample.Quantity.GetDoubleValue(hkUnit),
									source,
									unit,
									description: hkSample.Quantity.Description);
							healthValues.Add(sample);
						}
						tcs.TrySetResult(healthValues);
					}
					else
					{
						tcs.TrySetException(new HealthException(error?.LocalizedDescription ?? "Unknown error"));
					}
				});

			healthStore.ExecuteQuery(query);
		}
		catch (Exception ex)
		{
			tcs.TrySetException(new HealthException(ex.Message, ex));
		}
		finally
		{
			semaphore.Release();
		}

		return await tcs.Task.ConfigureAwait(false);
	}

	/// <summary>
	/// Writes a count-based health value to the HealthKit store.
	/// </summary>
	/// <param name="healthParameter">The type of health parameter to write (e.g., steps, heart rate).</param>
	/// <param name="date">The date associated with the value to store. If null, the current date and time is used.</param>
	/// <param name="valueToStore">The numerical value to store.</param>
	/// <returns>
	/// A <see cref="Task{TResult}"/> that represents the asynchronous operation. The task result indicates whether the value was successfully saved (true) or not (false).
	/// </returns>
	/// <example>
	/// This sample shows how to call the <see cref="WriteAsync"/> method for writing step counts for a specific date.
	/// <code>
	/// DateTime specificDate = new DateTime(2023, 8, 27);
	/// bool isSaved = await WriteCountBasedHealthValueAsync(HealthParameter.StepCount, specificDate, 1000);
	/// </code>
	/// </example>
	/// <exception cref="HealthException">
	/// Thrown when the specified health parameter is not available, when saving fails, or any other health-related error occurs.
	/// </exception>
	public async Task<bool> WriteAsync(HealthParameter healthParameter, DateTimeOffset? date, double valueToStore, string unit,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			var tcs = new TaskCompletionSource<bool>();

			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			var currentDate = date ?? DateTimeOffset.Now;

			NSDate startDate = ToNSDate(currentDate);
			NSDate endDate = startDate.AddSeconds(1);
			HKUnit hkUnit = GetHKUnit(unit);

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			HKQuantity quantity = HKQuantity.FromQuantity(hkUnit, valueToStore);

			NSDictionary metaData = new(HKMetadataKey.WasUserEntered, new NSNumber(true));
			HKQuantitySample sample = HKQuantitySample.FromType(quantityType, quantity, startDate, endDate, metaData);

			healthStore.SaveObject(sample, (success, error) =>
			{
				if (success)
				{
					tcs.TrySetResult(true);
				}
				else
				{
					tcs.TrySetException(new HealthException(error.LocalizedDescription));
				}
			});
			return await tcs.Task.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
		finally
		{
			semaphore.Release();
		}
	}

	static HKUnit GetHKUnit(string unit)
	{
		HKUnit hkUnit;

		if (unit == Constants.Units.Concentration.MillilitersPerKilogramPerMinute)
		{
			hkUnit = HKUnit.Liter.UnitDividedBy(HKUnit.FromString(Maui.Health.Constants.Units.Mass.Kilograms).UnitMultipliedBy(HKUnit.Minute));
		}
		else
		{
			hkUnit = HKUnit.FromString(unit);
		}

		return hkUnit;
	}

	public async Task<Sample?> ReadLatestAvailableAsync(HealthParameter healthParameter, string unit,
		CancellationToken cancellationToken = default)
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		var tcs = new TaskCompletionSource<Sample?>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			NSPredicate predicate = HKQuery.GetPredicateForSamples(default, default, HKQueryOptions.None);

			HKSampleQuery query = new(quantityType, predicate, 1, new NSSortDescriptor[] { new NSSortDescriptor(HKSample.SortIdentifierEndDate, false) },
				(HKSampleQuery _, HKSample[] results, NSError error) =>
				{
					if (error == null && results?.Length > 0)
					{
						if (results[0] is HKQuantitySample sample)
						{
							HKUnit hkUnit = GetHKUnit(unit);

							Sample returnSample = new(
								from: ToDateTimeOffset(sample.StartDate),
								until: ToDateTimeOffset(sample.EndDate),
								value: sample.Quantity.GetDoubleValue(hkUnit),
								source: sample.Source.Name,
								unit: unit,
								description: sample.Quantity.Description);

							tcs.TrySetResult(returnSample);
						}
						else
						{
							tcs.TrySetResult(default);
						}
					}
					else
					{
						if (error != null)
						{
							tcs.TrySetException(new HealthException(error?.LocalizedDescription ?? error?.Description));
						}
						else
						{
							tcs.TrySetResult(default);
						}
					}
				});

			healthStore.ExecuteQuery(query);
		}
		catch (Exception ex)
		{
			tcs.TrySetException(new HealthException(ex.Message, ex));
		}
		finally
		{
			semaphore.Release();
		}

		return await tcs.Task.ConfigureAwait(false);
	}

	// ── Workout methods ───────────────────────────────────────────────────────

	public async Task<List<WorkoutSession>> ReadWorkoutsAsync(WorkoutType workoutType, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default)
	{
		// Acquire semaphore only for the raw HealthKit query; route queries are per-workout and
		// independent, so they run outside the semaphore to avoid holding it across multiple round-trips.
		HKWorkout[] hkWorkouts;
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			hkWorkouts = await QueryHKWorkoutsAsync(workoutType, from, until, ascending: true, limit: HKSampleQuery.NoLimit).ConfigureAwait(false);
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
		finally
		{
			semaphore.Release();
		}

		var sessions = new List<WorkoutSession>();
		foreach (var workout in hkWorkouts)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var route = await QueryWorkoutRouteAsync(workout).ConfigureAwait(false);
			sessions.Add(MapHKWorkout(workout, route));
		}
		return sessions;
	}

	public async Task<WorkoutSession?> ReadLatestWorkoutAsync(WorkoutType workoutType, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default)
	{
		HKWorkout[] hkWorkouts;
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			hkWorkouts = await QueryHKWorkoutsAsync(workoutType, from, until, ascending: false, limit: 1).ConfigureAwait(false);
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
		finally
		{
			semaphore.Release();
		}

		if (hkWorkouts.Length == 0)
		{
			return null;
		}

		var route = await QueryWorkoutRouteAsync(hkWorkouts[0]).ConfigureAwait(false);
		return MapHKWorkout(hkWorkouts[0], route);
	}

	public async Task<bool> WriteWorkoutAsync(WorkoutSession session, CancellationToken cancellationToken = default)
	{
		// Phase 1: write the workout and retrieve it — hold semaphore only for this phase.
		HKWorkout? savedWorkout = null;
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			if (!workoutTypeMapping.TryGetValue(session.WorkoutType, out var activityType))
			{
				activityType = HKWorkoutActivityType.Other;
			}

			var config = new HKWorkoutConfiguration { ActivityType = activityType };
			var builder = new HKWorkoutBuilder(healthStore, config, device: null);

			var (_, beginError) = await builder.BeginCollectionAsync(ToNSDate(session.From)).ConfigureAwait(false);
			if (beginError is not null)
			{
				throw new HealthException(beginError.LocalizedDescription);
			}

			var (_, endError) = await builder.EndCollectionAsync(ToNSDate(session.Until)).ConfigureAwait(false);
			if (endError is not null)
			{
				throw new HealthException(endError.LocalizedDescription);
			}

			var (finishSuccess, finishError) = await builder.FinishWorkoutAsync().ConfigureAwait(false);
			if (finishError is not null)
			{
				throw new HealthException(finishError.LocalizedDescription);
			}

			if (session.Route.Count > 0 && finishSuccess)
			{
				// FinishWorkoutAsync() does not return the HKWorkout object (binding limitation).
				// Query for the workout we just saved using the known time window.
				var saved = await QueryHKWorkoutsAsync(
					session.WorkoutType, session.From, session.Until,
					ascending: false, limit: 1).ConfigureAwait(false);

				if (saved.Length == 0)
				{
					throw new HealthException("Workout was saved but could not be retrieved to attach the GPS route.");
				}

				savedWorkout = saved[0];
			}
		}
		catch (HealthException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
		finally
		{
			semaphore.Release();
		}

		// Phase 2: attach GPS route outside the semaphore — route builder ops are self-contained.
		if (savedWorkout is not null)
		{
			try
			{
				var routeBuilder = new HKWorkoutRouteBuilder(healthStore, device: null);
				var clLocations = session.Route
					.Select(p => new CLLocation(
						new CLLocationCoordinate2D(p.Latitude, p.Longitude),
						altitude: p.AltitudeInMeters ?? 0,
						hAccuracy: p.HorizontalAccuracyInMeters ?? -1,
						vAccuracy: p.VerticalAccuracyInMeters ?? -1,
						timestamp: ToNSDate(p.Time)))
					.ToArray();

				var (insertSuccess, insertError) = await routeBuilder.InsertRouteDataAsync(clLocations).ConfigureAwait(false);
				if (!insertSuccess)
				{
					throw new HealthException(insertError?.LocalizedDescription ?? "Failed to insert GPS route data.");
				}

				await routeBuilder.FinishRouteAsync(savedWorkout, metadata: null).ConfigureAwait(false);
			}
			catch (HealthException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new HealthException(ex.Message, ex);
			}
		}

		return true;
	}

	// ── Private workout helpers ───────────────────────────────────────────────

	Task<HKWorkout[]> QueryHKWorkoutsAsync(WorkoutType workoutType, DateTimeOffset from, DateTimeOffset until, bool ascending, nuint limit)
	{
		var tcs = new TaskCompletionSource<HKWorkout[]>();

		NSPredicate predicate;
		if (workoutType == WorkoutType.Other)
		{
			predicate = HKQuery.GetPredicateForSamples(ToNSDate(from), ToNSDate(until), HKQueryOptions.StrictStartDate);
		}
		else
		{
			var hkType = workoutTypeMapping.TryGetValue(workoutType, out var t) ? t : HKWorkoutActivityType.Other;
			var typePred = HKQuery.GetPredicateForWorkouts(hkType);
			var timePred = HKQuery.GetPredicateForSamples(ToNSDate(from), ToNSDate(until), HKQueryOptions.StrictStartDate);
			predicate = NSCompoundPredicate.CreateAndPredicate(new[] { typePred, timePred });
		}

		var sort = new NSSortDescriptor(HKSample.SortIdentifierEndDate, ascending);
		var query = new HKSampleQuery(HKObjectType.WorkoutType, predicate, limit, new[] { sort },
			(_, results, error) =>
			{
				if (error is not null)
				{
					tcs.TrySetException(new HealthException(error.LocalizedDescription));
				}
				else
				{
					tcs.TrySetResult(results?.Cast<HKWorkout>().ToArray() ?? Array.Empty<HKWorkout>());
				}
			});

		healthStore.ExecuteQuery(query);
		return tcs.Task;
	}

	async Task<IReadOnlyList<RoutePoint>> QueryWorkoutRouteAsync(HKWorkout workout)
	{
		// A workout with pause/resume can have multiple HKWorkoutRoute segments — fetch all of them.
		var routesTcs = new TaskCompletionSource<HKWorkoutRoute[]>();
		var routePredicate = HKQuery.GetPredicateForObjectsFromWorkout(workout);
		var routeQuery = new HKSampleQuery(HKSeriesType.WorkoutRouteType, routePredicate, HKSampleQuery.NoLimit, null,
			(_, results, error) =>
			{
				if (error is not null)
				{
					routesTcs.TrySetException(new HealthException(error.LocalizedDescription));
				}
				else
				{
					routesTcs.TrySetResult(results?.Cast<HKWorkoutRoute>().ToArray() ?? Array.Empty<HKWorkoutRoute>());
				}
			});
		healthStore.ExecuteQuery(routeQuery);

		var routes = await routesTcs.Task.ConfigureAwait(false);
		if (routes.Length == 0)
		{
			return Array.Empty<RoutePoint>();
		}

		var allPoints = new List<RoutePoint>();
		foreach (var route in routes)
		{
			var locations = new List<CLLocation>();
			var locationsTcs = new TaskCompletionSource<bool>();

			var locationQuery = new HKWorkoutRouteQuery(route, (_, newLocations, done, error) =>
			{
				if (error is not null)
				{
					locationsTcs.TrySetException(new HealthException(error.LocalizedDescription));
					return;
				}

				if (newLocations is not null)
				{
					locations.AddRange(newLocations);
				}

				if (done)
				{
					locationsTcs.TrySetResult(true);
				}
			});
			healthStore.ExecuteQuery(locationQuery);

			await locationsTcs.Task.ConfigureAwait(false);

			allPoints.AddRange(locations.Select(loc => new RoutePoint
			{
				Time = ToDateTimeOffset(loc.Timestamp),
				Latitude = loc.Coordinate.Latitude,
				Longitude = loc.Coordinate.Longitude,
				AltitudeInMeters = loc.Altitude,
				HorizontalAccuracyInMeters = loc.HorizontalAccuracy >= 0 ? loc.HorizontalAccuracy : null,
				VerticalAccuracyInMeters = loc.VerticalAccuracy >= 0 ? loc.VerticalAccuracy : null,
			}));
		}

		return allPoints.AsReadOnly();
	}

	WorkoutType GetActualWorkoutType(HKWorkoutActivityType activityType)
	{
		foreach (var kv in workoutTypeMapping)
		{
			if (kv.Value == activityType)
			{
				return kv.Key;
			}
		}

		return WorkoutType.Other;
	}

	WorkoutSession MapHKWorkout(HKWorkout workout, IReadOnlyList<RoutePoint> route) =>
		new()
		{
			WorkoutType = GetActualWorkoutType(workout.WorkoutActivityType),
			From = ToDateTimeOffset(workout.StartDate),
			Until = ToDateTimeOffset(workout.EndDate),
			DurationInSeconds = workout.Duration,
			EnergyBurnedInCalories = workout.TotalEnergyBurned?.GetDoubleValue(HKUnit.Kilocalorie),
			TotalDistanceInMeters = workout.TotalDistance?.GetDoubleValue(HKUnit.Meter),
			Title = null,
			Source = workout.SourceRevision?.Source?.Name,
			Route = route,
		};
}