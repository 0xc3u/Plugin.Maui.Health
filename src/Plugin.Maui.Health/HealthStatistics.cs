using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Models;

namespace Plugin.Maui.Health;

// Cross-platform (compiled for every target): bucketed aggregation built on the per-platform
// ReadAllAsync. Buckets in memory so it works uniformly for every parameter; for very large ranges
// prefer a narrower window. (Native aggregation — HKStatisticsCollectionQuery / aggregateGroupByPeriod
// — is a possible future optimisation for cumulative types.)
partial class HealthDataProviderImplementation
{
	public async Task<List<StatisticsBucket>> ReadStatisticsAsync(HealthParameter healthParameter,
		DateTimeOffset from, DateTimeOffset until, StatisticsInterval interval, string unit,
		CancellationToken cancellationToken = default)
	{
		var samples = await ReadAllAsync(healthParameter, from, until, unit, cancellationToken).ConfigureAwait(false);
		var cumulative = IsCumulative(healthParameter);

		// Only samples with a timestamp can be bucketed; sort once so the per-bucket scan is cheap.
		var points = samples
			.Where(s => s.From.HasValue && s.Value.HasValue)
			.Select(s => (Time: s.From!.Value, Value: s.Value!.Value))
			.OrderBy(p => p.Time)
			.ToList();

		var buckets = new List<StatisticsBucket>();
		int index = 0;
		for (var cursor = BucketStart(from, interval); cursor < until; cursor = BucketEnd(cursor, interval))
		{
			var next = BucketEnd(cursor, interval);

			double sum = 0;
			int count = 0;
			while (index < points.Count && points[index].Time < next)
			{
				if (points[index].Time >= cursor)
				{
					sum += points[index].Value;
					count++;
				}
				index++;
			}

			// Buckets are contiguous and points are sorted, so the shared index only moves forward.
			buckets.Add(new StatisticsBucket
			{
				From = cursor,
				Until = next < until ? next : until,
				Value = count == 0 ? (cumulative ? 0d : (double?)null) : (cumulative ? sum : sum / count),
				Count = count,
			});
		}

		return buckets;
	}

	static DateTimeOffset BucketStart(DateTimeOffset t, StatisticsInterval interval)
	{
		var lt = t.ToLocalTime();
		var d = lt.DateTime;
		return interval switch
		{
			StatisticsInterval.Hourly => new DateTimeOffset(d.Year, d.Month, d.Day, d.Hour, 0, 0, lt.Offset),
			StatisticsInterval.Monthly => new DateTimeOffset(d.Year, d.Month, 1, 0, 0, 0, lt.Offset),
			// Daily and Weekly both start at midnight of the range's first day; Weekly then steps 7 days.
			_ => new DateTimeOffset(d.Year, d.Month, d.Day, 0, 0, 0, lt.Offset),
		};
	}

	static DateTimeOffset BucketEnd(DateTimeOffset start, StatisticsInterval interval) => interval switch
	{
		StatisticsInterval.Hourly => start.AddHours(1),
		StatisticsInterval.Weekly => start.AddDays(7),
		StatisticsInterval.Monthly => start.AddMonths(1),
		_ => start.AddDays(1),
	};

	// Cumulative parameters are summed over a bucket; everything else is averaged. Nutrition (every
	// Dietary* value) and hydration are cumulative, as are counts/distances/energies/durations.
	static bool IsCumulative(HealthParameter p) => p switch
	{
		HealthParameter.StepCount
			or HealthParameter.DistanceWalkingRunning or HealthParameter.DistanceCycling
			or HealthParameter.DistanceSwimming or HealthParameter.DistanceWheelchair
			or HealthParameter.DistanceDownhillSnowSports
			or HealthParameter.ActiveEnergyBurned or HealthParameter.BasalEnergyBurned
			or HealthParameter.TotalEnergyBurned
			or HealthParameter.FlightsClimbed or HealthParameter.ElevationGained
			or HealthParameter.PushCount or HealthParameter.SwimmingStrokeCount
			or HealthParameter.ExerciseTime or HealthParameter.StandTime or HealthParameter.MoveTime
			or HealthParameter.NikeFuel or HealthParameter.NumberOfTimesFallen
			or HealthParameter.NumberOfAlcoholicBeverages or HealthParameter.InhalerUsage => true,
		_ => p.ToString().StartsWith("Dietary", StringComparison.Ordinal),
	};
}
