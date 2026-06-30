namespace Plugin.Maui.Health.Sample.Controls;

/// <summary>A single data point for the chart controls.</summary>
public class ChartEntry
{
	public ChartEntry() { }

	public ChartEntry(double value, string? label = null)
	{
		Value = value;
		Label = label;
	}

	public double Value { get; set; }

	public string? Label { get; set; }
}
