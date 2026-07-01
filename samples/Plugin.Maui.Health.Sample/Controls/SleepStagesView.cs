using System.Collections;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Models;

namespace Plugin.Maui.Health.Sample.Controls;

/// <summary>
/// Draws a sleep session as a horizontal, time-proportional colour band of its stages, using
/// MAUI.Graphics. Bind <see cref="Stages"/> to a collection of <see cref="SleepStageSample"/>.
/// </summary>
public class SleepStagesView : GraphicsView, IDrawable
{
	public SleepStagesView()
	{
		Drawable = this;
		HeightRequest = 76;
	}

	public static readonly BindableProperty StagesProperty = BindableProperty.Create(
		nameof(Stages), typeof(IList), typeof(SleepStagesView), null,
		propertyChanged: (b, _, __) => ((SleepStagesView)b).Invalidate());

	public IList? Stages
	{
		get => (IList?)GetValue(StagesProperty);
		set => SetValue(StagesProperty, value);
	}

	public void Draw(ICanvas canvas, RectF rect)
	{
		canvas.Antialias = true;

		var stages = new List<SleepStageSample>();
		if (Stages is not null)
			foreach (var item in Stages)
				if (item is SleepStageSample s)
					stages.Add(s);

		if (stages.Count == 0)
		{
			canvas.FontColor = Color.FromArgb("#B5BAC6");
			canvas.FontSize = 13f;
			canvas.DrawString("No sleep data", rect.Left, rect.Top, rect.Width, rect.Height,
				HorizontalAlignment.Center, VerticalAlignment.Center);
			return;
		}

		var start = stages.Min(s => s.From);
		var end = stages.Max(s => s.Until);
		double total = (end - start).TotalSeconds;
		if (total <= 0)
			return;

		const float pad = 8f;
		float x0 = rect.Left + pad, w = rect.Width - 2 * pad;
		float top = rect.Top + pad, h = rect.Height - 2 * pad - 16f; // room for time labels

		foreach (var s in stages)
		{
			float sx = x0 + (float)((s.From - start).TotalSeconds / total) * w;
			float sw = Math.Max(1.5f, (float)((s.Until - s.From).TotalSeconds / total) * w);
			canvas.FillColor = StageColor(s.Stage);
			canvas.FillRectangle(sx, top, sw, h);
		}

		canvas.FontColor = Color.FromArgb("#9AA0AE");
		canvas.FontSize = 11f;
		canvas.DrawString(start.ToLocalTime().ToString("HH:mm"), x0, top + h + 2, 60, 14, HorizontalAlignment.Left, VerticalAlignment.Center);
		canvas.DrawString(end.ToLocalTime().ToString("HH:mm"), x0 + w - 60, top + h + 2, 60, 14, HorizontalAlignment.Right, VerticalAlignment.Center);
	}

	internal static Color StageColor(SleepStage stage) => stage switch
	{
		SleepStage.Awake => Color.FromArgb("#F7B548"),
		SleepStage.Rem => Color.FromArgb("#28C2D1"),
		SleepStage.Light => Color.FromArgb("#3E8EED"),
		SleepStage.Deep => Color.FromArgb("#7C5CFC"),
		SleepStage.Sleeping => Color.FromArgb("#34C759"),
		SleepStage.InBed => Color.FromArgb("#C8C8C8"),
		_ => Color.FromArgb("#E1E1E1"),
	};
}
