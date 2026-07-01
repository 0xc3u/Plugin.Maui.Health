using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Plugin.Maui.Health.Constants;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Sample.Controls;
using Plugin.Maui.Health.Sample.Services;

namespace Plugin.Maui.Health.Sample.ViewModels;

public partial class HeartRateViewViewModel : BaseViewModel
{
	const string SeededHeartRateKey = "seeded.heartrate";

	// A representative daily curve (resting → active peaks → recovery), oldest → newest.
	static readonly int[] DemoBpm = { 61, 58, 60, 66, 82, 128, 104, 90, 75, 112, 88, 72, 68 };

	bool initialized;

	[ObservableProperty]
	ObservableCollection<ChartEntry> heartRateTrend = new();

	[ObservableProperty]
	double latestBpm;

	[ObservableProperty]
	double averageBpm;

	[ObservableProperty]
	double minBpm;

	[ObservableProperty]
	double maxBpm;

	[ObservableProperty]
	double newHeartRate;

	public HeartRateViewViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
		ShowDemoData();
	}

	public async Task InitializeAsync()
	{
		if (initialized)
			return;
		initialized = true;

		await RunInitializeAsync(async () =>
		{
			await SeedHeartRateOnceAsync();
			await LoadHeartRateAsync();
		});
	}

	[RelayCommand]
	async Task Refresh()
	{
		try
		{
			IsBusy = true;
			await LoadHeartRateAsync();
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

	[RelayCommand]
	async Task WriteHeartRate()
	{
		try
		{
			IsBusy = true;
			var hasPermission = await EnsurePermissionAsync(HealthParameter.HeartRate, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				await Health.WriteAsync(HealthParameter.HeartRate, DateTimeOffset.Now, NewHeartRate, Units.Others.CountPerMinute);
				await LoadHeartRateAsync();
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{HealthParameter.HeartRate}' Permission granted", "Ok");
			}
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

	async Task SeedHeartRateOnceAsync()
	{
		// Keyed per day so "today" always has a fresh curve (see MainPageViewModel steps seeding).
		var key = $"{SeededHeartRateKey}.{DateTime.Now:yyyyMMdd}";
		if (Preferences.Get(key, false))
			return;

		var now = DateTimeOffset.Now;
		for (int i = 0; i < DemoBpm.Length; i++)
		{
			var time = now.AddHours(-(DemoBpm.Length - 1 - i));
			await Health.WriteAsync(HealthParameter.HeartRate, time, DemoBpm[i], Units.Others.CountPerMinute);
		}

		Preferences.Set(key, true);
	}

	async Task LoadHeartRateAsync()
	{
		var from = DateTimeOffset.Now.AddHours(-12);
		var until = DateTimeOffset.Now;

		var samples = await Health.ReadAllAsync(HealthParameter.HeartRate, from, until, Units.Others.CountPerMinute);
		var points = samples.Where(s => s.Value.HasValue).ToList();
		if (points.Count == 0)
			return;

		HeartRateTrend = new ObservableCollection<ChartEntry>(
			points.Select(s => new ChartEntry(s.Value!.Value, s.From?.ToLocalTime().ToString("HH:mm"))));

		var values = points.Select(s => s.Value!.Value).ToList();
		AverageBpm = Math.Round(values.Average());
		MinBpm = values.Min();
		MaxBpm = values.Max();
		LatestBpm = values[^1];
	}

	// Representative data so the chart/stats look complete before real data loads.
	void ShowDemoData()
	{
		var now = DateTimeOffset.Now;
		HeartRateTrend = new ObservableCollection<ChartEntry>(
			DemoBpm.Select((v, i) => new ChartEntry(v, now.AddHours(-(DemoBpm.Length - 1 - i)).ToString("HH:mm"))));

		AverageBpm = Math.Round(DemoBpm.Average());
		MinBpm = DemoBpm.Min();
		MaxBpm = DemoBpm.Max();
		LatestBpm = DemoBpm[^1];
	}
}
