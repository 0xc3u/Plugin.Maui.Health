namespace Plugin.Maui.Health.Models;

/// <summary>
/// Represents a health-related sample, containing information such as time range, value, source, and unit.
/// </summary>
public sealed record Sample
{
	/// <summary>
	/// Gets the starting date and time of the sample.
	/// </summary>
	/// <value>The start date and time.</value>
	public DateTime? From { get; }

	/// <summary>
	/// Gets the ending date and time of the sample.
	/// </summary>
	/// <value>The end date and time.</value>
	public DateTime? Until { get; }

	/// <summary>
	/// Gets the value of the sample.
	/// </summary>
	/// <value>The sample value.</value>
	public double? Value { get; }

	/// <summary>
	/// Gets the source that generated the sample.
	/// </summary>
	/// <value>The source of the sample.</value>
	public string Source { get; }

	/// <summary>
	/// Gets the unit of the sample's value.
	/// </summary>
	/// <value>The unit of the sample.</value>
	public string Unit { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Sample"/> record.
	/// </summary>
	/// <param name="from">The starting date and time of the sample.</param>
	/// <param name="until">The ending date and time of the sample.</param>
	/// <param name="value">The value of the sample.</param>
	/// <param name="source">The source that generated the sample.</param>
	/// <param name="unit">The unit of the sample's value.</param>
	public Sample(DateTime? from, DateTime? until, double? value, string source, string unit)
	{
		From = from;
		Until = until;
		Value = value;
		Source = source;
		Unit = unit;
	}
}
