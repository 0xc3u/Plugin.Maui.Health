using Microsoft.Maui.Graphics;

namespace Plugin.Maui.Health.Sample.Controls;

/// <summary>A circular progress / activity ring drawn with MAUI.Graphics.</summary>
public class RingChartView : GraphicsView, IDrawable
{
	public RingChartView()
	{
		Drawable = this;
		HeightRequest = 150;
		WidthRequest = 150;
	}

	/// <summary>Progress from 0 to 1.</summary>
	public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
		nameof(Progress), typeof(double), typeof(RingChartView), 0d,
		propertyChanged: (b, _, __) => ((RingChartView)b).Invalidate());

	public double Progress
	{
		get => (double)GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	public static readonly BindableProperty RingColorProperty = BindableProperty.Create(
		nameof(RingColor), typeof(Color), typeof(RingChartView), Color.FromArgb("#34C759"),
		propertyChanged: (b, _, __) => ((RingChartView)b).Invalidate());

	public Color RingColor
	{
		get => (Color)GetValue(RingColorProperty);
		set => SetValue(RingColorProperty, value);
	}

	public static readonly BindableProperty ValueTextProperty = BindableProperty.Create(
		nameof(ValueText), typeof(string), typeof(RingChartView), string.Empty,
		propertyChanged: (b, _, __) => ((RingChartView)b).Invalidate());

	public string ValueText
	{
		get => (string)GetValue(ValueTextProperty);
		set => SetValue(ValueTextProperty, value);
	}

	public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
		nameof(Caption), typeof(string), typeof(RingChartView), string.Empty,
		propertyChanged: (b, _, __) => ((RingChartView)b).Invalidate());

	public string Caption
	{
		get => (string)GetValue(CaptionProperty);
		set => SetValue(CaptionProperty, value);
	}

	public void Draw(ICanvas canvas, RectF rect)
	{
		canvas.Antialias = true;

		float size = Math.Min(rect.Width, rect.Height);
		float thickness = Math.Max(8f, size * 0.12f);
		float inset = thickness / 2f + 2f;
		float d = size - inset * 2f;
		float x = rect.Left + (rect.Width - size) / 2f + inset;
		float y = rect.Top + (rect.Height - size) / 2f + inset;

		double p = Math.Clamp(Progress, 0d, 1d);

		canvas.StrokeSize = thickness;
		canvas.StrokeLineCap = LineCap.Round;

		// Track.
		canvas.StrokeColor = RingColor.WithAlpha(0.15f);
		canvas.DrawArc(x, y, d, d, 0, 360, true, false);

		// Progress (starts at 12 o'clock, sweeps clockwise).
		if (p > 0)
		{
			float start = 90f;
			float end = 90f - 360f * (float)p;
			canvas.StrokeColor = RingColor;
			canvas.DrawArc(x, y, d, d, start, end, true, false);
		}

		// Centre value.
		float cx = x, cy = y, cw = d, ch = d;
		if (!string.IsNullOrEmpty(ValueText))
		{
			canvas.FontColor = Color.FromArgb("#1B1D28");
			canvas.FontSize = size * 0.20f;
			canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
			float offset = string.IsNullOrEmpty(Caption) ? 0 : -size * 0.06f;
			canvas.DrawString(ValueText, cx, cy + offset, cw, ch,
				HorizontalAlignment.Center, VerticalAlignment.Center);
		}

		if (!string.IsNullOrEmpty(Caption))
		{
			canvas.FontColor = Color.FromArgb("#6B7280");
			canvas.FontSize = size * 0.095f;
			canvas.Font = Microsoft.Maui.Graphics.Font.Default;
			canvas.DrawString(Caption, cx, cy + size * 0.13f, cw, ch,
				HorizontalAlignment.Center, VerticalAlignment.Center);
		}
	}
}
