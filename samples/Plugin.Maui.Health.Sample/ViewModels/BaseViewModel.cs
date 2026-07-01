using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Sample.Interfaces;
using Plugin.Maui.Health.Sample.Services;

namespace Plugin.Maui.Health.Sample.ViewModels;

public partial class BaseViewModel : ObservableObject, IViewModel
{
	public IHealth Health { get; }
	public INavigationService Navigationservice { get; }

	[ObservableProperty]
	bool isBusy;

	public BaseViewModel(IHealth health, INavigationService navigationService)
	{
		Health = health;
		Navigationservice = navigationService;
	}

	public virtual void OnAppearing(object param) { }

	public virtual Task RefreshAsync()
	{
		return Task.CompletedTask;
	}

	/// <summary>
	/// Ensures the given permission is granted, requesting it through the platform consent UI if needed.
	/// The plugin handles the platform differences: on Android it launches the Health Connect consent
	/// UI (when not already granted); on iOS HealthKit presents its own sheet.
	/// </summary>
	protected Task<bool> EnsurePermissionAsync(HealthParameter parameter, PermissionType permissionType)
		=> Health.RequestPermissionAsync(parameter, permissionType);

	// Guards against prompting more than once per app session.
	static bool corePermissionsRequested;

	/// <summary>
	/// Requests every permission the sample uses in a single prompt. Doing this once up front avoids
	/// a string of consecutive consent prompts (which on iOS can hang the app). Subsequent per-screen
	/// reads/writes then run against already-granted permissions.
	/// </summary>
	protected async Task EnsureCorePermissionsAsync()
	{
		if (corePermissionsRequested)
			return;

		corePermissionsRequested = true;

		const PermissionType rw = PermissionType.Read | PermissionType.Write;
		var requests = new (HealthParameter, PermissionType)[]
		{
			(HealthParameter.StepCount, rw),
			(HealthParameter.HeartRate, rw),
			(HealthParameter.BodyMass, rw),
			(HealthParameter.Height, rw),
			(HealthParameter.BodyFatPercentage, rw),
			(HealthParameter.DietaryVitaminC, rw),
			(HealthParameter.DietaryVitaminD, rw),
			(HealthParameter.DietaryVitaminE, rw),
			(HealthParameter.DietaryVitaminB6, rw),
			(HealthParameter.DietaryVitaminB12, rw),
			(HealthParameter.DietaryVitaminK, rw),
		};

		try
		{
			await Health.RequestPermissionsAsync(requests);
		}
		catch (Plugin.Maui.Health.Exceptions.HealthException)
		{
			// Ignored — individual reads/writes will surface a clear error if access is missing.
		}
	}

	/// <summary>
	/// Requests the core permissions once, then runs <paramref name="seedAndLoad"/> while showing the
	/// busy indicator. The data work is bounded by a timeout so a slow/flaky platform health callback
	/// can never leave the UI spinning forever (the demo data simply remains in that case).
	/// </summary>
	protected async Task RunInitializeAsync(Func<Task> seedAndLoad, int timeoutSeconds = 15)
	{
		if (!Health.IsSupported)
			return; // keep in-memory demo data

		try
		{
			IsBusy = true;

			await EnsureCorePermissionsAsync(); // single consent prompt (user-paced, not timed out)

			var work = seedAndLoad();
			var finished = await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));
			if (finished == work)
				await work; // observe result/exception when it actually completed
		}
		catch (Plugin.Maui.Health.Exceptions.HealthException)
		{
			// Leave demo data in place; the error was surfaced as a clear HealthException.
		}
		finally
		{
			IsBusy = false;
		}
	}
}
