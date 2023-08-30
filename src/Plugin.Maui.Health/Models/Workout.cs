using Plugin.Maui.Health.Enums;

namespace Plugin.Maui.Health.Models;

public sealed record Workout
{
	public WorkoutType WorkoutType { get; }
	public string Source { get; }
	public DateTime? From { get; }
	public DateTime? Until { get; }
	public double DurationInSeconds { get; }
	public double? EnergyBurnedInCalorie { get; }
	public double? TotalDistanceInMeter { get; }

	public Workout(WorkoutType workoutType, DateTime? from, DateTime? until, double durationInSeconds,
		double? energyBurnedInCalorie, double? totalDistanceInMeter, string source)
	{
		WorkoutType = workoutType;
		Source = source;
		From = from;
		Until = until;
		DurationInSeconds = durationInSeconds;
		EnergyBurnedInCalorie = energyBurnedInCalorie;
		TotalDistanceInMeter = totalDistanceInMeter;
	}
}
