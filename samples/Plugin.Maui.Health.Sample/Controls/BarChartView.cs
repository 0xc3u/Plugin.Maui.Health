using System.Collections;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.Health.Sample.Controls;

/// <summary>A rounded bar chart drawn with MAUI.Graphics.</summary>
public class BarChartView : GraphicsView, IDrawable
{
	public BarChartView()
	{
		Drawable = this;
		HeightRequest = 170;
	}

	public static readonly BindableProperty EntriesProperty = BindableProperty.Create(
		nameof(Entries), typeof(IList), typeof(BarChartView), null,
		propertyChanged: (b, _, __) => ((BarChartView)b).Invalidate());

	public IList? Entries
	{
		get => (IList?)GetValue(EntriesProperty);
		set => SetValue(EntriesProperty, value);
	}

	public static readonly BindableProperty BarColorProperty = BindableProperty.Create(
		nameof(BarColor), typeof(Color), typeof(BarChartView), Color.FromArgb("#28C2D1"),
		propertyChanged: (b, _, __) => ((BarChartView)b).Invalidate());

	public Color BarColor
	{
		get => (Color)GetValue(BarColorProperty);
		set => SetValue(BarColorProperty, value);
	}

	/// <summary>Compact mode: tight padding, no axis labels — for small dashboard tiles.</summary>
	public static readonly BindableProperty CompactProperty = BindableProperty.Create(
		nameof(Compact), typeof(bool), typeof(BarChartView), false,
		propertyChanged: (b, _, __) => ((BarChartView)b).Invalidate());

	public bool Compact
	{
		get => (bool)GetValue(CompactProperty);
		set => SetValue(CompactProperty, value);
	}

	public void Draw(ICanvas canvas, RectF rect)
	{
		canvas.Antialias = true;

		var points = ChartData.ToList(Entries);
		if (points.Count == 0)
		{
			ChartData.DrawEmpty(canvas, rect);
			return;
		}

		float pad = Compact ? 4f : 14f;
		float labelSpace = Compact ? 0f : 16f;
		float left = rect.Left + pad, right = rect.Right - pad;
		float top = rect.Top + pad, bottom = rect.Bottom - pad - labelSpace;
		float w = right - left, h = bottom - top;

		double max = points.Max(p => p.Value);
		if (max <= 0) max = 1;
		int n = points.Count;

		float slot = w / n;
		float barW = Math.Min(slot * 0.55f, 38f);
		float corner = barW / 2.5f;

		canvas.FontColor = Color.FromArgb("#9AA0AE");
		canvas.FontSize = 11f;

		for (int i = 0; i < n; i++)
		{
			float cx = left + slot * i + slot / 2f;
			float barH = (float)(points[i].Value / max) * h;
			if (barH < 2 && points[i].Value > 0) barH = 2;
			float x = cx - barW / 2f;
			float y = bottom - barH;

			// Track behind the bar for a soft look.
			canvas.FillColor = BarColor.WithAlpha(0.12f);
			canvas.FillRoundedRectangle(x, top, barW, h, corner);

			canvas.FillColor = BarColor;
			canvas.FillRoundedRectangle(x, y, barW, barH, corner);

			if (!Compact && !string.IsNullOrEmpty(points[i].Label))
				canvas.DrawString(points[i].Label, cx - slot / 2f, bottom + 2, slot, 14,
					HorizontalAlignment.Center, VerticalAlignment.Center);
		}
	}
}
