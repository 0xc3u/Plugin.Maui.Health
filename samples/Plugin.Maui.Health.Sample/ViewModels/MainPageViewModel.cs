using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Plugin.Maui.Health.Constants;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Models;
using Plugin.Maui.Health.Sample.Controls;
using Plugin.Maui.Health.Sample.Services;
using Plugin.Maui.Health.Sample.Views;

namespace Plugin.Maui.Health.Sample.ViewModels;

/// <summary>
/// The dashboard: a "Today" activity hero plus an "At a glance" grid of live tiles. Every tile reads real
/// data through the plugin and taps through to its detail screen. Tiles fall back to representative demo
/// values until the matching data has been seeded/granted (steps are seeded here; heart rate, weight and
/// vitamins are covered by the batched core-permission request, while workouts and sleep become live once
/// their own consent is granted — e.g. by opening those screens or the Permissions screen — after which
/// the dashboard re-reads them on return).
/// </summary>
public partial class MainPageViewModel : BaseViewModel
{
	readonly IHealth health;

	public MainPageViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
		this.health = health;
	}

	// ── Today (activity) ──────────────────────────────────────────────────────
	[ObservableProperty] ObservableCollection<ChartEntry> stepsThisWeek = new();
	[ObservableProperty] double stepsGoalProgress;
	[ObservableProperty] string stepsGoalText = "0%";
	[ObservableProperty] string stepsTodayText = "0";
	[ObservableProperty] string exerciseText = "—";

	// ── Tiles ─────────────────────────────────────────────────────────────────
	[ObservableProperty] ObservableCollection<ChartEntry> heartRateTrend = new();
	[ObservableProperty] string heartRateText = "—";

	[ObservableProperty] ObservableCollection<SleepStageSample> sleepStages = new();
	[ObservableProperty] string sleepText = "—";

	[ObservableProperty] ObservableCollection<ChartEntry> weightTrend = new();
	[ObservableProperty] string weightText = "—";
	[ObservableProperty] string weightTrendText = "";

	[ObservableProperty] string workoutText = "—";
	[ObservableProperty] string workoutSubText = "";

	[ObservableProperty] ObservableCollection<ChartEntry> vitaminLevels = new();
	[ObservableProperty] string vitaminsText = "—";

	[ObservableProperty] string permissionsText = "—";

	// ── Navigation ────────────────────────────────────────────────────────────
	[RelayCommand]
	Task Open(string route) => Navigationservice.NavigateToViewAsync(route);

	// ── Lifecycle ─────────────────────────────────────────────────────────────
	bool initialized;

	public override void OnAppearing(object param)
	{
		if (!initialized)
		{
			ShowDemoData();          // instant placeholders so the page never flashes empty
			_ = InitializeAsync();   // request consent, seed steps, then load real values
		}
		else
		{
			// Coming back from a detail/Permissions screen: pick up anything newly seeded/granted.
			_ = RefreshTilesAsync();
		}
	}

	async Task InitializeAsync()
	{
		if (initialized)
			return;
		initialized = true;

		await RunInitializeAsync(async () =>
		{
			await SeedStepsOnceAsync();
			await LoadAllTilesAsync();
		}, timeoutSeconds: 25);
	}

	async Task RefreshTilesAsync()
	{
		if (!Health.IsSupported || IsBusy)
			return;

		await LoadAllTilesAsync();
	}

	async Task LoadAllTilesAsync()
	{
		await Safe(LoadStepsAsync);
		await Safe(LoadHeartRateTileAsync);
		await Safe(LoadSleepTileAsync);
		await Safe(LoadWeightTileAsync);
		await Safe(LoadWorkoutTileAsync);
		await Safe(LoadVitaminsTileAsync);
		await Safe(LoadPermissionsTileAsync);
	}

	// A read that fails (unsupported parameter, permission not yet granted) just leaves the tile's
	// current value in place — one tile can't take down the whole dashboard.
	static async Task Safe(Func<Task> load)
	{
		try { await load(); }
		catch (HealthException) { /* keep the existing (demo or previous) value */ }
	}

	// ── Steps ─────────────────────────────────────────────────────────────────
	const string SeededStepsKey = "seeded.steps";
	static readonly int[] DemoSteps = { 6400, 9100, 7300, 11200, 8400, 5200, 8432 };

	async Task SeedStepsOnceAsync()
	{
		// Key by day so "today" always gets seeded (and the dashboard never shows an empty day after
		// the clock rolls over), while avoiding re-seeding repeatedly within the same day.
		var key = $"{SeededStepsKey}.{DateTime.Now:yyyyMMdd}";
		if (Preferences.Get(key, false))
			return;

		for (int i = 0; i < DemoSteps.Length; i++)
		{
			var day = new DateTimeOffset(DateTime.Now.Date.AddDays(i - (DemoSteps.Length - 1)).AddHours(9));
			await Health.WriteAsync(HealthParameter.StepCount, day, DemoSteps[i], Units.Others.Count);
		}

		Preferences.Set(key, true);
	}

	async Task LoadStepsAsync()
	{
		var weekStart = new DateTimeOffset(DateTime.Now.Date.AddDays(-6));
		var samples = await Health.ReadAllAsync(HealthParameter.StepCount, weekStart, DateTimeOffset.Now.AddDays(1), Units.Others.Count);

		var bars = new List<ChartEntry>();
		double today = 0;
		for (int i = 6; i >= 0; i--)
		{
			var day = DateTime.Now.Date.AddDays(-i);
			var total = samples.Where(s => s.From?.LocalDateTime.Date == day).Sum(s => s.Value ?? 0);
			bars.Add(new ChartEntry(total, new DateTimeOffset(day).ToString("ddd")));
			if (i == 0)
				today = total;
		}

		StepsThisWeek = new ObservableCollection<ChartEntry>(bars);
		UpdateStepsHeader(today);
	}

	void UpdateStepsHeader(double today)
	{
		const double goal = 10000;
		StepsTodayText = today.ToString("#,0");
		StepsGoalProgress = Math.Clamp(today / goal, 0d, 1d);
		StepsGoalText = $"{StepsGoalProgress * 100:F0}%";
	}

	// ── Heart rate tile ───────────────────────────────────────────────────────
	async Task LoadHeartRateTileAsync()
	{
		var samples = await Health.ReadAllAsync(HealthParameter.HeartRate,
			DateTimeOffset.Now.AddHours(-12), DateTimeOffset.Now, Units.Others.CountPerMinute);

		var points = samples.Where(s => s.Value.HasValue).ToList();
		if (points.Count == 0)
			return;

		HeartRateTrend = new ObservableCollection<ChartEntry>(points.Select(s => new ChartEntry(s.Value!.Value)));
		HeartRateText = $"{points[^1].Value:F0} bpm";
	}

	// ── Sleep tile ────────────────────────────────────────────────────────────
	async Task LoadSleepTileAsync()
	{
		var sessions = await Health.ReadSleepAsync(DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now);
		if (sessions.Count == 0)
			return;

		var latest = sessions.OrderByDescending(s => s.From).First();
		SleepStages = new ObservableCollection<SleepStageSample>(latest.Stages.OrderBy(s => s.From));
		SleepText = FormatAsleep(latest.Stages);
	}

	// ── Weight tile ───────────────────────────────────────────────────────────
	async Task LoadWeightTileAsync()
	{
		var latest = await Health.ReadLatestAvailableAsync(HealthParameter.BodyMass, Units.Mass.Kilograms);
		if (latest?.Value is double w && w > 0)
			WeightText = $"{w:F1} kg";

		var samples = await Health.ReadAllAsync(HealthParameter.BodyMass,
			DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now, Units.Mass.Kilograms);
		if (samples.Count == 0)
			return;

		var ordered = samples.Where(s => s.Value.HasValue).OrderBy(s => s.From).ToList();
		WeightTrend = new ObservableCollection<ChartEntry>(ordered.Select(s => new ChartEntry(s.Value!.Value)));
		SetWeightTrend(ordered.First().Value!.Value, ordered.Last().Value!.Value);
	}

	void SetWeightTrend(double first, double last)
	{
		var delta = last - first;
		WeightTrendText = Math.Abs(delta) < 0.05 ? "steady" : $"{(delta < 0 ? "▼ " : "▲ ")}{Math.Abs(delta):F1} kg";
	}

	// ── Workout tile (also drives the Today "Exercise" line) ──────────────────
	async Task LoadWorkoutTileAsync()
	{
		var sessions = await Health.ReadWorkoutsAsync(WorkoutType.Other,
			DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now);
		if (sessions.Count == 0)
			return;

		ApplyWorkouts(sessions);
	}

	void ApplyWorkouts(IReadOnlyList<WorkoutSession> sessions)
	{
		var latest = sessions.OrderByDescending(w => w.From).First();
		WorkoutText = latest.TotalDistanceInMeters is double d and > 0
			? $"{d / 1000:F1} km · {latest.WorkoutType}"
			: $"{(int)(latest.Until - latest.From).TotalMinutes} min · {latest.WorkoutType}";
		WorkoutSubText = latest.From.ToLocalTime().ToString("ddd, HH:mm");

		var today = DateTime.Now.Date;
		var todays = sessions.Where(w => w.From.LocalDateTime.Date == today).ToList();
		if (todays.Count > 0)
		{
			var minutes = todays.Sum(w => (w.Until - w.From).TotalMinutes);
			var kcal = todays.Sum(w => w.EnergyBurnedInCalories ?? 0);
			ExerciseText = $"Exercise {minutes:F0}m · {kcal:F0} kcal";
		}
	}

	// ── Vitamins tile ─────────────────────────────────────────────────────────
	static readonly (HealthParameter Param, string Label, double Rdi)[] Vitamins =
	{
		(HealthParameter.DietaryVitaminC, "C", 90), (HealthParameter.DietaryVitaminD, "D", 0.02),
		(HealthParameter.DietaryVitaminE, "E", 15), (HealthParameter.DietaryVitaminB6, "B6", 1.7),
		(HealthParameter.DietaryVitaminB12, "B12", 0.0024), (HealthParameter.DietaryVitaminK, "K", 0.12),
	};

	async Task LoadVitaminsTileAsync()
	{
		var entries = new List<ChartEntry>();
		bool any = false;
		foreach (var (param, label, rdi) in Vitamins)
		{
			var mg = await Health.ReadLatestAsync(param, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now, Units.Mass.Milligrams);
			if (mg.HasValue)
				any = true;
			entries.Add(new ChartEntry(rdi > 0 ? Math.Round((mg ?? 0) / rdi * 100) : 0, label));
		}

		if (!any)
			return;

		VitaminLevels = new ObservableCollection<ChartEntry>(entries);
		VitaminsText = $"avg {entries.Average(e => e.Value):F0}% RDI";
	}

	// ── Permissions tile ──────────────────────────────────────────────────────
	async Task LoadPermissionsTileAsync()
	{
		// Batched check — no prompt on Android; on iOS the core types are already determined by the
		// dashboard's up-front request, so this doesn't present a sheet.
		var granted = await Health.CheckPermissionsAsync(CorePermissionRequests);
		PermissionsText = granted ? "All granted" : "Review access";
	}

	// ── Demo fallback ─────────────────────────────────────────────────────────
	void ShowDemoData()
	{
		string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
		StepsThisWeek = new ObservableCollection<ChartEntry>(DemoSteps.Select((v, i) => new ChartEntry(v, days[i])));
		UpdateStepsHeader(DemoSteps[^1]);
		ExerciseText = "Exercise 28m · 395 kcal";

		int[] bpm = { 61, 58, 60, 66, 82, 128, 104, 90, 75, 112, 88, 72, 68 };
		HeartRateTrend = new ObservableCollection<ChartEntry>(bpm.Select(v => new ChartEntry(v)));
		HeartRateText = $"{bpm[^1]} bpm";

		double[] weights = { 74.1, 73.6, 73.8, 73.1, 72.9, 72.4, 72.6, 72.5 };
		WeightTrend = new ObservableCollection<ChartEntry>(weights.Select(v => new ChartEntry(v)));
		WeightText = $"{weights[^1]:F1} kg";
		SetWeightTrend(weights[0], weights[^1]);

		SleepStages = new ObservableCollection<SleepStageSample>(BuildDemoNight());
		SleepText = FormatAsleep(SleepStages);

		WorkoutText = "5.1 km · Running";
		WorkoutSubText = DateTimeOffset.Now.ToString("ddd, HH:mm");

		VitaminLevels = new ObservableCollection<ChartEntry>(new[]
		{
			new ChartEntry(91, "C"), new ChartEntry(75, "D"), new ChartEntry(80, "E"),
			new ChartEntry(94, "B6"), new ChartEntry(100, "B12"), new ChartEntry(92, "K"),
		});
		VitaminsText = "avg 89% RDI";

		PermissionsText = "Review access";
	}

	// ── Helpers ───────────────────────────────────────────────────────────────
	static string FormatAsleep(IReadOnlyList<SleepStageSample> stages)
	{
		double Minutes(params SleepStage[] which) => stages
			.Where(s => which.Contains(s.Stage))
			.Sum(s => (s.Until - s.From).TotalMinutes);

		var asleep = TimeSpan.FromMinutes(Minutes(SleepStage.Light, SleepStage.Deep, SleepStage.Rem, SleepStage.Sleeping));
		return $"{(int)asleep.TotalHours}h {asleep.Minutes:00}m";
	}

	static List<SleepStageSample> BuildDemoNight()
	{
		(SleepStage Stage, int Minutes)[] night =
		{
			(SleepStage.Light, 42), (SleepStage.Deep, 55), (SleepStage.Light, 28), (SleepStage.Rem, 24),
			(SleepStage.Light, 36), (SleepStage.Deep, 40), (SleepStage.Awake, 6), (SleepStage.Light, 30),
			(SleepStage.Rem, 32), (SleepStage.Light, 24), (SleepStage.Rem, 34), (SleepStage.Light, 18),
		};

		var wake = new DateTimeOffset(DateTime.Now.Date.AddHours(7));
		var cursor = wake.AddMinutes(-night.Sum(x => x.Minutes));
		var stages = new List<SleepStageSample>();
		foreach (var (stage, minutes) in night)
		{
			var end = cursor.AddMinutes(minutes);
			stages.Add(new SleepStageSample { From = cursor, Until = end, Stage = stage });
			cursor = end;
		}
		return stages;
	}
}
