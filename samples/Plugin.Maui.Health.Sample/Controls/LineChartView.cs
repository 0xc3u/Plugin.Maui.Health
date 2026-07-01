using System.Collections;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.Health.Sample.Controls;

/// <summary>
/// A lightweight line/area chart drawn with MAUI.Graphics. Bind <see cref="Entries"/> to a
/// collection of <see cref="ChartEntry"/>; assign a new collection instance to trigger a redraw.
/// </summary>
public class LineChartView : GraphicsView, IDrawable
{
	public LineChartView()
	{
		Drawable = this;
		HeightRequest = 170;
	}

	public static readonly BindableProperty EntriesProperty = BindableProperty.Create(
		nameof(Entries), typeof(IList), typeof(LineChartView), null,
		propertyChanged: (b, _, __) => ((LineChartView)b).Invalidate());

	public IList? Entries
	{
		get => (IList?)GetValue(EntriesProperty);
		set => SetValue(EntriesProperty, value);
	}

	public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
		nameof(LineColor), typeof(Color), typeof(LineChartView), Color.FromArgb("#3E8EED"),
		propertyChanged: (b, _, __) => ((LineChartView)b).Invalidate());

	public Color LineColor
	{
		get => (Color)GetValue(LineColorProperty);
		set => SetValue(LineColorProperty, value);
	}

	public static readonly BindableProperty ShowDotsProperty = BindableProperty.Create(
		nameof(ShowDots), typeof(bool), typeof(LineChartView), true,
		propertyChanged: (b, _, __) => ((LineChartView)b).Invalidate());

	public bool ShowDots
	{
		get => (bool)GetValue(ShowDotsProperty);
		set => SetValue(ShowDotsProperty, value);
	}

	/// <summary>Sparkline mode: tight padding, no axis labels — for small dashboard tiles.</summary>
	public static readonly BindableProperty CompactProperty = BindableProperty.Create(
		nameof(Compact), typeof(bool), typeof(LineChartView), false,
		propertyChanged: (b, _, __) => ((LineChartView)b).Invalidate());

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
		float labelSpace = Compact ? 0f : 16f; // room for first/last labels
		float left = rect.Left + pad, right = rect.Right - pad;
		float top = rect.Top + pad, bottom = rect.Bottom - pad - labelSpace;
		float w = right - left, h = bottom - top;

		double min = points.Min(p => p.Value);
		double max = points.Max(p => p.Value);
		if (Math.Abs(max - min) < 1e-9) { min -= 1; max += 1; }
		double range = max - min;
		int n = points.Count;

		PointF P(int i)
		{
			float x = n == 1 ? left + w / 2f : left + i / (float)(n - 1) * w;
			float y = bottom - (float)((points[i].Value - min) / range) * h;
			return new PointF(x, y);
		}

		// Filled area under the line.
		var area = new PathF();
		area.MoveTo(P(0).X, bottom);
		for (int i = 0; i < n; i++) area.LineTo(P(i).X, P(i).Y);
		area.LineTo(P(n - 1).X, bottom);
		area.Close();
		canvas.FillColor = LineColor.WithAlpha(0.14f);
		canvas.FillPath(area);

		// The line itself.
		canvas.StrokeColor = LineColor;
		canvas.StrokeSize = 3f;
		canvas.StrokeLineJoin = LineJoin.Round;
		canvas.StrokeLineCap = LineCap.Round;
		var line = new PathF();
		line.MoveTo(P(0).X, P(0).Y);
		for (int i = 1; i < n; i++) line.LineTo(P(i).X, P(i).Y);
		canvas.DrawPath(line);

		// Dots.
		if (ShowDots)
		{
			for (int i = 0; i < n; i++)
			{
				var p = P(i);
				canvas.FillColor = Colors.White;
				canvas.FillCircle(p.X, p.Y, 4.5f);
				canvas.StrokeColor = LineColor;
				canvas.StrokeSize = 2.5f;
				canvas.DrawCircle(p.X, p.Y, 4.5f);
			}
		}

		// First / last labels (omitted in compact/sparkline mode).
		if (Compact)
			return;

		canvas.FontColor = Color.FromArgb("#9AA0AE");
		canvas.FontSize = 11f;
		if (!string.IsNullOrEmpty(points[0].Label))
			canvas.DrawString(points[0].Label, left, bottom + 2, 60, 14, HorizontalAlignment.Left, VerticalAlignment.Center);
		if (n > 1 && !string.IsNullOrEmpty(points[n - 1].Label))
			canvas.DrawString(points[n - 1].Label, right - 60, bottom + 2, 60, 14, HorizontalAlignment.Right, VerticalAlignment.Center);
	}
}
