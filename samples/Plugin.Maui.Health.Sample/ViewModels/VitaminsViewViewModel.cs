using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Health.Constants;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
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

	public VitaminsViewViewModel(IHealth health, INavigationService navigationService) : base(health, navigationService)
	{
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
			var hasPermission = await Health.CheckPermissionAsync(HealthParameter.DietaryVitaminC, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				await Health.WriteAsync(HealthParameter.DietaryVitaminC, DateTime.Now, NewVitaminC, Units.Mass.Milligrams);
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

}
