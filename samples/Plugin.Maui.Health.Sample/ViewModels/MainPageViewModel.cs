using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Plugin.Maui.Health.Constants;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Sample.Controls;
using Plugin.Maui.Health.Sample.Services;
using Plugin.Maui.Health.Sample.Views;

namespace Plugin.Maui.Health.Sample.ViewModels;
public partial class MainPageViewModel : BaseViewModel
{
	readonly IHealth health;

	[ObservableProperty]
	double stepsCount;

	[ObservableProperty]
	double stepsCountWrite;

	[ObservableProperty]
	double vo2max;

	[ObservableProperty]
	ObservableCollection<Models.MenuItem> menuItems;

	// Showcase data for the dashboard charts.
	[ObservableProperty]
	ObservableCollection<ChartEntry> stepsThisWeek = new();

	[ObservableProperty]
	double stepsGoalProgress;

	[ObservableProperty]
	string stepsGoalText = "0%";

	[ObservableProperty]
	string stepsTodayText = "0";

	public MainPageViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
		this.health = health;
	}

	[RelayCommand]
	async Task ReadStepsCountAsnyc()
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;

		try
		{
			var hasPermission = await EnsurePermissionAsync(HealthParameter.StepCount, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				StepsCount = await health.ReadCountAsync(Enums.HealthParameter.StepCount, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now);
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{HealthParameter.StepCount}' Permission granted", "Ok");
			}
		}
		catch(HealthException hex)
		{
			await App.Current.MainPage.DisplayAlert("Error", hex.Message, "Ok");
		}
		finally
		{
			IsBusy = false;
		}
	}


	//[RelayCommand]
	//async Task ReadAllBodyMassSamplesAsync()
	//{
	//	try
	//	{
	//		var hasPermission = await health.CheckPermissionAsync(HealthParameter.BodyMass, PermissionType.Read | PermissionType.Write);
	//		if (hasPermission)
	//		{
	//			var bodyMassSamples = await health.ReadAllAsync(Enums.HealthParameter.BodyMass, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now, Constants.Units.Mass.Kilograms);
	//			BodyMassSamples = new ObservableCollection<Models.Sample>(bodyMassSamples);
	//		}
	//		else
	//		{
	//			await App.Current.MainPage.DisplayAlert("Permission", $"No '{HealthParameter.BodyMass}' Permission granted", "Ok");
	//		}
	//	}
	//	catch (HealthException hex)
	//	{
	//		await App.Current.MainPage.DisplayAlert("Error", hex.Message, "Ok");
	//	}
	//}

	[RelayCommand]
	async Task ReadVO2MaxAsync()
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;

		try
		{
			var hasPermission = await EnsurePermissionAsync(HealthParameter.VO2Max, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var vo2max = await health.ReadLatestAsync(Enums.HealthParameter.VO2Max, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now, Constants.Units.Concentration.MillilitersPerKilogramPerMinute);
				Vo2max = (vo2max ?? 0) * 1000;
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{HealthParameter.VO2Max}' Permission granted", "Ok");
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

	[RelayCommand]
	async Task WriteStepsCountAsync()
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;

		try
		{
			var hasPermission = await EnsurePermissionAsync(HealthParameter.StepCount, PermissionType.Write);

			if (hasPermission)
			{
				await health.WriteAsync(HealthParameter.StepCount, DateTimeOffset.Now, StepsCountWrite, "Count");
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

	[RelayCommand]
	Task OpenView(Models.MenuItem menu)
	{
		return Navigationservice.NavigateToViewAsync(menu.ViewType.Name);
	}

	const string SeededStepsKey = "seeded.steps";
	static readonly int[] DemoSteps = { 6400, 9100, 7300, 11200, 8400, 5200, 8432 };

	bool initialized;

	public override void OnAppearing(object param)
	{
		MenuItems = new ObservableCollection<Models.MenuItem>
		{
			new Models.MenuItem("Body Measurements", typeof(BodyMeasurementsView)),
			new Models.MenuItem("Heart Rate", typeof(HeartRateView)),
			new Models.MenuItem("Vitamins", typeof(VitaminsView))
		};

		ShowDemoData();          // instant placeholder so the page never flashes empty
		_ = InitializeAsync();   // replace with real device data
	}

	/// <summary>Writes showcase steps into the health store once, then loads the real values back.</summary>
	async Task InitializeAsync()
	{
		if (initialized)
			return; // run once; OnAppearing can fire repeatedly
		initialized = true;

		await RunInitializeAsync(async () =>
		{
			await SeedStepsOnceAsync();
			await LoadStepsAsync();
		});
	}

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
		// Read step records for the week and bucket them per day. (ReadCountAsync aggregation is the
		// other option, but reading records works consistently across devices and emulators.)
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

	// Representative weekly steps so the dashboard looks complete before real data loads.
	void ShowDemoData()
	{
		string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
		StepsThisWeek = new ObservableCollection<ChartEntry>(
			DemoSteps.Select((v, i) => new ChartEntry(v, days[i])));
		UpdateStepsHeader(DemoSteps[^1]);
	}

	void UpdateStepsHeader(double today)
	{
		const double goal = 10000;
		StepsCount = today;
		StepsTodayText = today.ToString("#,0");
		StepsGoalProgress = Math.Clamp(today / goal, 0d, 1d);
		StepsGoalText = $"{StepsGoalProgress * 100:F0}%";
	}
}
