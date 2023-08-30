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
	public async Task<bool> CheckPermissionAsync(HealthParameter healthParameter, PermissionType permissionType)
	{
		if (!IsSupported)
		{
			throw new HealthException("HealthKit not available on your device");
		}

		if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
		{
			throw new HealthException($"{healthParameter} not available");
		}

		await semaphore.WaitAsync();

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
			var status = await healthStore.RequestAuthorizationToShareAsync(typesToWrite, typesToRead);

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
	public async Task<double> ReadCountAsync(HealthParameter healthParameter, DateTime from, DateTime until)
	{
		await semaphore.WaitAsync();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			var tcs = new TaskCompletionSource<double>(); // Task completion source

			var predicate = HKQuery.GetPredicateForSamples((NSDate)from, (NSDate)until, HKQueryOptions.StrictStartDate);


			HKStatisticsQuery query = new(quantityType, predicate, HKStatisticsOptions.CumulativeSum,
				(HKStatisticsQuery _, HKStatistics result, NSError error) =>
				{
					if (error == null && result != null)
					{
						HKQuantity sum = result.SumQuantity();
						if (sum != null)
						{
							tcs.SetResult(sum.GetDoubleValue(HKUnit.Count));
						}
						else
						{
							tcs.SetResult(0);
						}
					}
					else
					{
						tcs.SetException(new HealthException(error?.LocalizedDescription ?? "An error occurred"));
					}
				});

			healthStore.ExecuteQuery(query);

			return await tcs.Task;
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
	public async Task<double?> ReadAverageAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		try
		{
			var valuesRead = await ReadAllAsync(healthParameter, from, until, unit);
			if (valuesRead != null)
			{
				return valuesRead.Average(e => e.Value);
			}
			else
			{
				return null;
			}
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
	public async Task<double?> ReadMinAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		try
		{
			var valuesRead = await ReadAllAsync(healthParameter, from, until, unit);
			if (valuesRead != null)
			{
				return valuesRead.Min(e => e.Value);
			}
			else
			{
				return null;
			}
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
	public async Task<double?> ReadMaxAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		try
		{
			var valuesRead = await ReadAllAsync(healthParameter, from, until, unit);
			if (valuesRead != null)
			{
				return valuesRead.Max(e => e.Value);
			}
			else
			{
				return null;
			}
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
	public async Task<double?> ReadLatestAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		await semaphore.WaitAsync();
		var tcs = new TaskCompletionSource<double?>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			NSPredicate predicate = HKQuery.GetPredicateForSamples((NSDate)from, (NSDate)until, HKQueryOptions.StrictStartDate);

			HKSampleQuery query = new(quantityType, predicate, 1, new NSSortDescriptor[] { new NSSortDescriptor(HKSample.SortIdentifierEndDate, false) },
				(HKSampleQuery _, HKSample[] results, NSError error) =>
				{
					if (error == null && results?.Length > 0)
					{
						if(results[0] is HKQuantitySample sample)
						{
							HKUnit hkUnit = GetHKUnit(unit);
							double? returnValue = sample.Quantity.GetDoubleValue(hkUnit);
							tcs.SetResult(returnValue);
						}
						else
						{
							tcs.SetResult(0d);
						}
					}
					else
					{
						if (error != null)
						{
							tcs.SetException(new HealthException(error?.LocalizedDescription ?? error?.Description));
						}
						else
						{
							tcs.SetResult(0d);
						}
					}
				});

			healthStore.ExecuteQuery(query);
		}
		catch (Exception ex)
		{
			tcs.SetException(new HealthException(ex.Message, ex));
		}
		finally
		{
			semaphore.Release();
		}

		return await tcs.Task;
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
	public async Task<List<Sample>> ReadAllAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		await semaphore.WaitAsync();
		var tcs = new TaskCompletionSource<List<Sample>>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			NSPredicate predicate = HKQuery.GetPredicateForSamples((NSDate)from, (NSDate)until, HKQueryOptions.StrictStartDate);

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

							var source = string.Empty;

							if (hkSample.Source != null)
							{
								source = hkSample?.Source.Name;
							}

							HKUnit hkUnit = GetHKUnit(unit);

							var sample = new Sample((DateTime)hkSample.StartDate, (DateTime)hkSample.EndDate,
									hkSample.Quantity.GetDoubleValue(hkUnit),
									source,
									unit);
							healthValues.Add(sample);
						}
						tcs.SetResult(healthValues);
					}
					else
					{
						tcs.SetException(new HealthException(error?.LocalizedDescription ?? "Unknown error"));
					}
				});

			healthStore.ExecuteQuery(query);
		}
		catch (Exception ex)
		{
			tcs.SetException(new HealthException(ex.Message, ex));
		}
		finally
		{
			semaphore.Release();
		}

		return await tcs.Task;
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
	public async Task<bool> WriteAsync(HealthParameter healthParameter, DateTime? date, double valueToStore, string unit)
	{
		await semaphore.WaitAsync();

		try
		{
			var tcs = new TaskCompletionSource<bool>();

			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			

			var currentDate = DateTime.Now;
			if (date.HasValue)
			{
				currentDate = date.Value;
			}

			NSDate startDate = (NSDate)currentDate;
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
					tcs.SetResult(true);
				}
				else
				{
					tcs.SetException(new HealthException(error.LocalizedDescription));
				}
			});
			return await tcs.Task;
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
}