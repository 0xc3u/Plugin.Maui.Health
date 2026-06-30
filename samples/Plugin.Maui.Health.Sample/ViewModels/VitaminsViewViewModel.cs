using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Health.Constants;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
using Plugin.Maui.Health.Sample.Controls;
using Plugin.Maui.Health.Sample.Services;

namespace Plugin.Maui.Health.Sample.ViewModels;

public partial class VitaminsViewViewModel : BaseViewModel
{
	[ObservableProperty]
	double vitaminC;

	[ObservableProperty]
	double newVitaminC;

	[ObservableProperty]
	double vitaminA;

	[ObservableProperty]
	double vitaminB6;

	[ObservableProperty]
	double vitaminD;

	[ObservableProperty]
	double vitaminB12;

	[ObservableProperty]
	double vitaminE;

	[ObservableProperty]
	double vitaminK;

	// Bar chart showing each vitamin as a percentage of its reference daily intake,
	// so values on very different mg scales remain comparable.
	[ObservableProperty]
	ObservableCollection<ChartEntry> vitaminLevels = new();

	public VitaminsViewViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
		// Representative intake so the chart looks complete on first view; replaced by real reads.
		VitaminC = 82; VitaminD = 0.015; VitaminE = 12; VitaminB6 = 1.6; VitaminB12 = 0.0024; VitaminK = 0.11;
		RefreshChart();
	}

	void RefreshChart()
	{
		(string label, double value, double rdi)[] items =
		{
			("C", VitaminC, 90), ("D", VitaminD, 0.02), ("E", VitaminE, 15),
			("B6", VitaminB6, 1.7), ("B12", VitaminB12, 0.0024), ("K", VitaminK, 0.12),
		};
		VitaminLevels = new ObservableCollection<ChartEntry>(
			items.Select(i => new ChartEntry(i.rdi > 0 ? Math.Round(i.value / i.rdi * 100) : 0, i.label)));
	}

	[RelayCommand]
	async Task ReadVitaminC()
	{
		await ReadVitamin(HealthParameter.DietaryVitaminC, Units.Mass.Milligrams, value => VitaminC = value);
	}

	[RelayCommand]
	async Task WriteVitaminC()
	{
		try
		{
			IsBusy = true;
			var hasPermission = await EnsurePermissionAsync(HealthParameter.DietaryVitaminC, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				await Health.WriteAsync(HealthParameter.DietaryVitaminC, DateTimeOffset.Now, NewVitaminC, Units.Mass.Milligrams);
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{HealthParameter.DietaryVitaminC}' Permission granted", "Ok");
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
	async Task ReadVitaminK()
	{
		await ReadVitamin(HealthParameter.DietaryVitaminK, Units.Mass.Milligrams, value => VitaminK = value);
	}

	[RelayCommand]
	async Task ReadVitaminA()
	{
		await ReadVitamin(HealthParameter.DietaryVitaminA, Units.Mass.Milligrams, value => VitaminA = value);
	}

	[RelayCommand]
	async Task ReadVitaminB6()
	{
		await ReadVitamin(HealthParameter.DietaryVitaminB6, Units.Mass.Milligrams, value => VitaminB6 = value);
	}

	[RelayCommand]
	async Task ReadVitaminB12()
	{
		await ReadVitamin(HealthParameter.DietaryVitaminB12, Units.Mass.Milligrams, value => VitaminB12 = value);
	}

	[RelayCommand]
	async Task ReadVitaminD()
	{
		await ReadVitamin(HealthParameter.DietaryVitaminD, Units.Mass.Milligrams, value => VitaminD = value);
	}

	[RelayCommand]
	async Task ReadVitaminE()
	{
		await ReadVitamin(HealthParameter.DietaryVitaminE, Units.Mass.Milligrams, value => VitaminE = value);
	}

	async Task ReadVitamin(HealthParameter vitamin, string unit, Action<double> callback)
	{
		try
		{
			IsBusy = true;
			var hasPermission = await EnsurePermissionAsync(vitamin, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var value = await Health.ReadLatestAsync(vitamin, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now, unit);
				callback(value ?? 0);
				RefreshChart();
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{vitamin}' Permission granted", "Ok");
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

}
