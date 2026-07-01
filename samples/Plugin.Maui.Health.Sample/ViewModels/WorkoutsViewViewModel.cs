using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Models;
using Plugin.Maui.Health.Sample.Services;

namespace Plugin.Maui.Health.Sample.ViewModels;

public partial class WorkoutsViewViewModel : BaseViewModel
{
	const string SeededWorkoutsKey = "seeded.workouts.v2";

	bool initialized;

	[ObservableProperty]
	ObservableCollection<WorkoutSession> workouts = new();

	[ObservableProperty]
	ObservableCollection<RoutePoint> selectedRoute = new();

	[ObservableProperty]
	string selectedTitle = "—";

	[ObservableProperty]
	string selectedStats = string.Empty;

	public WorkoutsViewViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
		ShowDemoData();
	}

	public async Task InitializeAsync()
	{
		if (initialized)
			return;
		initialized = true;

		if (!Health.IsSupported)
			return; // keep demo data

		try
		{
			IsBusy = true;

			// Workouts use the exercise (+ route) permission, requested outside the timeout below.
			await Health.RequestWorkoutPermissionAsync(PermissionType.Read | PermissionType.Write);

			var work = SeedAndLoadAsync();
			var finished = await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(15)));
			if (finished == work)
				await work;
		}
		catch (HealthException)
		{
			// Leave demo data in place.
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	void Select(WorkoutSession workout)
	{
		if (workout is null)
			return;

		SelectedRoute = new ObservableCollection<RoutePoint>(workout.Route);
		SelectedTitle = workout.WorkoutType.ToString();
		SelectedStats = FormatStats(workout);
	}

	[RelayCommand]
	async Task LogWorkout()
	{
		try
		{
			IsBusy = true;
			var granted = await Health.RequestWorkoutPermissionAsync(PermissionType.Read | PermissionType.Write);
			if (!granted)
			{
				await App.Current.MainPage.DisplayAlert("Permission", "No workout permission granted", "Ok");
				return;
			}

			var start = DateTimeOffset.Now.AddMinutes(-28);
			await Health.WriteWorkoutAsync(new WorkoutSession
			{
				WorkoutType = WorkoutType.Running,
				From = start,
				Until = DateTimeOffset.Now,
				TotalDistanceInMeters = 5100,
				EnergyBurnedInCalories = 395,
				Route = BuildLoopRoute(47.3769, 8.5417, start, 28),
			});

			await LoadWorkoutsAsync();
		}
		catch (HealthException hex)
		{
			await App.Current.MainPage.DisplayAlert("Error", hex.Message, "Ok");
		}
		finally
		{
			IsBusy = false;
		}
	}

	async Task SeedAndLoadAsync()
	{
		await SeedWorkoutsOnceAsync();
		await LoadWorkoutsAsync();
	}

	async Task SeedWorkoutsOnceAsync()
	{
		if (Preferences.Get(SeededWorkoutsKey, false))
			return;

		foreach (var w in BuildDemoWorkouts())
			await Health.WriteWorkoutAsync(w);

		Preferences.Set(SeededWorkoutsKey, true);
	}

	async Task LoadWorkoutsAsync()
	{
		// WorkoutType.Other returns every workout type.
		var sessions = await Health.ReadWorkoutsAsync(WorkoutType.Other, DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now);
		if (sessions.Count == 0)
			return;

		Workouts = new ObservableCollection<WorkoutSession>(sessions.OrderByDescending(w => w.From));
		Select(Workouts[0]);
	}

	void ShowDemoData()
	{
		var demo = BuildDemoWorkouts().OrderByDescending(w => w.From).ToList();
		Workouts = new ObservableCollection<WorkoutSession>(demo);
		Select(demo[0]);
	}

	static List<WorkoutSession> BuildDemoWorkouts()
	{
		var now = DateTimeOffset.Now;
		return new List<WorkoutSession>
		{
			MakeWorkout(WorkoutType.Running, now.AddHours(-3), 28, 5100, 395, 47.3769, 8.5417),
			MakeWorkout(WorkoutType.Cycling, now.AddDays(-1), 52, 18600, 620, 47.3901, 8.5100),
			MakeWorkout(WorkoutType.Walking, now.AddDays(-3), 41, 3400, 210, 47.3660, 8.5500),
		};
	}

	static WorkoutSession MakeWorkout(WorkoutType type, DateTimeOffset start, int minutes,
		double distanceMeters, double calories, double lat, double lon) => new()
	{
		WorkoutType = type,
		From = start,
		Until = start.AddMinutes(minutes),
		TotalDistanceInMeters = distanceMeters,
		EnergyBurnedInCalories = calories,
		Route = BuildLoopRoute(lat, lon, start, minutes),
	};

	// A wobbly closed loop around a centre point — good enough to look like a real GPS track.
	static List<RoutePoint> BuildLoopRoute(double lat, double lon, DateTimeOffset start, int minutes)
	{
		const int n = 26;
		var route = new List<RoutePoint>(n);
		for (int i = 0; i < n; i++)
		{
			double a = 2 * Math.PI * i / (n - 1);
			double wobble = 0.0018 * Math.Sin(a * 3);
			route.Add(new RoutePoint
			{
				Time = start.AddSeconds(minutes * 60.0 * i / (n - 1)),
				Latitude = lat + (0.0060 + wobble) * Math.Sin(a),
				Longitude = lon + (0.0100 + wobble) * Math.Cos(a),
				AltitudeInMeters = 400 + 25 * Math.Sin(a * 2),
			});
		}
		return route;
	}

	static string FormatStats(WorkoutSession w)
	{
		var duration = w.Until - w.From;
		var parts = new List<string>();
		if (w.TotalDistanceInMeters is double d and > 0)
			parts.Add($"{d / 1000:F2} km");
		parts.Add($"{(int)duration.TotalMinutes} min");
		if (w.EnergyBurnedInCalories is double c and > 0)
			parts.Add($"{c:F0} kcal");
		parts.Add($"{w.Route.Count} GPS pts");
		return string.Join("  ·  ", parts);
	}
}
