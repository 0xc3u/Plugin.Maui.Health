using System.Collections.ObjectModel;
using System.Windows.Input;
using Plugin.Maui.Health.Exceptions;

namespace Plugin.Maui.Health.Sample.ViewModels;
public class MainPageViewModel : BaseViewModel
{
	readonly IHealth health;

	double stepsCount;
	public double StepsCount
	{
		get => stepsCount;
		set => SetProperty(ref stepsCount, value);
	}

	double bodyMass;
	public double BodyMass
	{
		get => bodyMass;
		set => SetProperty(ref bodyMass, value);
	}

	double stepsCountWrite;
	public double StepsCountWrite
	{
		get => stepsCountWrite;
		set => SetProperty(ref stepsCountWrite, value);
	}

	double vitaminC;
	public double VitaminC
	{
		get => vitaminC;
		set => SetProperty(ref vitaminC, value);
	}

	double vo2max;
	public double Vo2max
	{
		get => vo2max;
		set => SetProperty(ref vo2max, value);
	}

	ObservableCollection<Models.Sample> bodyMassSamples;
	public ObservableCollection<Models.Sample> BodyMassSamples
	{
		get => bodyMassSamples;
		set => SetProperty(ref bodyMassSamples, value);
	}

	public ICommand ReadStepsCountCommand { get; protected set; }
	public ICommand WriteStepsCountCommand { get; protected set; }
	public ICommand ReadBodyMassCommand { get; protected set; }
	public ICommand ReadVitaminCommand { get; protected set; }

	public ICommand ReadVO2MaxCommand { get; protected set; }


	public MainPageViewModel(IHealth health)
	{
		this.health = health;

		InitCommands();
	}

	void InitCommands()
	{
		ReadStepsCountCommand = new Command(async () => await ReadStepsCountAsnyc());
		ReadBodyMassCommand = new Command(async () => await ReadBodyMassAsnyc());
		ReadVitaminCommand = new Command(async () => await ReadVitaminCAsnyc());
		WriteStepsCountCommand = new Command(async () => await WriteStepsCountAsync());
		ReadVO2MaxCommand = new Command(async () => await ReadVO2MaxAsync());
	}

	async Task ReadStepsCountAsnyc()
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;

		try
		{
			var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.StepCount, Health.Enums.PermissionType.Read | Health.Enums.PermissionType.Write);
			if (hasPermission)
			{
				StepsCount = await health.ReadCountAsync(Enums.HealthParameter.StepCount, DateTime.Now.AddDays(-1), DateTime.Now);
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{Health.Enums.HealthParameter.StepCount}' Permission granted", "Ok");
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

	async Task ReadBodyMassAsnyc()
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;

		try
		{
			var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.BodyMass, Health.Enums.PermissionType.Read | Health.Enums.PermissionType.Write);
			if (hasPermission)
			{
				var bodyMass = await health.ReadLatestAsync(Enums.HealthParameter.BodyMass, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Mass.Kilograms);

				var avg = await health.ReadAverageAsync(Enums.HealthParameter.BodyMass, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Mass.Kilograms);

				BodyMass = bodyMass ?? 0;

				await ReadAllBodyMassSamplesAsync();
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{Health.Enums.HealthParameter.BodyMass}' Permission granted", "Ok");
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

	async Task ReadAllBodyMassSamplesAsync()
	{
		try
		{
			var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.BodyMass, Health.Enums.PermissionType.Read | Health.Enums.PermissionType.Write);
			if (hasPermission)
			{
				var bodyMassSamples = await health.ReadAllAsync(Enums.HealthParameter.BodyMass, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Mass.Kilograms);
				BodyMassSamples = new ObservableCollection<Models.Sample>(bodyMassSamples);
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{Health.Enums.HealthParameter.BodyMass}' Permission granted", "Ok");
			}
		}
		catch (HealthException hex)
		{
			await App.Current.MainPage.DisplayAlert("Error", hex.Message, "Ok");
		}
	}


	async Task ReadVitaminCAsnyc()
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;

		try
		{
			var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.DietaryVitaminC, Health.Enums.PermissionType.Read | Health.Enums.PermissionType.Write);
			if (hasPermission)
			{
				var vitaminC = await health.ReadLatestAsync(Enums.HealthParameter.DietaryVitaminC, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Mass.Milligrams);
				VitaminC = vitaminC ?? 0;
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{Health.Enums.HealthParameter.DietaryVitaminC}' Permission granted", "Ok");
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

	async Task ReadVO2MaxAsync()
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;

		try
		{
			var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.VO2Max, Health.Enums.PermissionType.Read | Health.Enums.PermissionType.Write);
			if (hasPermission)
			{
				var vo2max = await health.ReadLatestAsync(Enums.HealthParameter.VO2Max, DateTime.Now.AddDays(-1), DateTime.Now, Constants.Units.Concentration.MillilitersPerKilogramPerMinute);
				Vo2max = (vo2max ?? 0) * 1000;
			}
			else
			{
				await App.Current.MainPage.DisplayAlert("Permission", $"No '{Health.Enums.HealthParameter.VO2Max}' Permission granted", "Ok");
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


	async Task WriteStepsCountAsync()
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;

		try
		{
			var hasPermission = await health.CheckPermissionAsync(Health.Enums.HealthParameter.StepCount, Health.Enums.PermissionType.Write);

			if (hasPermission)
			{
				await health.WriteAsync(Health.Enums.HealthParameter.StepCount, DateTime.Now, StepsCountWrite);
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

	public override void OnAppearing(object param)
	{
		StepsCount = 0;
		BodyMass = 0;
		BodyMassSamples = new ObservableCollection<Models.Sample>();
	}
}
