namespace Plugin.Maui.Health.Models;

/// <summary>
/// A mindfulness / meditation session. Maps to <c>HKCategoryType.MindfulSession</c> on iOS.
/// </summary>
public sealed record MindfulnessSession
{
	public DateTimeOffset From { get; init; }
	public DateTimeOffset Until { get; init; }

	/// <summary>Total span of the session (<see cref="Until"/> − <see cref="From"/>).</summary>
	public double DurationInSeconds { get; init; }

	public string? Source { get; init; }
}
