using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Plugin.Maui.Health.Sample.Services;
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

	public BodyMeasurementsViewViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{

	}

	[RelayCommand]
	async Task ReadHeight()
	{
		await ReadBodyMeasurement(HealthParameter.Height, Units.Length.Centimeters, value => Height = value);
	}

	[RelayCommand]
	async Task ReadBodyMassIndex()
	{
		await ReadBodyMeasurement(HealthParameter.BodyMassIndex, Units.Others.Count, value => BodyMassIndex = value);
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
			var hasPermission = await Health.CheckPermissionAsync(HealthParameter.BodyMass, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				await Health.WriteAsync(HealthParameter.BodyMass, DateTime.Now, NewBodyMass, Units.Mass.Kilograms);

				var hasBmiPermission = await Health.CheckPermissionAsync(HealthParameter.BodyMassIndex, PermissionType.Read | PermissionType.Write);
				if (hasBmiPermission)
				{
					var newBodyMassIndex = CalculateBodyMassIndex(NewBodyMass, Height);
					await Health.WriteAsync(HealthParameter.BodyMassIndex, DateTime.Now, newBodyMassIndex, Units.Others.Count);
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
			var hasPermission = await Health.CheckPermissionAsync(HealthParameter.BodyMass, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var bodyMassSamples = await Health.ReadAllAsync(Enums.HealthParameter.BodyMass, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Mass.Kilograms);
				BodyMassSamples = new ObservableCollection<Health.Models.Sample>(bodyMassSamples);
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
			var hasPermission = await Health.CheckPermissionAsync(HealthParameter.BodyFatPercentage, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var bodyMassSamples = await Health.ReadAllAsync(Enums.HealthParameter.BodyFatPercentage, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Others.Percentage);
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

	async Task ReadBodyMeasurement(HealthParameter vitamin, string unit, Action<double> callback)
	{
		try
		{
			IsBusy = true;
			var hasPermission = await Health.CheckPermissionAsync(vitamin, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var value = await Health.ReadLatestAsync(vitamin, DateTime.Now.AddDays(-1), DateTime.Now, unit);
				callback(value ?? 0);
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
