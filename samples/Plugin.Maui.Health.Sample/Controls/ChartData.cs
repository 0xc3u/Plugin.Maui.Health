using System.Collections;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.Health.Sample.Controls;

/// <summary>Shared helpers for the chart controls.</summary>
static class ChartData
{
	/// <summary>Normalises a bound <see cref="IList"/> into a list of <see cref="ChartEntry"/>.</summary>
	public static List<ChartEntry> ToList(IList? source)
	{
		var result = new List<ChartEntry>();
		if (source is null)
			return result;

		foreach (var item in source)
		{
			if (item is ChartEntry e)
				result.Add(e);
		}

		return result;
	}

	public static void DrawEmpty(ICanvas canvas, RectF rect)
	{
		canvas.FontColor = Color.FromArgb("#B5BAC6");
		canvas.FontSize = 13f;
		canvas.DrawString("No data yet", rect.Left, rect.Top, rect.Width, rect.Height,
			HorizontalAlignment.Center, VerticalAlignment.Center);
	}
}
