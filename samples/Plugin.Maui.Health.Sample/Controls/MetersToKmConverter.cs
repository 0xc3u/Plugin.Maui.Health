using System.Globalization;

namespace Plugin.Maui.Health.Sample.Controls;

/// <summary>Formats a distance in metres as "x.x km", or an empty string when absent/zero.</summary>
public sealed class MetersToKmConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is double meters && meters > 0 ? $"{meters / 1000d:F1} km" : string.Empty;

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
