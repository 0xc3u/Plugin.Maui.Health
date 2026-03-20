using Plugin.Maui.Health.Enums;

namespace Plugin.Maui.Health.Models;

public sealed record WorkoutSession
{
	public WorkoutType WorkoutType { get; init; }
	public DateTimeOffset From { get; init; }
	public DateTimeOffset Until { get; init; }
	public double DurationInSeconds { get; init; }
	public double? EnergyBurnedInCalories { get; init; }
	public double? TotalDistanceInMeters { get; init; }
	/// <summary>On iOS, always null — HealthKit workouts have no title field.</summary>
	public string? Title { get; init; }
	public string? Source { get; init; }
	public IReadOnlyList<RoutePoint> Route { get; init; } = [];
}
