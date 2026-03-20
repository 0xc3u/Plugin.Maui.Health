namespace Plugin.Maui.Health.Models;

/// <summary>
/// Represents a health-related sample, containing information such as time range, value, source, and unit.
/// </summary>
public sealed record Sample
{
	/// <summary>
	/// Gets the starting date and time of the sample.
	/// </summary>
	public DateTimeOffset? From { get; init; }

	/// <summary>
	/// Gets the ending date and time of the sample.
	/// </summary>
	public DateTimeOffset? Until { get; init; }

	/// <summary>
	/// Gets the value of the sample.
	/// </summary>
	public double? Value { get; init; }

	/// <summary>
	/// Gets the source that generated the sample.
	/// </summary>
	public string Source { get; init; } = string.Empty;

	/// <summary>
	/// Gets a human-readable description. Defaults to "{value} {unit}" when not supplied.
	/// </summary>
	public string Description { get; init; } = string.Empty;

	/// <summary>
	/// Gets the unit of the sample's value.
	/// </summary>
	public string Unit { get; init; } = string.Empty;

	/// <summary>
	/// Initializes a new instance of the <see cref="Sample"/> record.
	/// </summary>
	/// <param name="from">The starting date and time of the sample.</param>
	/// <param name="until">The ending date and time of the sample.</param>
	/// <param name="value">The value of the sample.</param>
	/// <param name="source">The source that generated the sample.</param>
	/// <param name="unit">The unit of the sample's value.</param>
	/// <param name="description">Optional description; defaults to "{value} {unit}".</param>
	public Sample(DateTimeOffset? from, DateTimeOffset? until, double? value, string source, string unit, string description = null)
	{
		From = from;
		Until = until;
		Value = value;
		Source = source;
		Unit = unit;
		Description = string.IsNullOrEmpty(description)
			? value.HasValue ? $"{value.Value:G} {unit}" : unit
			: description;
	}
}
