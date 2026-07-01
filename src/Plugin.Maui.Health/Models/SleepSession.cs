using Plugin.Maui.Health.Enums;

namespace Plugin.Maui.Health.Models;

/// <summary>A single stage segment within a <see cref="SleepSession"/>.</summary>
public sealed record SleepStageSample
{
	public DateTimeOffset From { get; init; }
	public DateTimeOffset Until { get; init; }
	public SleepStage Stage { get; init; }
}

/// <summary>
/// A sleep session (a night's sleep) with its stage breakdown. Maps to a Health Connect
/// <c>SleepSessionRecord</c> on Android and a grouped set of <c>HKCategoryType.SleepAnalysis</c>
/// samples on iOS.
/// </summary>
public sealed record SleepSession
{
	public DateTimeOffset From { get; init; }
	public DateTimeOffset Until { get; init; }

	/// <summary>Total span of the session (<see cref="Until"/> − <see cref="From"/>).</summary>
	public double DurationInSeconds { get; init; }

	public string? Source { get; init; }

	public IReadOnlyList<SleepStageSample> Stages { get; init; } = [];
}
