using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Exceptions;
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
			var hasPermission = await health.CheckPermissionAsync(HealthParameter.StepCount, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				StepsCount = await health.ReadCountAsync(Enums.HealthParameter.StepCount, DateTime.Now.AddDays(-1), DateTime.Now);
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
	//			var bodyMassSamples = await health.ReadAllAsync(Enums.HealthParameter.BodyMass, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Mass.Kilograms);
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
			var hasPermission = await health.CheckPermissionAsync(HealthParameter.VO2Max, PermissionType.Read | PermissionType.Write);
			if (hasPermission)
			{
				var vo2max = await health.ReadLatestAsync(Enums.HealthParameter.VO2Max, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Concentration.MillilitersPerKilogramPerMinute);
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
			var hasPermission = await health.CheckPermissionAsync(HealthParameter.StepCount, PermissionType.Write);

			if (hasPermission)
			{
				await health.WriteAsync(HealthParameter.StepCount, DateTime.Now, StepsCountWrite, "Count");
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

	public override void OnAppearing(object param)
	{
		StepsCount = 0;

		MenuItems = new ObservableCollection<Models.MenuItem>
		{
			new Models.MenuItem("Body Measurements", typeof(BodyMeasurementsView)),
			new Models.MenuItem("Vitamins", typeof(VitaminsView))
		};

	}
}
