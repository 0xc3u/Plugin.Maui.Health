namespace Plugin.Maui.Health.Models;

/// <summary>
/// One time bucket of aggregated statistics returned by <c>ReadStatisticsAsync</c>. For cumulative
/// parameters (steps, distance, energy, nutrition…) <see cref="Value"/> is the sum over the bucket;
/// for discrete parameters (heart rate, weight, temperature…) it is the average, or <c>null</c> when
/// the bucket has no samples.
/// </summary>
public sealed record StatisticsBucket
{
	/// <summary>Start of the bucket (inclusive), in local time.</summary>
	public DateTimeOffset From { get; init; }

	/// <summary>End of the bucket (exclusive), clamped to the requested range for the final bucket.</summary>
	public DateTimeOffset Until { get; init; }

	/// <summary>Sum (cumulative) or average (discrete) of the samples in the bucket; null if empty and discrete.</summary>
	public double? Value { get; init; }

	/// <summary>Number of samples that fell into the bucket.</summary>
	public int Count { get; init; }
}
