using System.Collections;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Health.Models;

namespace Plugin.Maui.Health.Sample.Controls;

/// <summary>
/// Draws a GPS route (a list of <see cref="RoutePoint"/>) as an auto-scaled polyline with start/end
/// markers, using MAUI.Graphics. Latitude/longitude are fit to the canvas with a uniform scale so the
/// track keeps its shape. Assign a new collection instance to trigger a redraw.
/// </summary>
public class RouteMapView : GraphicsView, IDrawable
{
	public RouteMapView()
	{
		Drawable = this;
		HeightRequest = 220;
	}

	public static readonly BindableProperty RouteProperty = BindableProperty.Create(
		nameof(Route), typeof(IList), typeof(RouteMapView), null,
		propertyChanged: (b, _, __) => ((RouteMapView)b).Invalidate());

	public IList? Route
	{
		get => (IList?)GetValue(RouteProperty);
		set => SetValue(RouteProperty, value);
	}

	public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
		nameof(LineColor), typeof(Color), typeof(RouteMapView), Color.FromArgb("#3E8EED"),
		propertyChanged: (b, _, __) => ((RouteMapView)b).Invalidate());

	public Color LineColor
	{
		get => (Color)GetValue(LineColorProperty);
		set => SetValue(LineColorProperty, value);
	}

	public void Draw(ICanvas canvas, RectF rect)
	{
		canvas.Antialias = true;

		// Soft rounded backdrop so it reads as a "map" tile.
		canvas.FillColor = Color.FromArgb("#F1F4FA");
		canvas.FillRoundedRectangle(rect, 14f);

		var points = new List<RoutePoint>();
		if (Route is not null)
			foreach (var item in Route)
				if (item is RoutePoint p)
					points.Add(p);

		if (points.Count < 2)
		{
			canvas.FontColor = Color.FromArgb("#B5BAC6");
			canvas.FontSize = 13f;
			canvas.DrawString("No route", rect.Left, rect.Top, rect.Width, rect.Height,
				HorizontalAlignment.Center, VerticalAlignment.Center);
			return;
		}

		double minLat = points.Min(p => p.Latitude), maxLat = points.Max(p => p.Latitude);
		double minLon = points.Min(p => p.Longitude), maxLon = points.Max(p => p.Longitude);
		double latRange = Math.Max(maxLat - minLat, 1e-9);
		double lonRange = Math.Max(maxLon - minLon, 1e-9);

		const float pad = 18f;
		float w = rect.Width - 2 * pad, h = rect.Height - 2 * pad;
		double scale = Math.Min(w / lonRange, h / latRange);   // uniform → no distortion
		float drawnW = (float)(lonRange * scale), drawnH = (float)(latRange * scale);
		float ox = rect.Left + pad + (w - drawnW) / 2f;
		float oy = rect.Top + pad + (h - drawnH) / 2f;

		PointF Map(RoutePoint p) => new(
			ox + (float)((p.Longitude - minLon) * scale),
			oy + (float)((maxLat - p.Latitude) * scale));  // invert latitude (north up)

		var path = new PathF();
		var first = Map(points[0]);
		path.MoveTo(first.X, first.Y);
		for (int i = 1; i < points.Count; i++)
		{
			var pt = Map(points[i]);
			path.LineTo(pt.X, pt.Y);
		}

		canvas.StrokeColor = LineColor;
		canvas.StrokeSize = 4f;
		canvas.StrokeLineJoin = LineJoin.Round;
		canvas.StrokeLineCap = LineCap.Round;
		canvas.DrawPath(path);

		DrawMarker(canvas, Map(points[0]), Color.FromArgb("#34C759"));                 // start (green)
		DrawMarker(canvas, Map(points[^1]), Color.FromArgb("#FF3B30"));                // end (red)
	}

	static void DrawMarker(ICanvas canvas, PointF center, Color color)
	{
		canvas.FillColor = Colors.White;
		canvas.FillCircle(center.X, center.Y, 7f);
		canvas.FillColor = color;
		canvas.FillCircle(center.X, center.Y, 5f);
	}
}
