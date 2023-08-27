using System.Windows.Input;
using Plugin.Maui.Health.Exceptions;

namespace Plugin.Maui.Health.Sample.ViewModels;
public class MainPageViewModel : BaseViewModel
{
	readonly IHealth health;

	public ICommand GetStepsCountCommand { get; protected set; }
	public ICommand WriteStepsCountCommand { get; protected set; }

	double stepsCount;
	public double StepsCount
	{
		get => stepsCount;
		set => SetProperty(ref stepsCount, value);
	}

	double stepsCountWrite;
	public double StepsCountWrite
	{
		get => stepsCountWrite;
		set => SetProperty(ref stepsCountWrite, value);
	}

	public MainPageViewModel(IHealth health)
	{
		this.health = health;

		InitCommands();
	}

	void InitCommands()
	{
		GetStepsCountCommand = new Command(async () => await RetrieveStepsCountAsnyc());
		WriteStepsCountCommand = new Command(async () => await WriteStepsCountAsync());
	}

	async Task RetrieveStepsCountAsnyc()
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

	}
}
