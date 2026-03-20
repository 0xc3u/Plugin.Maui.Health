namespace Plugin.Maui.Health.Models;

public sealed record RoutePoint
{
	public DateTimeOffset Time { get; init; }
	public double Latitude { get; init; }
	public double Longitude { get; init; }
	public double? AltitudeInMeters { get; init; }
	public double? HorizontalAccuracyInMeters { get; init; }
	public double? VerticalAccuracyInMeters { get; init; }
}
