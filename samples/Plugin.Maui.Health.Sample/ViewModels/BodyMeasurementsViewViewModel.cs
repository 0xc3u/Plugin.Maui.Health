using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;
using Plugin.Maui.Health.Sample.Services;
using Plugin.Maui.Health.Sample.Controls;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Health.Constants;

namespace Plugin.Maui.Health.Sample.ViewModels;
public partial class BodyMeasurementsViewViewModel : BaseViewModel
{
	[ObservableProperty]
	double height;

	[ObservableProperty]
	double bodyMassIndex;

	[ObservableProperty]
	double bodyMass;

	[ObservableProperty]
	double newBodyMass;

	[ObservableProperty]
	ObservableCollection<Health.Models.Sample> bodyMassSamples;

	[ObservableProperty]
	ObservableCollection<Health.Models.Sample> bodyFatPercentageSamples;

	// Chart-bound data for the MAUI.Graphics controls.
	[ObservableProperty]
	ObservableCollection<ChartEntry> weightTrend = new();

	[ObservableProperty]
	double bmiProgress;

	[ObservableProperty]
	string bmiValueText = "--";

	const string SeededWeightKey = "seeded.weight";
	static readonly double[] DemoWeights = { 74.1, 73.6, 73.8, 73.1, 72.9, 72.4, 72.6, 72.5 };

	public BodyMeasurementsViewViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
		ShowDemoData();
	}

	/// <summary>Writes showcase body data into the health store once, then loads the real values back.</summary>
	public async Task InitializeAsync()
	{
		if (!Health.IsSupported)
			return; // keep demo data

		try
		{
			IsBusy = true;

			var granted = await Health.RequestPermissionAsync(HealthParameter.BodyMass, PermissionType.Read | PermissionType.Write);
			await Health.RequestPermissionAsync(HealthParameter.Height, PermissionType.Read | PermissionType.Write);
			if (!granted)
				return; // keep demo data

			await SeedBodyOnceAsync();
			await LoadBodyAsync();
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

	async Task SeedBodyOnceAsync()
	{
		if (Preferences.Get(SeededWeightKey, false))
			return;

		for (int i = 0; i < DemoWeights.Length; i++)
		{
			var day = DateTimeOffset.Now.AddDays(i - (DemoWeights.Length - 1));
			await Health.WriteAsync(HealthParameter.BodyMass, day, DemoWeights[i], Units.Mass.Kilograms);
		}

		await Health.WriteAsync(HealthParameter.Height, DateTimeOffset.Now, 178, Units.Length.Centimeters);
		Preferences.Set(SeededWeightKey, true);
	}

	async Task LoadBodyAsync()
	{
		var latestWeight = await Health.ReadLatestAvailableAsync(HealthParameter.BodyMass, Units.Mass.Kilograms);
		if (latestWeight?.Value is double w && w > 0)
			BodyMass = w;

		var latestHeight = await Health.ReadLatestAvailableAsync(HealthParameter.Height, Units.Length.Centimeters);
		if (latestHeight?.Value is double h && h > 0)
			Height = h;

		SetBmi(ComputeBmi(BodyMass, Height));

		var samples = await Health.ReadAllAsync(HealthParameter.BodyMass, DateTimeOffset.Now.AddDays(-30), DateTimeOffset.Now, Units.Mass.Kilograms);
		if (samples.Count > 0)
		{
			BodyMassSamples = new ObservableCollection<Health.Models.Sample>(samples);
			WeightTrend = new ObservableCollection<ChartEntry>(
				samples.Select(s => new ChartEntry(s.Value ?? 0, s.From?.ToString("MM/dd"))));
		}
	}

	// Representative data so the charts look complete before real data loads.
	void ShowDemoData()
	{
		Height = 178;
		BodyMass = 72.5;
		SetBmi(ComputeBmi(BodyMass, Height));

		var today = DateTimeOffset.Now;
		WeightTrend = new ObservableCollection<ChartEntry>(
			DemoWeights.Select((v, i) => new ChartEntry(v, today.AddDays(i - DemoWeights.Length + 1).ToString("MM/dd"))));
	}

	void SetBmi(double value)
	{
		BodyMassIndex = value;
		BmiValueText = value > 0 ? value.ToString("F1") : "--";
		BmiProgress = Math.Clamp(value / 40d, 0d, 1d);
	}

	// Health Connect has no BMI record type, so BMI is derived from weight + height.
	static double ComputeBmi(double weightKg, double heightCm)
	{
		if (weightKg <= 0 || heightCm <= 0)
			return 0;

		var meters = heightCm / 100d;
		return weightKg / (meters * meters);
	}

	[RelayCommand]
	async Task ReadHeight()
	{
		await ReadBodyMeasurement(HealthParameter.Height, Units.Length.Centimeters, value => Height = value);
	}

	[RelayCommand]
	void ReadBodyMassIndex()
	{
		// Derived from the latest weight + height (Health Connect has no BMI record type).
		SetBmi(ComputeBmi(BodyMass, Height));
	}

	[RelayCommand]
	async Task ReadBodyMass()
	{
		await ReadBodyMeasurement(HealthParameter.BodyMass, Units.Mass.Kilograms, value => BodyMass = value);
	}

	[RelayCommand]
	async Task WriteBodyMass()
	{
		try
		{
			IsBusy = true;
			var hasPermission = await EnsurePermissionAsync(HealthParameter.BodyMass, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				await Health.WriteAsync(HealthParameter.BodyMass, DateTimeOffset.Now, NewBodyMass, Units.Mass.Kilograms);

				var hasBmiPermission = await EnsurePermissionAsync(HealthParameter.BodyMassIndex, PermissionType.Read | PermissionType.Write);
				if (hasBmiPermission)
				{
					var newBodyMassIndex = CalculateBodyMassIndex(NewBodyMass, Height);
					await Health.WriteAsync(HealthParameter.BodyMassIndex, DateTimeOffset.Now, newBodyMassIndex, Units.Others.Count);
				}
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{HealthParameter.BodyMass}' Permission granted", "Ok");
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
	async Task ReadAllBodyMassSamples()
	{
		try
		{
			var hasPermission = await EnsurePermissionAsync(HealthParameter.BodyMass, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var bodyMassSamples = await Health.ReadAllAsync(Enums.HealthParameter.BodyMass, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now, Constants.Units.Mass.Kilograms);
				BodyMassSamples = new ObservableCollection<Health.Models.Sample>(bodyMassSamples);

				if (bodyMassSamples.Count > 0)
				{
					WeightTrend = new ObservableCollection<ChartEntry>(
						bodyMassSamples.Select(s => new ChartEntry(s.Value ?? 0, s.From?.ToString("MM/dd"))));
				}
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{HealthParameter.BodyMass}' Permission granted", "Ok");
			}
		}
		catch (HealthException hex)
		{
			await App.Current.MainPage.DisplayAlert("Error", hex.Message, "Ok");
		}
	}

	[RelayCommand]
	async Task ReadAllBodyFatSamples()
	{
		try
		{
			var hasPermission = await EnsurePermissionAsync(HealthParameter.BodyFatPercentage, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var bodyMassSamples = await Health.ReadAllAsync(Enums.HealthParameter.BodyFatPercentage, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now, Constants.Units.Others.Percentage);
				BodyMassSamples = new ObservableCollection<Health.Models.Sample>(bodyMassSamples);
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{HealthParameter.BodyFatPercentage}' Permission granted", "Ok");
			}
		}
		catch (HealthException hex)
		{
			await App.Current.MainPage.DisplayAlert("Error", hex.Message, "Ok");
		}
	}

	async Task ReadBodyMeasurement(HealthParameter healthParameter, string unit, Action<double> callback)
	{
		try
		{
			IsBusy = true;
			var hasPermission = await EnsurePermissionAsync(healthParameter, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var value = await Health.ReadLatestAvailableAsync(healthParameter, unit);

				if (value != null)
				{
					callback(value.Value ?? 0);
				}
				else
				{
					callback(0);
				}
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{healthParameter}' Permission granted", "Ok");
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

	double CalculateBodyMassIndex(double height, double bodyMass)
	{
		double bodyMassIndex = 0;

		if (height > 0 && bodyMass >0)
		{
			bodyMassIndex = bodyMass / Math.Pow(height/ 100, 2);
		}

		return bodyMassIndex;
	}
}
