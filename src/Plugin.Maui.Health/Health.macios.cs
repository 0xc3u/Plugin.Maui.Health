using HealthKit;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;

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

	public bool IsSupported => HKHealthStore.IsHealthDataAvailable;

	readonly SemaphoreSlim semaphore = new(1, 1);

	/// <summary>
	/// Asynchronously checks and requests the specified permissions for a given health parameter.
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
		catch(Exception ex)
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
	/// Asynchronously reads the cumulative count of a specified <see cref="HealthParameter"/> within a given date range.
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
	/// Asynchronously reads the average value of a specified <see cref="HealthParameter"/> within a given date range.
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
	public async Task<double> ReadAverageAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		await semaphore.WaitAsync();
		var tcs = new TaskCompletionSource<double>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");

			var predicate = HKQuery.GetPredicateForSamples((NSDate)from, (NSDate)until, HKQueryOptions.StrictStartDate);

			HKStatisticsQuery query = new(quantityType, predicate, HKStatisticsOptions.CumulativeSum,
				(HKStatisticsQuery _, HKStatistics result, NSError error) =>
				{
					if (error == null && result != null)
					{
						HKQuantity sum = result.AverageQuantity();
						if (sum != null)
						{
							double returnValue = sum.GetDoubleValue(HKUnit.FromString(unit));
							tcs.SetResult(returnValue);
						}
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
	/// Asynchronously reads the minimum value of a specified <see cref="HealthParameter"/> within a given date range.
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
	public async Task<double> ReadMinAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		await semaphore.WaitAsync();
		var tcs = new TaskCompletionSource<double>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");

			var predicate = HKQuery.GetPredicateForSamples((NSDate)from, (NSDate)until, HKQueryOptions.StrictStartDate);

			HKStatisticsQuery query = new(quantityType, predicate, HKStatisticsOptions.CumulativeSum,
				(HKStatisticsQuery _, HKStatistics result, NSError error) =>
				{
					if (error == null && result != null)
					{
						HKQuantity sum = result.MinimumQuantity();
						if (sum != null)
						{
							double returnValue = sum.GetDoubleValue(HKUnit.FromString(unit));
							tcs.SetResult(returnValue);
						}
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
	/// Asynchronously reads the maximum value of a specified <see cref="HealthParameter"/> within a given date range.
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
	public async Task<double> ReadMaxAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		await semaphore.WaitAsync();
		var tcs = new TaskCompletionSource<double>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");

			var predicate = HKQuery.GetPredicateForSamples((NSDate)from, (NSDate)until, HKQueryOptions.StrictStartDate);

			HKStatisticsQuery query = new(quantityType, predicate, HKStatisticsOptions.CumulativeSum,
				(HKStatisticsQuery _, HKStatistics result, NSError error) =>
				{
					if (error == null && result != null)
					{
						HKQuantity sum = result.MaximumQuantity();
						if (sum != null)
						{
							double returnValue = sum.GetDoubleValue(HKUnit.FromString(unit));
							tcs.SetResult(returnValue);
						}
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
	/// Asynchronously reads the latest health data value for a specified <see cref="HealthParameter"/> within a given date range.
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
					if (error == null && results?.Length > 0 && results[0] is HKQuantitySample sample)
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

						double? returnValue = sample.Quantity.GetDoubleValue(hkUnit);
						tcs.SetResult(returnValue);
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
	/// Asynchronously reads all health data for a specified <see cref="HealthParameter"/> within a given date range and returns them as a list of <see cref="double"/>.
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
	public async Task<List<double>> ReadAllAsync(HealthParameter healthParameter, DateTime from, DateTime until, string unit)
	{
		await semaphore.WaitAsync();
		var tcs = new TaskCompletionSource<List<double>>();

		try
		{
			if (!healthParameterMapping.TryGetValue(healthParameter, out var requestedHealthParameter))
			{
				throw new HealthException($"{healthParameter} not available");
			}

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			NSPredicate predicate = HKQuery.GetPredicateForSamples((NSDate)from, (NSDate)until, HKQueryOptions.StrictStartDate);

			List<double> healthValues = new();

			HKSampleQuery query = new(quantityType, predicate, HKSampleQuery.NoLimit, new NSSortDescriptor[] { new NSSortDescriptor(HKSample.SortIdentifierEndDate, true) },
				(HKSampleQuery _, HKSample[] results, NSError error) =>
				{
					if (error == null && results != null)
					{
						healthValues.AddRange(from HKQuantitySample sample in results.Cast<HKQuantitySample>()
											  select sample.Quantity.GetDoubleValue(HKUnit.FromString(unit)));
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
	/// Asynchronously writes a count-based health value to the HealthKit store.
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
	public async Task<bool> WriteAsync(HealthParameter healthParameter, DateTime? date, double valueToStore)
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

			HKQuantityType quantityType = HKQuantityType.Create(requestedHealthParameter) ?? throw new HealthException($"{requestedHealthParameter} not available");
			HKQuantity quantity = HKQuantity.FromQuantity(HKUnit.Count, valueToStore);

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
		catch(Exception ex)
		{
			throw new HealthException(ex.Message, ex);
		}
		finally
		{
			semaphore.Release();
		}
	}
}